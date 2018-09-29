using Motion_Recorder.Classes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Motion_Recorder
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Decoder : Page
    {
        public Decoder()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;

            SystemNavigationManager.GetForCurrentView().BackRequested += About_BackRequested;
        }

        public List<SensorData> SData { get; set; }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
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

            StorageFile file = e.Parameter as StorageFile;
            if (file != null)
            {
                if (file.FileType.Equals(".mdo"))
                {
                    string jsonString = await FileIO.ReadTextAsync(file);
                    using (IRandomAccessStream fileStream = await file.OpenReadAsync())
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        StringReader sr = new StringReader(jsonString);
                        SData = (List<SensorData>)serializer.Deserialize(new JsonTextReader(sr), typeof(List<SensorData>));
                    }

                    data_count.Text = SData.Count.ToString();
                }
            }
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

        private async void OpenObject(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".mdo");
            picker.SuggestedStartLocation = PickerLocationId.Desktop;
            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                string jsonString = await FileIO.ReadTextAsync(file);
                using (IRandomAccessStream fileStream = await file.OpenReadAsync())
                {
                    JsonSerializer serializer = new JsonSerializer();
                    StringReader sr = new StringReader(jsonString);
                    SData = (List<SensorData>)serializer.Deserialize(new JsonTextReader(sr), typeof(List<SensorData>));
                }

                data_count.Text = SData.Count.ToString();
            }
        }
    }
}
