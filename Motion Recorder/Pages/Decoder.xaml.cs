using Motion_Recorder.Classes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Motion_Recorder.Pages
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
        }

        public List<SensorData> SData { get; set; }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
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

        private async void open_object(object sender, RoutedEventArgs e)
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
