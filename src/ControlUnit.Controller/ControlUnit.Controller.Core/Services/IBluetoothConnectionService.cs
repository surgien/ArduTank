using ControlUnit.Controller.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ControlUnit.Controller.Core.Services
{
    public interface IBluetoothConnectionService
    {
        event EventHandler<ReceiveDataEventArgs> ReceiveData;

        event EventHandler<DeviceConnectionChangedEventArgs> DeviceConnectionChanged;

        Task TransmitData(byte[] data);

        Task<IEnumerable<BluetoothDevice>> GetAllDevicesAsync();

        Task<ConnectionState> ConnectToDeviceAsync(BluetoothDevice selectedDevice);
    }

    public class DeviceConnectionChangedEventArgs
    {
        public bool IsConnected { get; set; }
    }

    public class ReceiveDataEventArgs: EventArgs
    {
        public byte[] Data { get; set; }
    }

    public enum ConnectionState
    {
        Connected
    }
}
