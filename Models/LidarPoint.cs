using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp1.Models
{
    public class LidarPoint
    {
        public double Angle { get; set; }
        public double Distance { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double SignalStrength { get; set; }
        public uint Timestamp { get; set; }
    }
}
