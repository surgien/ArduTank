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
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;

namespace ControlUnit.Controller.App.Services
{
    class BluetoothConnectionService : IBluetoothConnectionService
    {
        // The Characteristic we want to obtain measurements for the UART characteristic
        private Guid SERVICE_UUID = new Guid("{6E400001-B5A3-F393-E0A9-E50E24DCCA9E}");
        private Guid TX_CHARACTERISTIC_UUID = new Guid("{6E400002-B5A3-F393-E0A9-E50E24DCCA9E}");
        private Guid RX_CHARACTERISTIC_UUID = new Guid("{6E400003-B5A3-F393-E0A9-E50E24DCCA9E}");
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
                return ConnectionState.Disconnected;
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

            foreach (var prop in args.Properties)
            {
                var xx = prop;
            }

            var _deviceContainerId = "BLABLALABLAB";

            if (!args.Id.ToString().StartsWith("{56")) //{56bd5956-4fd5-5829-af2f-c810613e82a4}
            {
                var xxxx = 23;
            }

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

                //Notifying subscribers of connection state updates
                if (DeviceConnectionChanged != null)
                {
                    DeviceConnectionChanged(this, new DeviceConnectionChangedEventArgs() { IsConnected = status == GattCommunicationStatus.Success });
                }
            }
        }

        /// <summary>
        /// Invoked when App receives data from device.
        /// </summary>
        /// <param name="sender">The characteristic object whose value is received.</param>
        /// <param name="args">The new characteristic value sent by the device.</param>
        private void RxCharacteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            var data = new byte[args.CharacteristicValue.Length];

            DataReader.FromBuffer(args.CharacteristicValue).ReadBytes(data);

            // Process the raw data received from the device.
            var astring = UART_Data_Converter.RX(data);

            var text = astring;

            //EVENT DAS ETWAS EMPFANGEN WURDE
        }

        public async Task TransmitData(byte[] data)
        {
            // try-catch block protects us from the race where the device disconnects
            // just after we've determined that it is connected.
            try
            {
                if (_txCharacteristic != null)
                {
                    var step = 20;
                    var rest = data.Count() % step;
                    var iterations = (data.Count() - rest) / step;

                    for (int i = 0; i < iterations; i++)
                    {
                        var segment = new ArraySegment<byte>(data, i * step, step);
                        await SendData(data, segment.ToArray());
                    }

                    if (rest > 0)
                    {
                        var segment = new ArraySegment<byte>(data, iterations * step, rest);
                        await SendData(data, segment.ToArray());
                    }
                }
            }
            catch (Exception) { }
        }

        private async Task SendData(byte[] data, byte[] segment)
        {
            var writer = new DataWriter();

            writer.WriteBytes(segment);

            var buffer = data.AsBuffer();
            var result = await _txCharacteristic.WriteValueAsync(writer.DetachBuffer()); //, GattWriteOption.WriteWithoutResponse
        }

        public async Task<IEnumerable<BluetoothDevice>> GetAllDevicesAsync()
        {
            //TODO FAKE:
            //return new List<BluetoothDevice>() { new BluetoothDevice("Test", "IDAAA", "CID") };
            var devInfos = GattDeviceService.GetDeviceSelectorFromUuid(SERVICE_UUID);
            var devices = await DeviceInformation.FindAllAsync(devInfos, new string[] { "System.Devices.ContainerId", "System.Devices.AepService.AepId", "System.Devices.AepService.ServiceClassId" }, DeviceInformationKind.AssociationEndpointContainer);
            var parsedDevices = new List<BluetoothDevice>();

            foreach (var d in devices)
            {
                var srv = await GattDeviceService.FromIdAsync(d.Id);
                var dev = await DeviceInformation.CreateFromIdAsync(srv.Session.DeviceId.Id);

                parsedDevices.Add(new BluetoothDevice(dev.Name, d.Id));
            }


            return parsedDevices;
        }
    }
}
