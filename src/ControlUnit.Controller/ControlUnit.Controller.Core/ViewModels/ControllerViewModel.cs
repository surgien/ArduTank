using System;
using System.Runtime.CompilerServices;
using Autofac;
using ControlUnit.Controller.Core.Services;
using GalaSoft.MvvmLight;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using System.Diagnostics;
using System.Threading;

namespace ControlUnit.Controller.Core.ViewModels
{
    public class ControllerViewModel : ViewModelBase
    {
        private const double SPEED_TOLERANCE = 10;
        private const int PROPERTY_DELAY_VALUE = 30;
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
                    if (Set(ref _leftTrackVelocity, value) && OverallTrackVelocity != 0)
                    {
                        DriveLeft(value);
                        if (Set(nameof(RightTrackVelocity), ref _rightTrackVelocity, OverallTrackVelocity))
                        {
                            DriveRight(RightTrackVelocity);
                        }
                    }
                    Func<Task> delay = async () =>
                    {
                        await Task.Delay(PROPERTY_DELAY_VALUE);
                        _isLeftTrackVelocityLocked = false;
                        Set(nameof(LeftTrackVelocity), ref _leftTrackVelocity, _currentLeftTrackVelocity);
                        if (_currentLeftTrackVelocity != _leftTrackVelocity) DriveLeft(_currentLeftTrackVelocity);
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
                    if (Set(ref _rightTrackVelocity, value) && OverallTrackVelocity != 0)
                    {
                        DriveRight(value);
                        if (Set(nameof(LeftTrackVelocity), ref _leftTrackVelocity, OverallTrackVelocity))
                        {
                            DriveLeft(LeftTrackVelocity);
                        }
                    }
                    Func<Task> delay = async () =>
                    {
                        await Task.Delay(PROPERTY_DELAY_VALUE);
                        _isRightTrackVelocityLocked = false;
                        Set(nameof(RightTrackVelocity), ref _rightTrackVelocity, _currentRightTrackVelocity);
                        if (_currentRightTrackVelocity != _rightTrackVelocity) DriveRight(_currentRightTrackVelocity);
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
                        DriveForward(value);
                        Set(nameof(RightTrackVelocity), ref _rightTrackVelocity, value);
                        Set(nameof(LeftTrackVelocity), ref _leftTrackVelocity, value);
                    }
                    Func<Task> delay = async () =>
                    {
                        await Task.Delay(PROPERTY_DELAY_VALUE);
                        _isOverallTrackVelocityLocked = false;
                        OverallTrackVelocity = _currentOverallTrackVelocity;
                        if (_currentOverallTrackVelocity != _overallTrackVelocity) DriveForward(_currentOverallTrackVelocity);
                    };
                    delay();
                }
            }
        }

        private async void DriveLeft(double value)
        {
            value = AddTolerance(value);
            await _deviceService.CallRemoteProcedureAsync(srv => srv.TurnLeft(value));
            Debug.WriteLine("Links auf: " + value);
        }

        private async void DriveForward(double value)
        {
            value = AddTolerance(value);
            await _deviceService.CallRemoteProcedureAsync(srv => srv.Accelerate(value));
            Debug.WriteLine("Beschleunige auf: " + value);
        }

        private async void DriveRight(double value)
        {
            value = AddTolerance(value);
            await _deviceService.CallRemoteProcedureAsync(srv => srv.TurnRight(value));
            Debug.WriteLine("Rechts auf: " + value);
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

        private double AddTolerance(double velocity)
        {
            var isBackward = velocity < 0;
            if (isBackward)
            {
                velocity *= -1;
            }
            if (velocity == 0) return 0;
            var result = Math.Round((((100 - SPEED_TOLERANCE) / 100) * velocity) + SPEED_TOLERANCE, 2);

            if (isBackward)
            {
                return result *= -1;
            }
            else
            {
                return result;
            }
        }
    }
}