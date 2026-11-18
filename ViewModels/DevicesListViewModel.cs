using SuperVisionObjectDesktop.Devices;
using SuperVisionObjectDesktop.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SuperVisionObjectDesktop.ViewModels
{
    public class DevicesListViewModel
    {
        SuperVisionObjectFinder _devicesFinder;
        FlowLayoutPanel _devicesFlowLayoutePanel;

        public DevicesListViewModel(FlowLayoutPanel devicesFlowLayoutePanel)
        {
            _devicesFinder = new SuperVisionObjectFinder();
            _devicesFlowLayoutePanel = devicesFlowLayoutePanel;
        }

        public async Task InitComponents()
        {
            await _devicesFinder.InitComponents();
            _devicesFlowLayoutePanel.Controls.Clear();
            if (_devicesFinder.Devices == null)
                return;

            foreach (var device in _devicesFinder.Devices)
            {
                // Создаем контрол с данными
                DeviceItemControl item = new DeviceItemControl(device);

                // Подписываемся на событие клика
                item.ItemClicked += DeviceItem_ItemClicked;

                // Добавляем в панель
                _devicesFlowLayoutePanel.Controls.Add(item);

                // Сохраняем связь
                //_itemMap[item] = person;
            }


        }

        // Обработчик клика по элементу
        private void DeviceItem_ItemClicked(object sender, IDevice device)
        {
            // Получаем контрол
            DeviceItemControl item = sender as DeviceItemControl;

            int s = 0;
        }

        public List<IDevice> GetUsingDevices()
        {
            if (_devicesFinder.Devices == null)
                return new List<IDevice>();
            return _devicesFinder.Devices.FindAll(x => x.IsUsing);
        }


    }
}
