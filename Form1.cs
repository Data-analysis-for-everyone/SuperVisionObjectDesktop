using Microsoft.VisualBasic.Devices;
using SuperVisionObjectDesktop._3dParts;
using SuperVisionObjectDesktop.Devices;
using SuperVisionObjectDesktop.Network;
using SuperVisionObjectDesktop.ViewModels;
using SuperVisionObjectDesktop.ViewModels.GraphicsTabs;
using System.Windows.Forms;

namespace SuperVisionObjectDesktop
{
    public partial class Form1 : Form
    {
        DevicesListViewModel _devicesListViewModel;
        DevicesManager _devicesManager;
        public Form1()
        {
            InitializeComponent();


            // Первая вкладка
            tabControl1.TabPages.Clear();
            tabControl1.TabPages.Add(new TabPage());
            tabControl1.TabPages[0].Controls.Add(new MAX30105Tabs(null) { Dock = DockStyle.Fill });
            //tabPage1.Controls.Add(_plotLogs1);

            // Вторая вкладка
            //_plotLogs2 = new PlotWithLogsControl { Dock = DockStyle.Fill };
            //tabPage2.Controls.Add(_plotLogs2);




            _devicesListViewModel = new DevicesListViewModel(DevicesFlowLayout);


            LoadPicture.Visible = false;
            this.Load += Form1_Load;
            this.Resize += Form1_Resize; // Добавляем обработчик изменения размера формы


            InitTasks();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Настройка FlowLayoutPanel
            //DevicesFlowLayout.FlowDirection = FlowDirection.TopDown;
            //DevicesFlowLayout.WrapContents = false;
            //DevicesFlowLayout.AutoScroll = true;
            //DevicesFlowLayout.Padding = new Padding(10);
            //DevicesFlowLayout.BackColor = Color.FromArgb(240, 240, 240);

            //// Чтобы панель растягивалась при изменении размера формы
            //DevicesFlowLayout.Dock = DockStyle.Fill;

            // Инициализация данных
            //InitializeData();

            // Заполнение панели
            //FillFlowLayoutPanel();
        }

        // Обработчик изменения размера формы
        private void Form1_Resize(object sender, EventArgs e)
        {
            // Принудительно обновляем размеры всех элементов
            foreach (Control control in DevicesFlowLayout.Controls)
            {
                if (control is DeviceItemControl item)
                {
                    // Вызываем обновление размера
                    item.Width = DevicesFlowLayout.ClientSize.Width - DevicesFlowLayout.Padding.Horizontal - 20;
                }
            }
        }

        private async void InitTasks()
        {
            await _devicesListViewModel.InitComponents();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            LoadPicture.Visible = true;
            await _devicesListViewModel.InitComponents();
            LoadPicture.Visible = false;
        }


        GraphicsTabViewModel _graphicsTabViewModel;

        private void button2_Click(object sender, EventArgs e)
        {
            var devices = _devicesListViewModel.GetUsingDevices();
            _graphicsTabViewModel = new GraphicsTabViewModel(tabControl1, devices);


            _devicesManager = new DevicesManager(devices);
            _devicesManager.RunProcess();

        }
    }
}
