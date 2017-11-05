using Autofac;
using ControlUnit.Controller.Core.Services;
using GalaSoft.MvvmLight;

namespace ControlUnit.Controller.Core.ViewModels
{
    public class ControllerViewModel : ViewModelBase
    {
        private RemoteCommunicationService<IControllerService> _deviceService;

        public ControllerViewModel(IContainer container)
        {
            _deviceService = container.Resolve<RemoteCommunicationService<IControllerService>>();
            //await  _service.CallRemoteProcedureAsync(srv => srv.Test(25,"aaa",444));
        }

        private BluetoothDevice _selectedDevice;
        public BluetoothDevice SelectedDevice { get => _selectedDevice; set => Set(ref _selectedDevice, value); }

        private byte _leftTrackVelocity;
        public double LeftTrackVelocity
        {
            get => _leftTrackVelocity; set
            {
                if (Set(ref _leftTrackVelocity, (byte)value))
                {
                    Set(nameof(RightTrackVelocity), ref _rightTrackVelocity, (byte)OverallTrackVelocity);
                }
            }
        }

        private byte _rightTrackVelocity;
        public double RightTrackVelocity
        {
            get => _rightTrackVelocity; set
            {
                if (Set(ref _rightTrackVelocity, (byte)value))
                {
                    Set(nameof(LeftTrackVelocity), ref _leftTrackVelocity, (byte)OverallTrackVelocity);
                }
            }
        }

        private byte _overallTrackVelocity;
        public double OverallTrackVelocity
        {
            get => _overallTrackVelocity; set
            {
                if (Set(ref _overallTrackVelocity, (byte)value))
                {
                    RightTrackVelocity = value;
                    LeftTrackVelocity = value;
                }
            }
        }

        private bool _lockedTurn = false;
        public bool LockedTurn
        {
            get => _lockedTurn; set => Set(ref _lockedTurn, value);
        }

        public void ResetVelocity()
        {
            RightTrackVelocity = OverallTrackVelocity;
            LeftTrackVelocity = OverallTrackVelocity;
        }
    }
}