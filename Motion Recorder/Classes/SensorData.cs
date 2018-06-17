using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motion_Recorder.Classes
{
    public class SensorData
    {

        public DateTimeOffset Time { get; set; }

        public int Span { get; set; }

        public string AccelerationX { get; set; }

        public string AccelerationY { get; set; }

        public string AccelerationZ { get; set; }

        public string PitchDegrees { get; set; }

        public string RollDegrees { get; set; }

        public string YawDegrees { get; set; }

        public string AngularVelocityX { get; set; }

        public string AngularVelocityY { get; set; }

        public string AngularVelocityZ { get; set; }

    }
}
