using Motion_Recorder.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.System.Profile;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Motion_Recorder
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class About : Page
    {
        public About()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;

            SystemNavigationManager.GetForCurrentView().BackRequested += About_BackRequested;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame.CanGoBack)
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    AppViewBackButtonVisibility.Visible;
            }
            else
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    AppViewBackButtonVisibility.Collapsed;
            }

            DisplaySystemInfo();
        }

        private void About_BackRequested(object sender, BackRequestedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
                return;

            if (rootFrame.CanGoBack && e.Handled == false)
            {
                e.Handled = true;
                rootFrame.GoBack();
            }
        }

        private async void Rate(object sender, RoutedEventArgs e)
            => await ApplicationService.ShowRatingReviewDialog();

        private void DisplaySystemInfo()
        {
            applicationName.Text = ApplicationService.ApplicationName;

            applicationVersion.Text = ApplicationService.ApplicationVersion;

            cultureInfo.Text = ApplicationService.Culture.DisplayName;

            oSVersion.Text = ApplicationService.OperatingSystemVersion.ToString();

            deviceModel.Text = ApplicationService.DeviceModel;

            availableMemory.Text = ApplicationService.AvailableMemory.ToString();

            firstVersionInstalled.Text = ApplicationService.FirstVersionInstalled;

            firstUseTime.Text = ApplicationService.FirstUseTime.ToString();

            launchTime.Text = ApplicationService.LaunchTime.ToString();

            lastLaunchTime.Text = ApplicationService.LastLaunchTime.ToString();

            totalLaunchCount.Text = ApplicationService.TotalLaunchCount.ToString();

            appUptime.Text = ApplicationService.AppUptime.ToString("G");
        }

    }
}
