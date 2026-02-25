using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using WinFormsApp1.Models;

namespace WinFormsApp1.Services
{
    public class LidarConfigManager
    {
        private static readonly string ConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WinFormsApp1",
            "lidar_configs.json"
        );

        // Configuration pour les 8 LIDAR
        public Dictionary<int, Affichage> Configs { get; private set; }

        // Adresses IP des 8 LIDAR
        public Dictionary<int, (string ip, int port)> LidarAddresses { get; private set; }

        public LidarConfigManager()
        {
            // Initialiser les adresses IP (À ADAPTER SELON VOS LIDAR)
            LidarAddresses = new Dictionary<int, (string ip, int port)>
            {
                { 1, ("192.168.1.100", 2368) },
                { 2, ("192.168.1.101", 2368) },
                { 3, ("192.168.1.102", 2368) },
                { 4, ("192.168.1.103", 2368) },
                { 5, ("192.168.1.104", 2368) },
                { 6, ("192.168.1.105", 2368) },
                { 7, ("192.168.1.106", 2368) },
                { 8, ("192.168.1.107", 2368) }
            };

            LoadConfigs();
        }

        public Affichage GetConfig(int lidarNumber)
        {
            if (!Configs.ContainsKey(lidarNumber))
            {
                Configs[lidarNumber] = GetDefaultConfig();
            }
            return Configs[lidarNumber];
        }

        public void SaveConfig(int lidarNumber, Affichage config)
        {
            Configs[lidarNumber] = config;
            SaveConfigs();
        }

        public (string ip, int port) GetAddress(int lidarNumber)
        {
            return LidarAddresses.ContainsKey(lidarNumber)
                ? LidarAddresses[lidarNumber]
                : ("127.0.0.1", 2368);
        }

        private void LoadConfigs()
        {
            Configs = new Dictionary<int, Affichage>();

            if (File.Exists(ConfigPath))
            {
                try
                {
                    string json = File.ReadAllText(ConfigPath);
                    Configs = JsonSerializer.Deserialize<Dictionary<int, Affichage>>(json);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erreur chargement configs: {ex.Message}");
                }
            }

            // Initialiser les configs manquantes
            for (int i = 1; i <= 8; i++)
            {
                if (!Configs.ContainsKey(i))
                {
                    Configs[i] = GetDefaultConfig();
                }
            }
        }

        private void SaveConfigs()
        {
            try
            {
                string directory = Path.GetDirectoryName(ConfigPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonSerializer.Serialize(Configs, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ConfigPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur sauvegarde configs: {ex.Message}");
            }
        }

        private Affichage GetDefaultConfig()
        {
            return new Affichage
            {
                xMin = -3000,
                xMax = 3000,
                yMin = -3000,
                yMax = 3000,
                auto = true,

                AngleMin = 0,
                AngleMax = 360,

                numericXminPlastic = -500,
                numericXmaxPlastic = 500,
                numericYminPlastic = -500,
                numericYmaxPlastic = 500,
                numericNbPointMin = 10,

                FiltreplasticMin = 0,
                FiltreplasticMax = 10000,
                FiltrepouletMin = 0,
                FiltrepouletMax = 10000,

                SignalStrengthMin = 0,
                SignalStrengthMax = 65535,
                SignalStrengthAuto = true,

                FilterPoulet = false,
                FilterPlastic = false,
                ColorAuto = true,

                textBoxCheminExport = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };
        }
    }
}