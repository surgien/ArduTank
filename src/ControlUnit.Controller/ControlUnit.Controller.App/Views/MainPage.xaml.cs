using Autofac;
using ControlUnit.Controller.Core.ViewModels;
using GalaSoft.MvvmLight.Views;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ControlUnit.Controller.App
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private NavigationService _navigationService;

        public MainPage()
        {
            this.InitializeComponent();
            _navigationService = App.Container.Resolve<INavigationService>() as NavigationService;
            _navigationService.CurrentFrame = contentFrame;

            contentFrame.Navigated += ContentFrame_Navigated;
            HamburgerMenu.ItemClick += HamburgerMenu_ItemClick;
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            string myPages = "";
            foreach (PageStackEntry page in contentFrame.BackStack)
            {
                myPages += page.SourcePageType.ToString() + "\n";
            }
            //stackCount.Text = myPages;

            if (contentFrame.CanGoBack)
            {
                // Show UI in title bar if opted-in and in-app backstack is not empty.
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    AppViewBackButtonVisibility.Visible;
            }
            else
            {
                // Remove the UI from the title bar if in-app back stack is empty.
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    AppViewBackButtonVisibility.Collapsed;
            }

            var contentPage = e.Content as Page;

            commandBar.PrimaryCommands.Clear();
            commandBar.SecondaryCommands.Clear();

            if (contentPage.TopAppBar != null)
            {
                SyncCommandBar(contentPage, (CommandBar)contentPage.TopAppBar);
            }
            else if (contentPage.BottomAppBar != null)
            {
                SyncCommandBar(contentPage, (CommandBar)contentPage.BottomAppBar);
            }

            foreach (HamburgerMenuItem item in (IEnumerable)HamburgerMenu.ItemsSource)
            {
                if (item.Tag as string == contentPage.DataContext.GetType().Name && HamburgerMenu.SelectedItem != item)
                {
                    HamburgerMenu.SelectedItem = item;
                }
            }
        }

        private void HamburgerMenu_ItemClick(object sender, ItemClickEventArgs e)
        {
            var target = ((HamburgerMenuItem)e.ClickedItem).Tag as string;

            if (HamburgerMenu.SelectedItem == null || ((HamburgerMenuItem)HamburgerMenu.SelectedItem).Tag as string != target)
            {
                _navigationService.NavigateTo(target);
            }
        }

        private void SyncCommandBar(Page con, CommandBar cmdBarTarget)
        {
            commandBar.PrimaryCommands.Clear();
            foreach (var cmd in (cmdBarTarget.PrimaryCommands))
            {
                commandBar.PrimaryCommands.Add(cmd);
            }

            commandBar.SecondaryCommands.Clear();
            foreach (var cmd in (cmdBarTarget).SecondaryCommands)
            {
                commandBar.SecondaryCommands.Add(cmd);
            }

            cmdBarTarget.Visibility = Visibility.Collapsed;
            commandBar.DataContext = cmdBarTarget.DataContext;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _navigationService.NavigateTo(nameof(DeviceDiscoveryViewModel));
        }
    }
}
