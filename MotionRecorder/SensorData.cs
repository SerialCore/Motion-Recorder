using System;

namespace MotionRecorder
{
    public class SensorData
    {
        public DateTimeOffset Time { get; set; }

        public double AccelerationX { get; set; }

        public double AccelerationY { get; set; }

        public double AccelerationZ { get; set; }

        public double PitchDegrees { get; set; }

        public double RollDegrees { get; set; }

        public double YawDegrees { get; set; }

        public double AngularVelocityX { get; set; }

        public double AngularVelocityY { get; set; }

        public double AngularVelocityZ { get; set; }
    }
}
