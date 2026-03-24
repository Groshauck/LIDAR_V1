using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using WinFormsApp1.Models;

namespace WinFormsApp1.Services
{
    /// <summary>
    /// Service Modbus TCP TOR — implémentation légère sans NuGet externe.
    /// Supporte FC02 (lecture Discrete Inputs) et FC05 (écriture Single Coil).
    /// </summary>
    public class ModbusService : IDisposable
    {
        private ModbusConfig _config;
        private System.Threading.Timer? _pollingTimer;
        private readonly object _lock = new object();

        // État courant des entrées par carte (index carte → tableau de bits)
        private Dictionary<int, bool[]> _etatEntrees = new Dictionary<int, bool[]>();

        // Événements
        public event Action? OnDemarrerTest;
        public event Action<bool>? OnCachePresent;  // true = cache présent → bloquer

        // État voyant (reflète les sorties)
        private bool _sortieNormal = false;
        private bool _sortieDeboite = false;
        public bool SortieNormal => _sortieNormal;
        public bool SortieDeboite => _sortieDeboite;

        public ModbusService(ModbusConfig config)
        {
            _config = config;
        }

        public void Start()
        {
            if (!_config.ActivationGlobale) return;
            _pollingTimer = new System.Threading.Timer(PollingCallback, null,
                0, _config.PollingIntervalMs);
        }

        public void Stop()
        {
            _pollingTimer?.Dispose();
            _pollingTimer = null;
        }

        private void PollingCallback(object? state)
        {
            lock (_lock)
            {
                for (int i = 0; i < _config.Cartes.Count; i++)
                {
                    var carte = _config.Cartes[i];
                    if (!carte.Enabled) continue;
                    try
                    {
                        bool[]? entrees = ReadDiscreteInputs(carte, 0, carte.NbEntrees);
                        if (entrees == null) continue;

                        bool[] ancienEtat = _etatEntrees.ContainsKey(i)
                            ? _etatEntrees[i]
                            : new bool[carte.NbEntrees];

                        _etatEntrees[i] = entrees;

                        // Détection front montant sur bit DémarrerTest
                        int bitDem = carte.BitEntreeDemarrerTest;
                        if (bitDem < entrees.Length && entrees[bitDem]
                            && (ancienEtat.Length <= bitDem || !ancienEtat[bitDem]))
                        {
                            OnDemarrerTest?.Invoke();
                        }

                        // Bit cache présent (niveau haut = cache présent)
                        int bitCache = carte.BitEntreeCachePresent;
                        if (bitCache < entrees.Length)
                        {
                            OnCachePresent?.Invoke(entrees[bitCache]);
                        }
                    }
                    catch { /* connexion perdue → ignorer */ }
                }
            }
        }

        /// <summary>FC02 — Lit NbBits discrete inputs à partir de l'adresse startAddr.</summary>
        private bool[]? ReadDiscreteInputs(ModbusCardConfig carte, ushort startAddr, int nbBits)
        {
            try
            {
                using var client = new TcpClient();
                client.Connect(carte.IpAddress, carte.Port);
                client.ReceiveTimeout = 500;
                client.SendTimeout = 500;
                var stream = client.GetStream();

                // ADU Modbus TCP : Transaction ID(2) + Protocol(2) + Length(2) + UnitID(1) + FC(1) + StartAddr(2) + Qty(2)
                byte[] req = new byte[12];
                req[0] = 0x00; req[1] = 0x01;           // Transaction ID
                req[2] = 0x00; req[3] = 0x00;           // Protocol = Modbus
                req[4] = 0x00; req[5] = 0x06;           // Length = 6
                req[6] = (byte)carte.SlaveId;            // Unit ID
                req[7] = 0x02;                           // FC02
                req[8] = (byte)(startAddr >> 8);
                req[9] = (byte)(startAddr & 0xFF);
                req[10] = (byte)(nbBits >> 8);
                req[11] = (byte)(nbBits & 0xFF);

                stream.Write(req, 0, req.Length);

                byte[] resp = new byte[256];
                int n = stream.Read(resp, 0, resp.Length);

                // resp[8] = byte count, resp[9..] = données
                if (n < 9 || resp[7] == 0x82) return null; // exception FC

                bool[] result = new bool[nbBits];
                for (int b = 0; b < nbBits; b++)
                {
                    int byteIdx = 9 + b / 8;
                    int bitIdx = b % 8;
                    if (byteIdx < n)
                        result[b] = ((resp[byteIdx] >> bitIdx) & 1) == 1;
                }
                return result;
            }
            catch { return null; }
        }

        /// <summary>FC05 — Écrit un seul coil (sortie TOR).</summary>
        public void WriteSingleCoil(ModbusCardConfig carte, ushort coilAddr, bool value)
        {
            try
            {
                using var client = new TcpClient();
                client.Connect(carte.IpAddress, carte.Port);
                client.ReceiveTimeout = 500;
                client.SendTimeout = 500;
                var stream = client.GetStream();

                ushort val = value ? (ushort)0xFF00 : (ushort)0x0000;
                byte[] req = new byte[12];
                req[0] = 0x00; req[1] = 0x02;
                req[2] = 0x00; req[3] = 0x00;
                req[4] = 0x00; req[5] = 0x06;
                req[6] = (byte)carte.SlaveId;
                req[7] = 0x05;                          // FC05
                req[8] = (byte)(coilAddr >> 8);
                req[9] = (byte)(coilAddr & 0xFF);
                req[10] = (byte)(val >> 8);
                req[11] = (byte)(val & 0xFF);

                stream.Write(req, 0, req.Length);
                byte[] resp = new byte[12];
                stream.Read(resp, 0, resp.Length);
            }
            catch { }
        }

        /// <summary>Écrit le résultat Normal/Déboîté/EnCours sur toutes les cartes actives.</summary>
        public void SetResultat(bool normal, bool deboite, bool enCours)
        {
            _sortieNormal = normal;
            _sortieDeboite = deboite;

            foreach (var carte in _config.Cartes)
            {
                if (!carte.Enabled) continue;
                WriteSingleCoil(carte, (ushort)carte.BitSortieNormal, normal);
                WriteSingleCoil(carte, (ushort)carte.BitSortieDeboite, deboite);
                WriteSingleCoil(carte, (ushort)carte.BitSortieTestEnCours, enCours);
            }
        }

        public void Dispose() => Stop();
    }
}
