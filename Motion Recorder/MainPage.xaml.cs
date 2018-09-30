using Motion_Recorder.Classes;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Text;
using Windows.Devices.Sensors;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System.Display;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Motion_Recorder
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;

            SData = new ObservableCollection<SensorData>();
            this.DataContext = this;

            accelerationX.AnimationsSpeed = TimeSpan.FromMilliseconds(100);
            accelerationY.AnimationsSpeed = TimeSpan.FromMilliseconds(100);
            accelerationZ.AnimationsSpeed = TimeSpan.FromMilliseconds(100);
            pitchDegrees.AnimationsSpeed = TimeSpan.FromMilliseconds(100);
            rollDegrees.AnimationsSpeed = TimeSpan.FromMilliseconds(100);
            yawDegrees.AnimationsSpeed = TimeSpan.FromMilliseconds(100);
            angularVelocityX.AnimationsSpeed = TimeSpan.FromMilliseconds(100);
            angularVelocityY.AnimationsSpeed = TimeSpan.FromMilliseconds(100);
            angularVelocityZ.AnimationsSpeed = TimeSpan.FromMilliseconds(100);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Disabled;

            _displayRequest.RequestActive();
            _accelerometer = Accelerometer.GetDefault();
            _inclinometer = Inclinometer.GetDefault();
            _gyrometer = Gyrometer.GetDefault();
            if (_accelerometer != null)
            {
                //_isAccelerometerEnable = true;
                _accelerometer.ReportInterval = 60;
                _accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
            }
            if (_inclinometer != null)
            {
                IsInclinometerEnable = true;
                _inclinometer.ReportInterval = 60;
                _inclinometer.ReadingChanged += Inclinometer_ReadingChanged;
            }
            if (_gyrometer != null)
            {
                IsGyrometerEnable = true;
                _gyrometer.ReportInterval = 60;
                _gyrometer.ReadingChanged += Gyrometer_ReadingChanged;
            }
            if (_accelerometer == null && _inclinometer == null && _gyrometer == null)
            {
                record_control.IsEnabled = false;
                record_control.Content = "All sensors unavailable";
                clear_control.IsEnabled = false;
                store_data.IsEnabled = false;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (PeriodicTimer != null)
                PeriodicTimer.Cancel();
            record_control.Icon = new SymbolIcon(Symbol.Play);
            record_control.Label = "Record";
            clear_control.IsEnabled = true;
            store_data.IsEnabled = true;
            IsRecording = false;

            _accelerometer.ReadingChanged -= Accelerometer_ReadingChanged;
            _inclinometer.ReadingChanged -= Inclinometer_ReadingChanged;
            _gyrometer.ReadingChanged -= Gyrometer_ReadingChanged;
        }

        DisplayRequest _displayRequest = new DisplayRequest();
        Accelerometer _accelerometer;
        Inclinometer _inclinometer;
        Gyrometer _gyrometer;

        AccelerometerReading areading;
        InclinometerReading ireading;
        GyrometerReading greading;

        //bool IsAccelerometerEnable = false;
        bool IsInclinometerEnable = false;
        bool IsGyrometerEnable = false;

        int time = 0;
        bool IsRecording = false;
        ThreadPoolTimer PeriodicTimer;
        public ObservableCollection<SensorData> SData { get; set; }

        private void Decoder_Click(object sender, RoutedEventArgs e)
            => this.Frame.Navigate(typeof(Decoder));

        private void About_Click(object sender, RoutedEventArgs e)
            => this.Frame.Navigate(typeof(About));

        private void Flyout_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
            => FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);

        #region Control

        private async void Record_Click(object sender, RoutedEventArgs e)
        {
            if (!IsRecording)
            {
                try
                {
                    time = Convert.ToInt32(timeSpan.Text);
                }
                catch
                {
                    await new MessageDialog("Could not identify input").ShowAsync();
                    return;
                }
                record_control.Icon = new SymbolIcon(Symbol.Pause);
                record_control.Label = "Suspend";
                clear_control.IsEnabled = false;
                store_data.IsEnabled = false;

                // 既然此按钮能按得动，说明至少有一个传感器能用，一般为加速度传感器
                if (IsInclinometerEnable && IsGyrometerEnable)
                    YYY();
                else if (!IsInclinometerEnable)
                    YNY();
                else if (!IsGyrometerEnable)
                    YYN();
                else
                    YNN();

                IsRecording = true;
            }
            else
            {
                if (PeriodicTimer != null)
                    PeriodicTimer.Cancel();
                record_control.Icon = new SymbolIcon(Symbol.Play);
                record_control.Label = "Record";
                clear_control.IsEnabled = true;
                store_data.IsEnabled = true;

                IsRecording = false;
            }
        }

        private void Clear_data(object sender, RoutedEventArgs e)
        {
            SData.Clear();
            data_count.Text = "0";
        }

        private async void Export_table(object sender, RoutedEventArgs e)
        {
            FileSavePicker picker = new FileSavePicker();
            picker.FileTypeChoices.Add("Motion Table", new String[] { ".txt" });
            picker.SuggestedStartLocation = PickerLocationId.Desktop;
            picker.SuggestedFileName = "Table-" + DateTime.Now.ToString("yyyyMMddHHmmss");
            StorageFile file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                StringBuilder data_record = new StringBuilder();
                data_record.AppendLine("date\tPeriod\tx-acceleration\ty-acceleration\tz-acceleration\tx-inclination\ty-inclination\tz-inclination\tx-angular-velocity\ty-angular-velocity\tz-angular-velocity");
                foreach (SensorData data in SData)
                {
                    data_record.AppendLine(data.Time + "\t" + data.Span + "\t"
                        + data.AccelerationX + "\t" + data.AccelerationY + "\t" + data.AccelerationZ + "\t"
                        + data.PitchDegrees + "\t" + data.RollDegrees + "\t" + data.YawDegrees + "\t"
                        + data.AngularVelocityX + "\t" + data.AngularVelocityY + "\t" + data.AngularVelocityZ);
                }

                await FileIO.WriteTextAsync(file, data_record.ToString(), Windows.Storage.Streams.UnicodeEncoding.Utf8);

                data_record.Clear();
            }
        }

        private async void Export_object(object sender, RoutedEventArgs e)
        {
            FileSavePicker picker = new FileSavePicker();
            picker.FileTypeChoices.Add("Motion Object", new String[] { ".mdo" });
            picker.SuggestedStartLocation = PickerLocationId.Desktop;
            picker.SuggestedFileName = "Object-" + DateTime.Now.ToString("yyyyMMddHHmmss");
            StorageFile file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                SensorData[] datas = new SensorData[SData.Count];
                SData.CopyTo(datas, 0);

                string jsonString = JsonConvert.SerializeObject(datas);
                await FileIO.WriteTextAsync(file, jsonString);
            }
        }

        #endregion

        #region Sensor

        private async void Accelerometer_ReadingChanged(Accelerometer sender, AccelerometerReadingChangedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                areading = args.Reading;
                accelerationX.Value = Math.Round(areading.AccelerationX, 3);
                accelerationY.Value = Math.Round(areading.AccelerationY, 3);
                accelerationZ.Value = Math.Round(areading.AccelerationZ, 3);
            });
        }

        private async void Inclinometer_ReadingChanged(Inclinometer sender, InclinometerReadingChangedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ireading = args.Reading;
                pitchDegrees.Value = Math.Round(ireading.PitchDegrees, 3);
                rollDegrees.Value = Math.Round(ireading.RollDegrees, 3);
                yawDegrees.Value = Math.Round(ireading.YawDegrees, 3);
            });
        }

        private async void Gyrometer_ReadingChanged(Gyrometer sender, GyrometerReadingChangedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                greading = args.Reading;
                angularVelocityX.Value = Math.Round(greading.AngularVelocityX, 3);
                angularVelocityY.Value = Math.Round(greading.AngularVelocityY, 3);
                angularVelocityZ.Value = Math.Round(greading.AngularVelocityZ, 3);
            });
        }

        private void YYY()
        {
            TimeSpan period = TimeSpan.FromMilliseconds(time);
            PeriodicTimer = ThreadPoolTimer.CreatePeriodicTimer(async (source) =>
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    SData.Add(new SensorData
                    {
                        Time = areading.Timestamp,
                        Span = time,
                        AccelerationX = areading.AccelerationX.ToString("F2"),
                        AccelerationY = areading.AccelerationY.ToString("F2"),
                        AccelerationZ = areading.AccelerationZ.ToString("F2"),
                        PitchDegrees = ireading.PitchDegrees.ToString("F2"),
                        RollDegrees = ireading.RollDegrees.ToString("F2"),
                        YawDegrees = ireading.YawDegrees.ToString("F2"),
                        AngularVelocityX = greading.AngularVelocityX.ToString("F2"),
                        AngularVelocityY = greading.AngularVelocityY.ToString("F2"),
                        AngularVelocityZ = greading.AngularVelocityZ.ToString("F2")
                    });

                    data_count.Text = SData.Count.ToString();
                });
            }, period);
        }

        private void YYN()
        {
            TimeSpan period = TimeSpan.FromMilliseconds(time);
            PeriodicTimer = ThreadPoolTimer.CreatePeriodicTimer(async (source) =>
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    SData.Add(new SensorData
                    {
                        Time = areading.Timestamp,
                        Span = time,
                        AccelerationX = areading.AccelerationX.ToString("F2"),
                        AccelerationY = areading.AccelerationY.ToString("F2"),
                        AccelerationZ = areading.AccelerationZ.ToString("F2"),
                        PitchDegrees = ireading.PitchDegrees.ToString("F2"),
                        RollDegrees = ireading.RollDegrees.ToString("F2"),
                        YawDegrees = ireading.YawDegrees.ToString("F2")
                    });

                    data_count.Text = SData.Count.ToString();
                });
            }, period);
        }

        private void YNY()
        {
            TimeSpan period = TimeSpan.FromMilliseconds(time);
            PeriodicTimer = ThreadPoolTimer.CreatePeriodicTimer(async (source) =>
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    SData.Add(new SensorData
                    {
                        Time = areading.Timestamp,
                        Span = time,
                        AccelerationX = areading.AccelerationX.ToString("F2"),
                        AccelerationY = areading.AccelerationY.ToString("F2"),
                        AccelerationZ = areading.AccelerationZ.ToString("F2"),
                        AngularVelocityX = greading.AngularVelocityX.ToString("F2"),
                        AngularVelocityY = greading.AngularVelocityY.ToString("F2"),
                        AngularVelocityZ = greading.AngularVelocityZ.ToString("F2")
                    });

                    data_count.Text = SData.Count.ToString();
                });
            }, period);
        }

        private void YNN()
        {
            TimeSpan period = TimeSpan.FromMilliseconds(time);
            PeriodicTimer = ThreadPoolTimer.CreatePeriodicTimer(async (source) =>
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    SData.Add(new SensorData
                    {
                        Time = areading.Timestamp,
                        Span = time,
                        AccelerationX = areading.AccelerationX.ToString("F2"),
                        AccelerationY = areading.AccelerationY.ToString("F2"),
                        AccelerationZ = areading.AccelerationZ.ToString("F2")
                    });

                    data_count.Text = SData.Count.ToString();
                });
            }, period);
        }

        #endregion

    }
}
