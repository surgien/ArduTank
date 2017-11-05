using ControlUnit.Controller.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlUnit.Controller.Core.ViewModels;
using System.Collections.ObjectModel;
using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration.Pnp;

namespace ControlUnit.Controller.App.Services
{
    class BluetoothConnectionService : IBluetoothConnectionService
    {
        // The Characteristic we want to obtain measurements for the UART characteristic
        private Guid SERVICE_UUID = new Guid("{6E400001-B5A3-F393-E0A9-E50E24DCCA9E}");
        private Guid TX_CHARACTERISTIC_UUID = new Guid("{6E400002-B5A3-F393-E0A9-E50E24DCCA9E}");
        private Guid RX_CHARACTERISTIC_UUID = new Guid("{6E400003-B5A3-F393-E0A9-E50E24DCCA9E}");
        private string _deviceContainerId;
        private GattDeviceService _service;
        private GattCharacteristic _txCharacteristic;
        private GattCharacteristic _rxCharacteristic;

        // The UART specification requires that the UART TX characteristic is write without response.
        // The UART specification requires that the UART RX characteristic is notifiable.
        private const GattClientCharacteristicConfigurationDescriptorValue RX_CHARACTERISTIC_NOTIFICATION_TYPE =
            GattClientCharacteristicConfigurationDescriptorValue.Notify;
        private PnpObjectWatcher _watcher;

        public event EventHandler<ReceiveDataEventArgs> ReceiveData;
        public event EventHandler<DeviceConnectionChangedEventArgs> DeviceConnectionChanged;

        public async Task<ConnectionState> ConnectToDeviceAsync(BluetoothDevice selectedDevice)
        {
            _deviceContainerId = selectedDevice.ContainerId;
            _service = await GattDeviceService.FromIdAsync(selectedDevice.Id);
            if (_service != null)
            {
                //IsServiceInitialized = true;
                _txCharacteristic = _service.GetCharacteristics(TX_CHARACTERISTIC_UUID).First();
                _rxCharacteristic = _service.GetCharacteristics(RX_CHARACTERISTIC_UUID).First();

                await _rxCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);

                // Register the event handler for receiving notifications
                _rxCharacteristic.ValueChanged += RxCharacteristic_ValueChanged;

                // In order to avoid unnecessary communication with the device, determine if the device is already
                // correctly configured to send notifications.
                // By default ReadClientCharacteristicConfigurationDescriptorAsync will attempt to get the current
                // value from the system cache and communication with the device is not typically required.
                var currentDescriptorValue = await _rxCharacteristic.ReadClientCharacteristicConfigurationDescriptorAsync();

                bool a = (currentDescriptorValue.Status != GattCommunicationStatus.Success);
                bool b = (currentDescriptorValue.ClientCharacteristicConfigurationDescriptor != RX_CHARACTERISTIC_NOTIFICATION_TYPE);

                if (a || b)
                {
                    // Set the Client Characteristic Configuration Descriptor to enable the device to send notifications
                    // when the Characteristic value changes
                    GattCommunicationStatus status =
                        await _rxCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                        RX_CHARACTERISTIC_NOTIFICATION_TYPE);

                    if (status == GattCommunicationStatus.Unreachable)
                    {
                        // Register a PnpObjectWatcher to detect when a connection to the device is established,
                        // such that the application can retry device configuration.
                        StartDeviceConnectionWatcher();
                    }
                }

                return ConnectionState.Connected;
            }
            else
            {
                //throw new Exception("Something Wrong in 'InitializeServiceAsync' ");
                return ConnectionState.Connected;//WRONG!
            }
        }

        /// <summary>
        /// Register to be notified when a connection is established to the Bluetooth device
        /// </summary>
        private void StartDeviceConnectionWatcher()
        {
            _watcher = PnpObject.CreateWatcher(PnpObjectType.DeviceContainer,
                new string[] { "System.Devices.Connected" }, String.Empty);

            _watcher.Updated += DeviceConnection_Updated;
            _watcher.Start();
        }

        private async void DeviceConnection_Updated(PnpObjectWatcher sender, PnpObjectUpdate args)
        {
            var connectedProperty = args.Properties["System.Devices.Connected"];
            bool isConnected = false;
            if ((_deviceContainerId == args.Id) && Boolean.TryParse(connectedProperty.ToString(), out isConnected) &&
                isConnected)
            {
                var status = await _rxCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                    RX_CHARACTERISTIC_NOTIFICATION_TYPE);

                if (status == GattCommunicationStatus.Success)
                {
                    //IsServiceInitialized = true;

                    // Once the Client Characteristic Configuration Descriptor is set, the watcher is no longer required
                    _watcher.Stop();
                    _watcher = null;
                }

                // Notifying subscribers of connection state updates
                //if (DeviceConnectionUpdated != null)
                //{
                //    DeviceConnectionUpdated(isConnected);
                //}
            }
        }

        private void RxCharacteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<BluetoothDevice>> GetAllDevicesAsync()
        {
            //TODO FAKE:
            return new List<BluetoothDevice>() { new BluetoothDevice("Test", "IDAAA", "CID") };

            var devices = await DeviceInformation.FindAllAsync(
                GattDeviceService.GetDeviceSelectorFromUuid(SERVICE_UUID),
                new string[] { "System.Devices.ContainerId" });

            return devices.Select(dev => new BluetoothDevice("name", dev.Id, (string)dev.Properties["System.Devices.ContainerId"])).ToList();
        }

        public Task TransmitData(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
