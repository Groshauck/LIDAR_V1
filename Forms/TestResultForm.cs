using ScottPlot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WinFormsApp1.Models;

namespace WinFormsApp1.Forms
{
    public class TestResultForm : Form
    {
        private const double AnglePrecision = 0.5;

        private ScottPlot.WinForms.FormsPlot plotResult;
        private System.Windows.Forms.Label lblInfo;
        private readonly List<List<LidarPoint>> allScans;
        private readonly int testDurationSeconds;
        private readonly double seuilEcartMm;

        private List<LidarPoint> pointsMin = new List<LidarPoint>();
        private List<LidarPoint> pointsMax = new List<LidarPoint>();
        private List<LidarPoint> pointsStable = new List<LidarPoint>();

        public TestResultForm(List<List<LidarPoint>> allScans, int testDurationSeconds = 30, double seuilEcartMm = 100.0)
        {
            this.allScans = allScans;
            this.testDurationSeconds = testDurationSeconds;
            this.seuilEcartMm = seuilEcartMm;
            InitializeUI();
            PlotResults();
        }

        private void InitializeUI()
        {
            this.Text = $"Résultats Test {testDurationSeconds}s — Positions X/Y min & max distance / angle";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            lblInfo = new System.Windows.Forms.Label
            {
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10),
                ForeColor = System.Drawing.Color.DimGray,
                Padding = new Padding(10, 0, 0, 0)
            };
            this.Controls.Add(lblInfo);

            plotResult = new ScottPlot.WinForms.FormsPlot
            {
                Dock = DockStyle.Fill
            };
            this.Controls.Add(plotResult);

            var panelBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50
            };

            var btnExport = new Button
            {
                Text = "💾 Exporter les courbes",
                Dock = DockStyle.Fill,
                Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold),
                BackColor = System.Drawing.Color.FromArgb(0, 120, 215),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnExport.Click += BtnExport_Click;
            panelBottom.Controls.Add(btnExport);
            this.Controls.Add(panelBottom);
        }

        private void PlotResults()
        {
            if (allScans == null || allScans.Count == 0)
            {
                lblInfo.Text = "Aucun scan enregistré.";
                return;
            }

            plotResult.Plot.Clear();

            var allPoints = allScans.SelectMany(scan => scan).ToList();

            // Regrouper tous les points par angle arrondi à AnglePrecision
            var groupedByAngle = allPoints
                .GroupBy(p => Math.Round(p.Angle / AnglePrecision, MidpointRounding.AwayFromZero) * AnglePrecision)
                .OrderBy(g => g.Key)
                .ToList();

            // Pour chaque angle : point à distance min et point à distance max
            var xsMin = new List<double>();
            var ysMin = new List<double>();
            var xsMax = new List<double>();
            var ysMax = new List<double>();
            var xsStable = new List<double>();
            var ysStable = new List<double>();

            pointsMin.Clear();
            pointsMax.Clear();
            pointsStable.Clear();

            foreach (var group in groupedByAngle)
            {
                var ptMin = group.OrderBy(p => p.Distance).First();
                var ptMax = group.OrderByDescending(p => p.Distance).First();

                xsMin.Add(ptMin.X);
                ysMin.Add(ptMin.Y);
                xsMax.Add(ptMax.X);
                ysMax.Add(ptMax.Y);

                pointsMin.Add(ptMin);
                pointsMax.Add(ptMax);

                double ecart = ptMax.Distance - ptMin.Distance;
                if (ecart < seuilEcartMm)
                {
                    xsStable.Add((ptMin.X + ptMax.X) / 2.0);
                    ysStable.Add((ptMin.Y + ptMax.Y) / 2.0);

                    pointsStable.Add(new LidarPoint
                    {
                        Angle = group.Key,
                        Distance = (ptMin.Distance + ptMax.Distance) / 2.0,
                        X = (ptMin.X + ptMax.X) / 2.0,
                        Y = (ptMin.Y + ptMax.Y) / 2.0,
                        SignalStrength = (ptMin.SignalStrength + ptMax.SignalStrength) / 2.0,
                        Timestamp = 0
                    });
                }
            }

            // ========== COURBE VERTE : Distance minimale par angle ==========
            var scatterMin = plotResult.Plot.Add.Scatter(xsMin.ToArray(), ysMin.ToArray());
            scatterMin.Color = ScottPlot.Colors.Green;
            scatterMin.MarkerSize = 4;
            scatterMin.LineWidth = 1;
            scatterMin.LegendText = "Position à dist. min / angle";

            // ========== COURBE ROUGE : Distance maximale par angle ==========
            var scatterMax = plotResult.Plot.Add.Scatter(xsMax.ToArray(), ysMax.ToArray());
            scatterMax.Color = ScottPlot.Colors.Red;
            scatterMax.MarkerSize = 4;
            scatterMax.LineWidth = 1;
            scatterMax.LegendText = "Position à dist. max / angle";

            // ========== COURBE NOIRE : Positions stables (écart < seuil) ==========
            if (xsStable.Count > 0)
            {
                var scatterStable = plotResult.Plot.Add.Scatter(xsStable.ToArray(), ysStable.ToArray());
                scatterStable.Color = ScottPlot.Colors.Black;
                scatterStable.MarkerSize = 5;
                scatterStable.LineWidth = 1;
                scatterStable.LegendText = $"Position stable (écart < {seuilEcartMm} mm)";
            }

            // ========== AXES & LÉGENDE ==========
            plotResult.Plot.Axes.Bottom.Label.Text = "X (mm)";
            plotResult.Plot.Axes.Bottom.Label.Bold = true;
            plotResult.Plot.Axes.Left.Label.Text = "Y (mm)";
            plotResult.Plot.Axes.Left.Label.Bold = true;

            plotResult.Plot.ShowLegend();
            plotResult.Plot.Title($"Test {testDurationSeconds}s — Positions X/Y min & max distance / angle");

            plotResult.Plot.Grid.MajorLineColor = ScottPlot.Colors.Gray.WithAlpha(0.3);

            plotResult.Plot.Axes.AutoScale();
            plotResult.Refresh();

            lblInfo.Text = $"Scans enregistrés : {allScans.Count}  |  Points totaux : {allPoints.Count}  |  Angles couverts : {groupedByAngle.Count}  |  Points stables : {xsStable.Count}";
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Sélectionnez le dossier racine d'export";
                if (fbd.ShowDialog() != DialogResult.OK) return;

                string racine = fbd.SelectedPath;
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

                string dossierMin    = Path.Combine(racine, "minimum");
                string dossierMax    = Path.Combine(racine, "maximum");
                string dossierStable = Path.Combine(racine, "stable");

                if (!Directory.Exists(dossierMin))    Directory.CreateDirectory(dossierMin);
                if (!Directory.Exists(dossierMax))    Directory.CreateDirectory(dossierMax);
                if (!Directory.Exists(dossierStable)) Directory.CreateDirectory(dossierStable);

                try
                {
                    string fileMin    = Path.Combine(dossierMin,    $"test_minimum_{timestamp}.csv");
                    string fileMax    = Path.Combine(dossierMax,    $"test_maximum_{timestamp}.csv");
                    string fileStable = Path.Combine(dossierStable, $"test_stable_{timestamp}.csv");

                    ExportCsv(fileMin,    pointsMin);
                    ExportCsv(fileMax,    pointsMax);
                    ExportCsv(fileStable, pointsStable);

                    MessageBox.Show(
                        $"✓ Export réussi !\n\n" +
                        $"minimum/  → {pointsMin.Count} points\n" +
                        $"maximum/  → {pointsMax.Count} points\n" +
                        $"stable/   → {pointsStable.Count} points\n\n" +
                        $"Dossier : {racine}",
                        "Export terminé",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'export : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ExportCsv(string filePath, List<LidarPoint> points)
        {
            using (var sw = new StreamWriter(filePath))
            {
                sw.WriteLine("Angle;Distance;X;Y;SignalStrength;Timestamp");
                foreach (var p in points)
                    sw.WriteLine($"{p.Angle};{p.Distance};{p.X};{p.Y};{p.SignalStrength};{p.Timestamp}");
            }
        }
    }
}
