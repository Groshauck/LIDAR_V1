using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
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

        // ✅ Intervalle d'affichage configurable (par défaut 1 seconde)
        public int DisplayIntervalMs { get; set; } = 250;

        private volatile bool isRunning = false;

        private List<LidarPoint> currentScan = new List<LidarPoint>();
        private List<LidarPoint> lastCompletedScan = null; // ✅ Dernier scan complet bufferisé
        private double previousAngle = -1;
        private bool firstPacket = true;

        private readonly object lockObject = new object();
        private readonly object scanLock = new object(); // ✅ Verrou dédié au scan bufferisé
        private readonly string ipAddress;
        private readonly int udpPort;

        private System.Threading.Timer displayTimer; // ✅ Timer de cadence

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

                // ✅ Démarrage du timer de cadence
                displayTimer = new System.Threading.Timer(OnDisplayTick, null, DisplayIntervalMs, DisplayIntervalMs);

                LidarUdpManager.Instance.RegisterLidar(ipAddress, OnDataReceived);
                LidarUdpManager.Instance.Start();
            }
        }

        public void Stop()
        {
            lock (lockObject)
            {
                isRunning = false;
                firstPacket = true;
                previousAngle = -1;
                currentScan.Clear();

                // ✅ Arrêt propre du timer
                displayTimer?.Dispose();
                displayTimer = null;

                LidarUdpManager.Instance.UnregisterLidar(ipAddress);
            }
        }

        // ✅ Appelé toutes les X ms par le timer → envoie le dernier scan bufferisé
        private void OnDisplayTick(object state)
        {
            if (!isRunning) return;

            List<LidarPoint> scanToDisplay;

            lock (scanLock)
            {
                if (lastCompletedScan == null || lastCompletedScan.Count == 0) return;
                scanToDisplay = new List<LidarPoint>(lastCompletedScan);
            }

            OnNewScan?.Invoke(scanToDisplay);
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
            lock (lockObject) // ✅ Protection thread-safety
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
                            // ✅ On bufferise le scan au lieu de l'envoyer directement
                            lock (scanLock)
                            {
                                lastCompletedScan = new List<LidarPoint>(currentScan);
                            }
                        }

                        currentScan.Clear();
                    }

                    currentScan.Add(point);
                    previousAngle = point.Angle;
                }
            }
        }
    }
}