using MathNet.Numerics;
using Microsoft.ML;
using Microsoft.ML.Data;
using ScottPlot;
using ScottPlot.Colormaps;
using ScottPlot.Plottables;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;


namespace WinFormsApp1
{

    public partial class Courbe : Form
    {
        private Microsoft.ML.ITransformer mlModel;
        private Microsoft.ML.PredictionEngine<LidarSummaryFeatures, LidarPrediction> mlPredictionEngine;
        // Pour accumuler les features annotés
        private List<LidarSummaryFeatures> allFeatures = new List<LidarSummaryFeatures>();
        private int currentNuageIndex = 0;
        private List<LidarPoint> points = new List<LidarPoint>();
        private LidarUdpReceiver lidarReceiver;
        private Affichage affichage;
        private volatile bool lectureEnCours = false;
        private System.Windows.Forms.ToolTip toolTip = new System.Windows.Forms.ToolTip();

        public object Properties { get; private set; }

        public static class AffichageSettingsManager
        {
            private static readonly string Chemin = "affichage.json"; // ou ailleurs si tu veux

            public static Affichage Charger()
            {
                if (File.Exists(Chemin))
                    return JsonSerializer.Deserialize<Affichage>(File.ReadAllText(Chemin)) ?? new Affichage();
                return new Affichage();
            }
            public static void Sauvegarder(Affichage a)
            {
                string json = JsonSerializer.Serialize(a, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(Chemin, json);
            }
        }

        public Courbe()
        {
            InitializeComponent();
            CourbeLidar1.MouseMove += CourbeLidar1_MouseMove;

            this.Load += new System.EventHandler(this.Courbe_Load);
        }

        private void Courbe_Load(object sender, EventArgs e)
        {

            affichage = AffichageSettingsManager.Charger();
            // Initialise les contrôles avec les valeurs chargées
            // NumericUpDowns génériques :
            numericXmin.Value = (decimal)affichage.xMin;
            numericXmax.Value = (decimal)affichage.xMax;
            numericYmin.Value = (decimal)affichage.yMin;
            numericYmax.Value = (decimal)affichage.yMax;

            numericXminPlastic.Value = (decimal)affichage.numericXminPlastic;
            numericXmaxPlastic.Value = (decimal)affichage.numericXmaxPlastic;
            numericYminPlastic.Value = (decimal)affichage.numericYminPlastic;
            numericYmaxPlastic.Value = (decimal)affichage.numericYmaxPlastic;

            NumericSignalStrengthMinFilet.Value = (decimal)affichage.FiltrefiletMin;
            NumericSignalStrengthMaxFilet.Value = (decimal)affichage.FiltrefiletMax;

            NumericSignalStrengthMinPlastic.Value = (decimal)affichage.FiltreplasticMin;
            NumericSignalStrengthMaxPlastic.Value = (decimal)affichage.FiltreplasticMax;

            NumericSignalStrengthMinPoulet.Value = (decimal)affichage.FiltrepouletMin;
            NumericSignalStrengthMaxPoulet.Value = (decimal)affichage.FiltrepouletMax;

            NumericAngleMin.Value = (decimal)affichage.AngleMin;
            NumericAngleMax.Value = (decimal)affichage.AngleMax;
            NumericSignalStrengthMin.Value = (decimal)affichage.SignalStrengthMin;
            NumericSignalStrengthMax.Value = (decimal)affichage.SignalStrengthMax;
            numericNbPointMin.Value = (decimal)affichage.numericNbPointMin;
            NumericSignalStrengthCoeff.Value = (decimal)affichage.SignalStrengthCoeff;
            NumericSignalStrengthCenterSingularity.Value = (decimal)affichage.SignalStrengthCenterSingularity;

            // CheckBox
            checkBoxAutoScale.Checked = affichage.auto;
            checkBoxFilterPoulet.Checked = affichage.FilterPoulet;
            checkBoxFilterPlastic.Checked = affichage.FilterPlastic;
            checkBoxFilterFilet.Checked = affichage.FilterFilet;
            checkBoxColorAuto.Checked = affichage.ColorAuto;
            checkBoxSignalStrengthAuto.Checked = affichage.SignalStrengthAuto;
            checkBoxSignalStrengthCoeff.Checked = affichage.checkBoxSignalStrengthCoeff;

            affichage.SignalStrengthXPlage = 600;
            textBoxCheminExport.Text = affichage.textBoxCheminExport;


            double anglemin = (double)NumericAngleMin.Value;
            lidarReceiver = new LidarUdpReceiver();
            lidarReceiver.AngleMin = affichage.AngleMin;
            lidarReceiver.AngleMax = affichage.AngleMax;
            lidarReceiver.CheckBoxSignalStrengthCoeff = affichage.checkBoxSignalStrengthCoeff;
            lidarReceiver.SignalStrengthCoeff = (float)affichage.SignalStrengthCoeff;
            lidarReceiver.SignalStrengthCenterSingularity = affichage.SignalStrengthCenterSingularity;
            lidarReceiver.SignalStrengthXPlage = affichage.SignalStrengthXPlage;
            NumericAngleMin.ValueChanged += NumericAngleMin_ValueChanged;
            NumericAngleMax.ValueChanged += NumericAngleMax_ValueChanged;

            lidarReceiver.OnNewScan += pts =>
            {
                if (!this.IsHandleCreated) return;
                BeginInvoke((Action)(() =>
                {
                    points = pts;
                    if (lectureEnCours)
                    {
                        PlotCourbe();
                    }
                }));
            };
            lidarReceiver.Start();
        }

        private void PlotCourbe()
        {
            if (points.Count == 0) return;

            CourbeLidar1.Plot.Clear();

            // Extraire les données des points
            var xs = points.Select(p => p.X).ToArray();
            var ys = points.Select(p => p.Y).ToArray();
            var sigs = points.Select(p => (double)p.SignalStrength).ToArray();


            // Filtrer les points où signal
            var Filtrefilet = points.Where(p => p.SignalStrength >= affichage.FiltrefiletMin && p.SignalStrength <= affichage.FiltrefiletMax).ToList();
            var Filtreplastic = points.Where(p => p.SignalStrength >= affichage.FiltreplasticMin && p.SignalStrength <= affichage.FiltreplasticMax).ToList();
            var FiltrePoulet = points.Where(p => p.SignalStrength >= affichage.FiltrepouletMin && p.SignalStrength <= affichage.FiltrepouletMax).ToList();
            var FiltreplasticX = Filtreplastic.Where(p => p.X >= affichage.numericXminPlastic && p.X <= affichage.numericYmaxPlastic).ToList();
            var FiltreplasticXY = FiltreplasticX.Where(p => p.Y >= affichage.numericYminPlastic && p.Y <= affichage.numericYmaxPlastic).ToList();
            // Protections sur les signaux
            if (sigs.Length == 0) return;

            double minS;
            double maxS;



            if (!affichage.SignalStrengthAuto)
            {
                minS = (double)affichage.SignalStrengthMin;
                maxS = (double)affichage.SignalStrengthMax;
                NumericSignalStrengthMin.Enabled = true;
                NumericSignalStrengthMax.Enabled = true;
            }
            else
            {
                minS = sigs.Min();
                maxS = sigs.Max();
                NumericSignalStrengthMin.Value = (decimal)minS;
                NumericSignalStrengthMax.Value = (decimal)maxS;
                NumericSignalStrengthMin.Enabled = false;
                NumericSignalStrengthMax.Enabled = false;
            }
            //Prendre la colormap ScottPlot(Turbo)
            var cmap = new ScottPlot.Colormaps.Turbo();

            //Génération du tableau de couleurs selon SignalStrength
            var colors = sigs.Select(s =>
            {
                double t = (s - minS) / Math.Max(1, maxS - minS);
                return cmap.GetColor(t);
            }).ToArray();

            //Ajoute la courbe principale en rouge (invisible si ColorAuto)
            CourbeLidar1.Plot.Add.Scatter(xs, ys).Color = ScottPlot.Colors.Red;

            //Ajoute chaque point, chacun avec sa couleur, et règle le marker
            if (affichage.ColorAuto)
            {
                for (int i = 0; i < xs.Length; i++)
                {
                    var scatter = CourbeLidar1.Plot.Add.Scatter(
                        new double[] { xs[i] },
                        new double[] { ys[i] },
                        color: colors[i]
                    );
                    scatter.MarkerSize = 7;
                    scatter.LineWidth = 0; // Pas de ligne entre les points
                    scatter.MarkerShape = ScottPlot.MarkerShape.FilledCircle;
                }
                ;
            }


            if (mlPredictionEngine != null)
            {
                var features = ExtractSummarySuperFeatures(points);
                try
                {
                    var prediction = mlPredictionEngine.Predict(features);

                    string label = prediction.Prediction ? "Caisse NOK" : "Caisse OK";
                    float confiance = prediction.Score;

                    EtatCaisseIA.Text = label;

                    //MessageBox.Show($"Prédiction:  {label}\nConfiance: {confiance:P2}");
                }
                catch (Exception ex)
                {
                    //MessageBox.Show($"Erreur prédiction: {ex.Message}");
                }
            }



            if (affichage.FilterFilet)
            {
                CourbeLidar1.Plot.Add.Scatter(Filtrefilet.Select(p => p.X).ToArray(), Filtrefilet.Select(p => p.Y).ToArray()).Color = ScottPlot.Colors.Black;
                if (Filtrefilet.Select(p => p.X).ToArray().Length != 0) CourbeLidar1.Plot.Add.HorizontalLine(Filtrefilet.Select(p => p.Y).ToArray().Average()).Color = ScottPlot.Colors.Black;
            }
            if (affichage.FilterPlastic)
            {
                CourbeLidar1.Plot.Add.Scatter(Filtreplastic.Select(p => p.X).ToArray(), Filtreplastic.Select(p => p.Y).ToArray()).Color = ScottPlot.Colors.Orange;
                if (Filtreplastic.Select(p => p.X).ToArray().Length != 0)
                {
                    if (FiltreplasticXY.Count > 0)
                    {
                        double moyPlastic = FiltreplasticXY.Select(p => p.Y).ToArray().Average();
                        CourbeLidar1.Plot.Add.HorizontalLine(moyPlastic).Color = ScottPlot.Colors.Orange;
                        if (moyPlastic > affichage.numericYminPlastic && moyPlastic < affichage.numericYmaxPlastic && FiltreplasticXY.Count > affichage.numericNbPointMin)
                        {
                            EtatCaisse.ForeColor = System.Drawing.Color.Red;
                            EtatCaisse.Text = "Caisse NOK";
                        }
                        else
                        {
                            EtatCaisse.ForeColor = System.Drawing.Color.Green;
                            EtatCaisse.Text = "Caisse OK";
                        }
                    }
                    else
                    {
                        EtatCaisse.ForeColor = System.Drawing.Color.Green;
                        EtatCaisse.Text = "Caisse OK";
                    }
                }
            }
            if (affichage.FilterPoulet)
            {
                CourbeLidar1.Plot.Add.Scatter(FiltrePoulet.Select(p => p.X).ToArray(), FiltrePoulet.Select(p => p.Y).ToArray()).Color = ScottPlot.Colors.Brown;
                if (FiltrePoulet.Select(p => p.X).ToArray().Length != 0) CourbeLidar1.Plot.Add.HorizontalLine(FiltrePoulet.Select(p => p.Y).ToArray().Average()).Color = ScottPlot.Colors.Brown;
            }

            CourbeLidar1.Plot.Add.HorizontalLine(affichage.numericYmaxPlastic).Color = ScottPlot.Colors.Orange;
            CourbeLidar1.Plot.Add.HorizontalLine(affichage.numericYminPlastic).Color = ScottPlot.Colors.Orange;
            CourbeLidar1.Plot.Add.VerticalLine(affichage.numericXmaxPlastic).Color = ScottPlot.Colors.Orange;
            CourbeLidar1.Plot.Add.VerticalLine(affichage.numericXminPlastic).Color = ScottPlot.Colors.Orange;

            double moy = ys.Average();
            CourbeLidar1.Plot.Add.HorizontalLine(moy).Color = ScottPlot.Colors.DarkBlue;


            if (affichage.auto)
            {
                CourbeLidar1.Plot.Axes.AutoScale();
            }
            else
            {
                CourbeLidar1.Plot.Axes.SetLimits(affichage.xMin, affichage.xMax, affichage.yMin, affichage.yMax);
            }
            CourbeLidar1.Refresh();
        }

        private void CourbeLidar_Load(object sender, EventArgs e)
        {

        }
        private void NumericAngleMin_ValueChanged(object sender, EventArgs e)
        {
            affichage.AngleMin = (double)NumericAngleMin.Value;
            if (lidarReceiver != null) lidarReceiver.AngleMin = (double)NumericAngleMin.Value;
        }
        private void NumericAngleMax_ValueChanged(object sender, EventArgs e)
        {
            affichage.AngleMax = (double)NumericAngleMax.Value;
            if (lidarReceiver != null) lidarReceiver.AngleMax = (double)NumericAngleMax.Value;
        }

        private void checkBoxSignalStrengthAuto_CheckedChanged(object sender, EventArgs e)
        {
            affichage.SignalStrengthAuto = (bool)checkBoxSignalStrengthAuto.Checked;
        }

        private void NumericSignalStrengthMin_ValueChanged(object sender, EventArgs e)
        {
            affichage.SignalStrengthMin = (double)NumericSignalStrengthMin.Value;
        }

        private void NumericSignalStrengthMax_ValueChanged(object sender, EventArgs e)
        {
            affichage.SignalStrengthMax = (double)NumericSignalStrengthMax.Value;
        }

        private void checkBoxAutoScale_CheckedChanged(object sender, EventArgs e)
        {
            affichage.auto = (bool)checkBoxAutoScale.Checked;
        }

        private void numericXmin_ValueChanged(object sender, EventArgs e)
        {
            affichage.xMin = (double)numericXmin.Value;
        }

        private void numericXmax_ValueChanged(object sender, EventArgs e)
        {
            affichage.xMax = (double)numericXmax.Value;
        }

        private void numericYmin_ValueChanged(object sender, EventArgs e)
        {
            affichage.yMin = (double)numericYmin.Value;
        }

        private void numericYmax_ValueChanged(object sender, EventArgs e)
        {
            affichage.yMax = (double)numericYmax.Value;
        }

        private void NumericSignalStrengthMinPoulet_ValueChanged(object sender, EventArgs e)
        {
            affichage.FiltrepouletMin = (double)NumericSignalStrengthMinPoulet.Value;
        }
        private void NumericSignalStrengthMaxPoulet_ValueChanged(object sender, EventArgs e)
        {
            affichage.FiltrepouletMax = (double)NumericSignalStrengthMaxPoulet.Value;
        }
        private void NumericSignalStrengthMinPlastic_ValueChanged(object sender, EventArgs e)
        {
            affichage.FiltreplasticMin = (double)NumericSignalStrengthMinPlastic.Value;
        }
        private void NumericSignalStrengthMaxPlastic_ValueChanged(object sender, EventArgs e)
        {
            affichage.FiltreplasticMax = (double)NumericSignalStrengthMaxPlastic.Value;
        }
        private void NumericSignalStrengthMinFilet_ValueChanged(object sender, EventArgs e)
        {
            affichage.FiltrefiletMin = (double)NumericSignalStrengthMinFilet.Value;
        }
        private void NumericSignalStrengthMaxFilet_ValueChanged(object sender, EventArgs e)
        {
            affichage.FiltrefiletMax = (double)NumericSignalStrengthMaxFilet.Value;
        }
        private void checkBoxFilterPoulet_CheckedChanged(object sender, EventArgs e)
        {
            affichage.FilterPoulet = (bool)checkBoxFilterPoulet.Checked;
        }
        private void checkBoxFilterPlastic_CheckedChanged(object sender, EventArgs e)
        {
            affichage.FilterPlastic = (bool)checkBoxFilterPlastic.Checked;
        }
        private void checkBoxFilterFilet_CheckedChanged(object sender, EventArgs e)
        {
            affichage.FilterFilet = (bool)checkBoxFilterFilet.Checked;
        }
        private void checkBoxColorAuto_CheckedChanged(object sender, EventArgs e)
        {
            affichage.ColorAuto = (bool)checkBoxColorAuto.Checked;
        }

        private void CourbeLidar1_MouseMove(object sender, MouseEventArgs e)
        {
            // Obtenir coordonnées souris en axes ScottPlot
            var mouseCoords = CourbeLidar1.Plot.GetCoordinates(e.X, e.Y);
            double mouseCoordX = mouseCoords.X;
            double mouseCoordY = mouseCoords.Y;

            // Recherche du point le plus proche
            double seuilPixel = 10; // distance en pixels pour déclencher le tooltip
            int closestIndex = -1;
            double minDist = double.MaxValue;

            // Suppose points est ta liste de LidarPoint
            var xAxis = CourbeLidar1.Plot.Axes.Bottom;
            var yAxis = CourbeLidar1.Plot.Axes.Left;

            for (int i = 0; i < points.Count; i++)
            {
                var coord = new ScottPlot.Coordinates(points[i].X, points[i].Y);
                ScottPlot.Pixel pixel = CourbeLidar1.Plot.GetPixel(coord, xAxis, yAxis);
                double dpix = Math.Sqrt(Math.Pow(pixel.X - e.X, 2) + Math.Pow(pixel.Y - e.Y, 2));

                if (dpix < seuilPixel && dpix < minDist)
                {
                    minDist = dpix;
                    closestIndex = i;
                }
            }

            if (closestIndex != -1)
            {
                // Affiche le tooltip (avec infos personnalisées)
                var pt = points[closestIndex];
                string txt = $"X = {pt.X:0.##}\nY = {pt.Y:0.##}\nAngle = {pt.Angle:0.##}\nDistance = {pt.Distance:0.##}\nSignal = {pt.SignalStrength}";
                toolTip.Show(txt, CourbeLidar1, e.X + 10, e.Y + 10, 1000); // 1000 ms = 1s d'affichage
            }
            else
            {
                toolTip.Hide(CourbeLidar1);
            }
        }

        private void buttonPlayStop_Click(object sender, EventArgs e)
        {
            if (!lectureEnCours)
            {
                // --- PLAY ---
                lectureEnCours = true;
                buttonPlayStop.Text = "Stop";
                buttonPlayStop.BackColor = System.Drawing.Color.Red;

            }
            else
            {
                // --- STOP ---
                lectureEnCours = false;
                buttonPlayStop.Text = "Play";
                buttonPlayStop.BackColor = System.Drawing.Color.Lime;

            }
        }
        private void BoutonEnregistrerCsv_Click(object sender, EventArgs e)
        {
            // Récupère le chemin depuis la TextBox
            string filePath = textBoxCheminExport.Text.Trim();

            // Vérification du chemin
            if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("Veuillez entrer un chemin valide !", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ajouter le nom du fichier
            filePath = Path.Combine(filePath, $"mes_points_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

            // Vérification du répertoire
            string directory = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(directory))
            {
                directory = Environment.CurrentDirectory;
                filePath = Path.Combine(directory, filePath);
            }

            if (!Directory.Exists(directory))
            {
                MessageBox.Show($"Le répertoire '{directory}' n'existe pas !", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (StreamWriter sw = new StreamWriter(filePath))
                {
                    // En-tête CSV
                    sw.WriteLine("Angle,Distance,X,Y,SignalStrength,Timestamp");
                    // Chaque point sur une ligne
                    foreach (var p in points)
                        sw.WriteLine($"{p.Angle};{p.Distance};{p.X};{p.Y};{p.SignalStrength};{p.Timestamp}");
                }
                MessageBox.Show($"Export CSV réussi !\n{filePath}", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de l'export : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void TextBoxCheminExport_DoubleClick(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Sélectionnez le dossier de destination";
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    //string fileName = $"mes_points_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    textBoxCheminExport.Text = Path.Combine(fbd.SelectedPath);
                }
            }
        }


        private void buttonChargerCSV_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Fichiers CSV (*.csv)|*.csv";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        List<LidarPoint> points = new List<LidarPoint>();

                        using (StreamReader sr = new StreamReader(ofd.FileName))
                        {
                            string? line;
                            bool isFirstLine = true;
                            while ((line = sr.ReadLine()) != null)
                            {
                                if (isFirstLine) // Sauter l’en-tête CSV
                                {
                                    isFirstLine = false;
                                    continue;
                                }
                                // Si séparateur = ';' adapte le Split !
                                var parts = line.Split(';');
                                if (parts.Length < 3) continue;
                                double a, d, x, y, s;
                                if (double.TryParse(parts[0], out a)
                                    && double.TryParse(parts[1], out d)
                                    && double.TryParse(parts[2], out x)
                                    && double.TryParse(parts[3], out y)
                                    && double.TryParse(parts[4], out s))
                                {
                                    points.Add(new LidarPoint { Angle = a, Distance = d, X = x, Y = y, SignalStrength = s });
                                }
                            }
                        }

                        // Tu as chargé tes points
                        // -> Stocke la liste où tu veux :
                        this.points = points; // ou autre copie de référence
                        PlotCourbe();
                        MessageBox.Show("Chargement CSV terminé", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // (Optionnel) Mettre à jour le graphique si besoin :
                        // RedessineCourbe();

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Erreur lors du chargement : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void buttonGenererDataset_Click(object sender, EventArgs e)
        {
            // ✅ Sélectionner le dossier "Déboîté"
            string folderDeboite = SelectFolder("Sélectionnez le dossier contenant les données DÉBOÎTÉES (Label = True)");
            if (string.IsNullOrEmpty(folderDeboite)) return;

            // ✅ Sélectionner le dossier "Non déboîté"
            string folderNormal = SelectFolder("Sélectionnez le dossier contenant les données NORMALES (Label = False)");
            if (string.IsNullOrEmpty(folderNormal)) return;

            try
            {
                string outputPath = Path.Combine(Environment.CurrentDirectory, "mon_dataset.csv");

                var csvFilesDeboite = Directory.GetFiles(folderDeboite, "*.csv");
                var csvFilesNormal = Directory.GetFiles(folderNormal, "*.csv");

                if (csvFilesDeboite.Length == 0 && csvFilesNormal.Length == 0)
                {
                    MessageBox.Show("Aucun fichier CSV trouvé dans les dossiers !", "Avertissement", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show($"Fusion de {csvFilesDeboite.Length} fichiers DÉBOÎTÉS + {csvFilesNormal.Length} fichiers NORMAUX.. .", "En cours", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Fusionner avec labels différents
                FusionnerCSVAvecLabels(csvFilesDeboite, csvFilesNormal, outputPath);

                MessageBox.Show($"✓ Dataset généré avec succès !\n\nFichier:  {outputPath}\n\nDéboîtés (Label=1): {csvFilesDeboite.Length}\nNormaux (Label=0): {csvFilesNormal.Length}", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la génération du dataset :\n{ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void FusionnerCSVAvecLabels(string[] csvFilesDeboite, string[] csvFilesNormal, string outputPath)
        {
            using (StreamWriter sw = new StreamWriter(outputPath))
            {
                // ✅ En-tête du CSV
                sw.WriteLine("Label;NbPoints;XMin;XMax;YMin;YMax;XSpan;YSpan;XMoy;YMoy;XStd;YStd;SignalStrengthMoyen;SignalStrengthStd;SuperX1;SuperX2;SuperX3;SuperX4;SuperX5;SuperX6;SuperX7;SuperX8;SuperX9;SuperX10;SuperY1;SuperY2;SuperY3;SuperY4;SuperY5;SuperY6;SuperY7;SuperY8;SuperY9;SuperY10;SuperSignal1;SuperSignal2;SuperSignal3;SuperSignal4;SuperSignal5;SuperSignal6;SuperSignal7;SuperSignal8;SuperSignal9;SuperSignal10");

                int totalLinesDeboite = 0;
                int totalLinesNormal = 0;

                // ✅ Traiter les fichiers DÉBOÎTÉS (Label = 1)
                System.Diagnostics.Debug.WriteLine("📦 Traitement des fichiers DÉBOÎTÉS (Label = 1)");
                foreach (var csvFile in csvFilesDeboite)
                {
                    try
                    {
                        totalLinesDeboite += TraiterFichierCSV(sw, csvFile, label: true); // Label = 1
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Erreur lecture {csvFile}: {ex.Message}");
                    }
                }

                // ✅ Traiter les fichiers NORMAUX (Label = 0)
                System.Diagnostics.Debug.WriteLine("📦 Traitement des fichiers NORMAUX (Label = 0)");
                foreach (var csvFile in csvFilesNormal)
                {
                    try
                    {
                        totalLinesNormal += TraiterFichierCSV(sw, csvFile, label: false); // Label = 0
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Erreur lecture {csvFile}: {ex.Message}");
                    }
                }

                System.Diagnostics.Debug.WriteLine($"✓ Déboîtés: {totalLinesDeboite} lignes");
                System.Diagnostics.Debug.WriteLine($"✓ Normaux: {totalLinesNormal} lignes");
                System.Diagnostics.Debug.WriteLine($"✓ Total: {totalLinesDeboite + totalLinesNormal} lignes fusionnées");
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

        private void buttonDeboite_Click(object sender, EventArgs e)
        {
            AjouteAvecLabel(true);

        }
        private void buttonNormal_Click(object sender, EventArgs e)
        {
            AjouteAvecLabel(false);
        }

        private void AjouteAvecLabel(bool label)
        {
            if (points.Count > 0)
            {
                {
                    var features = ExtractSummarySuperFeatures(points);
                    features.Label = label;
                    allFeatures.Add(features);
                    currentNuageIndex++;

                    // affiche la courbe suivante, etc.
                    // Si fin, signaler ou désactiver les boutons
                }
            }
        }

        private void buttonSaveDataIA_Click(object sender, EventArgs e)
        {
            //ExportFeaturesToCsv(allFeatures, "mon_dataset.csv");
            //MessageBox.Show("Export terminé !");
        }

        private LidarSummaryFeatures ExtractSummarySuperFeatures(List<LidarPoint> points)
        {
            var features = new LidarSummaryFeatures();
            features.NbPoints = points.Count;

            if (points.Count == 0)
            {
                // Initialiser tous les champs à 0 pour éviter les valeurs null
                features.XMin = features.XMax = features.YMin = features.YMax = 0;
                features.XSpan = features.YSpan = 0;
                features.XMoy = features.YMoy = 0;
                features.XStd = features.YStd = 0;
                features.SignalStrengthMoyen = features.SignalStrengthStd = 0;

                // Initialiser tous les super-points à 0
                features.SuperX1 = features.SuperX2 = features.SuperX3 = features.SuperX4 = features.SuperX5 = 0;
                features.SuperX6 = features.SuperX7 = features.SuperX8 = features.SuperX9 = features.SuperX10 = 0;
                features.SuperY1 = features.SuperY2 = features.SuperY3 = features.SuperY4 = features.SuperY5 = 0;
                features.SuperY6 = features.SuperY7 = features.SuperY8 = features.SuperY9 = features.SuperY10 = 0;
                features.SuperSignal1 = features.SuperSignal2 = features.SuperSignal3 = features.SuperSignal4 = features.SuperSignal5 = 0;
                features.SuperSignal6 = features.SuperSignal7 = features.SuperSignal8 = features.SuperSignal9 = features.SuperSignal10 = 0;

                return features;
            }

            features.XMin = (float)points.Min(p => p.X);
            features.XMax = (float)points.Max(p => p.X);
            features.YMin = (float)points.Min(p => p.Y);
            features.YMax = (float)points.Max(p => p.Y);

            features.XSpan = features.XMax - features.XMin;
            features.YSpan = features.YMax - features.YMin;

            features.XMoy = (float)points.Average(p => p.X);
            features.YMoy = (float)points.Average(p => p.Y);

            features.XStd = (float)Math.Sqrt(points.Average(p => Math.Pow(p.X - features.XMoy, 2)));
            features.YStd = (float)Math.Sqrt(points.Average(p => Math.Pow(p.Y - features.YMoy, 2)));

            features.SignalStrengthMoyen = (float)points.Average(p => p.SignalStrength);
            double signalMean = features.SignalStrengthMoyen;
            features.SignalStrengthStd = (float)Math.Sqrt(points.Average(p => Math.Pow(p.SignalStrength - signalMean, 2)));

            // ==== 10 super-points APLATIS ====
            var sorted = points.OrderBy(p => p.Angle).ToList();
            int n = sorted.Count / 10;

            float[] superX = new float[10];
            float[] superY = new float[10];
            float[] superSignal = new float[10];

            for (int i = 0; i < 10; i++)
            {
                int start = i * n;
                int end = (i == 9) ? sorted.Count : (i + 1) * n;
                if (start < end)
                {
                    var segment = sorted.Skip(start).Take(end - start).ToList();
                    superX[i] = (float)segment.Average(p => p.X);
                    superY[i] = (float)segment.Average(p => p.Y);
                    superSignal[i] = (float)segment.Average(p => p.SignalStrength);
                }
                else
                {
                    superX[i] = (float)sorted.Last().X;
                    superY[i] = (float)sorted.Last().Y;
                    superSignal[i] = (float)sorted.Last().SignalStrength;
                }
            }

            // Assigner SuperX1-10 (indice 0-9 du tableau)
            features.SuperX1 = superX[0];
            features.SuperX2 = superX[1];
            features.SuperX3 = superX[2];
            features.SuperX4 = superX[3];
            features.SuperX5 = superX[4];
            features.SuperX6 = superX[5];
            features.SuperX7 = superX[6];
            features.SuperX8 = superX[7];
            features.SuperX9 = superX[8];
            features.SuperX10 = superX[9];

            // Assigner SuperY1-10
            features.SuperY1 = superY[0];
            features.SuperY2 = superY[1];
            features.SuperY3 = superY[2];
            features.SuperY4 = superY[3];
            features.SuperY5 = superY[4];
            features.SuperY6 = superY[5];
            features.SuperY7 = superY[6];
            features.SuperY8 = superY[7];
            features.SuperY9 = superY[8];
            features.SuperY10 = superY[9];

            // Assigner SuperSignal1-10
            features.SuperSignal1 = superSignal[0];
            features.SuperSignal2 = superSignal[1];
            features.SuperSignal3 = superSignal[2];
            features.SuperSignal4 = superSignal[3];
            features.SuperSignal5 = superSignal[4];
            features.SuperSignal6 = superSignal[5];
            features.SuperSignal7 = superSignal[6];
            features.SuperSignal8 = superSignal[7];
            features.SuperSignal9 = superSignal[8];
            features.SuperSignal10 = superSignal[9];

            return features;
        }

        private int TraiterFichierCSV(StreamWriter sw, string csvFile, bool label)
        {
            int lineCount = 0;
            var lidarPoints = new List<LidarPoint>();

            using (StreamReader sr = new StreamReader(csvFile))
            {
                string line;
                bool isFirstLine = true;

                while ((line = sr.ReadLine()) != null)
                {
                    // ✅ Sauter l'en-tête du fichier source
                    if (isFirstLine)
                    {
                        isFirstLine = false;
                        continue;
                    }


                    // ✅ Parser la ligne CSV et AJOUTER les points à la liste
                    var points = ParserLigneCSV(line);
                    lidarPoints.AddRange(points); // ✅ Accumule tous les points
                }

                if (lidarPoints.Count > 0)
                {
                    // ✅ Extraire les features résumées
                    var features = ExtractSummarySuperFeatures(lidarPoints);

                    // ✅ Assigner le label correct
                    features.Label = label; // true = déboîté, false = normal

                    // ✅ Écrire les features dans le fichier de sortie
                    EcrireFeatureLigne(sw, features);
                    lineCount++;
                    System.Diagnostics.Debug.WriteLine($"  ✓ {csvFile.Split('\\').Last()}: {lidarPoints.Count} points accumulés → 1 ligne de features");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"  ⚠️  {csvFile.Split('\\').Last()}: Aucun point trouvé");
                }
            }
           
            return lineCount;
        }

        // ✅ Nouvelle méthode pour parser une ligne CSV en LidarPoint
        private List<LidarPoint> ParserLigneCSV(string line)
        {
            var points = new List<LidarPoint>();
            var parts = line.Split(';');

            // Format :  Angle;Distance;X;Y;SignalStrength;Timestamp
            if (parts.Length >= 6)
            {
                if (double.TryParse(parts[0], out double angle) &&
                    double.TryParse(parts[1], out double distance) &&
                    double.TryParse(parts[2], out double x) &&
                    double.TryParse(parts[3], out double y) &&
                    double.TryParse(parts[4], out double signal) &&
                    uint.TryParse(parts[5], out uint timestamp))
                {
                    points.Add(new LidarPoint
                    {
                        Angle = angle,
                        Distance = distance,
                        X = x,
                        Y = y,
                        SignalStrength = signal,
                        Timestamp = timestamp
                    });
                }
            }

            return points;
        }

        // ✅ Nouvelle méthode pour écrire une feature dans le CSV de sortie
        private void EcrireFeatureLigne(StreamWriter sw, LidarSummaryFeatures f)
        {
            var sb = new StringBuilder();

            // Label
            sb.Append($"{(f.Label ? 1 : 0)}");
            sb.Append(";" + f.NbPoints.ToString(CultureInfo.InvariantCulture));
            sb.Append(";" + f.XMin.ToString(CultureInfo.InvariantCulture));
            sb.Append(";" + f.XMax.ToString(CultureInfo.InvariantCulture));
            sb.Append(";" + f.YMin.ToString(CultureInfo.InvariantCulture));
            sb.Append(";" + f.YMax.ToString(CultureInfo.InvariantCulture));
            sb.Append(";" + f.XSpan.ToString(CultureInfo.InvariantCulture));
            sb.Append(";" + f.YSpan.ToString(CultureInfo.InvariantCulture));
            sb.Append(";" + f.XMoy.ToString(CultureInfo.InvariantCulture));
            sb.Append(";" + f.YMoy.ToString(CultureInfo.InvariantCulture));
            sb.Append(";" + f.XStd.ToString(CultureInfo.InvariantCulture));
            sb.Append(";" + f.YStd.ToString(CultureInfo.InvariantCulture));
            sb.Append(";" + f.SignalStrengthMoyen.ToString(CultureInfo.InvariantCulture));
            sb.Append(";" + f.SignalStrengthStd.ToString(CultureInfo.InvariantCulture));

            // SuperX1-10
            sb.Append(";" + f.SuperX1.ToString(CultureInfo.InvariantCulture));
            sb.Append(";" + f.SuperX2.ToString(CultureInfo.InvariantCulture));
            sb.Append(";" + f.SuperX3.ToString(CultureInfo.InvariantCulture));
            sb.Append(";" + f.SuperX4.ToString(CultureInfo.InvariantCulture));
            sb.Append(";" + f.SuperX5.ToString(CultureInfo.InvariantCulture));
            sb.Append(";" + f.SuperX6.ToString(CultureInfo.InvariantCulture));
            sb.Append(";" + f.SuperX7.ToString(CultureInfo.InvariantCulture));
            sb.Append(";" + f.SuperX8.ToString(CultureInfo.InvariantCulture));
            sb.Append(";" + f.SuperX9.ToString(CultureInfo.InvariantCulture));
            sb.Append(";" + f.SuperX10.ToString(CultureInfo.InvariantCulture));

            // SuperY1-10
            sb.Append(";" + f.SuperY1.ToString(CultureInfo.InvariantCulture));
            sb.Append(";" + f.SuperY2.ToString(CultureInfo.InvariantCulture));
            sb.Append(";" + f.SuperY3.ToString(CultureInfo.InvariantCulture));
            sb.Append(";" + f.SuperY4.ToString(CultureInfo.InvariantCulture));
            sb.Append(";" + f.SuperY5.ToString(CultureInfo.InvariantCulture));
            sb.Append(";" + f.SuperY6.ToString(CultureInfo.InvariantCulture));
            sb.Append(";" + f.SuperY7.ToString(CultureInfo.InvariantCulture));
            sb.Append(";" + f.SuperY8.ToString(CultureInfo.InvariantCulture));
            sb.Append(";" + f.SuperY9.ToString(CultureInfo.InvariantCulture));
            sb.Append(";" + f.SuperY10.ToString(CultureInfo.InvariantCulture));

            // SuperSignal1-10
            sb.Append(";" + f.SuperSignal1.ToString(CultureInfo.InvariantCulture));
            sb.Append(";" + f.SuperSignal2.ToString(CultureInfo.InvariantCulture));
            sb.Append(";" + f.SuperSignal3.ToString(CultureInfo.InvariantCulture));
            sb.Append(";" + f.SuperSignal4.ToString(CultureInfo.InvariantCulture));
            sb.Append(";" + f.SuperSignal5.ToString(CultureInfo.InvariantCulture));
            sb.Append(";" + f.SuperSignal6.ToString(CultureInfo.InvariantCulture));
            sb.Append(";" + f.SuperSignal7.ToString(CultureInfo.InvariantCulture));
            sb.Append(";" + f.SuperSignal8.ToString(CultureInfo.InvariantCulture));
            sb.Append(";" + f.SuperSignal9.ToString(CultureInfo.InvariantCulture));
            sb.Append(";" + f.SuperSignal10.ToString(CultureInfo.InvariantCulture));

            sw.WriteLine(sb.ToString());
        }


        private void ButtonTrain_Click(object sender, EventArgs e)
        {

            // ✅ SUPPRIMEZ L'ANCIEN MODÈLE CORRECTEMENT
            string modelPath = Path.Combine(Environment.CurrentDirectory, "model-lidar.zip");
            if (File.Exists(modelPath))
            {
                try
                {
                    File.Delete(modelPath);
                    MessageBox.Show("Ancien modèle supprimé");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Impossible de supprimer l'ancien modèle:  {ex.Message}");
                }
            }

            try
            {
                string dataPath = "mon_dataset.csv";
                var mlContext = new MLContext();

                // ===== Étape 1 : Charger les données =====
                MessageBox.Show("Étape 1 : Chargement CSV...");
                var data = mlContext.Data.LoadFromTextFile<LidarSummaryFeatures>(
                    path: dataPath,
                    hasHeader: true,
                    separatorChar: ';');
                MessageBox.Show("✓ CSV chargé");

                // ===== Étape 2 :  AFFICHER TOUTES LES COLONNES =====
                MessageBox.Show("Étape 2 :  Vérification des colonnes disponibles...");
                var schema = data.Schema;

                string colonnesDisponibles = "Colonnes disponibles:\n\n";
                foreach (var column in schema)
                {
                    colonnesDisponibles += $"- {column.Name} (Type: {column.Type})\n";
                }

                MessageBox.Show(colonnesDisponibles);
                System.Diagnostics.Debug.WriteLine(colonnesDisponibles);

                // ===== Étape 3 : VÉRIFIER LES COLONNES ATTENDUES =====
                MessageBox.Show("Étape 3 : Vérification des colonnes requises...");

                var colonnesATester = new[]
                {
            "Label", "NbPoints", "XMin", "XMax", "YMin", "YMax", "XSpan", "YSpan",
            "XMoy", "YMoy", "XStd", "YStd", "SignalStrengthMoyen", "SignalStrengthStd",
            "SuperX1", "SuperX2", "SuperX3", "SuperX4", "SuperX5", "SuperX6", "SuperX7", "SuperX8", "SuperX9", "SuperX10",
            "SuperY1", "SuperY2", "SuperY3", "SuperY4", "SuperY5", "SuperY6", "SuperY7", "SuperY8", "SuperY9", "SuperY10",
            "SuperSignal1", "SuperSignal2", "SuperSignal3", "SuperSignal4", "SuperSignal5", "SuperSignal6", "SuperSignal7", "SuperSignal8", "SuperSignal9", "SuperSignal10"
        };

                var colonnesDisponiblesListe = new List<string>();
                var colonnesMissing = new List<string>();

                foreach (var colonne in colonnesATester)
                {
                    try
                    {
                        var col = schema[colonne];
                        colonnesDisponiblesListe.Add(colonne);
                        System.Diagnostics.Debug.WriteLine($"✓ {colonne} existe");
                    }
                    catch
                    {
                        colonnesMissing.Add(colonne);
                        System.Diagnostics.Debug.WriteLine($"✗ {colonne} MANQUANTE");
                    }
                }

                if (colonnesMissing.Count > 0)
                {
                    string message = $"⚠️  COLONNES MANQUANTES ({colonnesMissing.Count}):\n\n{string.Join("\n", colonnesMissing)}\n\n" +
                                   $"✓ COLONNES DISPONIBLES ({colonnesDisponiblesListe.Count}):\n\n{string.Join("\n", colonnesDisponiblesListe)}";
                    MessageBox.Show(message);
                    System.Diagnostics.Debug.WriteLine(message);
                }
                else
                {
                    MessageBox.Show("✓ Toutes les colonnes existent !");
                }

                // ===== Étape 4 : Split train/test =====
                MessageBox.Show("Étape 4 : Split train/test...");
                var split = mlContext.Data.TrainTestSplit(data, testFraction: 0.3, seed: 42);
                MessageBox.Show("✓ Split créé");

                // ===== Étape 5 :  Créer le PIPELINE avec TOUTES les features disponibles =====
                MessageBox.Show("Étape 5 : Création pipeline avec " + colonnesDisponiblesListe.Count + " features...");

                try
                {
                    // Retirer "Label" de la liste (c'est pas une feature, c'est la cible)
                    var features = colonnesDisponiblesListe.Where(c => c != "Label").ToArray();


                    // ❌ ANCIEN (faible)
                    //. Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(...))
                    //new Microsoft.ML.Trainers.SdcaLogisticRegressionBinaryTrainer.Options
                    // ✅ MEILLEUR :  Random Forest
                    //.Append(mlContext.BinaryClassification.Trainers.FastForest(
                    //new Microsoft.ML.Trainers.FastForestBinaryTrainer. Options
                    // ✅ MEILLEUR : Gradient Boosting (XGBoost)
                    //.Append(mlContext.BinaryClassification.Trainers.FastTree(
                    //new Microsoft.ML.Trainers.FastTreeBinaryTrainer.Options
                    // ✅ MEILLEUR : LightGBM
                    //.Append(mlContext.BinaryClassification.Trainers.LightGbm(
                    //new Microsoft.ML.Trainers.LightGbm.LightGbmBinaryTrainer.Options



                    var pipeline = mlContext.Transforms.Concatenate("Features", features)
                       .Append(mlContext.Transforms.NormalizeMeanVariance("Features", useCdf: true))
                       .Append(mlContext.BinaryClassification.Trainers.FastForest(
                        labelColumnName: "Label",
                        featureColumnName: "Features",
                        numberOfTrees: 300,              // ✅ Plus d'arbres
                        numberOfLeaves: 50,              // ✅ Feuilles plus profondes
                        minimumExampleCountPerLeaf: 2));  // ✅ Moins strict

                    MessageBox.Show("✓ Pipeline créé avec " + features.Length + " features");

                    // ===== Étape 6 : Entraînement =====
                    MessageBox.Show("Étape 6 : Entraînement...");

                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    var model = pipeline.Fit(split.TrainSet);
                    sw.Stop();

                    MessageBox.Show($"✓ Entraînement réussi en {sw.ElapsedMilliseconds}ms");

                    // Sauvegarde du modèle
                    this.mlModel = model;
                    this.mlPredictionEngine = mlContext.Model.CreatePredictionEngine<LidarSummaryFeatures, LidarPrediction>(model);

                    // ===== Étape 7 : Évaluation =====
                    MessageBox.Show("Étape 7 : Évaluation...");
                    var predictions = model.Transform(split.TestSet);
                    var metrics = mlContext.BinaryClassification.EvaluateNonCalibrated(predictions, "Label");
                    //var metrics = mlContext.BinaryClassification.Evaluate(predictions);

                    string resultats = $"✓ Résultats:\n" +
                        $"Accuracy:   {metrics.Accuracy:P2}\n" +
                        $"AUC:  {metrics.AreaUnderRocCurve:P2}\n" +
                        $"F1: {metrics.F1Score:P2}\n" +
                        $"Precision: {metrics.PositivePrecision:P2}\n" +
                        $"Recall: {metrics.PositiveRecall:P2}";

                    MessageBox.Show(resultats);
                    System.Diagnostics.Debug.WriteLine(resultats);

                    // ===== Étape 8 : Sauvegarde =====
                    MessageBox.Show("Étape 8 : Sauvegarde du modèle.. .");
                    mlContext.Model.Save(model, data.Schema, "model-lidar. zip");
                    MessageBox.Show("✓✓✓ SUCCÈS !  Modèle sauvegardé ✓✓✓");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"✗ Erreur Pipeline:\n\nType: {ex.GetType().Name}\n\nMessage: {ex.Message}\n\nStackTrace: {ex.StackTrace}");
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"✗ Erreur générale:\n{ex}");
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        private void buttonTestIA_Click(object sender, EventArgs e)
        {
            if (mlPredictionEngine == null)
            {
                MessageBox.Show("Modèle non entraîné !");
                return;
            }

            var features = ExtractSummarySuperFeatures(points);

            try
            {
                var prediction = mlPredictionEngine.Predict(features);

                string label = prediction.Prediction ? "Caisse NOK" : "Caisse OK";
                float confiance = prediction.Score;

                MessageBox.Show($"Prédiction:  {label}\nConfiance: {confiance:P2}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur prédiction: {ex.Message}");
            }
        }

        private void numericYmaxPlastic_ValueChanged(object sender, EventArgs e)
        {
            affichage.numericYmaxPlastic = (double)numericYmaxPlastic.Value;

        }

        private void numericYminPlastic_ValueChanged(object sender, EventArgs e)
        {
            affichage.numericYminPlastic = (double)numericYminPlastic.Value;
        }

        private void numericXmaxPlastic_ValueChanged(object sender, EventArgs e)
        {
            affichage.numericXmaxPlastic = (double)numericXmaxPlastic.Value;

        }

        private void numericXminPlastic_ValueChanged(object sender, EventArgs e)
        {
            affichage.numericXminPlastic = (double)numericXminPlastic.Value;

        }

        private void numericNbPointMin_ValueChanged(object sender, EventArgs e)
        {
            affichage.numericNbPointMin = (double)numericNbPointMin.Value;

        }

        private void NumericSignalStrengthCoeff_ValueChanged(object sender, EventArgs e)
        {
            affichage.SignalStrengthCoeff = (float)NumericSignalStrengthCoeff.Value;
            if (lidarReceiver != null) lidarReceiver.SignalStrengthCoeff = (float)NumericSignalStrengthCoeff.Value;

        }

        private void NumericSignalStrengthCenterSingularity_ValueChanged(object sender, EventArgs e)
        {
            affichage.SignalStrengthCenterSingularity = (double)NumericSignalStrengthCenterSingularity.Value;
            if (lidarReceiver != null) lidarReceiver.SignalStrengthCenterSingularity = (double)NumericSignalStrengthCenterSingularity.Value;
        }
        private void checkBoxSignalStrengthCoeff_CheckedChanged(object sender, EventArgs e)
        {
            affichage.checkBoxSignalStrengthCoeff = (bool)checkBoxSignalStrengthCoeff.Checked;
            if (lidarReceiver != null) lidarReceiver.CheckBoxSignalStrengthCoeff = (bool)checkBoxSignalStrengthCoeff.Checked;
        }

       


        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Mets à jour chaque propriété de l'objet affichage avec les valeurs des contrôles
            affichage.xMin = (double)numericXmin.Value;
            affichage.xMax = (double)numericXmax.Value;
            affichage.yMin = (double)numericYmin.Value;
            affichage.yMax = (double)numericYmax.Value;
            affichage.auto = checkBoxAutoScale.Checked;

            affichage.numericXminPlastic = (double)numericXminPlastic.Value;
            affichage.numericXmaxPlastic = (double)numericXmaxPlastic.Value;
            affichage.numericYminPlastic = (double)numericYminPlastic.Value;
            affichage.numericYmaxPlastic = (double)numericYmaxPlastic.Value;

            affichage.FiltrefiletMin = (double)NumericSignalStrengthMinFilet.Value;
            affichage.FiltrefiletMax = (double)NumericSignalStrengthMaxFilet.Value;

            affichage.FiltreplasticMin = (double)NumericSignalStrengthMinPlastic.Value;
            affichage.FiltreplasticMax = (double)NumericSignalStrengthMaxPlastic.Value;

            affichage.FiltrepouletMin = (double)NumericSignalStrengthMinPoulet.Value;
            affichage.FiltrepouletMax = (double)NumericSignalStrengthMaxPoulet.Value;

            affichage.FilterPoulet = checkBoxFilterPoulet.Checked;
            affichage.FilterPlastic = checkBoxFilterPlastic.Checked;
            affichage.FilterFilet = checkBoxFilterFilet.Checked;
            affichage.ColorAuto = checkBoxColorAuto.Checked;

            affichage.AngleMin = (double)NumericAngleMin.Value;
            affichage.AngleMax = (double)NumericAngleMax.Value;
            affichage.SignalStrengthMin = (double)NumericSignalStrengthMin.Value;
            affichage.SignalStrengthMax = (double)NumericSignalStrengthMax.Value;
            affichage.SignalStrengthAuto = checkBoxSignalStrengthAuto.Checked;
            affichage.numericNbPointMin = (double)numericNbPointMin.Value;
            affichage.SignalStrengthCoeff = (float)NumericSignalStrengthCoeff.Value;
            affichage.SignalStrengthCenterSingularity = (double)NumericSignalStrengthCenterSingularity.Value;
            affichage.checkBoxSignalStrengthCoeff = (bool)checkBoxSignalStrengthCoeff.Checked;
            affichage.textBoxCheminExport = textBoxCheminExport.Text;

            // Ajoute ici d'autres champs si tu en as dans ta classe Affichage

            AffichageSettingsManager.Sauvegarder(affichage);

            lidarReceiver?.Stop();
            base.OnFormClosing(e);

        }

        public class LidarPoint
        {
            public double Angle { get; set; }
            public double Distance { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
            public double SignalStrength { get; set; }
            public uint Timestamp { get; set; }

        }

        public class Affichage
        {
            public double xMin { get; set; }
            public double xMax { get; set; }
            public double yMin { get; set; }
            public double yMax { get; set; }
            public bool auto { get; set; }
            public double FiltrefiletMin { get; set; }
            public double FiltrefiletMax { get; set; }
            public double FiltreplasticMin { get; set; }
            public double FiltreplasticMax { get; set; }
            public double FiltrepouletMin { get; set; }
            public double FiltrepouletMax { get; set; }
            public bool FilterPoulet { get; set; }
            public bool FilterPlastic { get; set; }
            public bool FilterFilet { get; set; }
            public bool ColorAuto { get; set; }
            public double numericYmaxPlastic { get; set; }
            public double numericYminPlastic { get; set; }
            public double numericXmaxPlastic { get; set; }
            public double numericXminPlastic { get; set; }
            public double numericNbPointMin { get; set; }
            public double AngleMin { get; set; }
            public double AngleMax { get; set; }
            public double SignalStrengthMin { get; set; }
            public double SignalStrengthMax { get; set; }
            public bool SignalStrengthAuto { get; set; }
            public float SignalStrengthCoeff { get; set; }
            public double SignalStrengthCenterSingularity { get; set; }
            public bool checkBoxSignalStrengthCoeff { get; set; }
            public double SignalStrengthXPlage { get; set; } = 600;
            public string textBoxCheminExport { get; set; }
        }

        public class LidarSummaryFeatures
        {
            [LoadColumn(0)] public bool Label { get; set; }
            [LoadColumn(1)] public float NbPoints { get; set; }
            [LoadColumn(2)] public float XMin { get; set; }
            [LoadColumn(3)] public float XMax { get; set; }
            [LoadColumn(4)] public float YMin { get; set; }
            [LoadColumn(5)] public float YMax { get; set; }
            [LoadColumn(6)] public float XSpan { get; set; }
            [LoadColumn(7)] public float YSpan { get; set; }
            [LoadColumn(8)] public float XMoy { get; set; }
            [LoadColumn(9)] public float YMoy { get; set; }
            [LoadColumn(10)] public float XStd { get; set; }
            [LoadColumn(11)] public float YStd { get; set; }
            [LoadColumn(12)] public float SignalStrengthMoyen { get; set; }
            [LoadColumn(13)] public float SignalStrengthStd { get; set; }

            // SuperX1 à SuperX10 (colonnes 14-23)
            [LoadColumn(14)] public float SuperX1 { get; set; }
            [LoadColumn(15)] public float SuperX2 { get; set; }
            [LoadColumn(16)] public float SuperX3 { get; set; }
            [LoadColumn(17)] public float SuperX4 { get; set; }
            [LoadColumn(18)] public float SuperX5 { get; set; }
            [LoadColumn(19)] public float SuperX6 { get; set; }
            [LoadColumn(20)] public float SuperX7 { get; set; }
            [LoadColumn(21)] public float SuperX8 { get; set; }
            [LoadColumn(22)] public float SuperX9 { get; set; }
            [LoadColumn(23)] public float SuperX10 { get; set; }

            // SuperY1 à SuperY10 (colonnes 24-33)
            [LoadColumn(24)] public float SuperY1 { get; set; }
            [LoadColumn(25)] public float SuperY2 { get; set; }
            [LoadColumn(26)] public float SuperY3 { get; set; }
            [LoadColumn(27)] public float SuperY4 { get; set; }
            [LoadColumn(28)] public float SuperY5 { get; set; }
            [LoadColumn(29)] public float SuperY6 { get; set; }
            [LoadColumn(30)] public float SuperY7 { get; set; }
            [LoadColumn(31)] public float SuperY8 { get; set; }
            [LoadColumn(32)] public float SuperY9 { get; set; }
            [LoadColumn(33)] public float SuperY10 { get; set; }

            // SuperSignal1 à SuperSignal10 (colonnes 34-43)
            [LoadColumn(34)] public float SuperSignal1 { get; set; }
            [LoadColumn(35)] public float SuperSignal2 { get; set; }
            [LoadColumn(36)] public float SuperSignal3 { get; set; }
            [LoadColumn(37)] public float SuperSignal4 { get; set; }
            [LoadColumn(38)] public float SuperSignal5 { get; set; }
            [LoadColumn(39)] public float SuperSignal6 { get; set; }
            [LoadColumn(40)] public float SuperSignal7 { get; set; }
            [LoadColumn(41)] public float SuperSignal8 { get; set; }
            [LoadColumn(42)] public float SuperSignal9 { get; set; }
            [LoadColumn(43)] public float SuperSignal10 { get; set; }
        }

        public class LidarPrediction
        {
            [ColumnName("PredictedLabel")]
            public bool Prediction { get; set; }  // ✅ Type bool pour classification binaire


            [ColumnName("Score")]
            public float Score { get; set; }  // ✅ Type float scalaire, pas tableau

            //public float Probability { get; set; }  // Optionnel :  pour la probabilité
        }

        public class LidarUdpReceiver
        {
            public event Action<List<LidarPoint>> OnNewScan;

            const int LidarPort = 2368;
            const int PacketSize = 1240;
            const int HeaderSize = 40;
            const int BlockSize = 8;
            const int DataBlockCount = 150;  // 1200/8
            public double AngleMin { get; set; }
            public double AngleMax { get; set; }
            public float SignalStrengthCoeff { get; set; }
            public double SignalStrengthCenterSingularity { get; set; }
            public double SignalStrengthXPlage { get; set; }
            public bool CheckBoxSignalStrengthCoeff { get; set; }


            private System.Net.Sockets.UdpClient udpClient;
            private volatile bool run = false;

            // === AJOUT POUR L'ACCUMULATION DES POINTS SUR UN TOUR ===
            private List<LidarPoint> currentScan = new List<LidarPoint>();
            private double previousAngle = -1;
            private bool firstPacket = true;

            public void Start()
            {

                run = true;
                udpClient = new System.Net.Sockets.UdpClient(LidarPort);
                udpClient.BeginReceive(ReceiveCallback, null);
            }

            public void Stop()
            {
                run = false;
                udpClient?.Close();
            }

            private void ReceiveCallback(IAsyncResult ar)
            {
                if (!run) return;
                var ep = new System.Net.IPEndPoint(System.Net.IPAddress.Any, LidarPort);
                try
                {
                    var data = udpClient?.EndReceive(ar, ref ep);

                    if (data != null && data.Length == PacketSize)
                    {
                        var points = ParsePacket(data);
                        AccumulatePoints(points); // ACCUMULE les points sur un tour complet
                    }
                }
                catch (ObjectDisposedException) { }
                catch (Exception ex)
                {
                    Debug.WriteLine("Erreur LIDAR : " + ex.Message);
                }

                if (run) udpClient.BeginReceive(ReceiveCallback, null);
            }

            // ===========================
            // ACCUMULATION DES POINTS
            // ===========================
            private void AccumulatePoints(List<LidarPoint> points)
            {
                foreach (var point in points)
                {
                    if (firstPacket)
                    {
                        previousAngle = point.Angle;
                        firstPacket = false;
                    }
                    // Détection du nouveau tour (retour à 0°)
                    if (!firstPacket && point.Angle < previousAngle)
                    {
                        // TOUR TERMINÉ !
                        if (currentScan.Count > 50) // Filtrage (optionnel) : s'il y a assez de points
                            OnNewScan?.Invoke(new List<LidarPoint>(currentScan));

                        currentScan.Clear();
                    }

                    currentScan.Add(point);
                    previousAngle = point.Angle;
                }
            }

            private List<LidarPoint> ParsePacket(byte[] data)
            {
                var points = new List<LidarPoint>();
                uint headerId = BitConverter.ToUInt32(data, 0);
                if (headerId != 0xFEF0010F) return points;
                byte distanceScale = data[6];
                uint timestamp = BitConverter.ToUInt32(data, 28);

                for (int i = 0; i < DataBlockCount; i++)
                {
                    int pos = HeaderSize + i * BlockSize;
                    ushort angleRaw = BitConverter.ToUInt16(data, pos);
                    ushort distRaw = BitConverter.ToUInt16(data, pos + 2);
                    ushort strength = BitConverter.ToUInt16(data, pos + 4);

                    if (angleRaw >= 0xFF00) continue;
                    double angle = angleRaw * 0.01;
                    double distance = distRaw * distanceScale;
                    double x = distance * Math.Cos(angle * Math.PI / 180.0);
                    double center = SignalStrengthCenterSingularity;
                    double plage = SignalStrengthXPlage;
                    double coef = SignalStrengthCoeff;
                    double signal = strength;



                    // ======= AJOUT DE FILTRE =======
                    if (distance == 0 || angle < AngleMin || angle > AngleMax)
                        continue; // ignore les points à distance nulle
                                  // ===============================

                    // Applique ton coefficient autour d'une valeur de X
                    if (CheckBoxSignalStrengthCoeff && x >= center - plage && x <= center + plage)
                    {
                        double distToCenter = Math.Abs(x - center);
                        double t = distToCenter / plage;
                        double impact = 1 + t * (coef - 1);
                        signal *= impact;
                    }

                    points.Add(new LidarPoint
                    {
                        Angle = angle,
                        Distance = distance,
                        X = distance * Math.Cos(angle * Math.PI / 180.0),
                        Y = distance * Math.Sin(angle * Math.PI / 180.0),
                        SignalStrength = signal,
                        Timestamp = timestamp
                    });

                }
                return points;
            }
        }

       
    }
}
