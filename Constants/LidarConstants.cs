using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp1.Constants
{
    public static class LidarConstants
    {
        public const int LidarPort = 2368;
        public const int PacketSize = 1240;
        public const int HeaderSize = 40;
        public const int BlockSize = 8;
        public const int DataBlockCount = 150;
    }
}
