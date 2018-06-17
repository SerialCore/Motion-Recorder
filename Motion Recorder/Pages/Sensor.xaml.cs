using Motion_Recorder.Classes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System.Display;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Popups;
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
    public sealed partial class Sensor : Page
    {
        public Sensor()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;

            SData = new ObservableCollection<SensorData>();
            SView = new ObservableCollection<SensorData>();
            this.DataContext = this;

            _displayRequest.RequestActive();
            _accelerometer = Accelerometer.GetDefault();
            _inclinometer = Inclinometer.GetDefault();
            _gyrometer = Gyrometer.GetDefault();
            if (_accelerometer != null)
            {
                //_isAccelerometerEnable = true;
                _accelerometer.ReportInterval = 60;
                _accelerometer.ReadingChanged += _accelerometer_ReadingChanged;
            }
            if (_inclinometer != null)
            {
                _isInclinometerEnable = true;
                _inclinometer.ReportInterval = 60;
                _inclinometer.ReadingChanged += _inclinometer_ReadingChanged;
            }
            if (_gyrometer != null)
            {
                _isGyrometerEnable = true;
                _gyrometer.ReportInterval = 60;
                _gyrometer.ReadingChanged += _gyrometer_ReadingChanged;
            }
            if (_accelerometer == null && _inclinometer == null && _gyrometer == null)
            {
                record_control.IsEnabled = false;
                record_control.Content = "所有所需传感器都不适用";
                clear_control.IsEnabled = false;
                store_table.IsEnabled = false;
                store_object.IsEnabled = false;
            }
        }

        DisplayRequest _displayRequest = new DisplayRequest();
        Accelerometer _accelerometer;
        Inclinometer _inclinometer;
        Gyrometer _gyrometer;

        AccelerometerReading areading;
        InclinometerReading ireading;
        GyrometerReading greading;

        //bool _isAccelerometerEnable = false;
        bool _isInclinometerEnable = false;
        bool _isGyrometerEnable = false;

        int time = 0;
        ThreadPoolTimer PeriodicTimer;
        public ObservableCollection<SensorData> SData { get; set; }
        public ObservableCollection<SensorData> SView { get; set; }

        private async void _accelerometer_ReadingChanged(Accelerometer sender, AccelerometerReadingChangedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                areading = args.Reading;
                acceleration_x.Text = areading.AccelerationX.ToString("F2");
                acceleration_y.Text = areading.AccelerationY.ToString("F2");
                acceleration_z.Text = areading.AccelerationZ.ToString("F2");
            });
        }

        private async void _inclinometer_ReadingChanged(Inclinometer sender, InclinometerReadingChangedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ireading = args.Reading;
                inclination_x.Text = ireading.PitchDegrees.ToString("F2");
                inclination_y.Text = ireading.RollDegrees.ToString("F2");
                inclination_z.Text = ireading.YawDegrees.ToString("F2");
            });
        }

        private async void _gyrometer_ReadingChanged(Gyrometer sender, GyrometerReadingChangedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                greading = args.Reading;
                angular_velocity_x.Text = greading.AngularVelocityX.ToString("F2");
                angular_velocity_y.Text = greading.AngularVelocityY.ToString("F2");
                angular_velocity_z.Text = greading.AngularVelocityZ.ToString("F2");
            });
        }

        private async void record_control_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                time = Convert.ToInt32(timeSpan.Text);
            }
            catch
            {
                await new MessageDialog("无法获取采集周期").ShowAsync();
                return;
            }
            record_control.Content = "停止暂停记录";
            clear_control.IsEnabled = false;
            store_table.IsEnabled = false;
            store_object.IsEnabled = false;

            // 既然此按钮能按得动，说明至少有一个传感器能用，一般为加速度传感器
            if (_isInclinometerEnable && _isGyrometerEnable)
                YYY();
            else if (!_isInclinometerEnable)
                YNY();
            else if (!_isGyrometerEnable)
                YYN();
            else
                YNN();
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

                    SView.Add(new SensorData
                    {
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

                    // 控制数据表的长度，以便在UI上友好显示
                    if (SView.Count >= 8)
                    {
                        SView.RemoveAt(0);
                    }

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

                    SView.Add(new SensorData
                    {
                        AccelerationX = areading.AccelerationX.ToString("F2"),
                        AccelerationY = areading.AccelerationY.ToString("F2"),
                        AccelerationZ = areading.AccelerationZ.ToString("F2"),
                        PitchDegrees = ireading.PitchDegrees.ToString("F2"),
                        RollDegrees = ireading.RollDegrees.ToString("F2"),
                        YawDegrees = ireading.YawDegrees.ToString("F2"),
                    });

                    // 控制数据表的长度，以便在UI上友好显示
                    if (SView.Count >= 8)
                    {
                        SView.RemoveAt(0);
                    }

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

                    SView.Add(new SensorData
                    {
                        AccelerationX = areading.AccelerationX.ToString("F2"),
                        AccelerationY = areading.AccelerationY.ToString("F2"),
                        AccelerationZ = areading.AccelerationZ.ToString("F2"),
                        AngularVelocityX = greading.AngularVelocityX.ToString("F2"),
                        AngularVelocityY = greading.AngularVelocityY.ToString("F2"),
                        AngularVelocityZ = greading.AngularVelocityZ.ToString("F2")
                    });

                    // 控制数据表的长度，以便在UI上友好显示
                    if (SView.Count >= 8)
                    {
                        SView.RemoveAt(0);
                    }

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

                    SView.Add(new SensorData
                    {
                        AccelerationX = areading.AccelerationX.ToString("F2"),
                        AccelerationY = areading.AccelerationY.ToString("F2"),
                        AccelerationZ = areading.AccelerationZ.ToString("F2")
                    });

                    // 控制数据表的长度，以便在UI上友好显示
                    if (SView.Count >= 8)
                    {
                        SView.RemoveAt(0);
                    }

                    data_count.Text = SData.Count.ToString();
                });
            }, period);
        }

        private void record_control_Unchecked(object sender, RoutedEventArgs e)
        {
            if (PeriodicTimer != null)
                PeriodicTimer.Cancel();
            record_control.Content = "开始继续记录";
            clear_control.IsEnabled = true;
            store_table.IsEnabled = true;
            store_object.IsEnabled = true;
        }

        private void clear_data(object sender, RoutedEventArgs e)
        {
            SData.Clear();
            SView.Clear();
            data_count.Text = "0";
        }

        private async void export_table(object sender, RoutedEventArgs e)
        {
            FileSavePicker picker = new FileSavePicker();
            // 指定文件类型
            picker.FileTypeChoices.Add("力学量表格", new String[] { ".txt" });
            // 默认定位到桌面
            picker.SuggestedStartLocation = PickerLocationId.Desktop;
            // 默认文件名
            picker.SuggestedFileName = "力学量表格" + DateTime.Now.ToString("yyyyMMddHHmmss");
            // 显示选择器
            StorageFile file = await picker.PickSaveFileAsync();
            // 保存文本
            if (file != null)
            {
                StringBuilder data_record = new StringBuilder();
                data_record.AppendLine("时间\t周期\tx加速度\ty加速度\tz加速度\tx磁倾角\ty磁倾角\tz磁倾角\tx角加速度\ty角加速度\tz角加速度");
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

        private async void export_object(object sender, RoutedEventArgs e)
        {
            FileSavePicker picker = new FileSavePicker();
            // 指定文件类型
            picker.FileTypeChoices.Add("力学量对象", new String[] { ".mdo" });
            // 默认定位到桌面
            picker.SuggestedStartLocation = PickerLocationId.Desktop;
            // 默认文件名
            picker.SuggestedFileName = "力学量对象" + DateTime.Now.ToString("yyyyMMddHHmmss");
            // 显示选择器
            StorageFile file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                SensorData[] datas = new SensorData[SData.Count];
                SData.CopyTo(datas, 0);

                string jsonString = JsonConvert.SerializeObject(datas);
                await FileIO.WriteTextAsync(file, jsonString);
            }
        }
    }
}
