using ControlUnit.Controller.Core.Services;
using GalaSoft.MvvmLight;

namespace ControlUnit.Controller.Core.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private RemoteCommunicationService<IControllerService> _service;

        public MainViewModel()
        {
            _service = new RemoteCommunicationService<IControllerService>(new RemoteCommunicationFormatProvider());
            _service.CallRemoteProcedure(srv => srv.Test(25,"aaa",444));
        }
    }
}
