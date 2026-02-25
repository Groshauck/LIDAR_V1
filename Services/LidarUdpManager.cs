using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using WinFormsApp1.Constants;
using WinFormsApp1.Models;

namespace WinFormsApp1.Services
{
    /// <summary>
    /// Gestionnaire UDP UNIQUE pour tous les LIDAR
    /// UN SEUL socket UDP qui distribue les paquets selon l'IP source
    /// </summary>
    public class LidarUdpManager
    {
        private UdpClient udpClient;
        private volatile bool isRunning = false;
        private readonly int udpPort;
        private readonly object lockObject = new object();

        // Dictionnaire : IP source → Callback
        private Dictionary<string, Action<List<LidarPoint>>> callbacks = new Dictionary<string, Action<List<LidarPoint>>>();

        private static LidarUdpManager instance;
        public static LidarUdpManager Instance => instance ?? (instance = new LidarUdpManager(2368));

        private LidarUdpManager(int port)
        {
            this.udpPort = port;
        }

        public void Start()
        {
            lock (lockObject)
            {
                if (isRunning && udpClient != null)
                {
                    return;
                }

                Stop();

                try
                {
                    isRunning = true;

                    IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, udpPort);

                    udpClient = new UdpClient();
                    udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    udpClient.Client.Bind(localEndPoint);

                    udpClient.BeginReceive(ReceiveCallback, null);
                }
                catch (SocketException ex)
                {
                    isRunning = false;
                    throw new InvalidOperationException($"Impossible de démarrer le gestionnaire UDP sur le port {udpPort}", ex);
                }
            }
        }

        public void Stop()
        {
            lock (lockObject)
            {
                isRunning = false;

                if (udpClient != null)
                {
                    try
                    {
                        udpClient.Close();
                        udpClient.Dispose();
                    }
                    catch { }
                    finally
                    {
                        udpClient = null;
                    }
                }
            }
        }

        public void RegisterLidar(string ipAddress, Action<List<LidarPoint>> onDataReceived)
        {
            lock (lockObject)
            {
                if (!callbacks.ContainsKey(ipAddress))
                {
                    callbacks[ipAddress] = onDataReceived;
                }
            }
        }

        public void UnregisterLidar(string ipAddress)
        {
            lock (lockObject)
            {
                if (callbacks.ContainsKey(ipAddress))
                {
                    callbacks.Remove(ipAddress);
                }
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            if (!isRunning) return;

            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, udpPort);
            try
            {
                byte[] data = udpClient?.EndReceive(ar, ref remoteEP);

                if (data != null && data.Length == LidarConstants.PacketSize)
                {
                    string sourceIp = remoteEP.Address.ToString();

                    // Distribuer aux callbacks
                    lock (lockObject)
                    {
                        if (callbacks.ContainsKey(sourceIp))
                        {
                            var points = ParsePacket(data);
                            if (points.Count > 0)
                            {
                                callbacks[sourceIp]?.Invoke(points);
                            }
                        }
                    }
                }
            }
            catch (ObjectDisposedException) { }
            catch (Exception) { }

            if (isRunning && udpClient != null)
            {
                try
                {
                    udpClient.BeginReceive(ReceiveCallback, null);
                }
                catch (ObjectDisposedException) { }
            }
        }

        private List<LidarPoint> ParsePacket(byte[] data)
        {
            var points = new List<LidarPoint>();
            uint headerId = BitConverter.ToUInt32(data, 0);
            if (headerId != 0xFEF0010F) return points;

            byte distanceScale = data[6];
            uint timestamp = BitConverter.ToUInt32(data, 28);

            for (int i = 0; i < LidarConstants.DataBlockCount; i++)
            {
                int pos = LidarConstants.HeaderSize + i * LidarConstants.BlockSize;
                ushort angleRaw = BitConverter.ToUInt16(data, pos);
                ushort distRaw = BitConverter.ToUInt16(data, pos + 2);
                ushort strength = BitConverter.ToUInt16(data, pos + 4);

                if (angleRaw >= 0xFF00) continue;

                double angle = angleRaw * 0.01;
                double distance = distRaw * distanceScale;

                if (distance == 0) continue;

                points.Add(new LidarPoint
                {
                    Angle = angle,
                    Distance = distance,
                    X = distance * Math.Cos(angle * Math.PI / 180.0),
                    Y = distance * Math.Sin(angle * Math.PI / 180.0),
                    SignalStrength = strength,
                    Timestamp = timestamp
                });
            }

            return points;
        }
    }
}