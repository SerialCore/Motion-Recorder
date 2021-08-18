using System;
using System.Collections.Generic;
using Windows.Devices.Sensors;

namespace MotionRecorder
{
    class Sensor
    {
        // the instances of sensors
        private Accelerometer _accelerometer;
        private Inclinometer _inclinometer;
        private Gyrometer _gyrometer;

        // the readers of sensor data
        private AccelerometerReading areading;
        private InclinometerReading ireading;
        private GyrometerReading greading;

        // the existance of sensors
        private bool _isAccelerometerEnable = false;
        private bool _isInclinometerEnable = false;
        private bool _isGyrometerEnable = false;

        private List<SensorData> _record;

        public List<SensorData> Record { get { return _record; } }

        // the real-time data
        public DateTimeOffset Time { get { return areading.Timestamp; } }
        public double AccelerationX { get { return areading.AccelerationX; } }
        public double AccelerationY { get { return areading.AccelerationY; } }
        public double AccelerationZ { get { return areading.AccelerationZ; } }
        public double PitchDegrees { get { return ireading.PitchDegrees; } }
        public double RollDegrees { get { return ireading.RollDegrees; } }
        public double YawDegrees { get { return ireading.YawDegrees; } }
        public double AngularVelocityX { get { return greading.AngularVelocityX; } }
        public double AngularVelocityY { get { return greading.AngularVelocityY; } }
        public double AngularVelocityZ { get { return greading.AngularVelocityZ; } }

        public bool IsAccelerometerEnable { get { return _isAccelerometerEnable; } }
        public bool IsInclinometerEnable { get { return _isInclinometerEnable; } }
        public bool IsGyrometerEnable { get { return _isGyrometerEnable; } }

        public Sensor()
        {
            this._record = new List<SensorData>();

            // launch the sensors
            this._accelerometer = Accelerometer.GetDefault();
            this._inclinometer = Inclinometer.GetDefault();
            this._gyrometer = Gyrometer.GetDefault();

            if (this._accelerometer != null)
            {
                this._isAccelerometerEnable = true;
                this._accelerometer.ReportInterval = 60;
                this._accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
            }
            if (this._inclinometer != null)
            {
                this._isInclinometerEnable = true;
                this._inclinometer.ReportInterval = 60;
                this._inclinometer.ReadingChanged += Inclinometer_ReadingChanged;
            }
            if (this._gyrometer != null)
            {
                this._isGyrometerEnable = true;
                this._gyrometer.ReportInterval = 60;
                this._gyrometer.ReadingChanged += Gyrometer_ReadingChanged;
            }
        }

        public void RecordData()
        {
            if (_isInclinometerEnable && _isGyrometerEnable)
                AddData123();
            else if (!_isInclinometerEnable)
                AddData13();
            else if (!_isGyrometerEnable)
                AddData12();
            else
                AddData1();
        }

        public void ClearData()
        {
            this._record.Clear();
        }

        private void Accelerometer_ReadingChanged(Accelerometer sender, AccelerometerReadingChangedEventArgs args)
        {
            try
            {
                areading = args.Reading;
            }
            catch { }
        }

        private void Inclinometer_ReadingChanged(Inclinometer sender, InclinometerReadingChangedEventArgs args)
        {
            try
            {
                ireading = args.Reading;
            }
            catch { }
        }

        private void Gyrometer_ReadingChanged(Gyrometer sender, GyrometerReadingChangedEventArgs args)
        {
            try
            {
                greading = args.Reading;
            }
            catch { }
        }

        private void AddData123()
        {
            _record.Add(new SensorData
            {
                Time = areading.Timestamp,
                AccelerationX = areading.AccelerationX,
                AccelerationY = areading.AccelerationY,
                AccelerationZ = areading.AccelerationZ,
                PitchDegrees = ireading.PitchDegrees,
                RollDegrees = ireading.RollDegrees,
                YawDegrees = ireading.YawDegrees,
                AngularVelocityX = greading.AngularVelocityX,
                AngularVelocityY = greading.AngularVelocityY,
                AngularVelocityZ = greading.AngularVelocityZ
            });
        }

        private void AddData12()
        {
            _record.Add(new SensorData
            {
                Time = areading.Timestamp,
                AccelerationX = areading.AccelerationX,
                AccelerationY = areading.AccelerationY,
                AccelerationZ = areading.AccelerationZ,
                PitchDegrees = ireading.PitchDegrees,
                RollDegrees = ireading.RollDegrees,
                YawDegrees = ireading.YawDegrees
            });
        }

        private void AddData13()
        {
            _record.Add(new SensorData
            {
                Time = areading.Timestamp,
                AccelerationX = areading.AccelerationX,
                AccelerationY = areading.AccelerationY,
                AccelerationZ = areading.AccelerationZ,
                AngularVelocityX = greading.AngularVelocityX,
                AngularVelocityY = greading.AngularVelocityY,
                AngularVelocityZ = greading.AngularVelocityZ
            });
        }

        private void AddData1()
        {
            _record.Add(new SensorData
            {
                Time = areading.Timestamp,
                AccelerationX = areading.AccelerationX,
                AccelerationY = areading.AccelerationY,
                AccelerationZ = areading.AccelerationZ
            });
        }

    }
}
