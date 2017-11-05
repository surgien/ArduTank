using ControlUnit.Controller.Core.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ControlUnit.Controller.App.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DeviceDiscoveryPage : Page
    {
        private DeviceDiscoveryViewModel vm;

        public DeviceDiscoveryPage()
        {
            this.InitializeComponent();
            vm = new DeviceDiscoveryViewModel(App.Container);
            DataContext = vm;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await vm.LoadAsync();
        }
    }
}
