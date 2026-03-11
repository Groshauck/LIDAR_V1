using ScottPlot;
using System;
using System.Collections.Generic;
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

        public TestResultForm(List<List<LidarPoint>> allScans, int testDurationSeconds = 30)
        {
            this.allScans = allScans;
            this.testDurationSeconds = testDurationSeconds;
            InitializeUI();
            PlotResults();
        }

        private void InitializeUI()
        {
            this.Text = $"Résultats Test {testDurationSeconds}s — Distance min par angle";
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
        }

        private void PlotResults()
        {
            if (allScans == null || allScans.Count == 0)
            {
                lblInfo.Text = "Aucun scan enregistré.";
                return;
            }

            plotResult.Plot.Clear();

            // ========== COURBE 1 : Distance min globale par angle ==========
            var allPoints = allScans.SelectMany(scan => scan).ToList();

            var groupedByAngle = allPoints
                .GroupBy(p => Math.Round(p.Angle / AnglePrecision, MidpointRounding.AwayFromZero) * AnglePrecision)
                .OrderBy(g => g.Key)
                .ToList();

            var anglesGlobal = groupedByAngle.Select(g => g.Key).ToArray();
            var distMinGlobal = groupedByAngle.Select(g => g.Min(p => p.Distance)).ToArray();

            var scatter1 = plotResult.Plot.Add.Scatter(anglesGlobal, distMinGlobal);
            scatter1.Color = ScottPlot.Colors.Red;
            scatter1.MarkerSize = 3;
            scatter1.LineWidth = 1.5f;
            scatter1.LegendText = "Distance min globale";

            // ========== COURBE 2 : Moyenne des distances min par scan, par angle ==========
            var angleSet = new SortedSet<double>(anglesGlobal);
            var distMinParScanParAngle = new Dictionary<double, List<double>>();

            foreach (var angle in angleSet)
                distMinParScanParAngle[angle] = new List<double>();

            foreach (var scan in allScans)
            {
                var scanGrouped = scan
                    .GroupBy(p => Math.Round(p.Angle / AnglePrecision, MidpointRounding.AwayFromZero) * AnglePrecision)
                    .ToDictionary(g => g.Key, g => g.Min(p => p.Distance));

                foreach (var angle in angleSet)
                {
                    if (scanGrouped.ContainsKey(angle))
                        distMinParScanParAngle[angle].Add(scanGrouped[angle]);
                }
            }

            var anglesMoy = distMinParScanParAngle.Keys.OrderBy(a => a).ToArray();
            var distMoy = anglesMoy
                .Select(a => distMinParScanParAngle[a].Count > 0 ? distMinParScanParAngle[a].Average() : double.NaN)
                .ToArray();

            var validIndices = anglesMoy
                .Select((a, i) => i)
                .Where(i => !double.IsNaN(distMoy[i]))
                .ToArray();

            var anglesFiltered = validIndices.Select(i => anglesMoy[i]).ToArray();
            var distMoyFiltered = validIndices.Select(i => distMoy[i]).ToArray();

            var scatter2 = plotResult.Plot.Add.Scatter(anglesFiltered, distMoyFiltered);
            scatter2.Color = ScottPlot.Colors.Blue;
            scatter2.MarkerSize = 3;
            scatter2.LineWidth = 1.5f;
            scatter2.LegendText = "Moyenne dist. min / scan";

            // ========== AXES & LÉGENDE ==========
            plotResult.Plot.Axes.Bottom.Label.Text = "Angle (°)";
            plotResult.Plot.Axes.Bottom.Label.Bold = true;
            plotResult.Plot.Axes.Left.Label.Text = "Distance (mm)";
            plotResult.Plot.Axes.Left.Label.Bold = true;

            plotResult.Plot.ShowLegend();
            plotResult.Plot.Title($"Test {testDurationSeconds}s — {allScans.Count} scans enregistrés");

            plotResult.Plot.Grid.MajorLineColor = ScottPlot.Colors.Gray.WithAlpha(0.3);

            plotResult.Plot.Axes.AutoScale();
            plotResult.Refresh();

            lblInfo.Text = $"Scans enregistrés : {allScans.Count}  |  Points totaux : {allPoints.Count}  |  Angles couverts : {anglesGlobal.Length}";
        }
    }
}
