using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using WinFormsApp1.Controls;
using WinFormsApp1.Services;

namespace WinFormsApp1.Forms
{
    public partial class DashboardForm : Form
    {
        private readonly LidarConfigManager configManager;
        private List<LidarDisplayControl> lidarControls = new List<LidarDisplayControl>();

        private Button btnPlayStop;
        private Button btnReturnConfig;
        private bool isRunning = false;

        public DashboardForm(LidarConfigManager configManager)
        {
            InitializeComponent();

            this.configManager = configManager;

            this.Load += DashboardForm_Load;
        }

        private void DashboardForm_Load(object sender, EventArgs e)
        {
            CreateDashboardUI();
        }

        private void CreateDashboardUI()
        {
            this.Text = "Dashboard - 8 LIDAR";
            this.WindowState = FormWindowState.Maximized;

            // Panel du haut avec boutons
            var panelTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            // Bouton Retour Config
            btnReturnConfig = new Button
            {
                Text = "⬅ Retour Configuration",
                Location = new Point(20, 15),
                Size = new Size(200, 35),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnReturnConfig.Click += BtnReturnConfig_Click;
            panelTop.Controls.Add(btnReturnConfig);

            // Bouton Play/Stop
            btnPlayStop = new Button
            {
                Text = "▶ Démarrer tous les LIDAR",
                Location = new Point(240, 15),
                Size = new Size(250, 35),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Color.LimeGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnPlayStop.Click += BtnPlayStop_Click;
            panelTop.Controls.Add(btnPlayStop);

            this.Controls.Add(panelTop);

            // Panel principal scrollable pour les 8 LIDAR
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.White
            };

            // Grille 4x2 (4 colonnes, 2 lignes)
            int cols = 4;
            int rows = 2;
            int controlWidth = 400;
            int controlHeight = 350;
            int margin = 10;

            for (int i = 1; i <= 8; i++)
            {
                int row = (i - 1) / cols;
                int col = (i - 1) % cols;

                var config = configManager.GetConfig(i);
                var (ip, port) = configManager.GetAddress(i);

                System.Diagnostics.Debug.WriteLine($"Dashboard - LIDAR {i}: {ip}:{port}");

                var lidarControl = new LidarDisplayControl
                {
                    Location = new Point(
                        col * (controlWidth + margin) + margin,
                        row * (controlHeight + margin) + margin + 80
                    ),
                    Size = new Size(controlWidth, controlHeight),
                    LidarTitle = $"LIDAR {i} ({ip}:{port})"
                };

                lidarControl.Initialize(ip, port, config);
                lidarControls.Add(lidarControl);
                mainPanel.Controls.Add(lidarControl);
            }

            this.Controls.Add(mainPanel);
        }

        // ✅ MÉTHODE CORRIGÉE
        private void BtnPlayStop_Click(object sender, EventArgs e)
        {
            if (!isRunning)
            {
                // ✅ DÉMARRER TOUS LES LIDAR
                System.Diagnostics.Debug.WriteLine("=== DÉMARRAGE DES 8 LIDAR ===");

                foreach (var lidar in lidarControls)
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine($"🚀 Démarrage {lidar.LidarTitle}");

                        lidar.StartLidar();    // ✅ Démarre le receiver UDP
                        lidar.StartDisplay();  // ✅ Active l'affichage
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Erreur démarrage {lidar.LidarTitle}: {ex.Message}");
                        MessageBox.Show(
                            $"Erreur démarrage {lidar.LidarTitle}:\n{ex.Message}",
                            "Erreur",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                    }
                }

                btnPlayStop.Text = "⏸ Arrêter tous les LIDAR";
                btnPlayStop.BackColor = Color.Red;
                isRunning = true;
            }
            else
            {
                // ✅ ARRÊTER TOUS LES LIDAR
                System.Diagnostics.Debug.WriteLine("=== ARRÊT DES 8 LIDAR ===");

                foreach (var lidar in lidarControls)
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine($"⏸ Arrêt {lidar.LidarTitle}");

                        lidar.StopLidar();     // ✅ Arrête le receiver UDP
                        lidar.StopDisplay();   // ✅ Désactive l'affichage
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"⚠️ Erreur arrêt {lidar.LidarTitle}: {ex.Message}");
                    }
                }

                btnPlayStop.Text = "▶ Démarrer tous les LIDAR";
                btnPlayStop.BackColor = Color.LimeGreen;
                isRunning = false;
            }
        }

        private void BtnReturnConfig_Click(object sender, EventArgs e)
        {
            // Retourner à la configuration
            this.Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Arrêter tous les LIDAR
            foreach (var lidar in lidarControls)
            {
                lidar.Stop();
            }

            base.OnFormClosing(e);
        }
    }
}