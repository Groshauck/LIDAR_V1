using ScottPlot;
using ScottPlot.Plottables;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Windows.Forms;
using WinFormsApp1.Forms;
using WinFormsApp1.Models;
using WinFormsApp1.Properties;
using WinFormsApp1.Services;
using System.Threading.Tasks;
using WinFormsLabel = System.Windows.Forms.Label;
using System.Diagnostics;


namespace WinFormsApp1.Forms
{
    public partial class ConfigurationForm : Form
    {
        private LidarConfigManager configManager;
        private LidarUdpReceiver lidarReceiver;
        private PointNetService pointNetService;
        private Process pythonApiProcess = null;

        private int currentLidarNumber = 1;
        private Affichage currentConfig;
        private List<LidarPoint> points = new List<LidarPoint>();
        private volatile bool isLive = false;
        private System.Windows.Forms.Timer refreshLidarTimer;


        // ========== VARIABLES POUR RECTANGLE DYNAMIQUE ==========
        private bool caisseOK = true;              
        private int nbPointsPlastique = 0;

        private bool plastique2OK = true;
        private int nbPointsPlastique2 = 0;

        private bool plastique3OK = true;
        private int nbPointsPlastique3 = 0;

        // ==========Variables pour le tooltip interactif
        private System.Windows.Forms.Label lblTooltip;
        private DateTime lastTooltipUpdate = DateTime.MinValue;
        private LidarPoint highlightedPoint = null;

        // ========== CONTRÔLES UI ==========

        // Top Panel
        private ComboBox comboLidarSelect;
        private Button btnDashboard;
        private Button btnPlayStop;

        // Angles
        private NumericUpDown numericAngleMin;
        private NumericUpDown numericAngleMax;

        // Axes
        private CheckBox checkBoxAutoScale;
        private NumericUpDown numericXmin;
        private NumericUpDown numericXmax;
        private NumericUpDown numericYmin;
        private NumericUpDown numericYmax;

        // Filtres Signal
        private CheckBox checkBoxFilterPoulet;
        private NumericUpDown numericSignalMinPoulet;
        private NumericUpDown numericSignalMaxPoulet;

        private CheckBox checkBoxFilterPlastic;
        private NumericUpDown numericSignalMinPlastic;
        private NumericUpDown numericSignalMaxPlastic;

        // Zone Plastique
        private NumericUpDown numericXminPlastic;
        private NumericUpDown numericXmaxPlastic;
        private NumericUpDown numericYminPlastic;
        private NumericUpDown numericYmaxPlastic;
        private NumericUpDown numericNbPointMin;

        // ========== ZONE PLASTIQUE 2 ==========
        private NumericUpDown numericXminPlastic2;
        private NumericUpDown numericXmaxPlastic2;
        private NumericUpDown numericYminPlastic2;
        private NumericUpDown numericYmaxPlastic2;
        private NumericUpDown numericNbPointMinPlastic2;

        // ========== ZONE PLASTIQUE 3 ==========
        private NumericUpDown numericXminPlastic3;
        private NumericUpDown numericXmaxPlastic3;
        private NumericUpDown numericYminPlastic3;
        private NumericUpDown numericYmaxPlastic3;
        private NumericUpDown numericNbPointMinPlastic3;

        // ========== ZONE IA/CSV ==========
        private NumericUpDown numericXminIA;
        private NumericUpDown numericXmaxIA;
        private NumericUpDown numericYminIA;
        private NumericUpDown numericYmaxIA;

        // Signal Strength
        private CheckBox checkBoxSignalStrengthAuto;
        private NumericUpDown numericSignalStrengthMin;
        private NumericUpDown numericSignalStrengthMax;


        // Affichage
        private CheckBox checkBoxColorAuto;
        private NumericUpDown numericGraphOffsetRight;

        // Export
        private TextBox textBoxCheminExport;
        private Button btnBrowseExport;
        private Button btnEnregistrerCsv;
        private Button btnChargerCsv;

        // IA
        private Button btnGenererDataset;
        private Button btnTrain;
        private Button btnStartIA;
        private Button btnDeboite;
        private Button btnNormal;

        // Graphique
        private ScottPlot.WinForms.FormsPlot plotPreview;
        private System.Windows.Forms.Label lblEtatCaisse;
        private System.Windows.Forms.Label lblEtatCaisseIA;

        // Panels
        private Panel panelTop;
        private Panel panelLeft;
        private Panel panelRight;

        // ========== TEST 30 SECONDES ==========
        private System.Windows.Forms.Timer testTimer;
        private DateTime testStartTime = DateTime.MinValue;
        private int testDureeTotaleSecondes = 30;
        private int testNormalCount = 0;
        private int testDeboiteCount = 0;
        private bool testEnCours = false;

        // Contrôles UI pour le test
        private Button btnTest30s;
        private System.Windows.Forms.Label lblTestCountdown;
        private System.Windows.Forms.Label lblTestNormal;
        private System.Windows.Forms.Label lblTestDeboite;

        public ConfigurationForm()
        {
            InitializeComponent();

            configManager = new LidarConfigManager();
            pointNetService = new PointNetService();
            CheckPointNetService();

            InitializeUI();
            LoadLidarConfig(1);

            //refreshLidarTimer = new System.Windows.Forms.Timer();
            //refreshLidarTimer.Interval = 10000; // 1000 ms = 1 seconde
            //refreshLidarTimer.Tick += RefreshLidarTimer_Tick;
            //refreshLidarTimer.Start();
        }

        private void RefreshLidarTimer_Tick(object sender, EventArgs e)
        {
            //RafraichirCourbeLidar();
        }

        private async void CheckPointNetService()
        {
            bool available = await pointNetService.IsAvailable();
            if (available)
            {
                lblEtatCaisseIA.Text = "IA: PointNet++ prêt ✓";
                lblEtatCaisseIA.ForeColor = System.Drawing.Color.Green;
            }
            else
            {
                lblEtatCaisseIA.Text = "IA: Service Python non démarré";
                lblEtatCaisseIA.ForeColor = System.Drawing.Color.Orange;
            }
        }

        private void InitializeUI()
        {
            this.Text = "Configuration LIDAR";
            this.Size = new Size(1600, 900);
            this.StartPosition = FormStartPosition.CenterScreen;

            // ========== PANEL TOP ==========
            panelTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = System.Drawing.Color.FromArgb(45, 45, 48)
            };

            // Sélection LIDAR
            var lblSelect = new System.Windows.Forms.Label
            {
                Text = "Sélectionner LIDAR :",
                Location = new Point(20, 25),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.White
            };
            panelTop.Controls.Add(lblSelect);

            comboLidarSelect = new ComboBox
            {
                Location = new Point(200, 22),
                Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            for (int i = 1; i <= 8; i++)
            {
                var (ip, port) = configManager.GetAddress(i);
                comboLidarSelect.Items.Add($"LIDAR {i} ({ip})");
            }
            comboLidarSelect.SelectedIndex = 0;
            comboLidarSelect.SelectedIndexChanged += ComboLidarSelect_SelectedIndexChanged;
            panelTop.Controls.Add(comboLidarSelect);

            // Bouton Dashboard
            btnDashboard = new Button
            {
                Text = "📊 Ouvrir Dashboard",
                Location = new Point(480, 18),
                Size = new Size(200, 35),
                Font = new Font("Segoe UI", 10, System.Drawing.FontStyle.Bold),
                BackColor = System.Drawing.Color.DodgerBlue,
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnDashboard.FlatAppearance.BorderSize = 0;
            btnDashboard.Click += BtnDashboard_Click;
            panelTop.Controls.Add(btnDashboard);

            // Bouton Play/Stop
            btnPlayStop = new Button
            {
                Text = "▶ Play",
                Location = new Point(700, 18),
                Size = new Size(100, 35),
                Font = new Font("Segoe UI", 10, System.Drawing.FontStyle.Bold),
                BackColor = System.Drawing.Color.LimeGreen,
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnPlayStop.FlatAppearance.BorderSize = 0;
            btnPlayStop.Click += BtnPlayStop_Click;
            panelTop.Controls.Add(btnPlayStop);

            // Bouton Test 30s
            btnTest30s = new Button
            {
                Text = "🧪 Test 30s",
                Location = new Point(820, 18),
                Size = new Size(130, 35),
                Font = new Font("Segoe UI", 10, System.Drawing.FontStyle.Bold),
                BackColor = System.Drawing.Color.DarkCyan,
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnTest30s.FlatAppearance.BorderSize = 0;
            btnTest30s.Click += BtnTest30s_Click;
            panelTop.Controls.Add(btnTest30s);

            // Label compte à rebours
            lblTestCountdown = new System.Windows.Forms.Label
            {
                Text = "",
                Location = new Point(960, 10),
                Size = new Size(80, 40),
                Font = new Font("Segoe UI", 11, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.Yellow,
                BackColor = System.Drawing.Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };
            panelTop.Controls.Add(lblTestCountdown);

            // Label caisses normales
            lblTestNormal = new System.Windows.Forms.Label
            {
                Text = "✅ 0",
                Location = new Point(1050, 10),
                Size = new Size(80, 40),
                Font = new Font("Segoe UI", 11, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.LimeGreen,
                BackColor = System.Drawing.Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };
            panelTop.Controls.Add(lblTestNormal);

            // Label caisses déboîtées
            lblTestDeboite = new System.Windows.Forms.Label
            {
                Text = "❌ 0",
                Location = new Point(1140, 10),
                Size = new Size(80, 40),
                Font = new Font("Segoe UI", 11, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.OrangeRed,
                BackColor = System.Drawing.Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };
            panelTop.Controls.Add(lblTestDeboite);

            this.Controls.Add(panelTop);

            // ========== PANEL RIGHT (Graphique) D'ABORD ==========
            panelRight = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = System.Drawing.Color.White
            };

            // Labels état
            var panelStatus = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = System.Drawing.Color.FromArgb(240, 240, 240)
            };

            lblEtatCaisse = new System.Windows.Forms.Label
            {
                Text = "État: -",
                Location = new Point(20, 15),
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 14, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.Gray
            };
            panelStatus.Controls.Add(lblEtatCaisse);

            lblEtatCaisseIA = new System.Windows.Forms.Label
            {
                Text = "IA: -",
                Location = new Point(350, 15),
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 14, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.Gray
            };
            panelStatus.Controls.Add(lblEtatCaisseIA);

            panelRight.Controls.Add(panelStatus);

            // ✅ CRÉER LE GRAPHIQUE (VERSION ORIGINALE)
            int offsetRight = 20;
            int graphWidth = this.ClientSize.Width - 400 - 20 - offsetRight;
            int graphHeight = this.ClientSize.Height - 70 - 60 - 40;

            plotPreview = new ScottPlot.WinForms.FormsPlot
            {
                Location = new Point(0, 70),
                Size = new Size(Math.Max(100, graphWidth), Math.Max(100, graphHeight)),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left
            };
            InitializePlotAxes();

            // ✅ AJOUT UNIQUEMENT : ÉVÉNEMENTS SOURIS POUR LE TOOLTIP
            plotPreview.MouseMove += PlotPreview_MouseMove;
            plotPreview.MouseLeave += PlotPreview_MouseLeave;

            panelRight.Controls.Add(plotPreview);

            // ✅ AJOUT UNIQUEMENT : LABEL TOOLTIP
            lblTooltip = new System.Windows.Forms.Label
            {
                Text = "",
                AutoSize = true,
                BackColor = System.Drawing.Color.FromArgb(240, 50, 50, 50),
                ForeColor = System.Drawing.Color.White,
                Font = new Font("Consolas", 10, System.Drawing.FontStyle.Bold),
                Padding = new Padding(10, 6, 10, 6),
                BorderStyle = BorderStyle.None,
                Visible = false
            };
            panelRight.Controls.Add(lblTooltip);
            lblTooltip.BringToFront();

            this.Controls.Add(panelRight);

            // ========== PANEL LEFT (Paramètres) ENSUITE ==========
            panelLeft = new Panel
            {
                Dock = DockStyle.Left,
                Width = 400,
                AutoScroll = true,
                BackColor = System.Drawing.Color.FromArgb(250, 250, 250)
            };

            CreateParametersPanel();
            this.Controls.Add(panelLeft);
        }

        private void InitializePlotAxes()
        {
            // ✅ FORCER LES MARGES (crucial pour les grands graphiques)
            plotPreview.Plot.Layout.Frameless(false);
            var padding = new ScottPlot.PixelPadding(
                left: 80,    // Espace pour Y
                right: 20,
                bottom: 780,  // Espace pour X
                top: 20
            );
            plotPreview.Plot.Layout.Fixed(padding);

            // Configuration initiale
            plotPreview.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericAutomatic();
            plotPreview.Plot.Axes.Left.TickGenerator = new ScottPlot.TickGenerators.NumericAutomatic();

            plotPreview.Plot.Axes.Bottom.Label.Text = "X (mm)";
            plotPreview.Plot.Axes.Bottom.Label.FontSize = 12;
            plotPreview.Plot.Axes.Bottom.Label.Bold = true;

            plotPreview.Plot.Axes.Left.Label.Text = "Y (mm)";
            plotPreview.Plot.Axes.Left.Label.FontSize = 12;
            plotPreview.Plot.Axes.Left.Label.Bold = true;

            plotPreview.Plot.Axes.Bottom.TickLabelStyle.FontSize = 10;
            plotPreview.Plot.Axes.Left.TickLabelStyle.FontSize = 10;

            plotPreview.Plot.Grid.MajorLineColor = ScottPlot.Colors.Gray.WithAlpha(0.3);
            plotPreview.Plot.Grid.MajorLineWidth = 1;

            plotPreview.Plot.FigureBackground.Color = ScottPlot.Colors.White;
            plotPreview.Plot.DataBackground.Color = ScottPlot.Colors.White;

            // Limites par défaut
            plotPreview.Plot.Axes.SetLimits(-3000, 3000, -3000, 3000);

            plotPreview.Refresh();
        }

        private void CreateParametersPanel()
        {
            int yPos = 20;
            int leftMargin = 20;
            int labelWidth = 180;
            int controlWidth = 180;

            // ========== ANGLES ==========
            AddSectionTitle(panelLeft, "Angles", ref yPos);

            numericAngleMin = AddNumericInput(panelLeft, "Angle Min:", ref yPos, 0, 360, 0);
            numericAngleMin.ValueChanged += (s, e) => { currentConfig.AngleMin = (double)numericAngleMin.Value; };

            numericAngleMax = AddNumericInput(panelLeft, "Angle Max:", ref yPos, 0, 360, 360);
            numericAngleMax.ValueChanged += (s, e) => { currentConfig.AngleMax = (double)numericAngleMax.Value; };

            // ========== AXES ==========
            yPos += 10;
            AddSectionTitle(panelLeft, "Axes", ref yPos);

            checkBoxAutoScale = AddCheckBox(panelLeft, "Auto Scale", ref yPos);
            checkBoxAutoScale.CheckedChanged += (s, e) => { currentConfig.auto = checkBoxAutoScale.Checked; };

            numericXmin = AddNumericInput(panelLeft, "X Min:", ref yPos, -10000, 10000, -3000);
            numericXmin.ValueChanged += (s, e) => { currentConfig.xMin = (double)numericXmin.Value; };

            numericXmax = AddNumericInput(panelLeft, "X Max:", ref yPos, -10000, 10000, 3000);
            numericXmax.ValueChanged += (s, e) => { currentConfig.xMax = (double)numericXmax.Value; };

            numericYmin = AddNumericInput(panelLeft, "Y Min:", ref yPos, -10000, 10000, -3000);
            numericYmin.ValueChanged += (s, e) => { currentConfig.yMin = (double)numericYmin.Value; };

            numericYmax = AddNumericInput(panelLeft, "Y Max:", ref yPos, -10000, 10000, 3000);
            numericYmax.ValueChanged += (s, e) => { currentConfig.yMax = (double)numericYmax.Value; };

            // ========== FILTRES SIGNAL ==========
            yPos += 10;
            AddSectionTitle(panelLeft, "Filtres Signal", ref yPos);

            checkBoxFilterPoulet = AddCheckBox(panelLeft, "Filtre Poulet", ref yPos);
            checkBoxFilterPoulet.CheckedChanged += (s, e) => { currentConfig.FilterPoulet = checkBoxFilterPoulet.Checked; };

            numericSignalMinPoulet = AddNumericInput(panelLeft, "Signal Min Poulet:", ref yPos, 0, 65535, 0);
            numericSignalMinPoulet.ValueChanged += (s, e) => { currentConfig.FiltrepouletMin = (double)numericSignalMinPoulet.Value; };

            numericSignalMaxPoulet = AddNumericInput(panelLeft, "Signal Max Poulet:", ref yPos, 0, 65535, 10000);
            numericSignalMaxPoulet.ValueChanged += (s, e) => { currentConfig.FiltrepouletMax = (double)numericSignalMaxPoulet.Value; };

            checkBoxFilterPlastic = AddCheckBox(panelLeft, "Filtre Plastic", ref yPos);
            checkBoxFilterPlastic.CheckedChanged += (s, e) => { currentConfig.FilterPlastic = checkBoxFilterPlastic.Checked; };

            numericSignalMinPlastic = AddNumericInput(panelLeft, "Signal Min Plastic:", ref yPos, 0, 65535, 0);
            numericSignalMinPlastic.ValueChanged += (s, e) => { currentConfig.FiltreplasticMin = (double)numericSignalMinPlastic.Value; };

            numericSignalMaxPlastic = AddNumericInput(panelLeft, "Signal Max Plastic:", ref yPos, 0, 65535, 10000);
            numericSignalMaxPlastic.ValueChanged += (s, e) => { currentConfig.FiltreplasticMax = (double)numericSignalMaxPlastic.Value; };

            // ========== ZONE PLASTIQUE ==========
            yPos += 10;
            AddSectionTitle(panelLeft, "Zone Plastique", ref yPos);

            numericXminPlastic = AddNumericInput(panelLeft, "X Min Plastic:", ref yPos, -10000, 10000, -500);
            numericXminPlastic.ValueChanged += (s, e) => { currentConfig.numericXminPlastic = (double)numericXminPlastic.Value; };

            numericXmaxPlastic = AddNumericInput(panelLeft, "X Max Plastic:", ref yPos, -10000, 10000, 500);
            numericXmaxPlastic.ValueChanged += (s, e) => { currentConfig.numericXmaxPlastic = (double)numericXmaxPlastic.Value; };

            numericYminPlastic = AddNumericInput(panelLeft, "Y Min Plastic:", ref yPos, -10000, 10000, -500);
            numericYminPlastic.ValueChanged += (s, e) => { currentConfig.numericYminPlastic = (double)numericYminPlastic.Value; };

            numericYmaxPlastic = AddNumericInput(panelLeft, "Y Max Plastic:", ref yPos, -10000, 10000, 500);
            numericYmaxPlastic.ValueChanged += (s, e) => { currentConfig.numericYmaxPlastic = (double)numericYmaxPlastic.Value; };

            numericNbPointMin = AddNumericInput(panelLeft, "Nb Points Min:", ref yPos, 0, 1000, 10);
            numericNbPointMin.ValueChanged += (s, e) => { currentConfig.numericNbPointMin = (double)numericNbPointMin.Value; };

            // ========== ZONE PLASTIQUE 2 ==========
            yPos += 10;
            AddSectionTitle(panelLeft, "Zone Plastique 2", ref yPos);

            numericXminPlastic2 = AddNumericInput(panelLeft, "X Min Plastic 2:", ref yPos, -10000, 10000, -800);
            numericXminPlastic2.ValueChanged += (s, e) =>
            {
                currentConfig.numericXminPlastic2 = (double)numericXminPlastic2.Value;
                if (points != null && points.Count > 0) PlotPreview();
            };

            numericXmaxPlastic2 = AddNumericInput(panelLeft, "X Max Plastic 2:", ref yPos, -10000, 10000, -200);
            numericXmaxPlastic2.ValueChanged += (s, e) =>
            {
                currentConfig.numericXmaxPlastic2 = (double)numericXmaxPlastic2.Value;
                if (points != null && points.Count > 0) PlotPreview();
            };

            numericYminPlastic2 = AddNumericInput(panelLeft, "Y Min Plastic 2:", ref yPos, -10000, 10000, 500);
            numericYminPlastic2.ValueChanged += (s, e) =>
            {
                currentConfig.numericYminPlastic2 = (double)numericYminPlastic2.Value;
                if (points != null && points.Count > 0) PlotPreview();
            };

            numericYmaxPlastic2 = AddNumericInput(panelLeft, "Y Max Plastic 2:", ref yPos, -10000, 10000, 1000);
            numericYmaxPlastic2.ValueChanged += (s, e) =>
            {
                currentConfig.numericYmaxPlastic2 = (double)numericYmaxPlastic2.Value;
                if (points != null && points.Count > 0) PlotPreview();
            };

            numericNbPointMinPlastic2 = AddNumericInput(panelLeft, "Nb Points Min P2:", ref yPos, 0, 1000, 50);
            numericNbPointMinPlastic2.ValueChanged += (s, e) =>
            {
                currentConfig.numericNbPointMinPlastic2 = (double)numericNbPointMinPlastic2.Value;
                if (points != null && points.Count > 0) PlotPreview();
            };

            // ========== ZONE PLASTIQUE 3 ==========
            yPos += 10;
            AddSectionTitle(panelLeft, "Zone Plastique 3", ref yPos);

            numericXminPlastic3 = AddNumericInput(panelLeft, "X Min Plastic 3:", ref yPos, -10000, 10000, 200);
            numericXminPlastic3.ValueChanged += (s, e) =>
            {
                currentConfig.numericXminPlastic3 = (double)numericXminPlastic3.Value;
                if (points != null && points.Count > 0) PlotPreview();
            };

            numericXmaxPlastic3 = AddNumericInput(panelLeft, "X Max Plastic 3:", ref yPos, -10000, 10000, 800);
            numericXmaxPlastic3.ValueChanged += (s, e) =>
            {
                currentConfig.numericXmaxPlastic3 = (double)numericXmaxPlastic3.Value;
                if (points != null && points.Count > 0) PlotPreview();
            };

            numericYminPlastic3 = AddNumericInput(panelLeft, "Y Min Plastic 3:", ref yPos, -10000, 10000, 500);
            numericYminPlastic3.ValueChanged += (s, e) =>
            {
                currentConfig.numericYminPlastic3 = (double)numericYminPlastic3.Value;
                if (points != null && points.Count > 0) PlotPreview();
            };

            numericYmaxPlastic3 = AddNumericInput(panelLeft, "Y Max Plastic 3:", ref yPos, -10000, 10000, 1000);
            numericYmaxPlastic3.ValueChanged += (s, e) =>
            {
                currentConfig.numericYmaxPlastic3 = (double)numericYmaxPlastic3.Value;
                if (points != null && points.Count > 0) PlotPreview();
            };

            numericNbPointMinPlastic3 = AddNumericInput(panelLeft, "Nb Points Min P3:", ref yPos, 0, 1000, 50);
            numericNbPointMinPlastic3.ValueChanged += (s, e) =>
            {
                currentConfig.numericNbPointMinPlastic3 = (double)numericNbPointMinPlastic3.Value;
                if (points != null && points.Count > 0) PlotPreview();
            };

            // ========== ZONE IA/CSV ==========
            yPos += 10;
            AddSectionTitle(panelLeft, "Zone IA / Export CSV", ref yPos);

            numericXminIA = AddNumericInput(panelLeft, "X Min IA/CSV:", ref yPos, -10000, 10000, -600);
            numericXminIA.ValueChanged += (s, e) =>
            {
                currentConfig.numericXminIA = (double)numericXminIA.Value;
                if (points != null && points.Count > 0) PlotPreview();
            };

            numericXmaxIA = AddNumericInput(panelLeft, "X Max IA/CSV:", ref yPos, -10000, 10000, 600);
            numericXmaxIA.ValueChanged += (s, e) =>
            {
                currentConfig.numericXmaxIA = (double)numericXmaxIA.Value;
                if (points != null && points.Count > 0) PlotPreview();
            };

            numericYminIA = AddNumericInput(panelLeft, "Y Min IA/CSV:", ref yPos, -10000, 10000, -600);
            numericYminIA.ValueChanged += (s, e) =>
            {
                currentConfig.numericYminIA = (double)numericYminIA.Value;
                if (points != null && points.Count > 0) PlotPreview();
            };

            numericYmaxIA = AddNumericInput(panelLeft, "Y Max IA/CSV:", ref yPos, -10000, 10000, 600);
            numericYmaxIA.ValueChanged += (s, e) =>
            {
                currentConfig.numericYmaxIA = (double)numericYmaxIA.Value;
                if (points != null && points.Count > 0) PlotPreview();
            };

            // Ajouter un label explicatif
            var lblInfoIA = new System.Windows.Forms.Label
            {
                Text = "Cette zone définit les points utilisés\npour l'IA et l'export CSV",
                Location = new System.Drawing.Point(20, yPos),
                Size = new System.Drawing.Size(360, 35),
                Font = new System.Drawing.Font("Segoe UI", 8f, System.Drawing.FontStyle.Italic),
                ForeColor = System.Drawing.Color.Gray
            };
            panelLeft.Controls.Add(lblInfoIA);
            yPos += 40;

            // ========== SIGNAL STRENGTH ==========
            yPos += 10;
            AddSectionTitle(panelLeft, "Signal Strength", ref yPos);

            checkBoxSignalStrengthAuto = AddCheckBox(panelLeft, "Auto Signal Strength", ref yPos);
            checkBoxSignalStrengthAuto.CheckedChanged += (s, e) => { currentConfig.SignalStrengthAuto = checkBoxSignalStrengthAuto.Checked; };

            numericSignalStrengthMin = AddNumericInput(panelLeft, "Signal Strength Min:", ref yPos, 0, 65535, 0);
            numericSignalStrengthMin.ValueChanged += (s, e) => { currentConfig.SignalStrengthMin = (double)numericSignalStrengthMin.Value; };

            numericSignalStrengthMax = AddNumericInput(panelLeft, "Signal Strength Max:", ref yPos, 0, 65535, 65535);
            numericSignalStrengthMax.ValueChanged += (s, e) => { currentConfig.SignalStrengthMax = (double)numericSignalStrengthMax.Value; };


            // ========== AFFICHAGE ==========
            yPos += 10;
            AddSectionTitle(panelLeft, "Affichage", ref yPos);

            checkBoxColorAuto = AddCheckBox(panelLeft, "Couleurs Automatiques", ref yPos);
            checkBoxColorAuto.CheckedChanged += (s, e) => { currentConfig.ColorAuto = checkBoxColorAuto.Checked; };

            // ========== EXPORT CSV ==========
            yPos += 10;
            AddSectionTitle(panelLeft, "Export CSV", ref yPos);

            var lblExport = new System.Windows.Forms.Label
            {
                Text = "Chemin Export:",
                Location = new Point(leftMargin, yPos),
                Size = new Size(labelWidth, 25),
                Font = new Font("Segoe UI", 9)
            };
            panelLeft.Controls.Add(lblExport);
            yPos += 25;

            textBoxCheminExport = new TextBox
            {
                Location = new Point(leftMargin, yPos),
                Size = new Size(280, 25),
                Font = new Font("Segoe UI", 9)
            };
            textBoxCheminExport.DoubleClick += TextBoxCheminExport_DoubleClick;
            panelLeft.Controls.Add(textBoxCheminExport);

            btnBrowseExport = new Button
            {
                Text = "...",
                Location = new Point(leftMargin + 285, yPos),
                Size = new Size(75, 25)
            };
            btnBrowseExport.Click += BtnBrowseExport_Click;
            panelLeft.Controls.Add(btnBrowseExport);
            yPos += 35;

            btnEnregistrerCsv = CreateButton("💾 Enregistrer CSV", leftMargin, ref yPos, 180);
            btnEnregistrerCsv.Click += BtnEnregistrerCsv_Click;

            btnChargerCsv = CreateButton("📂 Charger CSV", leftMargin + 190, ref yPos, 170);
            btnChargerCsv.Click += BtnChargerCsv_Click;
            yPos += 10;

            // ========== IA ==========
            yPos += 10;
            AddSectionTitle(panelLeft, "Intelligence Artificielle", ref yPos);

            btnGenererDataset = CreateButton("📦 Générer Dataset", leftMargin, ref yPos, 360);
            btnGenererDataset.Click += BtnGenererDataset_Click;

            btnTrain = CreateButton("🎓 Entraîner Modèle", leftMargin, ref yPos, 360);
            btnTrain.Click += BtnTrain_Click;
            btnTrain.BackColor = System.Drawing.Color.Orange;

            btnStartIA = CreateButton("🔬 Start IA", leftMargin, ref yPos, 360);
            btnStartIA.Click += btnStartIA_Click;
            btnStartIA.BackColor = System.Drawing.Color.Purple;

            var panelButtons = new Panel
            {
                Location = new Point(leftMargin, yPos),
                Size = new Size(360, 40)
            };

            btnDeboite = new Button
            {
                Text = "❌ Déboîté",
                Location = new Point(0, 0),
                Size = new Size(175, 35),
                BackColor = System.Drawing.Color.Red,
                ForeColor = System.Drawing.Color.White,
                Font = new Font("Segoe UI", 9, System.Drawing.FontStyle.Bold)
            };
            btnDeboite.Click += BtnDeboite_Click;
            panelButtons.Controls.Add(btnDeboite);

            btnNormal = new Button
            {
                Text = "✅ Normal",
                Location = new Point(185, 0),
                Size = new Size(175, 35),
                BackColor = System.Drawing.Color.Green,
                ForeColor = System.Drawing.Color.White,
                Font = new Font("Segoe UI", 9, System.Drawing.FontStyle.Bold)
            };
            btnNormal.Click += BtnNormal_Click;
            panelButtons.Controls.Add(btnNormal);

            panelLeft.Controls.Add(panelButtons);
            yPos += 50;

            // ========== SAUVEGARDER CONFIG ==========
            yPos += 10;
            var btnSaveConfig = new Button
            {
                Text = "💾 SAUVEGARDER CONFIGURATION",
                Location = new Point(leftMargin, yPos),
                Size = new Size(360, 45),
                BackColor = System.Drawing.Color.FromArgb(0, 120, 215),
                ForeColor = System.Drawing.Color.White,
                Font = new Font("Segoe UI", 11, System.Drawing.FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnSaveConfig.FlatAppearance.BorderSize = 0;
            btnSaveConfig.Click += BtnSaveConfig_Click;
            panelLeft.Controls.Add(btnSaveConfig);
            yPos += 60;
        }

        // ========== MÉTHODES UTILITAIRES UI ==========

        private void AddSectionTitle(Panel panel, string text, ref int yPos)
        {
            var lbl = new System.Windows.Forms.Label
            {
                Text = text,
                Location = new Point(20, yPos),
                Size = new Size(360, 30),
                Font = new Font("Segoe UI", 12, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.FromArgb(0, 120, 215),
                BackColor = System.Drawing.Color.FromArgb(230, 230, 230),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };
            panel.Controls.Add(lbl);
            yPos += 35;
        }

        private NumericUpDown AddNumericInput(Panel panel, string label, ref int yPos, decimal min, decimal max, decimal defaultValue, int decimals = 0)
        {
            var lbl = new System.Windows.Forms.Label
            {
                Text = label,
                Location = new Point(20, yPos),
                Size = new Size(180, 25),
                Font = new Font("Segoe UI", 9)
            };
            panel.Controls.Add(lbl);

            var numeric = new NumericUpDown
            {
                Location = new Point(210, yPos),
                Size = new Size(150, 25),
                Minimum = min,
                Maximum = max,
                Value = defaultValue,
                DecimalPlaces = decimals,
                Font = new Font("Segoe UI", 9)
            };
            panel.Controls.Add(numeric);

            yPos += 30;
            return numeric;
        }

        private CheckBox AddCheckBox(Panel panel, string label, ref int yPos)
        {
            var chk = new CheckBox
            {
                Text = label,
                Location = new Point(20, yPos),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };
            panel.Controls.Add(chk);

            yPos += 30;
            return chk;
        }

        private Button CreateButton(string text, int x, ref int yPos, int width)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(x, yPos),
                Size = new Size(width, 35),
                BackColor = System.Drawing.Color.FromArgb(0, 120, 215),
                ForeColor = System.Drawing.Color.White,
                Font = new Font("Segoe UI", 9, System.Drawing.FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btn.FlatAppearance.BorderSize = 0;
            panelLeft.Controls.Add(btn);

            yPos += 40;
            return btn;
        }

        // ========== ÉVÉNEMENTS ==========

        private void ComboLidarSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedLidar = comboLidarSelect.SelectedIndex + 1;
            LoadLidarConfig(selectedLidar);
        }

        private void LoadLidarConfig(int lidarNumber)
        {
            // Arrêter l'ancien receiver
            lidarReceiver?.Stop();
            isLive = false;
            btnPlayStop.Text = "▶ Play";
            btnPlayStop.BackColor = System.Drawing.Color.LimeGreen;

            currentLidarNumber = lidarNumber;
            currentConfig = configManager.GetConfig(lidarNumber);

            // Charger les valeurs dans l'UI
            numericAngleMin.Value = (decimal)currentConfig.AngleMin;
            numericAngleMax.Value = (decimal)currentConfig.AngleMax;

            checkBoxAutoScale.Checked = currentConfig.auto;
            numericXmin.Value = (decimal)currentConfig.xMin;
            numericXmax.Value = (decimal)currentConfig.xMax;
            numericYmin.Value = (decimal)currentConfig.yMin;
            numericYmax.Value = (decimal)currentConfig.yMax;

            checkBoxFilterPoulet.Checked = currentConfig.FilterPoulet;
            numericSignalMinPoulet.Value = (decimal)currentConfig.FiltrepouletMin;
            numericSignalMaxPoulet.Value = (decimal)currentConfig.FiltrepouletMax;

            checkBoxFilterPlastic.Checked = currentConfig.FilterPlastic;
            numericSignalMinPlastic.Value = (decimal)currentConfig.FiltreplasticMin;
            numericSignalMaxPlastic.Value = (decimal)currentConfig.FiltreplasticMax;

            // Zone Plastique 1
            numericXminPlastic.Value = (decimal)currentConfig.numericXminPlastic;
            numericXmaxPlastic.Value = (decimal)currentConfig.numericXmaxPlastic;
            numericYminPlastic.Value = (decimal)currentConfig.numericYminPlastic;
            numericYmaxPlastic.Value = (decimal)currentConfig.numericYmaxPlastic;
            numericNbPointMin.Value = (decimal)currentConfig.numericNbPointMin;

            // Zone Plastique 2
            numericXminPlastic2.Value = (decimal)currentConfig.numericXminPlastic2;
            numericXmaxPlastic2.Value = (decimal)currentConfig.numericXmaxPlastic2;
            numericYminPlastic2.Value = (decimal)currentConfig.numericYminPlastic2;
            numericYmaxPlastic2.Value = (decimal)currentConfig.numericYmaxPlastic2;
            numericNbPointMinPlastic2.Value = (decimal)currentConfig.numericNbPointMinPlastic2;

            // Zone Plastique 3
            numericXminPlastic3.Value = (decimal)currentConfig.numericXminPlastic3;
            numericXmaxPlastic3.Value = (decimal)currentConfig.numericXmaxPlastic3;
            numericYminPlastic3.Value = (decimal)currentConfig.numericYminPlastic3;
            numericYmaxPlastic3.Value = (decimal)currentConfig.numericYmaxPlastic3;
            numericNbPointMinPlastic3.Value = (decimal)currentConfig.numericNbPointMinPlastic3;

            // Zone IA/CSV
            numericXminIA.Value = (decimal)currentConfig.numericXminIA;
            numericXmaxIA.Value = (decimal)currentConfig.numericXmaxIA;
            numericYminIA.Value = (decimal)currentConfig.numericYminIA;
            numericYmaxIA.Value = (decimal)currentConfig.numericYmaxIA;

            checkBoxSignalStrengthAuto.Checked = currentConfig.SignalStrengthAuto;
            numericSignalStrengthMin.Value = (decimal)currentConfig.SignalStrengthMin;
            numericSignalStrengthMax.Value = (decimal)currentConfig.SignalStrengthMax;

            checkBoxColorAuto.Checked = currentConfig.ColorAuto;

            textBoxCheminExport.Text = currentConfig.textBoxCheminExport;

            this.Text = $"Configuration LIDAR {lidarNumber}";
        }

        private void BtnSaveConfig_Click(object sender, EventArgs e)
        {
            configManager.SaveConfig(currentLidarNumber, currentConfig);
            MessageBox.Show($"✓ Configuration LIDAR {currentLidarNumber} sauvegardée !", "Succès",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnPlayStop_Click(object sender, EventArgs e)
        {
            if (!isLive)
            {
                // Démarrer
                var (ip, port) = configManager.GetAddress(currentLidarNumber);
                lidarReceiver = new LidarUdpReceiver(ip, port);
                lidarReceiver.AngleMin = currentConfig.AngleMin;
                lidarReceiver.AngleMax = currentConfig.AngleMax;
                lidarReceiver.OnNewScan += OnNewScan;

                try
                {
                    lidarReceiver.Start();
                    isLive = true;
                    btnPlayStop.Text = "⏸ Stop";
                    btnPlayStop.BackColor = System.Drawing.Color.Red;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur: {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                // Arrêter
                lidarReceiver?.Stop();
                isLive = false;
                btnPlayStop.Text = "▶ Play";
                btnPlayStop.BackColor = System.Drawing.Color.LimeGreen;
            }
        }

        private void NumericGraphOffsetRight_ValueChanged(object sender, EventArgs e)
        {
            int offsetRight = (int)numericGraphOffsetRight.Value;
            int graphWidth = this.ClientSize.Width - 400 - 20 - offsetRight;
            int graphHeight = this.ClientSize.Height - 100 - 60 - 40;
            plotPreview.Size = new Size(Math.Max(600, graphWidth), Math.Max(400, graphHeight));
        }

        private void OnNewScan(List<LidarPoint> newPoints)
        {
            if (!this.IsHandleCreated) return;

            BeginInvoke((Action)(() =>
            {
                points = newPoints;
                PlotPreview();
            }));
        }

        private async void PlotPreview()
        {
            if (points.Count == 0) return;

            plotPreview.Plot.Clear();

            var xs = points.Select(p => p.X).ToArray();
            var ys = points.Select(p => p.Y).ToArray();
            var sigs = points.Select(p => (double)p.SignalStrength).ToArray();

            if (sigs.Length == 0) return;

            // Filtrage plastique
            var Filtreplastic = points.Where(p => p.SignalStrength >= currentConfig.FiltreplasticMin &&
                                                   p.SignalStrength <= currentConfig.FiltreplasticMax).ToList();

            var FiltrePoulet = points.Where(p => p.SignalStrength >= currentConfig.FiltrepouletMin &&
                                                  p.SignalStrength <= currentConfig.FiltrepouletMax).ToList();

            var FiltreplasticX = Filtreplastic.Where(p => p.X >= currentConfig.numericXminPlastic &&
                                                           p.X <= currentConfig.numericXmaxPlastic).ToList();

            var FiltreplasticXY = FiltreplasticX.Where(p => p.Y >= currentConfig.numericYminPlastic &&
                                                             p.Y <= currentConfig.numericYmaxPlastic).ToList();

            // Colormap
            double minS = currentConfig.SignalStrengthAuto ? sigs.Min() : currentConfig.SignalStrengthMin;
            double maxS = currentConfig.SignalStrengthAuto ? sigs.Max() : currentConfig.SignalStrengthMax;

            var cmap = new ScottPlot.Colormaps.Turbo();
            var colors = sigs.Select(s =>
            {
                double t = (s - minS) / Math.Max(1, maxS - minS);
                return cmap.GetColor(t);
            }).ToArray();

            // Affichage des points
            if (currentConfig.ColorAuto)
            {
                for (int i = 0; i < xs.Length; i++)
                {
                    var scatter = plotPreview.Plot.Add.Scatter(
                        new double[] { xs[i] },
                        new double[] { ys[i] },
                        color: colors[i]
                    );
                    scatter.MarkerSize = 5;
                    scatter.LineWidth = 0;
                    scatter.LineStyle.Width = 0;
                    scatter.MarkerShape = ScottPlot.MarkerShape.FilledCircle;
                }
            }
            else
            {
                plotPreview.Plot.Add.Scatter(xs, ys).Color = ScottPlot.Colors.Red;
            }

            // Détection classique
            if (currentConfig.FilterPlastic && Filtreplastic.Count > 0)
            {
                plotPreview.Plot.Add.Scatter(Filtreplastic.Select(p => p.X).ToArray(),
                                             Filtreplastic.Select(p => p.Y).ToArray()).Color = ScottPlot.Colors.Orange;

                if (FiltreplasticXY.Count > 0)
                {
                    double moyPlastic = FiltreplasticXY.Select(p => p.Y).Average();
                    var moyLine = plotPreview.Plot.Add.HorizontalLine(moyPlastic);
                    moyLine.Color = ScottPlot.Colors.Orange;
                    moyLine.LineWidth = 2;
                    moyLine.LineStyle.Pattern = ScottPlot.LinePattern.Dashed;

                    if (moyPlastic > currentConfig.numericYminPlastic &&
                        moyPlastic < currentConfig.numericYmaxPlastic &&
                        FiltreplasticXY.Count > currentConfig.numericNbPointMin)
                    {
                        lblEtatCaisse.ForeColor = System.Drawing.Color.Red;
                        lblEtatCaisse.Text = "État: NOK";
                    }
                    else
                    {
                        lblEtatCaisse.ForeColor = System.Drawing.Color.Green;
                        lblEtatCaisse.Text = "État: OK";
                    }
                }
            }

            if (currentConfig.FilterPoulet && FiltrePoulet.Count > 0)
            {
                plotPreview.Plot.Add.Scatter(FiltrePoulet.Select(p => p.X).ToArray(),
                                             FiltrePoulet.Select(p => p.Y).ToArray()).Color = ScottPlot.Colors.Brown;
            }

            // Limites plastique
            var pointsPlastiqueInRect = Filtreplastic.Where(p =>
                p.X >= currentConfig.numericXminPlastic &&
                p.X <= currentConfig.numericXmaxPlastic &&
                p.Y >= currentConfig.numericYminPlastic &&
                p.Y <= currentConfig.numericYmaxPlastic
            ).ToList();

            nbPointsPlastique = pointsPlastiqueInRect.Count;

            int seuilMin = (int)numericNbPointMin.Value;
            caisseOK = nbPointsPlastique <= seuilMin;
            ScottPlot.Color couleurRectangle = caisseOK ? ScottPlot.Colors.Green : ScottPlot.Colors.Red;

            var rectPlastic = plotPreview.Plot.Add.Rectangle(
                currentConfig.numericXminPlastic,
                currentConfig.numericXmaxPlastic,
                currentConfig.numericYminPlastic,
                currentConfig.numericYmaxPlastic
            );
            rectPlastic.LineColor = couleurRectangle;
            rectPlastic.LineWidth = 3;
            rectPlastic.LineStyle.Pattern = ScottPlot.LinePattern.Solid;
            rectPlastic.FillColor = couleurRectangle.WithAlpha(0.1);

            double centerX = (currentConfig.numericXminPlastic + currentConfig.numericXmaxPlastic) / 2;
            double centerY = (currentConfig.numericYminPlastic + currentConfig.numericYmaxPlastic) / 2;

            var textLabel = plotPreview.Plot.Add.Text(nbPointsPlastique.ToString(), centerX, centerY);
            textLabel.LabelFontSize = 24;
            textLabel.LabelBold = true;
            textLabel.LabelFontColor = couleurRectangle;
            textLabel.LabelBackgroundColor = ScottPlot.Colors.White.WithAlpha(0.9);
            textLabel.LabelBorderColor = couleurRectangle;
            textLabel.LabelBorderWidth = 2;
            textLabel.LabelPadding = 10;

            var textPoints = plotPreview.Plot.Add.Text("points", centerX, centerY - 150);
            textPoints.LabelFontSize = 12;
            textPoints.LabelFontColor = ScottPlot.Colors.Gray;
            textPoints.LabelBackgroundColor = ScottPlot.Colors.Transparent;

            var pointsPlastique2 = Filtreplastic.Where(p =>
                p.X >= currentConfig.numericXminPlastic2 &&
                p.X <= currentConfig.numericXmaxPlastic2 &&
                p.Y >= currentConfig.numericYminPlastic2 &&
                p.Y <= currentConfig.numericYmaxPlastic2
            ).ToList();

            nbPointsPlastique2 = pointsPlastique2.Count;
            int seuilMinPlastic2 = (int)currentConfig.numericNbPointMinPlastic2;
            plastique2OK = nbPointsPlastique2 <= seuilMinPlastic2;
            ScottPlot.Color couleurPlastic2 = plastique2OK ? ScottPlot.Colors.Green : ScottPlot.Colors.Red;

            var rectPlastic2 = plotPreview.Plot.Add.Rectangle(
                currentConfig.numericXminPlastic2,
                currentConfig.numericXmaxPlastic2,
                currentConfig.numericYminPlastic2,
                currentConfig.numericYmaxPlastic2
            );
            rectPlastic2.LineColor = couleurPlastic2;
            rectPlastic2.LineWidth = 3;
            rectPlastic2.LineStyle.Pattern = ScottPlot.LinePattern.Solid;
            rectPlastic2.FillColor = couleurPlastic2.WithAlpha(0.1);

            double centerXPlastic2 = (currentConfig.numericXminPlastic2 + currentConfig.numericXmaxPlastic2) / 2;
            double centerYPlastic2 = (currentConfig.numericYminPlastic2 + currentConfig.numericYmaxPlastic2) / 2;

            var textLabelPlastic2 = plotPreview.Plot.Add.Text(nbPointsPlastique2.ToString(), centerXPlastic2, centerYPlastic2);
            textLabelPlastic2.LabelFontSize = 16;
            textLabelPlastic2.LabelBold = true;
            textLabelPlastic2.LabelFontColor = couleurPlastic2;
            textLabelPlastic2.LabelBackgroundColor = ScottPlot.Colors.White.WithAlpha(0.9);
            textLabelPlastic2.LabelBorderColor = couleurPlastic2;
            textLabelPlastic2.LabelBorderWidth = 2;
            textLabelPlastic2.LabelPadding = 6;

            var textPointsPlastic2 = plotPreview.Plot.Add.Text("pts", centerXPlastic2, centerYPlastic2 - 80);
            textPointsPlastic2.LabelFontSize = 9;
            textPointsPlastic2.LabelFontColor = ScottPlot.Colors.Gray;
            textPointsPlastic2.LabelBackgroundColor = ScottPlot.Colors.Transparent;

            var pointsPlastique3 = Filtreplastic.Where(p =>
                p.X >= currentConfig.numericXminPlastic3 &&
                p.X <= currentConfig.numericXmaxPlastic3 &&
                p.Y >= currentConfig.numericYminPlastic3 &&
                p.Y <= currentConfig.numericYmaxPlastic3
            ).ToList();

            nbPointsPlastique3 = pointsPlastique3.Count;
            int seuilMinPlastic3 = (int)currentConfig.numericNbPointMinPlastic3;
            plastique3OK = nbPointsPlastique3 <= seuilMinPlastic3;
            ScottPlot.Color couleurPlastic3 = plastique3OK ? ScottPlot.Colors.Green : ScottPlot.Colors.Red;

            var rectPlastic3 = plotPreview.Plot.Add.Rectangle(
                currentConfig.numericXminPlastic3,
                currentConfig.numericXmaxPlastic3,
                currentConfig.numericYminPlastic3,
                currentConfig.numericYmaxPlastic3
            );
            rectPlastic3.LineColor = couleurPlastic3;
            rectPlastic3.LineWidth = 3;
            rectPlastic3.LineStyle.Pattern = ScottPlot.LinePattern.Solid;
            rectPlastic3.FillColor = couleurPlastic3.WithAlpha(0.1);

            double centerXPlastic3 = (currentConfig.numericXminPlastic3 + currentConfig.numericXmaxPlastic3) / 2;
            double centerYPlastic3 = (currentConfig.numericYminPlastic3 + currentConfig.numericYmaxPlastic3) / 2;

            var textLabelPlastic3 = plotPreview.Plot.Add.Text(nbPointsPlastique3.ToString(), centerXPlastic3, centerYPlastic3);
            textLabelPlastic3.LabelFontSize = 16;
            textLabelPlastic3.LabelBold = true;
            textLabelPlastic3.LabelFontColor = couleurPlastic3;
            textLabelPlastic3.LabelBackgroundColor = ScottPlot.Colors.White.WithAlpha(0.9);
            textLabelPlastic3.LabelBorderColor = couleurPlastic3;
            textLabelPlastic3.LabelBorderWidth = 2;
            textLabelPlastic3.LabelPadding = 6;

            var textPointsPlastic3 = plotPreview.Plot.Add.Text("pts", centerXPlastic3, centerYPlastic3 - 80);
            textPointsPlastic3.LabelFontSize = 9;
            textPointsPlastic3.LabelFontColor = ScottPlot.Colors.Gray;
            textPointsPlastic3.LabelBackgroundColor = ScottPlot.Colors.Transparent;

            if (lblEtatCaisse != null)
            {
                bool toutOK = caisseOK && plastique2OK && plastique3OK;

                if (toutOK)
                {
                    lblEtatCaisse.Text = $"✅ OK (P1:{nbPointsPlastique}/P2:{nbPointsPlastique2}/P3:{nbPointsPlastique3})";
                    lblEtatCaisse.ForeColor = System.Drawing.Color.Green;
                }
                else
                {
                    lblEtatCaisse.Text = $"❌ NOK (P1:{nbPointsPlastique}/P2:{nbPointsPlastique2}/P3:{nbPointsPlastique3})";
                    lblEtatCaisse.ForeColor = System.Drawing.Color.Red;
                }

                // ========== COMPTAGE TEST (cadence = rafraîchissement LIDAR) ==========
                if (testEnCours)
                {
                    if (toutOK)
                    {
                        testNormalCount++;
                        lblTestNormal.Text = $"✅ {testNormalCount}";
                    }
                    else
                    {
                        testDeboiteCount++;
                        lblTestDeboite.Text = $"❌ {testDeboiteCount}";
                    }
                }
            }

            var rectIA = plotPreview.Plot.Add.Rectangle(
                currentConfig.numericXminIA,
                currentConfig.numericXmaxIA,
                currentConfig.numericYminIA,
                currentConfig.numericYmaxIA
            );
            rectIA.LineColor = ScottPlot.Colors.Blue;
            rectIA.LineWidth = 2;
            rectIA.LineStyle.Pattern = ScottPlot.LinePattern.Dotted;
            rectIA.FillColor = ScottPlot.Colors.Blue.WithAlpha(0.05);

            double centerXIA = (currentConfig.numericXminIA + currentConfig.numericXmaxIA) / 2;
            double topYIA = currentConfig.numericYmaxIA;

            var textIA = plotPreview.Plot.Add.Text("IA/CSV", centerXIA, topYIA + 100);
            textIA.LabelFontSize = 14;
            textIA.LabelBold = true;
            textIA.LabelFontColor = ScottPlot.Colors.Blue;
            textIA.LabelBackgroundColor = ScottPlot.Colors.White.WithAlpha(0.9);
            textIA.LabelBorderColor = ScottPlot.Colors.Blue;
            textIA.LabelBorderWidth = 2;
            textIA.LabelPadding = 5;

            if (highlightedPoint != null)
            {
                var highlightScatter = plotPreview.Plot.Add.Scatter(
                    new double[] { highlightedPoint.X },
                    new double[] { highlightedPoint.Y }
                );
                highlightScatter.Color = ScottPlot.Colors.Black;
                highlightScatter.MarkerSize = 15;
                highlightScatter.MarkerShape = ScottPlot.MarkerShape.OpenCircle;
                highlightScatter.LineWidth = 5;
            }

            if (currentConfig.auto)
            {
                plotPreview.Plot.Axes.AutoScale();
            }
            else
            {
                plotPreview.Plot.Axes.SetLimits(currentConfig.xMin, currentConfig.xMax,
                                                currentConfig.yMin, currentConfig.yMax);
            }

            plotPreview.Plot.Layout.Frameless(false);
            var padding = new ScottPlot.PixelPadding(
                left: 80,
                right: 20,
                bottom: 780,
                top: 20
            );

            plotPreview.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericAutomatic();
            plotPreview.Plot.Axes.Left.TickGenerator = new ScottPlot.TickGenerators.NumericAutomatic();

            plotPreview.Plot.Axes.Bottom.Label.Text = "X (mm)";
            plotPreview.Plot.Axes.Bottom.Label.FontSize = 12;
            plotPreview.Plot.Axes.Bottom.Label.Bold = true;

            plotPreview.Plot.Axes.Left.Label.Text = "Y (mm)";
            plotPreview.Plot.Axes.Left.Label.FontSize = 12;
            plotPreview.Plot.Axes.Left.Label.Bold = true;

            plotPreview.Plot.Axes.Bottom.TickLabelStyle.FontSize = 10;
            plotPreview.Plot.Axes.Left.TickLabelStyle.FontSize = 10;

            plotPreview.Plot.Grid.MajorLineColor = ScottPlot.Colors.Gray.WithAlpha(0.3);
            plotPreview.Plot.Grid.MajorLineWidth = 1;

            plotPreview.Plot.FigureBackground.Color = ScottPlot.Colors.White;
            plotPreview.Plot.DataBackground.Color = ScottPlot.Colors.White;

            // ============= INTÉGRATION IA (identique) =============
            Task.Run(async () =>
            {
                if (await pointNetService.IsAvailable() && points.Count > 0)
                {
                    try
                    {
                        var pointsRect = points.Where(p =>
                             p.X >= currentConfig.numericXminIA &&
                             p.X <= currentConfig.numericXmaxIA &&
                             p.Y >= currentConfig.numericYminIA &&
                             p.Y <= currentConfig.numericYmaxIA
                         ).ToList();

                        if (pointsRect.Count > 10)
                        {
                            string tempCsv = Path.Combine(Path.GetTempPath(), $"ia_{DateTime.Now.Ticks}.csv");

                            using (StreamWriter sw = new StreamWriter(tempCsv))
                            {
                                sw.WriteLine("Angle;Distance;X;Y;SignalStrength;Timestamp");
                                foreach (var p in pointsRect)
                                    sw.WriteLine($"{p.Angle};{p.Distance};{p.X};{p.Y};{p.SignalStrength};{p.Timestamp}");
                            }

                            var result = await pointNetService.PredictAsync(tempCsv);

                            this.Invoke((Action)(() =>
                            {
                                lblEtatCaisseIA.Text = $"Normal: {result.ProbNormal:P1} | Déboîté: {result.ProbDeboite:P1}";
                                //lblEtatCaisseIA.Text = $"IA: {result.Prediction} ({result.Confidence:P0})";
                                lblEtatCaisseIA.ForeColor = result.IsDeboite ? System.Drawing.Color.Red : System.Drawing.Color.Green;
                            }));

                            if (File.Exists(tempCsv))
                                File.Delete(tempCsv);
                        }
                        else
                        {
                            // Pas assez de points dans la zone IA pour faire prédire
                            this.Invoke((Action)(() =>
                            {
                                lblEtatCaisseIA.Text = "IA : Pas assez de points";
                                lblEtatCaisseIA.ForeColor = System.Drawing.Color.Gray;
                            }));
                        }
                    }
                    catch (Exception ex)
                    {
                        // Une erreur s'est produite pendant la requête IA
                        this.Invoke((Action)(() =>
                        {
                            lblEtatCaisseIA.Text = "IA : Erreur prédiction";
                            lblEtatCaisseIA.ForeColor = System.Drawing.Color.OrangeRed;
                        }));
                    }
                }
                else
                {
                    // Service Python non disponible ou pas de points
                    this.Invoke((Action)(() =>
                    {
                        lblEtatCaisseIA.Text = "IA: Service Python non démarré";
                        lblEtatCaisseIA.ForeColor = System.Drawing.Color.Orange;
                    }));
                }
            });
            plotPreview.Refresh();
        }

        private void BtnDashboard_Click(object sender, EventArgs e)
        {
            var dashboardForm = new DashboardForm(configManager);
            this.Hide();
            dashboardForm.ShowDialog();
            this.Show();
        }

        private void BtnTest30s_Click(object sender, EventArgs e)
        {
            if (testEnCours)
            {
                StopTest();
                return;
            }

            if (!isLive)
            {
                MessageBox.Show("Démarrez le LIDAR avant de lancer le test !", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            testNormalCount = 0;
            testDeboiteCount = 0;
            testStartTime = DateTime.Now;
            testEnCours = true;

            lblTestNormal.Text = "✅ 0";
            lblTestDeboite.Text = "❌ 0";
            lblTestCountdown.Text = "30s";
            btnTest30s.Text = "⏹ Stop Test";
            btnTest30s.BackColor = System.Drawing.Color.DarkRed;

            testTimer?.Stop();
            testTimer?.Dispose();
            testTimer = new System.Windows.Forms.Timer();
            testTimer.Interval = 1000;
            testTimer.Tick += TestTimer_Tick;
            testTimer.Start();
        }

        private void TestTimer_Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            double elapsed = (now - testStartTime).TotalSeconds;
            int restantes = Math.Max(0, testDureeTotaleSecondes - (int)elapsed);
            lblTestCountdown.Text = $"{restantes}s";

            if (restantes <= 0)
            {
                StopTest();
                MessageBox.Show(
                    $"🧪 Test terminé !\n\n" +
                    $"✅ Normal   : {testNormalCount}\n" +
                    $"❌ Déboîté : {testDeboiteCount}\n\n" +
                    $"Total : {testNormalCount + testDeboiteCount} caisses détectées",
                    "Résultat du test",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
        }

        private void StopTest()
        {
            testEnCours = false;
            testTimer?.Stop();
            testTimer?.Dispose();
            testTimer = null;

            lblTestCountdown.Text = "";
            btnTest30s.Text = "🧪 Test 30s";
            btnTest30s.BackColor = System.Drawing.Color.DarkCyan;
        }

        private void BtnBrowseExport_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Sélectionnez le dossier d'export";
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    textBoxCheminExport.Text = fbd.SelectedPath;
                    currentConfig.textBoxCheminExport = fbd.SelectedPath;
                }
            }
        }

        private void TextBoxCheminExport_DoubleClick(object sender, EventArgs e)
        {
            BtnBrowseExport_Click(sender, e);
        }

        private void BtnEnregistrerCsv_Click(object sender, EventArgs e)
        {
            if (points.Count == 0)
            {
                MessageBox.Show("Aucune donnée à enregistrer !", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string filePath = textBoxCheminExport.Text.Trim();
            if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("Veuillez sélectionner un chemin d'export !", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            filePath = Path.Combine(filePath, $"lidar{currentLidarNumber}_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

            try
            {
                // ✅ FILTRER : Uniquement par position X et Y (pas de filtre signal)
                var pointsDansRectangle = points.Where(p =>
                    p.X >= currentConfig.numericXminIA &&
                    p.X <= currentConfig.numericXmaxIA &&
                    p.Y >= currentConfig.numericYminIA &&
                    p.Y <= currentConfig.numericYmaxIA
                ).ToList();

                // Vérifier s'il y a des points à sauvegarder
                if (pointsDansRectangle.Count == 0)
                {
                    MessageBox.Show("Aucun point dans le rectangle à enregistrer !",
                                  "Avertissement",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Warning);
                    return;
                }

                using (StreamWriter sw = new StreamWriter(filePath))
                {
                    sw.WriteLine("Angle;Distance;X;Y;SignalStrength;Timestamp");
                    foreach (var p in pointsDansRectangle)
                        sw.WriteLine($"{p.Angle};{p.Distance};{p.X};{p.Y};{p.SignalStrength};{p.Timestamp}");
                }

                MessageBox.Show($"✓ Export CSV réussi !\n\nFichier: {filePath}\n\nPoints total: {points.Count}\nPoints dans rectangle: {pointsDansRectangle.Count}",
                              "Succès",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'export : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnChargerCsv_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Fichiers CSV (*.csv)|*.csv";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        List<LidarPoint> loadedPoints = new List<LidarPoint>();

                        using (StreamReader sr = new StreamReader(ofd.FileName))
                        {
                            string line;
                            bool isFirstLine = true;
                            while ((line = sr.ReadLine()) != null)
                            {
                                if (isFirstLine)
                                {
                                    isFirstLine = false;
                                    continue;
                                }
                                var parts = line.Split(';');
                                if (parts.Length < 5) continue;

                                if (double.TryParse(parts[0], out double a) &&
                                    double.TryParse(parts[1], out double d) &&
                                    double.TryParse(parts[2], out double x) &&
                                    double.TryParse(parts[3], out double y) &&
                                    double.TryParse(parts[4], out double s))
                                {
                                    loadedPoints.Add(new LidarPoint { Angle = a, Distance = d, X = x, Y = y, SignalStrength = s });
                                }
                            }
                        }

                        points = loadedPoints;
                        PlotPreview();
                        MessageBox.Show("✓ Chargement CSV terminé", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // ✅ TEST IA AUTOMATIQUE APRÈS CHARGEMENT
                        TestIAAutomatique();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erreur lors du chargement : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnGenererDataset_Click(object sender, EventArgs e)
        {
            string dataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            "Detection de caisses déboitées",
            "03--API--",
            "LidarIA",
            "Data"
            );

            MessageBox.Show(
                $"📂 Structure des données pour PointNet++:\n\n" +
                $"Placez vos CSV dans:\n" +
                $"• {dataPath}\\normal\\     (caisses OK)\n" +
                $"• {dataPath}\\deboite\\    (caisses NOK)\n\n" +
                $"Utilisez les boutons ✅ Normal et ❌ Déboîté\n" +
                $"pour sauvegarder automatiquement les scans.",
                "Organisation des données",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            if (Directory.Exists(dataPath))
            {
                System.Diagnostics.Process.Start("explorer.exe", dataPath);
            }
        }

        private void BtnTrain_Click(object sender, EventArgs e)
        {
            try
            {
                string pythonPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    "Detection de caisses déboitées",
                    "03--API--",
                    "LidarIA"
                );

                string scriptPath = Path.Combine(pythonPath, "train_pointnet.py");

                if (!File.Exists(scriptPath))
                {
                    MessageBox.Show(
                        $"Script Python introuvable:\n{scriptPath}\n\n" +
                        "Vérifiez que les fichiers Python sont bien créés !",
                        "Erreur",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return;
                }

                var result = MessageBox.Show(
                    "🎓 Lancer l'entraînement PointNet++ ?\n\n" +
                    "Cela va ouvrir un terminal Python.\n" +
                    "L'entraînement prend 10-30 minutes.\n\n" +
                    "Assurez-vous d'avoir au moins 20 CSV dans:\n" +
                    "• Data/normal/\n" +
                    "• Data/deboite/",
                    "Entraînement IA",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.OK)
                {
                    var processInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/K cd /d \"{pythonPath}\" && python train_pointnet.py",
                        UseShellExecute = true,
                        CreateNoWindow = false
                    };
                    System.Diagnostics.Process.Start(processInfo);

                    MessageBox.Show(
                        "✓ Terminal d'entraînement lancé !\n\n" +
                        "Attendez la fin de l'entraînement.\n" +
                        "Vous verrez: ✅ Modèle sauvegardé",
                        "Information",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur: {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnStartIA_Click(object sender, EventArgs e)
        {
            // Lance l’API Python si nécessaire
            if (pythonApiProcess == null || pythonApiProcess.HasExited)
            {
                string pythonExe = @"C:\Users\Utilisateur\AppData\Local\Python\bin\python.exe";
                string scriptPath = @"C:\Users\Utilisateur\Desktop\Detection de caisses déboitées\03--API--\LidarIA\api_service.py";
                var startInfo = new ProcessStartInfo()
                {
                    FileName = pythonExe,
                    Arguments = $"\"{scriptPath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = @"C:\Users\Utilisateur\Desktop\Detection de caisses déboitées\03--API--\LidarIA"
                };

                pythonApiProcess = new Process();
                pythonApiProcess.StartInfo = startInfo;
                pythonApiProcess.Start();

                await Task.Delay(1000); // attends le démarrage du serveur Python
            }

            // Vérifie qu'il y a des points
            if (points.Count == 0)
            {
                MessageBox.Show("Aucune donnée à tester !", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Vérifie que le service Python est dispo
            if (!await pointNetService.IsAvailable())
            {
                MessageBox.Show(
                    "⚠️ Service Python non disponible !",
                    "Erreur",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            try
            {
                string tempCsv = Path.Combine(Path.GetTempPath(), $"test_{DateTime.Now:yyyyMMddHHmmss}.csv");

                var pointsRect = points.Where(p =>
                    p.X >= currentConfig.numericXminIA &&
                    p.X <= currentConfig.numericXmaxIA &&
                    p.Y >= currentConfig.numericYminIA &&
                    p.Y <= currentConfig.numericYmaxIA
                ).ToList();

                if (pointsRect.Count < 10)
                {
                    MessageBox.Show("Pas assez de points dans le rectangle !", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (StreamWriter sw = new StreamWriter(tempCsv))
                {
                    sw.WriteLine("Angle;Distance;X;Y;SignalStrength;Timestamp");
                    foreach (var p in pointsRect)
                        sw.WriteLine($"{p.Angle};{p.Distance};{p.X};{p.Y};{p.SignalStrength};{p.Timestamp}");
                }

                var result = await pointNetService.PredictAsync(tempCsv);

                MessageBox.Show(
                    $"🤖 PointNet++ Prédiction:\n\n" +
                    $"Résultat: {result.Prediction}\n" +
                    $"Confiance: {result.Confidence:P1}\n\n" +
                    $"Probabilités:\n" +
                    $"• Normal: {result.ProbNormal:P1}\n" +
                    $"• Déboîté: {result.ProbDeboite:P1}\n\n" +
                    $"Points analysés: {pointsRect.Count}",
                    "Test IA PointNet++",
                    MessageBoxButtons.OK,
                    result.IsDeboite ? MessageBoxIcon.Warning : MessageBoxIcon.Information
                );

                this.Invoke((Action)(() =>
                {
                    lblEtatCaisseIA.Text = $"Normal: {result.ProbNormal:P1} | Déboîté: {result.ProbDeboite:P1}";
                    lblEtatCaisseIA.ForeColor = result.IsDeboite ? System.Drawing.Color.Red : System.Drawing.Color.Green;
                }));

                if (File.Exists(tempCsv))
                    File.Delete(tempCsv);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur: {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnNormal_Click(object sender, EventArgs e)
        {
            if (points.Count == 0)
            {
                MessageBox.Show("Aucune donnée à sauvegarder !", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Chemin vers le dossier normal
                string normalFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    "Detection de caisses déboitées",
                    "03--API--",
                    "LidarIA",
                    "Data",
                    "normal"
                );
                Directory.CreateDirectory(normalFolder);

                // Filtrer les points dans le rectangle
                var pointsRect = points.Where(p =>
                    p.X >= currentConfig.numericXminIA &&
                    p.X <= currentConfig.numericXmaxIA &&
                    p.Y >= currentConfig.numericYminIA &&
                    p.Y <= currentConfig.numericYmaxIA
                ).ToList();     

                if (pointsRect.Count < 10)
                {
                    MessageBox.Show("Pas assez de points dans le rectangle !", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string fileName = $"normal_lidar{currentLidarNumber}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                string filePath = Path.Combine(normalFolder, fileName);

                using (StreamWriter sw = new StreamWriter(filePath))
                {
                    sw.WriteLine("Angle;Distance;X;Y;SignalStrength;Timestamp");
                    foreach (var p in pointsRect)
                        sw.WriteLine($"{p.Angle};{p.Distance};{p.X};{p.Y};{p.SignalStrength};{p.Timestamp}");
                }

                MessageBox.Show(
                    $"✅ Sauvegardé comme NORMAL\n\n" +
                    $"Fichier: {fileName}\n" +
                    $"Points: {pointsRect.Count}\n" +
                    $"Dossier: Data/normal/",
                    "Succès",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur: {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDeboite_Click(object sender, EventArgs e)
        {
            if (points.Count == 0)
            {
                MessageBox.Show("Aucune donnée à sauvegarder !", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Chemin vers le dossier déboité
                string deboiteFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    "Detection de caisses déboitées",
                    "03--API--",
                    "LidarIA",
                    "Data",
                    "deboite"
                );
                Directory.CreateDirectory(deboiteFolder);

                // Filtrer les points dans le rectangle
                var pointsRect = points.Where(p =>
                    p.X >= currentConfig.numericXminIA &&
                    p.X <= currentConfig.numericXmaxIA &&
                    p.Y >= currentConfig.numericYminIA &&
                    p.Y <= currentConfig.numericYmaxIA
                ).ToList();

                if (pointsRect.Count < 10)
                {
                    MessageBox.Show("Pas assez de points dans le rectangle !", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string fileName = $"deboite_lidar{currentLidarNumber}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                string filePath = Path.Combine(deboiteFolder, fileName);

                using (StreamWriter sw = new StreamWriter(filePath))
                {
                    sw.WriteLine("Angle;Distance;X;Y;SignalStrength;Timestamp");
                    foreach (var p in pointsRect)
                        sw.WriteLine($"{p.Angle};{p.Distance};{p.X};{p.Y};{p.SignalStrength};{p.Timestamp}");
                }

                MessageBox.Show(
                    $"✅ Sauvegardé comme DEBOITE\n\n" +
                    $"Fichier: {fileName}\n" +
                    $"Points: {pointsRect.Count}\n" +
                    $"Dossier: Data/deboite/",
                    "Succès",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur: {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string SelectFolder(string title)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = title;
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    return fbd.SelectedPath;
                }
            }
            return null;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            lidarReceiver?.Stop();
            configManager.SaveConfig(currentLidarNumber, currentConfig);
            base.OnFormClosing(e);
        }

        private async void TestIAAutomatique()
        {
            // Vérifier qu'il y a des points
            if (points == null || points.Count == 0)
            {
                lblEtatCaisseIA.Text = "IA: Aucune donnée";
                lblEtatCaisseIA.ForeColor = System.Drawing.Color.Gray;
                return;
            }

            // Vérifier que le service IA est disponible
            if (!await pointNetService.IsAvailable())
            {
                lblEtatCaisseIA.Text = "IA: Service non disponible";
                lblEtatCaisseIA.ForeColor = System.Drawing.Color.Orange;
                return;
            }

            try
            {
                // Afficher un message de chargement
                lblEtatCaisseIA.Text = "IA: Analyse en cours...";
                lblEtatCaisseIA.ForeColor = System.Drawing.Color.Blue;
                this.Refresh(); // Forcer le rafraîchissement visuel

                // Créer un fichier CSV temporaire
                string tempCsv = Path.Combine(Path.GetTempPath(), $"auto_{DateTime.Now:yyyyMMddHHmmss}.csv");

                // Filtrer les points dans le rectangle plastique
                var pointsRect = points.Where(p =>
                    p.X >= currentConfig.numericXminIA &&
                    p.X <= currentConfig.numericXmaxIA &&
                    p.Y >= currentConfig.numericYminIA &&
                    p.Y <= currentConfig.numericYmaxIA
                ).ToList();

                // Vérifier qu'il y a assez de points
                if (pointsRect.Count < 10)
                {
                    lblEtatCaisseIA.Text = "IA: Pas assez de points";
                    lblEtatCaisseIA.ForeColor = System.Drawing.Color.Orange;
                    return;
                }

                // Écrire le CSV temporaire
                using (StreamWriter sw = new StreamWriter(tempCsv))
                {
                    sw.WriteLine("Angle;Distance;X;Y;SignalStrength;Timestamp");
                    foreach (var p in pointsRect)
                    {
                        long timestamp = p.Timestamp > 0 ? p.Timestamp : DateTime.Now.Ticks;
                        sw.WriteLine($"{p.Angle};{p.Distance};{p.X};{p.Y};{p.SignalStrength};{timestamp}");
                    }
                }

                // Appeler l'API PointNet++
                var result = await pointNetService.PredictAsync(tempCsv);

                // Mettre à jour le label avec le résultat
                lblEtatCaisseIA.Text = $"IA: {result.Prediction} ({result.Confidence:P0})";
                lblEtatCaisseIA.ForeColor = result.IsDeboite ? System.Drawing.Color.Red : System.Drawing.Color.Green;

                // Afficher un popup avec les détails
                MessageBox.Show(
                    $"🤖 Détection automatique :\n\n" +
                    $"Résultat: {result.Prediction}\n" +
                    $"Confiance: {result.Confidence:P1}\n\n" +
                    $"Probabilités:\n" +
                    $"• Normal: {result.ProbNormal:P1}\n" +
                    $"• Déboîté: {result.ProbDeboite:P1}\n\n" +
                    $"Points analysés: {pointsRect.Count}",
                    "Prédiction IA Automatique",
                    MessageBoxButtons.OK,
                    result.IsDeboite ? MessageBoxIcon.Warning : MessageBoxIcon.Information
                );

                // Nettoyer le fichier temporaire
                if (File.Exists(tempCsv))
                    File.Delete(tempCsv);
            }
            catch (Exception ex)
            {
                lblEtatCaisseIA.Text = "IA: Erreur";
                lblEtatCaisseIA.ForeColor = System.Drawing.Color.Red;
                MessageBox.Show($"Erreur IA: {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        // ========== ÉVÉNEMENTS SOURIS POUR LE TOOLTIP ==========

        private void PlotPreview_MouseMove(object sender, MouseEventArgs e)
        {
            if (points == null || points.Count == 0)
            {
                lblTooltip.Visible = false;
                return;
            }

            // Limiter la fréquence de mise à jour (éviter la surcharge)
            if ((DateTime.Now - lastTooltipUpdate).TotalMilliseconds < 50)
                return;

            lastTooltipUpdate = DateTime.Now;

            try
            {
                // Convertir la position de la souris en coordonnées du graphique
                var pixel = new ScottPlot.Pixel(e.X, e.Y);
                var coords = plotPreview.Plot.GetCoordinates(pixel);

                // Trouver le point le plus proche
                LidarPoint closestPoint = null;
                double minDistance = double.MaxValue;

                foreach (var point in points)
                {
                    double dx = point.X - coords.X;
                    double dy = point.Y - coords.Y;
                    double dist = Math.Sqrt(dx * dx + dy * dy);

                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        closestPoint = point;
                    }
                }

                // Afficher le tooltip si un point est assez proche (seuil: 150mm)
                if (closestPoint != null && minDistance < 150)
                {
                    // ✅ Si le point survolé a changé, redessiner avec surbrillance
                    if (highlightedPoint == null ||
                        highlightedPoint.X != closestPoint.X ||
                        highlightedPoint.Y != closestPoint.Y)
                    {
                        highlightedPoint = closestPoint;
                        PlotPreview();  // Redessiner avec le point en surbrillance
                    }

                    lblTooltip.Text = $"X: {closestPoint.X:F0} mm  |  Y: {closestPoint.Y:F0} mm\n" +
                                     $"Distance: {closestPoint.Distance:F0} mm  |  Angle: {closestPoint.Angle:F1}°\n" +
                                     $"Signal: {closestPoint.SignalStrength:F0}";

                    // Positionner le tooltip près de la souris
                    int tooltipX = e.X + 15;
                    int tooltipY = e.Y + 15;

                    // Éviter que le tooltip sorte de l'écran
                    if (tooltipX + lblTooltip.Width > plotPreview.Width)
                        tooltipX = e.X - lblTooltip.Width - 15;

                    if (tooltipY + lblTooltip.Height > plotPreview.Height)
                        tooltipY = e.Y - lblTooltip.Height - 15;

                    lblTooltip.Location = new Point(tooltipX, tooltipY);
                    lblTooltip.Visible = true;
                }
                else
                {
                    // ✅ Désactiver la surbrillance si on s'éloigne
                    if (highlightedPoint != null)
                    {
                        highlightedPoint = null;
                        PlotPreview();  // Redessiner sans surbrillance
                    }
                    lblTooltip.Visible = false;
                }
            }
            catch
            {
                lblTooltip.Visible = false;
            }
        }

        private void PlotPreview_MouseLeave(object sender, EventArgs e)
        {
            lblTooltip.Visible = false;

            // ✅ Désactiver la surbrillance quand la souris quitte le graphique
            if (highlightedPoint != null)
            {
                highlightedPoint = null;
                PlotPreview();  // Redessiner sans surbrillance
            }
        }
    }
}