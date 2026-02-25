using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using WinFormsApp1.Controls;
using WinFormsApp1.Models;
using WinFormsApp1.Services;

namespace WinFormsApp1
{
    public partial class Courbe : Form
    {
        // Services partagés
        private Affichage settings;

        // 8 contrôles LIDAR
        private List<LidarDisplayControl> lidarControls = new List<LidarDisplayControl>();

        // ✅ CHANGEZ CES IP POUR VOS VRAIS LIDAR
        private readonly (string ip, int port)[] lidarConfigs =
        {
            ("192.168.1.100", 2368),  // LIDAR 1
            ("192.168.1.202", 2368),  // LIDAR 2
            ("192.168.1.203", 2368),  // LIDAR 3
            ("192.168.1.204", 2368),  // LIDAR 4
            ("192.168.1.205", 2368),  // LIDAR 5
            ("192.168.1.206", 2368),  // LIDAR 6
            ("192.168.1.207", 2368),  // LIDAR 7
            ("192.168.1.208", 2368)   // LIDAR 8
        };

        // Bouton Play/Stop
        private Button btnPlayStop;
        private bool isRunning = false;




        public Courbe()
        {
            InitializeComponent();

            this.Load += Courbe_Load;

        }

       


        private void Courbe_Load(object sender, EventArgs e)
        {
            // Charger les paramètres
            settings = AffichageSettingsManager.Charger();


            // Créer l'interface avec 8 LIDAR
            CreateLidarGrid();
        }

        private void CreateLidarGrid()
        {
            // Nettoyer les anciens contrôles
            this.Controls.Clear();

            // Panel principal scrollable
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.White
            };

            // Bouton Play/Stop en haut
            btnPlayStop = new Button
            {
                Text = "▶ Démarrer tous les LIDAR",
                Dock = DockStyle.Top,
                Height = 50,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                BackColor = Color.LimeGreen,
                ForeColor = Color.White
            };
            btnPlayStop.Click += BtnPlayStop_Click;

            // Grille 4x2 (4 colonnes, 2 lignes)
            int cols = 4;
            int rows = 2;
            int controlWidth = 400;
            int controlHeight = 350;
            int margin = 10;

            for (int i = 0; i < 8; i++)
            {
                int row = i / cols;
                int col = i % cols;

                var (ip, port) = lidarConfigs[i];

                var lidarControl = new LidarDisplayControl
                {
                    Location = new Point(col * (controlWidth + margin) + margin,
                                        row * (controlHeight + margin) + margin),
                    Size = new Size(controlWidth, controlHeight),
                    LidarTitle = $"LIDAR {i + 1} ({ip}:{port})"
                };

                // Initialiser avec IP et Port
                lidarControl.Initialize(ip, port, settings);
                lidarControls.Add(lidarControl);
                mainPanel.Controls.Add(lidarControl);
            }

            // Ajouter les contrôles au formulaire
            this.Controls.Add(mainPanel);
            this.Controls.Add(btnPlayStop);

            // Ajuster la taille du formulaire
            this.WindowState = FormWindowState.Maximized; // Plein écran
        }

        private void BtnPlayStop_Click(object sender, EventArgs e)
        {
            if (!isRunning)
            {
                // Démarrer tous
                foreach (var lidar in lidarControls)
                {
                    lidar.StartDisplay();
                }
                btnPlayStop.Text = "⏸ Arrêter tous les LIDAR";
                btnPlayStop.BackColor = Color.Red;
                isRunning = true;
            }
            else
            {
                // Arrêter tous
                foreach (var lidar in lidarControls)
                {
                    lidar.StopDisplay();
                }
                btnPlayStop.Text = "▶ Démarrer tous les LIDAR";
                btnPlayStop.BackColor = Color.LimeGreen;
                isRunning = false;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Arrêter tous les LIDAR
            foreach (var lidar in lidarControls)
            {
                lidar.Stop();
            }

            AffichageSettingsManager.Sauvegarder(settings);
            base.OnFormClosing(e);
        }
    }
}