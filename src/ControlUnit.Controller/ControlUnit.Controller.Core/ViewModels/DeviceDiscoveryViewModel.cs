using Autofac;
using ControlUnit.Controller.Core.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Views;
using System;

namespace ControlUnit.Controller.Core.ViewModels
{
    public class DeviceDiscoveryViewModel : ViewModelBase
    {
        private IBluetoothConnectionService _bluetoothService;
        private INavigationService _navigationService;

        public DeviceDiscoveryViewModel() { }//{ if (!IsInDesignMode) throw new Exception(); }

        public DeviceDiscoveryViewModel(IContainer container)
        {
            _bluetoothService = container.Resolve<IBluetoothConnectionService>();
            _navigationService = container.Resolve<INavigationService>();

            ConnectToDeviceCommand = new RelayCommand(async () =>
             {
                 if (await _bluetoothService.ConnectToDeviceAsync(SelectedDevice) == ConnectionState.Connected)
                 {
                     //Navigate
                     _navigationService.NavigateTo(nameof(ControllerViewModel), SelectedDevice);
                 }
             }, () => SelectedDevice != null);
        }

        public async Task LoadAsync()
        {
            Devices = new ObservableCollection<BluetoothDevice>(await _bluetoothService.GetAllDevicesAsync());
        }

        private ObservableCollection<BluetoothDevice> _devices;
        public ObservableCollection<BluetoothDevice> Devices { get => _devices; set => Set(ref _devices, value); }

        private BluetoothDevice _selectedDevice;
        public BluetoothDevice SelectedDevice
        {
            get => _selectedDevice; set
            {
                Set(ref _selectedDevice, value);
                ConnectToDeviceCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand ConnectToDeviceCommand { get; private set; }
    }
}