using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ScottPlot.WinForms;
using WinFormsApp1.Models;
using WinFormsApp1.Services;

namespace WinFormsApp1.Controls
{
    public partial class LidarDisplayControl : UserControl
    {
        // Services
        private LidarUdpReceiver lidarReceiver;

        // État
        private List<LidarPoint> points = new List<LidarPoint>();
        private Affichage settings;
        private volatile bool isActive = false;


        // ========== VARIABLES POUR RECTANGLE DYNAMIQUE ==========
        private bool caisseOK = true;
        private bool plastique2OK = true;
        private bool plastique3OK = true;
        private int nbPointsPlastique = 0;
        private int nbPointsPlastique2 = 0;
        private int nbPointsPlastique3 = 0;

        // Contrôles
        private FormsPlot plotControl;
        private Label lblStatus;
        private Label lblIA;
        private Label lblTitle;

        public string LidarTitle
        {
            get => lblTitle.Text;
            set => lblTitle.Text = value;
        }

        public LidarDisplayControl()
        {
            InitializeComponent();
            InitializeControls();
        }

        private void InitializeControls()
        {
            this.Size = new Size(400, 350);
            this.BorderStyle = BorderStyle.FixedSingle;

            // Titre
            lblTitle = new Label
            {
                Text = "LIDAR",
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Color.LightBlue
            };
            this.Controls.Add(lblTitle);

            // Graphique ScottPlot
            plotControl = new FormsPlot
            {
                Dock = DockStyle.Fill,
                Location = new Point(0, 30)
            };
            this.Controls.Add(plotControl);

            // Panel pour les labels en bas
            var bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.WhiteSmoke
            };

            lblStatus = new Label
            {
                Text = "Caisse OK",
                Location = new Point(10, 10),
                Size = new Size(180, 40),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.Green
            };
            bottomPanel.Controls.Add(lblStatus);

            lblIA = new Label
            {
                Text = "IA: -",
                Location = new Point(200, 10),
                Size = new Size(180, 40),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray
            };
            bottomPanel.Controls.Add(lblIA);

            this.Controls.Add(bottomPanel);
        }

        /// <summary>
        /// Initialise le LIDAR avec un port UDP spécifique
        /// </summary>
        public void Initialize(string ip, int udpPort, Affichage settings)
        {
            this.settings = settings;

            lidarReceiver = new LidarUdpReceiver(ip, udpPort);
            lidarReceiver.AngleMin = settings.AngleMin;
            lidarReceiver.AngleMax = settings.AngleMax;
            lidarReceiver.OnNewScan += OnNewScan;

            // ✅ Ne démarre PAS automatiquement
            // Le Dashboard appellera StartLidar() plus tard
            System.Diagnostics.Debug.WriteLine($"✓ LIDAR {ip}:{udpPort} initialisé (pas encore démarré)");

            lblStatus.Text = "Prêt";
            lblStatus.ForeColor = Color.Gray;
        }

        public void StartLidar()
        {
            System.Diagnostics.Debug.WriteLine($"🚀 StartLidar() appelé pour {lblTitle.Text}");

            if (lidarReceiver == null)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ lidarReceiver est NULL pour {lblTitle.Text}");
                lblStatus.Text = "Erreur: Non initialisé";
                lblStatus.ForeColor = Color.Red;
                return;
            }

            try
            {
                lidarReceiver.Start();
                isActive = true;
                lblStatus.Text = "Démarrage...";
                lblStatus.ForeColor = Color.Orange;
                System.Diagnostics.Debug.WriteLine($"✅ LIDAR démarré: {lblTitle.Text}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur StartLidar {lblTitle.Text}: {ex.Message}");
                lblStatus.Text = $"Erreur: {ex.Message}";
                lblStatus.ForeColor = Color.Red;
            }
        }

        /// Arrête le LIDAR (appelé par le Dashboard)
        public void StopLidar()
        {
            System.Diagnostics.Debug.WriteLine($"⏸ StopLidar() appelé pour {lblTitle.Text}");

            isActive = false;

            if (lidarReceiver != null)
            {
                try
                {
                    lidarReceiver.Stop();
                    System.Diagnostics.Debug.WriteLine($"✅ LIDAR arrêté: {lblTitle.Text}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ Erreur StopLidar {lblTitle.Text}: {ex.Message}");
                }
            }

            // Effacer l'affichage
            if (this.IsHandleCreated)
            {
                BeginInvoke((Action)(() =>
                {
                    plotControl.Plot.Clear();
                    plotControl.Refresh();
                    lblStatus.Text = "Arrêté";
                    lblStatus.ForeColor = Color.Gray;
                }));
            }
        }

        private void OnNewScan(List<LidarPoint> newPoints)
        {
            if (!this.IsHandleCreated) return;

            BeginInvoke((Action)(() =>
            {
                points = newPoints;
                if (isActive)
                {
                    PlotData();
                }
            }));
        }

        public void StartDisplay()
        {
            isActive = true;
        }

        public void StopDisplay()
        {
            isActive = false;
        }
        public bool HasNewScan { get; set; } = false;

        public void RefreshLidarView()
        {
            PlotData();
        }
        private void PlotData()
        {
            if (points.Count == 0) return;

            plotControl.Plot.Clear();

            var xs = points.Select(p => p.X).ToArray();
            var ys = points.Select(p => p.Y).ToArray();
            var sigs = points.Select(p => (double)p.SignalStrength).ToArray();

            if (sigs.Length == 0) return;

            // Filtrage plastique
            var Filtreplastic = points.Where(p => p.SignalStrength >= settings.FiltreplasticMin &&
                                                   p.SignalStrength <= settings.FiltreplasticMax).ToList();

            var FiltrePoulet = points.Where(p => p.SignalStrength >= settings.FiltrepouletMin &&
                                     p.SignalStrength <= settings.FiltrepouletMax).ToList();

            var FiltreplasticX = Filtreplastic.Where(p => p.X >= settings.numericXminPlastic &&
                                                           p.X <= settings.numericXmaxPlastic).ToList();

            var FiltreplasticXY = FiltreplasticX.Where(p => p.Y >= settings.numericYminPlastic &&
                                                             p.Y <= settings.numericYmaxPlastic).ToList();

            // Colormap
            double minS = settings.SignalStrengthAuto ? sigs.Min() : settings.SignalStrengthMin;
            double maxS = settings.SignalStrengthAuto ? sigs.Max() : settings.SignalStrengthMax;

            var cmap = new ScottPlot.Colormaps.Turbo();
            var colors = sigs.Select(s =>
            {
                double t = (s - minS) / Math.Max(1, maxS - minS);
                return cmap.GetColor(t);
            }).ToArray();


            // Affichage des points
            if (settings.ColorAuto)
            {
                for (int i = 0; i < xs.Length; i++)
                {
                    var scatter = plotControl.Plot.Add.Scatter(
                        new double[] { xs[i] },
                        new double[] { ys[i] },
                        color: colors[i]
                    );
                    scatter.MarkerSize = 3;
                    scatter.LineWidth = 0;
                    scatter.MarkerShape = ScottPlot.MarkerShape.FilledCircle;
                }
            }
            else
            {
                var scatter = plotControl.Plot.Add.Scatter(xs, ys);
                    scatter.Color = ScottPlot.Colors.Blue;  
                    scatter.MarkerSize = 3;
                    scatter.LineWidth = 0;
                    scatter.MarkerLineWidth = 0;
                    scatter.MarkerShape = ScottPlot.MarkerShape.FilledCircle;
            }

            // Détection classique
            if (settings.FilterPlastic && Filtreplastic.Count > 0)
            {
                var scatterPlastic = plotControl.Plot.Add.Scatter( Filtreplastic.Select(p => p.X).ToArray(),
                                                                    Filtreplastic.Select(p => p.Y).ToArray());

                scatterPlastic.Color = ScottPlot.Colors.Orange;
                scatterPlastic.LineStyle.Width = 0;  // ✅ Forcer ligne invisible
                scatterPlastic.LineWidth = 0;
                scatterPlastic.MarkerSize = 3;
                scatterPlastic.MarkerShape = ScottPlot.MarkerShape.FilledCircle;

                if (FiltreplasticXY.Count > 0)
                {
                    double moyPlastic = FiltreplasticXY.Select(p => p.Y).Average();
                    var moyLine = plotControl.Plot.Add.HorizontalLine(moyPlastic);
                    moyLine.Color = ScottPlot.Colors.Orange;
                    moyLine.LineWidth = 2;
                    moyLine.LineStyle.Pattern = ScottPlot.LinePattern.Dashed;

                    if (moyPlastic > settings.numericYminPlastic &&
                        moyPlastic < settings.numericYmaxPlastic &&
                        FiltreplasticXY.Count > settings.numericNbPointMin)
                    {
                        lblStatus.ForeColor = Color.Red;
                        lblStatus.Text = "Caisse NOK";
                    }
                    else
                    {
                        lblStatus.ForeColor = Color.Green;
                        lblStatus.Text = "Caisse OK";
                    }
                }
            }

            //  FILTRE POULET (MARRON) 
            if (settings.FilterPoulet && FiltrePoulet.Count > 0)
            {
                var scatterPoulet = plotControl.Plot.Add.Scatter(
                    FiltrePoulet.Select(p => p.X).ToArray(),
                    FiltrePoulet.Select(p => p.Y).ToArray()
                );
                scatterPoulet.Color = ScottPlot.Colors.Brown;
                scatterPoulet.LineStyle.Width = 0;
                scatterPoulet.MarkerSize = 3;
                scatterPoulet.MarkerShape = ScottPlot.MarkerShape.FilledCircle;
            }

            // Limites plastique
            // ============================================
            // ✅ RECTANGLE PLASTIQUE 1 DYNAMIQUE
            // ============================================

            var pointsPlastique = points.Where(p =>
                p.X >= settings.numericXminPlastic &&
                p.X <= settings.numericXmaxPlastic &&
                p.Y >= settings.numericYminPlastic &&
                p.Y <= settings.numericYmaxPlastic
            ).ToList();

            nbPointsPlastique = pointsPlastique.Count;
            int seuilMin = (int)settings.numericNbPointMin;
            caisseOK = nbPointsPlastique <= seuilMin;
            ScottPlot.Color couleurRectangle = caisseOK ? ScottPlot.Colors.Green : ScottPlot.Colors.Red;

            var rectPlastic = plotControl.Plot.Add.Rectangle(
                settings.numericXminPlastic,
                settings.numericXmaxPlastic,
                settings.numericYminPlastic,
                settings.numericYmaxPlastic
            );
            rectPlastic.LineColor = couleurRectangle;
            rectPlastic.LineWidth = 3;
            rectPlastic.LineStyle.Pattern = ScottPlot.LinePattern.Solid;
            rectPlastic.FillColor = couleurRectangle.WithAlpha(0.1);

            double centerX = (settings.numericXminPlastic + settings.numericXmaxPlastic) / 2;
            double centerY = (settings.numericYminPlastic + settings.numericYmaxPlastic) / 2;

            var textLabel = plotControl.Plot.Add.Text(nbPointsPlastique.ToString(), centerX, centerY);
            textLabel.LabelFontSize = 16;
            textLabel.LabelBold = true;
            textLabel.LabelFontColor = couleurRectangle;
            textLabel.LabelBackgroundColor = ScottPlot.Colors.White.WithAlpha(0.9);
            textLabel.LabelBorderColor = couleurRectangle;
            textLabel.LabelBorderWidth = 2;
            textLabel.LabelPadding = 6;

            var textPoints = plotControl.Plot.Add.Text("pts", centerX, centerY - 80);
            textPoints.LabelFontSize = 9;
            textPoints.LabelFontColor = ScottPlot.Colors.Gray;
            textPoints.LabelBackgroundColor = ScottPlot.Colors.Transparent;

            // ============================================
            // ✅ RECTANGLE PLASTIQUE 2 DYNAMIQUE
            // ============================================

            var pointsPlastique2 = points.Where(p =>
                p.X >= settings.numericXminPlastic2 &&
                p.X <= settings.numericXmaxPlastic2 &&
                p.Y >= settings.numericYminPlastic2 &&
                p.Y <= settings.numericYmaxPlastic2
            ).ToList();

            nbPointsPlastique2 = pointsPlastique2.Count;
            int seuilMinPlastic2 = (int)settings.numericNbPointMinPlastic2;
            plastique2OK = nbPointsPlastique2 <= seuilMinPlastic2;
            ScottPlot.Color couleurPlastic2 = plastique2OK ? ScottPlot.Colors.Green : ScottPlot.Colors.Red;

            var rectPlastic2 = plotControl.Plot.Add.Rectangle(
                settings.numericXminPlastic2,
                settings.numericXmaxPlastic2,
                settings.numericYminPlastic2,
                settings.numericYmaxPlastic2
            );
            rectPlastic2.LineColor = couleurPlastic2;
            rectPlastic2.LineWidth = 3;
            rectPlastic2.LineStyle.Pattern = ScottPlot.LinePattern.Solid;
            rectPlastic2.FillColor = couleurPlastic2.WithAlpha(0.1);

            double centerXPlastic2 = (settings.numericXminPlastic2 + settings.numericXmaxPlastic2) / 2;
            double centerYPlastic2 = (settings.numericYminPlastic2 + settings.numericYmaxPlastic2) / 2;

            var textLabelPlastic2 = plotControl.Plot.Add.Text(nbPointsPlastique2.ToString(), centerXPlastic2, centerYPlastic2);
            textLabelPlastic2.LabelFontSize = 14;  // ✅ Encore plus petit pour le dashboard
            textLabelPlastic2.LabelBold = true;
            textLabelPlastic2.LabelFontColor = couleurPlastic2;
            textLabelPlastic2.LabelBackgroundColor = ScottPlot.Colors.White.WithAlpha(0.9);
            textLabelPlastic2.LabelBorderColor = couleurPlastic2;
            textLabelPlastic2.LabelBorderWidth = 2;
            textLabelPlastic2.LabelPadding = 5;

            var textPointsPlastic2 = plotControl.Plot.Add.Text("pts", centerXPlastic2, centerYPlastic2 - 60);
            textPointsPlastic2.LabelFontSize = 8;
            textPointsPlastic2.LabelFontColor = ScottPlot.Colors.Gray;
            textPointsPlastic2.LabelBackgroundColor = ScottPlot.Colors.Transparent;

            // ============================================
            // ✅ RECTANGLE PLASTIQUE 3 DYNAMIQUE
            // ============================================

            var pointsPlastique3 = points.Where(p =>
                p.X >= settings.numericXminPlastic3 &&
                p.X <= settings.numericXmaxPlastic3 &&
                p.Y >= settings.numericYminPlastic3 &&
                p.Y <= settings.numericYmaxPlastic3
            ).ToList();

            nbPointsPlastique3 = pointsPlastique3.Count;
            int seuilMinPlastic3 = (int)settings.numericNbPointMinPlastic3;
            plastique3OK = nbPointsPlastique3 <= seuilMinPlastic3;
            ScottPlot.Color couleurPlastic3 = plastique3OK ? ScottPlot.Colors.Green : ScottPlot.Colors.Red;

            var rectPlastic3 = plotControl.Plot.Add.Rectangle(
                settings.numericXminPlastic3,
                settings.numericXmaxPlastic3,
                settings.numericYminPlastic3,
                settings.numericYmaxPlastic3
            );
            rectPlastic3.LineColor = couleurPlastic3;
            rectPlastic3.LineWidth = 3;
            rectPlastic3.LineStyle.Pattern = ScottPlot.LinePattern.Solid;
            rectPlastic3.FillColor = couleurPlastic3.WithAlpha(0.1);

            double centerXPlastic3 = (settings.numericXminPlastic3 + settings.numericXmaxPlastic3) / 2;
            double centerYPlastic3 = (settings.numericYminPlastic3 + settings.numericYmaxPlastic3) / 2;

            var textLabelPlastic3 = plotControl.Plot.Add.Text(nbPointsPlastique3.ToString(), centerXPlastic3, centerYPlastic3);
            textLabelPlastic3.LabelFontSize = 14;  // ✅ Encore plus petit pour le dashboard
            textLabelPlastic3.LabelBold = true;
            textLabelPlastic3.LabelFontColor = couleurPlastic3;
            textLabelPlastic3.LabelBackgroundColor = ScottPlot.Colors.White.WithAlpha(0.9);
            textLabelPlastic3.LabelBorderColor = couleurPlastic3;
            textLabelPlastic3.LabelBorderWidth = 2;
            textLabelPlastic3.LabelPadding = 5;

            var textPointsPlastic3 = plotControl.Plot.Add.Text("pts", centerXPlastic3, centerYPlastic3 - 60);
            textPointsPlastic3.LabelFontSize = 8;
            textPointsPlastic3.LabelFontColor = ScottPlot.Colors.Gray;
            textPointsPlastic3.LabelBackgroundColor = ScottPlot.Colors.Transparent;

            // ============================================
            // RECTANGLE ZONE IA/CSV (BLEU) - DASHBOARD
            // ============================================

            var rectIA = plotControl.Plot.Add.Rectangle(
                settings.numericXminIA,
                settings.numericXmaxIA,
                settings.numericYminIA,
                settings.numericYmaxIA
            );
            rectIA.LineColor = ScottPlot.Colors.Blue;
            rectIA.LineWidth = 2;
            rectIA.LineStyle.Pattern = ScottPlot.LinePattern.Dotted;
            rectIA.FillColor = ScottPlot.Colors.Blue.WithAlpha(0.05);

            // Ajouter un label "IA" au-dessus du rectangle (plus petit pour le dashboard)
            double centerXIA = (settings.numericXminIA + settings.numericXmaxIA) / 2;
            double topYIA = settings.numericYmaxIA;

            var textIA = plotControl.Plot.Add.Text("IA", centerXIA, topYIA + 80);
            textIA.LabelFontSize = 10;  // ✅ Plus petit que ConfigurationForm
            textIA.LabelBold = true;
            textIA.LabelFontColor = ScottPlot.Colors.Blue;
            textIA.LabelBackgroundColor = ScottPlot.Colors.White.WithAlpha(0.9);
            textIA.LabelBorderColor = ScottPlot.Colors.Blue;
            textIA.LabelBorderWidth = 1;
            textIA.LabelPadding = 3;


            // ✅ Mettre à jour le label d'état (toutes les zones)
            if (lblStatus != null)
            {
                bool toutOK = caisseOK && plastique2OK && plastique3OK;

                if (toutOK)
                {
                    lblStatus.Text = $"✅ OK (P1:{nbPointsPlastique}/P2:{nbPointsPlastique2}/P3:{nbPointsPlastique3})";
                    lblStatus.ForeColor = Color.Green;
                }
                else
                {
                    lblStatus.Text = $"❌ NOK (P1:{nbPointsPlastique}/P2:{nbPointsPlastique2}/P3:{nbPointsPlastique3})";
                    lblStatus.ForeColor = Color.Red;
                }
            }

            // Prédiction IA
            // Axes
            if (settings.auto)
            {
                plotControl.Plot.Axes.AutoScale();
            }
            else
            {
                plotControl.Plot.Axes.SetLimits(settings.xMin, settings.xMax, settings.yMin, settings.yMax);
            }

            // Afficher les ticks (graduations) sur les axes
            plotControl.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericAutomatic();
            plotControl.Plot.Axes.Left.TickGenerator = new ScottPlot.TickGenerators.NumericAutomatic();

            // Configurer le style des axes
            plotControl.Plot.Axes.Bottom.Label.Text = "X (mm)";
            plotControl.Plot.Axes.Left.Label.Text = "Y (mm)";

            // Taille de police des valeurs sur les axes
            plotControl.Plot.Axes.Bottom.TickLabelStyle.FontSize = 10;
            plotControl.Plot.Axes.Left.TickLabelStyle.FontSize = 10;

            // Afficher la grille
            plotControl.Plot.Grid.MajorLineColor = ScottPlot.Colors.Gray.WithAlpha(0.3);
            plotControl.Plot.Grid.MajorLineWidth = 1;

            // Axes existants (auto ou manuel)
            if (settings.auto)
            {
                plotControl.Plot.Axes.AutoScale();
            }
            else
            {
                plotControl.Plot.Axes.SetLimits(settings.xMin, settings.xMax, settings.yMin, settings.yMax);
            }
            // ========== CONFIGURATION DES AXES ET ÉCHELLES ==========

            // Afficher les graduations automatiques
            plotControl.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericAutomatic();
            plotControl.Plot.Axes.Left.TickGenerator = new ScottPlot.TickGenerators.NumericAutomatic();

            // Labels des axes
            plotControl.Plot.Axes.Bottom.Label.Text = "X (mm)";
            plotControl.Plot.Axes.Bottom.Label.FontSize = 12;
            plotControl.Plot.Axes.Bottom.Label.Bold = true;

            plotControl.Plot.Axes.Left.Label.Text = "Y (mm)";
            plotControl.Plot.Axes.Left.Label.FontSize = 12;
            plotControl.Plot.Axes.Left.Label.Bold = true;

            // Taille de police des valeurs numériques sur les axes
            plotControl.Plot.Axes.Bottom.TickLabelStyle.FontSize = 10;
            plotControl.Plot.Axes.Left.TickLabelStyle.FontSize = 10;

            // Grille
            plotControl.Plot.Grid.MajorLineColor = ScottPlot.Colors.Gray.WithAlpha(0.3);
            plotControl.Plot.Grid.MajorLineWidth = 1;

            // Couleur de fond
            plotControl.Plot.FigureBackground.Color = ScottPlot.Colors.White;
            plotControl.Plot.DataBackground.Color = ScottPlot.Colors.White;

            plotControl.Refresh();
        }

        public void Stop()
        {
            isActive = false;
            lidarReceiver?.Stop();
        }
    }
}