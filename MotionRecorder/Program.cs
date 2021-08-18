using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Windows.Storage;
using Windows.System.Threading;

// This example code shows how you could implement the required main function for a 
// Console UWP Application. You can replace all the code inside Main with your own custom code.

// You should also change the Alias value in the AppExecutionAlias Extension in the 
// Package.appxmanifest to a value that you define. To edit this file manually, right-click
// it in Solution Explorer and select View Code, or open it with the XML Editor.

namespace MotionRecorder
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                ControlCenter();
            }
            else
            {
                for (int i = 0; i < args.Length; i++)
                {
                    Console.WriteLine($"arg[{i}] = {args[i]}");
                }
            }
        }

        static void ControlCenter()
        {
            Console.WriteLine("Sensor Data Serial");

            Sensor sensor = new Sensor();
            if (!sensor.IsAccelerometerEnable && !sensor.IsInclinometerEnable && !sensor.IsGyrometerEnable)
            {
                Console.WriteLine("No sensor available, ready to exit.");
                Console.Read();
                return;
            }

            int timespan = 0;
            Console.WriteLine("Please input time span in millisecond for data recorder: ");
            timespan = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine($"Time span is set to be {timespan} milliseconds.");

            int wholetime = 0;
            Console.WriteLine("Please input whole time in second the recorder will spend: ");
            wholetime = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine($"The recorder will work for {wholetime} seconds.");

            Recording(sensor, timespan, wholetime);
            Console.WriteLine("Recorder ends.");

            ExportTable(sensor.Record);
            Console.WriteLine("The data has been archieved in documents library.");
            Console.WriteLine("Thank you for using this app.");
            Console.Read();
        }

        static void Recording(Sensor sensor, int timespan, int wholetime)
        {
            DateTime start = DateTime.Now;

            ThreadPoolTimer threadPool = ThreadPoolTimer.CreatePeriodicTimer((source) =>
            {
                sensor.RecordData();
            }, TimeSpan.FromMilliseconds(timespan));

            Console.WriteLine("\tTime\t\t\t\taX\taY\taZ\tthetaX\tthetaY\tthetaZ\tomegaX\tomegaY\tomegaZ");
            while (true)
            {
                Console.WriteLine($"\t{sensor.Time}" +
                    $"\t{sensor.AccelerationX:F2}\t{sensor.AccelerationY:F2}\t{sensor.AccelerationZ:F2}" +
                    $"\t{sensor.PitchDegrees:F2}\t{sensor.RollDegrees:F2}\t{sensor.YawDegrees:F2}" +
                    $"\t{sensor.AngularVelocityX:F2}\t{sensor.AngularVelocityY:F2}\t{sensor.AngularVelocityZ:F2}");
                Thread.Sleep(1000);

                if ((DateTime.Now - start).Seconds >= wholetime)
                {
                    threadPool.Cancel();
                    break;
                }
            }
        }

        static async void ExportTable(List<SensorData> record)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile file = await folder.CreateFileAsync("MotionRecord-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt");
            if (file != null)
            {
                StringBuilder dataString = new StringBuilder();
                dataString.AppendLine("Time\tAccelerationX\tAccelerationY\tAccelerationZ\tPitchDegrees\tRollDegrees\tYawDegrees\tAngularVelocityX\tAngularVelocityY\tAngularVelocityZ");
                foreach (SensorData data in record)
                {
                    dataString.AppendLine(data.Time + "\t"
                        + data.AccelerationX + "\t" + data.AccelerationY + "\t" + data.AccelerationZ + "\t"
                        + data.PitchDegrees + "\t" + data.RollDegrees + "\t" + data.YawDegrees + "\t"
                        + data.AngularVelocityX + "\t" + data.AngularVelocityY + "\t" + data.AngularVelocityZ);
                }

                await FileIO.WriteTextAsync(file, dataString.ToString(), Windows.Storage.Streams.UnicodeEncoding.Utf8);
                await file.MoveAsync(KnownFolders.DocumentsLibrary);

                dataString.Clear();
            }
        }
    }
}
