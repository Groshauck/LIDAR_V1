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

            foreach (var group in groupedByAngle)
            {
                var ptMin = group.OrderBy(p => p.Distance).First();
                var ptMax = group.OrderByDescending(p => p.Distance).First();

                xsMin.Add(ptMin.X);
                ysMin.Add(ptMin.Y);
                xsMax.Add(ptMax.X);
                ysMax.Add(ptMax.Y);
            }

            // ========== COURBE VERTE : Distance minimale par angle ==========
            var scatterMin = plotResult.Plot.Add.Scatter(xsMin.ToArray(), ysMin.ToArray());
            scatterMin.Color = ScottPlot.Colors.Green;
            scatterMin.MarkerSize = 4;
            scatterMin.LineWidth = 0;
            scatterMin.LegendText = "Position à dist. min / angle";

            // ========== COURBE ROUGE : Distance maximale par angle ==========
            var scatterMax = plotResult.Plot.Add.Scatter(xsMax.ToArray(), ysMax.ToArray());
            scatterMax.Color = ScottPlot.Colors.Red;
            scatterMax.MarkerSize = 4;
            scatterMax.LineWidth = 0;
            scatterMax.LegendText = "Position à dist. max / angle";

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

            lblInfo.Text = $"Scans enregistrés : {allScans.Count}  |  Points totaux : {allPoints.Count}  |  Angles couverts : {groupedByAngle.Count}";
        }
    }
}
