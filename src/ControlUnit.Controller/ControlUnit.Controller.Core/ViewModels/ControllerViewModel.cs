using System;
using System.Runtime.CompilerServices;
using Autofac;
using ControlUnit.Controller.Core.Services;
using GalaSoft.MvvmLight;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;

namespace ControlUnit.Controller.Core.ViewModels
{
    public class ControllerViewModel : ViewModelBase
    {
        private const double SPEED_TOLERANCE = 20;
        private const int PROPERTY_DELAY_VALUE = 20;
        private RemoteCommunicationService<IControllerService> _deviceService;
        public RelayCommand AccelerateCommand { get; private set; }
        public RelayCommand BreakCommand { get; private set; }

        public ControllerViewModel(IContainer container)
        {
            AccelerateCommand = new RelayCommand(() => { OverallTrackVelocity += 1; });
            BreakCommand = new RelayCommand(() => { OverallTrackVelocity -= 1; });
            
            _deviceService = container.Resolve<RemoteCommunicationService<IControllerService>>();
            //await  _service.CallRemoteProcedureAsync(srv => srv.Test(25,"aaa",444));
        }

        private BluetoothDevice _selectedDevice;
        public BluetoothDevice SelectedDevice { get => _selectedDevice; set => Set(ref _selectedDevice, value); }

        private double _leftTrackVelocity;
        private bool _isLeftTrackVelocityLocked = false;
        private double _currentLeftTrackVelocity;
        public double LeftTrackVelocity
        {
            get => _leftTrackVelocity; set
            {
                _currentLeftTrackVelocity = value;
                if (!_isLeftTrackVelocityLocked)
                {
                    _isLeftTrackVelocityLocked = true;
                    if (Set(ref _leftTrackVelocity, value) && OverallTrackVelocity != 0 && !LockedTurn)
                    {
                        Set(nameof(RightTrackVelocity), ref _rightTrackVelocity, OverallTrackVelocity);
                    }
                    Func<Task> delay = async () =>
                    {
                        await Task.Delay(PROPERTY_DELAY_VALUE);
                        _isLeftTrackVelocityLocked = false;
                        Set(nameof(LeftTrackVelocity), ref _leftTrackVelocity, _currentLeftTrackVelocity);
                    };
                    delay();
                }
            }
        }

        private double _rightTrackVelocity;
        private bool _isRightTrackVelocityLocked = false;
        private double _currentRightTrackVelocity;
        public double RightTrackVelocity
        {
            get => _rightTrackVelocity; set
            {
                _currentRightTrackVelocity = value;
                if (!_isRightTrackVelocityLocked)
                {
                    _isRightTrackVelocityLocked = true;
                    if (Set(ref _rightTrackVelocity, value) && OverallTrackVelocity != 0 && !LockedTurn)
                    {
                        Set(nameof(LeftTrackVelocity), ref _leftTrackVelocity, OverallTrackVelocity);
                    }
                    Func<Task> delay = async () =>
                    {
                        await Task.Delay(PROPERTY_DELAY_VALUE);
                        _isRightTrackVelocityLocked = false;
                        Set(nameof(RightTrackVelocity), ref _rightTrackVelocity, _currentRightTrackVelocity);
                    };
                    delay();
                }
            }
        }

        private double _overallTrackVelocity;
        private bool _isOverallTrackVelocityLocked = false;
        private double _currentOverallTrackVelocity;
        public double OverallTrackVelocity
        {
            get => _overallTrackVelocity; set
            {
                _currentOverallTrackVelocity = value;
                if (!_isOverallTrackVelocityLocked)
                {
                    _isOverallTrackVelocityLocked = true;
                    if (Set(ref _overallTrackVelocity, value))
                    {
                        RightTrackVelocity = value;
                        LeftTrackVelocity = value;
                    }
                    Func<Task> delay = async () =>
                    {
                        await Task.Delay(PROPERTY_DELAY_VALUE);
                        _isOverallTrackVelocityLocked = false;
                        OverallTrackVelocity = _currentOverallTrackVelocity;
                    };
                    delay();
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

        public override async void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.RaisePropertyChanged(propertyName);

            switch (propertyName)
            {
                case nameof(LeftTrackVelocity):
                    var valL = AddTolerance(LeftTrackVelocity);
                    await _deviceService.CallRemoteProcedureAsync(srv => srv.AccelerateLeftTrack(valL));
                    break;
                case nameof(RightTrackVelocity):
                    var valR = AddTolerance(RightTrackVelocity);
                    await _deviceService.CallRemoteProcedureAsync(srv => srv.AccelerateRightTrack(valR));
                    break;
                default:
                    break;
            }
        }

        private double AddTolerance(double velocity)
        {
            if (velocity == 0) return 0;
            var result = Math.Round((((100 - SPEED_TOLERANCE) / 100) * velocity) + SPEED_TOLERANCE, 2);

            return result;
        }
    }
}