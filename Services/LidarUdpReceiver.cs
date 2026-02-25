using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using WinFormsApp1.Constants;
using WinFormsApp1.Models;

namespace WinFormsApp1.Services
{
    public class LidarUdpReceiver
    {
        public event Action<List<LidarPoint>> OnNewScan;

        public double AngleMin { get; set; }
        public double AngleMax { get; set; }
        public float SignalStrengthCoeff { get; set; }
        public double SignalStrengthCenterSingularity { get; set; }
        public double SignalStrengthXPlage { get; set; }
        public bool CheckBoxSignalStrengthCoeff { get; set; }

        private volatile bool isRunning = false;

        private List<LidarPoint> currentScan = new List<LidarPoint>();
        private double previousAngle = -1;
        private bool firstPacket = true;

        private readonly object lockObject = new object();
        private readonly string ipAddress;
        private readonly int udpPort;

        public LidarUdpReceiver(string ip, int port = 2368)
        {
            this.ipAddress = ip;
            this.udpPort = port;
        }

        public void Start()
        {
            lock (lockObject)
            {
                if (isRunning) return;

                isRunning = true;

                LidarUdpManager.Instance.RegisterLidar(ipAddress, OnDataReceived);
                LidarUdpManager.Instance.Start();
            }
        }

        public void Stop()
        {
            lock (lockObject)
            {
                isRunning = false;
                LidarUdpManager.Instance.UnregisterLidar(ipAddress);
            }
        }

        private void OnDataReceived(List<LidarPoint> points)
        {
            if (!isRunning) return;

            var filteredPoints = new List<LidarPoint>();

            foreach (var point in points)
            {
                if (point.Angle < AngleMin || point.Angle > AngleMax)
                    continue;

                double x = point.X;
                double center = SignalStrengthCenterSingularity;
                double plage = SignalStrengthXPlage;
                double coef = SignalStrengthCoeff;
                double signal = point.SignalStrength;

                if (CheckBoxSignalStrengthCoeff && x >= center - plage && x <= center + plage)
                {
                    double distToCenter = Math.Abs(x - center);
                    double t = distToCenter / plage;
                    double impact = 1 + t * (coef - 1);
                    signal *= impact;
                }

                filteredPoints.Add(new LidarPoint
                {
                    Angle = point.Angle,
                    Distance = point.Distance,
                    X = point.X,
                    Y = point.Y,
                    SignalStrength = signal,
                    Timestamp = point.Timestamp
                });
            }

            if (filteredPoints.Count > 0)
            {
                AccumulatePoints(filteredPoints);
            }
        }

        private void AccumulatePoints(List<LidarPoint> points)
        {
            foreach (var point in points)
            {
                if (firstPacket)
                {
                    previousAngle = point.Angle;
                    firstPacket = false;
                }

                if (!firstPacket && point.Angle < previousAngle)
                {
                    if (currentScan.Count > 50)
                    {
                        OnNewScan?.Invoke(new List<LidarPoint>(currentScan));
                    }

                    currentScan.Clear();
                }

                currentScan.Add(point);
                previousAngle = point.Angle;
            }
        }
    }
}