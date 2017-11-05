using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControlUnit.Controller.Core.ViewModels
{
    public class BluetoothDevice : ObservableObject
    {
        public BluetoothDevice(string name, string id, string containerId)
        {
            Name = name;
            Id = id;
            ContainerId = containerId;
        }

        private string _name;
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get => _name; set => Set(ref _name, value); }

        private string _containerId;
        /// <summary>
        /// The identifier of the Windows.Devices.Enumeration.Pnp.PnpObject.
        /// </summary>
        public string ContainerId { get => _containerId; set => Set(ref _containerId, value); }

        private string _id;
        /// <summary>
        /// A string representing the identity of the device.
        /// </summary>
        public string Id { get => _id; set => Set(ref _id, value); }
    }
}
