using SuperVisionObjectDesktop.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperVisionObjectDesktop.ViewModels
{
    public class DeviceItemControl : Panel
    {
        // Ссылка на объект данных
        private IDevice _device;
        public IDevice Device => _device;

        // Контролы
        private Label[] labels = new Label[6];
        private CheckBox checkBox;
        private Button viewButton;

        // Событие для уведомления о клике по элементу
        public event EventHandler<IDevice> ItemClicked;

        private bool _isResizing = false;

        public DeviceItemControl(IDevice device)
        {
            _device = device;

            this.Margin = new Padding(5);
            this.BackColor = Color.White;
            this.BorderStyle = BorderStyle.FixedSingle;
            this.Padding = new Padding(10);
            this.Cursor = Cursors.Hand;

            InitializeControls();

            this.MinimumSize = new Size(200, 140);

            
            
            UpdateData(device);

            this.ParentChanged += PersonItemControl_ParentChanged;
        }

        private void InitializeControls()
        {
            // Чекбокс справа
            checkBox = new CheckBox();
            checkBox.Size = new Size(20, 20);
            checkBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            checkBox.Checked = _device.IsUsing;
            checkBox.CheckedChanged += CheckBox_CheckedChanged;

            // Кнопка "Подробнее"
            viewButton = new Button();
            viewButton.Text = "Просмотр";
            viewButton.Size = new Size(80, 25);
            viewButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            viewButton.Click += ViewButton_Click;

            string[] labelNames = { "Устройство:", "Тип:", "Сенсор:", "IP:", "Каналы-Частота:", "Уровень сигнала:" };
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i] = new Label();
                labels[i].Text = $"{labelNames[i]} ";
                labels[i].Location = new Point(10, 10 + i * 22);
                labels[i].AutoSize = true;
                labels[i].Font = new Font("Arial", 9);
                labels[i].Tag = i; // Запоминаем индекс
            }

            // Добавляем все контролы
            this.Controls.AddRange(labels);
            this.Controls.Add(checkBox);
            this.Controls.Add(viewButton);

            // Добавляем обработчик клика на всю панель
            this.Click += PersonItemControl_Click;
        }


        private void PersonItemControl_ParentChanged(object sender, EventArgs e)
        {
            if (this.Parent != null)
            {
                // Подписываемся на изменение размера родителя
                this.Parent.Resize += Parent_Resize;
                // Обновляем размер при первом добавлении
                UpdateSize();
            }
        }

        private void Parent_Resize(object sender, EventArgs e)
        {
            UpdateSize();
        }

        private void UpdateSize()
        {
            if (_isResizing || this.Parent == null) return;

            _isResizing = true;

            try
            {
                // Получаем ширину родителя с учетом отступов
                int parentWidth = this.Parent.ClientSize.Width;
                int padding = this.Parent.Padding.Horizontal;
                int margin = this.Margin.Horizontal;

                // Вычисляем доступную ширину
                int availableWidth = parentWidth - padding - margin;

                // Устанавливаем ширину (минус небольшие отступы)
                this.Width = Math.Max(this.MinimumSize.Width, availableWidth - 5);

                // Обновляем расположение контролов
                UpdateControlsLayout();
            }
            finally
            {
                _isResizing = false;
            }
        }

        private void UpdateControlsLayout()
        {
            // Размещаем чекбокс и кнопку справа
            int rightMargin = 10;
            int checkBoxWidth = 20;
            int buttonWidth = 80;
            int spacing = 5;

            // Чекбокс справа вверху
            checkBox.Location = new Point(
                this.Width - checkBoxWidth - rightMargin,
                10
            );

            // Кнопка под чекбоксом
            viewButton.Location = new Point(
                this.Width - buttonWidth - rightMargin,
                40
            );

            // Для лейблов делаем отступ, чтобы они не наезжали на кнопку
            int maxLabelWidth = this.Width - buttonWidth - rightMargin - 20;

            foreach (var label in labels)
            {
                if (label != null)
                {
                    // Если текст слишком длинный, обрезаем или переносим
                    if (label.Width > maxLabelWidth)
                    {
                        label.AutoSize = false;
                        label.Width = maxLabelWidth;
                        label.Text = TruncateText(label.Text, maxLabelWidth);
                    }
                    else
                    {
                        label.AutoSize = true;
                    }
                }
            }
        }

        private string TruncateText(string text, int maxWidth)
        {
            // Простая обрезка для длинных текстов
            if (text.Length > 30)
            {
                return text.Substring(0, 30) + "...";
            }
            return text;
        }




        public void UpdateData(IDevice device)
        {
            _device = device;

            // Обновляем тексты лейблов
            labels[0].Text = $"Устройство: {_device.Name}";
            labels[1].Text = $"Тип: {_device.DeviceType}";
            labels[2].Text = $"Сенсор: {_device.SensorType}";
            labels[3].Text = $"IP: {_device.IPAddress.ToString()}";
            labels[4].Text = $"Каналы-Частота: {_device.ChannelsCount} - {_device.SampleFrequancy}";
            labels[5].Text = $"Уровень сигнала: {_device.RSSI}";

            // Обновляем чекбокс
            if (checkBox != null)
                checkBox.Checked = _device.IsUsing;

            // Меняем цвет фона в зависимости от выбора
            this.BackColor = _device.IsUsing ? Color.LightGreen : Color.White;
            UpdateControlsLayout();
        }

        private void PersonItemControl_Click(object sender, EventArgs e)
        {
            OnItemClicked();
        }

        private void OnItemClicked()
        {
            // Инвертируем состояние выбора при клике
            _device.IsUsing = !_device.IsUsing;
            checkBox.Checked = _device.IsUsing;
            this.BackColor = _device.IsUsing ? Color.LightGreen : Color.White;
            // Вызываем событие
            ItemClicked?.Invoke(this, _device);
        }

        private void CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _device.IsUsing = checkBox.Checked;
            this.BackColor = _device.IsUsing ? Color.LightGreen : Color.White;
        }

        private void ViewButton_Click(object sender, EventArgs e)
        {
            // Показываем детальную информацию
            string details = $"Устройство: {_device.Name}\n" +
                            $"Тип: {_device.DeviceType}\n" +
                            $"Сенсор: {_device.SensorType}\n" +
                            $"IP: {_device.IPAddress}\n" +
                            $"Каналы-Частота: {_device.ChannelsCount} - {_device.SampleFrequancy}\n" +
                            $"Уровень сигнала: {_device.RSSI}\n";

            MessageBox.Show(details, "Информация об устройстве", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Метод для доступа к чекбоксу
        public bool IsSelected => checkBox.Checked;

        // Метод для установки состояния выбора
        public void SetSelected(bool selected)
        {
            checkBox.Checked = selected;
            _device.IsUsing = selected;
            this.BackColor = selected ? Color.LightGreen : Color.White;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (!_isResizing)
            {
                UpdateControlsLayout();
            }
        }
    }
}
