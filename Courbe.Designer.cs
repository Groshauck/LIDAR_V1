namespace WinFormsApp1
{
    partial class Courbe
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            CourbeLidar1 = new ScottPlot.WinForms.FormsPlot();
            NumericAngleMin = new NumericUpDown();
            NumericAngleMax = new NumericUpDown();
            labelMin = new Label();
            labelMax = new Label();
            NumericSignalStrengthMin = new NumericUpDown();
            NumericSignalStrengthMax = new NumericUpDown();
            labelSignalStrengthMin = new Label();
            labelSignalStrengthMax = new Label();
            checkBoxSignalStrengthAuto = new CheckBox();
            label1 = new Label();
            label2 = new Label();
            numericXmax = new NumericUpDown();
            numericXmin = new NumericUpDown();
            label3 = new Label();
            label4 = new Label();
            numericYmax = new NumericUpDown();
            numericYmin = new NumericUpDown();
            checkBoxAutoScale = new CheckBox();
            checkBoxFilterPoulet = new CheckBox();
            checkBoxFilterPlastic = new CheckBox();
            checkBoxFilterFilet = new CheckBox();
            NumericSignalStrengthMaxPoulet = new NumericUpDown();
            NumericSignalStrengthMinPoulet = new NumericUpDown();
            NumericSignalStrengthMaxPlastic = new NumericUpDown();
            NumericSignalStrengthMinPlastic = new NumericUpDown();
            NumericSignalStrengthMaxFilet = new NumericUpDown();
            NumericSignalStrengthMinFilet = new NumericUpDown();
            checkBoxColorAuto = new CheckBox();
            EtatCaisse = new Label();
            buttonPlayStop = new Button();
            BoutonEnregistrer = new Button();
            buttonChargerCSV = new Button();
            numericXmaxPlastic = new NumericUpDown();
            numericXminPlastic = new NumericUpDown();
            numericYminPlastic = new NumericUpDown();
            numericYmaxPlastic = new NumericUpDown();
            label5 = new Label();
            label6 = new Label();
            label7 = new Label();
            label8 = new Label();
            numericNbPointMin = new NumericUpDown();
            label9 = new Label();
            ButtonTrain = new Button();
            buttonTestIA = new Button();
            NumericSignalStrengthCoeff = new NumericUpDown();
            NumericSignalStrengthCenterSingularity = new NumericUpDown();
            label10 = new Label();
            label11 = new Label();
            checkBoxSignalStrengthCoeff = new CheckBox();
            textBoxCheminExport = new TextBox();
            buttonGenererDataset = new Button();
            EtatCaisseIA = new Label();
            ((System.ComponentModel.ISupportInitialize)NumericAngleMin).BeginInit();
            ((System.ComponentModel.ISupportInitialize)NumericAngleMax).BeginInit();
            ((System.ComponentModel.ISupportInitialize)NumericSignalStrengthMin).BeginInit();
            ((System.ComponentModel.ISupportInitialize)NumericSignalStrengthMax).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericXmax).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericXmin).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericYmax).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericYmin).BeginInit();
            ((System.ComponentModel.ISupportInitialize)NumericSignalStrengthMaxPoulet).BeginInit();
            ((System.ComponentModel.ISupportInitialize)NumericSignalStrengthMinPoulet).BeginInit();
            ((System.ComponentModel.ISupportInitialize)NumericSignalStrengthMaxPlastic).BeginInit();
            ((System.ComponentModel.ISupportInitialize)NumericSignalStrengthMinPlastic).BeginInit();
            ((System.ComponentModel.ISupportInitialize)NumericSignalStrengthMaxFilet).BeginInit();
            ((System.ComponentModel.ISupportInitialize)NumericSignalStrengthMinFilet).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericXmaxPlastic).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericXminPlastic).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericYminPlastic).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericYmaxPlastic).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericNbPointMin).BeginInit();
            ((System.ComponentModel.ISupportInitialize)NumericSignalStrengthCoeff).BeginInit();
            ((System.ComponentModel.ISupportInitialize)NumericSignalStrengthCenterSingularity).BeginInit();
            SuspendLayout();
            // 
            // CourbeLidar1
            // 
            CourbeLidar1.DisplayScale = 1F;
            CourbeLidar1.Location = new Point(30, 130);
            CourbeLidar1.Margin = new Padding(200);
            CourbeLidar1.Name = "CourbeLidar1";
            CourbeLidar1.Size = new Size(960, 433);
            CourbeLidar1.TabIndex = 0;
            CourbeLidar1.Load += CourbeLidar_Load;
            // 
            // NumericAngleMin
            // 
            NumericAngleMin.Location = new Point(86, 61);
            NumericAngleMin.Maximum = new decimal(new int[] { 358, 0, 0, 0 });
            NumericAngleMin.Name = "NumericAngleMin";
            NumericAngleMin.Size = new Size(140, 23);
            NumericAngleMin.TabIndex = 1;
            // 
            // NumericAngleMax
            // 
            NumericAngleMax.Location = new Point(86, 90);
            NumericAngleMax.Maximum = new decimal(new int[] { 360, 0, 0, 0 });
            NumericAngleMax.Name = "NumericAngleMax";
            NumericAngleMax.Size = new Size(140, 23);
            NumericAngleMax.TabIndex = 2;
            NumericAngleMax.Value = new decimal(new int[] { 360, 0, 0, 0 });
            // 
            // labelMin
            // 
            labelMin.AutoSize = true;
            labelMin.Location = new Point(18, 63);
            labelMin.Name = "labelMin";
            labelMin.Size = new Size(62, 15);
            labelMin.TabIndex = 3;
            labelMin.Text = "Angle Min";
            // 
            // labelMax
            // 
            labelMax.AutoSize = true;
            labelMax.Location = new Point(17, 92);
            labelMax.Name = "labelMax";
            labelMax.Size = new Size(63, 15);
            labelMax.TabIndex = 4;
            labelMax.Text = "Angle Max";
            // 
            // NumericSignalStrengthMin
            // 
            NumericSignalStrengthMin.Increment = new decimal(new int[] { 25, 0, 0, 0 });
            NumericSignalStrengthMin.Location = new Point(358, 63);
            NumericSignalStrengthMin.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            NumericSignalStrengthMin.Name = "NumericSignalStrengthMin";
            NumericSignalStrengthMin.Size = new Size(100, 23);
            NumericSignalStrengthMin.TabIndex = 5;
            NumericSignalStrengthMin.ValueChanged += NumericSignalStrengthMin_ValueChanged;
            // 
            // NumericSignalStrengthMax
            // 
            NumericSignalStrengthMax.Increment = new decimal(new int[] { 25, 0, 0, 0 });
            NumericSignalStrengthMax.Location = new Point(358, 92);
            NumericSignalStrengthMax.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            NumericSignalStrengthMax.Name = "NumericSignalStrengthMax";
            NumericSignalStrengthMax.Size = new Size(100, 23);
            NumericSignalStrengthMax.TabIndex = 6;
            NumericSignalStrengthMax.ValueChanged += NumericSignalStrengthMax_ValueChanged;
            // 
            // labelSignalStrengthMin
            // 
            labelSignalStrengthMin.AutoSize = true;
            labelSignalStrengthMin.Location = new Point(247, 65);
            labelSignalStrengthMin.Name = "labelSignalStrengthMin";
            labelSignalStrengthMin.Size = new Size(108, 15);
            labelSignalStrengthMin.TabIndex = 7;
            labelSignalStrengthMin.Text = "SignalStrength Min";
            // 
            // labelSignalStrengthMax
            // 
            labelSignalStrengthMax.AutoSize = true;
            labelSignalStrengthMax.Location = new Point(247, 94);
            labelSignalStrengthMax.Name = "labelSignalStrengthMax";
            labelSignalStrengthMax.Size = new Size(109, 15);
            labelSignalStrengthMax.TabIndex = 8;
            labelSignalStrengthMax.Text = "SignalStrength Max";
            // 
            // checkBoxSignalStrengthAuto
            // 
            checkBoxSignalStrengthAuto.AutoSize = true;
            checkBoxSignalStrengthAuto.Checked = true;
            checkBoxSignalStrengthAuto.CheckState = CheckState.Checked;
            checkBoxSignalStrengthAuto.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            checkBoxSignalStrengthAuto.Location = new Point(358, 42);
            checkBoxSignalStrengthAuto.Name = "checkBoxSignalStrengthAuto";
            checkBoxSignalStrengthAuto.Size = new Size(99, 17);
            checkBoxSignalStrengthAuto.TabIndex = 9;
            checkBoxSignalStrengthAuto.Text = "Strength Auto";
            checkBoxSignalStrengthAuto.UseVisualStyleBackColor = true;
            checkBoxSignalStrengthAuto.CheckedChanged += checkBoxSignalStrengthAuto_CheckedChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(839, 573);
            label1.Name = "label1";
            label1.Size = new Size(39, 15);
            label1.TabIndex = 13;
            label1.Text = "X Max";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.BackColor = Color.Transparent;
            label2.Location = new Point(171, 575);
            label2.Name = "label2";
            label2.Size = new Size(38, 15);
            label2.TabIndex = 12;
            label2.Text = "X Min";
            // 
            // numericXmax
            // 
            numericXmax.Increment = new decimal(new int[] { 25, 0, 0, 0 });
            numericXmax.Location = new Point(884, 569);
            numericXmax.Maximum = new decimal(new int[] { 15000, 0, 0, 0 });
            numericXmax.Minimum = new decimal(new int[] { 15000, 0, 0, int.MinValue });
            numericXmax.Name = "numericXmax";
            numericXmax.Size = new Size(120, 23);
            numericXmax.TabIndex = 11;
            numericXmax.ValueChanged += numericXmax_ValueChanged;
            // 
            // numericXmin
            // 
            numericXmin.Increment = new decimal(new int[] { 25, 0, 0, 0 });
            numericXmin.Location = new Point(215, 571);
            numericXmin.Maximum = new decimal(new int[] { 15000, 0, 0, 0 });
            numericXmin.Minimum = new decimal(new int[] { 15000, 0, 0, int.MinValue });
            numericXmin.Name = "numericXmin";
            numericXmin.Size = new Size(120, 23);
            numericXmin.TabIndex = 10;
            numericXmin.ValueChanged += numericXmin_ValueChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(1, 120);
            label3.Name = "label3";
            label3.Size = new Size(39, 15);
            label3.TabIndex = 17;
            label3.Text = "Y Max";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(1, 575);
            label4.Name = "label4";
            label4.Size = new Size(38, 15);
            label4.TabIndex = 16;
            label4.Text = "Y Min";
            // 
            // numericYmax
            // 
            numericYmax.Increment = new decimal(new int[] { 25, 0, 0, 0 });
            numericYmax.Location = new Point(46, 118);
            numericYmax.Maximum = new decimal(new int[] { 15000, 0, 0, 0 });
            numericYmax.Minimum = new decimal(new int[] { 15000, 0, 0, int.MinValue });
            numericYmax.Name = "numericYmax";
            numericYmax.Size = new Size(120, 23);
            numericYmax.TabIndex = 15;
            numericYmax.ValueChanged += numericYmax_ValueChanged;
            // 
            // numericYmin
            // 
            numericYmin.Increment = new decimal(new int[] { 25, 0, 0, 0 });
            numericYmin.Location = new Point(45, 571);
            numericYmin.Maximum = new decimal(new int[] { 15000, 0, 0, 0 });
            numericYmin.Minimum = new decimal(new int[] { 15000, 0, 0, int.MinValue });
            numericYmin.Name = "numericYmin";
            numericYmin.Size = new Size(120, 23);
            numericYmin.TabIndex = 14;
            numericYmin.ValueChanged += numericYmin_ValueChanged;
            // 
            // checkBoxAutoScale
            // 
            checkBoxAutoScale.AutoSize = true;
            checkBoxAutoScale.Checked = true;
            checkBoxAutoScale.CheckState = CheckState.Checked;
            checkBoxAutoScale.Location = new Point(754, 571);
            checkBoxAutoScale.Name = "checkBoxAutoScale";
            checkBoxAutoScale.Size = new Size(79, 19);
            checkBoxAutoScale.TabIndex = 18;
            checkBoxAutoScale.Text = "AutoScale";
            checkBoxAutoScale.UseVisualStyleBackColor = true;
            checkBoxAutoScale.CheckedChanged += checkBoxAutoScale_CheckedChanged;
            // 
            // checkBoxFilterPoulet
            // 
            checkBoxFilterPoulet.AutoSize = true;
            checkBoxFilterPoulet.Checked = true;
            checkBoxFilterPoulet.CheckState = CheckState.Checked;
            checkBoxFilterPoulet.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            checkBoxFilterPoulet.Location = new Point(463, 42);
            checkBoxFilterPoulet.Name = "checkBoxFilterPoulet";
            checkBoxFilterPoulet.Size = new Size(107, 17);
            checkBoxFilterPoulet.TabIndex = 20;
            checkBoxFilterPoulet.Text = "Strength Poulet";
            checkBoxFilterPoulet.UseVisualStyleBackColor = true;
            checkBoxFilterPoulet.CheckedChanged += checkBoxFilterPoulet_CheckedChanged;
            // 
            // checkBoxFilterPlastic
            // 
            checkBoxFilterPlastic.AutoSize = true;
            checkBoxFilterPlastic.Checked = true;
            checkBoxFilterPlastic.CheckState = CheckState.Checked;
            checkBoxFilterPlastic.Font = new Font("Segoe UI", 8F);
            checkBoxFilterPlastic.Location = new Point(570, 42);
            checkBoxFilterPlastic.Name = "checkBoxFilterPlastic";
            checkBoxFilterPlastic.Size = new Size(106, 17);
            checkBoxFilterPlastic.TabIndex = 21;
            checkBoxFilterPlastic.Text = "Strength Plastic";
            checkBoxFilterPlastic.UseVisualStyleBackColor = true;
            checkBoxFilterPlastic.CheckedChanged += checkBoxFilterPlastic_CheckedChanged;
            // 
            // checkBoxFilterFilet
            // 
            checkBoxFilterFilet.AutoSize = true;
            checkBoxFilterFilet.Checked = true;
            checkBoxFilterFilet.CheckState = CheckState.Checked;
            checkBoxFilterFilet.Font = new Font("Segoe UI", 8F);
            checkBoxFilterFilet.Location = new Point(676, 42);
            checkBoxFilterFilet.Name = "checkBoxFilterFilet";
            checkBoxFilterFilet.Size = new Size(96, 17);
            checkBoxFilterFilet.TabIndex = 22;
            checkBoxFilterFilet.Text = "Strength Filet";
            checkBoxFilterFilet.UseVisualStyleBackColor = true;
            checkBoxFilterFilet.CheckedChanged += checkBoxFilterFilet_CheckedChanged;
            // 
            // NumericSignalStrengthMaxPoulet
            // 
            NumericSignalStrengthMaxPoulet.Increment = new decimal(new int[] { 25, 0, 0, 0 });
            NumericSignalStrengthMaxPoulet.Location = new Point(464, 94);
            NumericSignalStrengthMaxPoulet.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            NumericSignalStrengthMaxPoulet.Name = "NumericSignalStrengthMaxPoulet";
            NumericSignalStrengthMaxPoulet.Size = new Size(100, 23);
            NumericSignalStrengthMaxPoulet.TabIndex = 24;
            NumericSignalStrengthMaxPoulet.ValueChanged += NumericSignalStrengthMaxPoulet_ValueChanged;
            // 
            // NumericSignalStrengthMinPoulet
            // 
            NumericSignalStrengthMinPoulet.Increment = new decimal(new int[] { 25, 0, 0, 0 });
            NumericSignalStrengthMinPoulet.Location = new Point(464, 65);
            NumericSignalStrengthMinPoulet.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            NumericSignalStrengthMinPoulet.Name = "NumericSignalStrengthMinPoulet";
            NumericSignalStrengthMinPoulet.Size = new Size(100, 23);
            NumericSignalStrengthMinPoulet.TabIndex = 23;
            NumericSignalStrengthMinPoulet.ValueChanged += NumericSignalStrengthMinPoulet_ValueChanged;
            // 
            // NumericSignalStrengthMaxPlastic
            // 
            NumericSignalStrengthMaxPlastic.Increment = new decimal(new int[] { 25, 0, 0, 0 });
            NumericSignalStrengthMaxPlastic.Location = new Point(570, 94);
            NumericSignalStrengthMaxPlastic.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            NumericSignalStrengthMaxPlastic.Name = "NumericSignalStrengthMaxPlastic";
            NumericSignalStrengthMaxPlastic.Size = new Size(100, 23);
            NumericSignalStrengthMaxPlastic.TabIndex = 26;
            NumericSignalStrengthMaxPlastic.ValueChanged += NumericSignalStrengthMaxPlastic_ValueChanged;
            // 
            // NumericSignalStrengthMinPlastic
            // 
            NumericSignalStrengthMinPlastic.Increment = new decimal(new int[] { 25, 0, 0, 0 });
            NumericSignalStrengthMinPlastic.Location = new Point(570, 65);
            NumericSignalStrengthMinPlastic.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            NumericSignalStrengthMinPlastic.Name = "NumericSignalStrengthMinPlastic";
            NumericSignalStrengthMinPlastic.Size = new Size(100, 23);
            NumericSignalStrengthMinPlastic.TabIndex = 25;
            NumericSignalStrengthMinPlastic.ValueChanged += NumericSignalStrengthMinPlastic_ValueChanged;
            // 
            // NumericSignalStrengthMaxFilet
            // 
            NumericSignalStrengthMaxFilet.Increment = new decimal(new int[] { 25, 0, 0, 0 });
            NumericSignalStrengthMaxFilet.Location = new Point(676, 94);
            NumericSignalStrengthMaxFilet.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            NumericSignalStrengthMaxFilet.Name = "NumericSignalStrengthMaxFilet";
            NumericSignalStrengthMaxFilet.Size = new Size(100, 23);
            NumericSignalStrengthMaxFilet.TabIndex = 28;
            NumericSignalStrengthMaxFilet.ValueChanged += NumericSignalStrengthMaxFilet_ValueChanged;
            // 
            // NumericSignalStrengthMinFilet
            // 
            NumericSignalStrengthMinFilet.Increment = new decimal(new int[] { 25, 0, 0, 0 });
            NumericSignalStrengthMinFilet.Location = new Point(676, 65);
            NumericSignalStrengthMinFilet.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            NumericSignalStrengthMinFilet.Name = "NumericSignalStrengthMinFilet";
            NumericSignalStrengthMinFilet.Size = new Size(100, 23);
            NumericSignalStrengthMinFilet.TabIndex = 27;
            NumericSignalStrengthMinFilet.ValueChanged += NumericSignalStrengthMinFilet_ValueChanged;
            // 
            // checkBoxColorAuto
            // 
            checkBoxColorAuto.AutoSize = true;
            checkBoxColorAuto.Checked = true;
            checkBoxColorAuto.CheckState = CheckState.Checked;
            checkBoxColorAuto.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            checkBoxColorAuto.Location = new Point(247, 42);
            checkBoxColorAuto.Name = "checkBoxColorAuto";
            checkBoxColorAuto.Size = new Size(82, 17);
            checkBoxColorAuto.TabIndex = 29;
            checkBoxColorAuto.Text = "Color Auto";
            checkBoxColorAuto.UseVisualStyleBackColor = true;
            checkBoxColorAuto.CheckedChanged += checkBoxColorAuto_CheckedChanged;
            // 
            // EtatCaisse
            // 
            EtatCaisse.AccessibleDescription = "";
            EtatCaisse.AccessibleName = "EtatCaisse";
            EtatCaisse.AutoSize = true;
            EtatCaisse.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            EtatCaisse.ForeColor = Color.Green;
            EtatCaisse.Location = new Point(676, 118);
            EtatCaisse.Name = "EtatCaisse";
            EtatCaisse.Size = new Size(78, 27);
            EtatCaisse.TabIndex = 30;
            EtatCaisse.Text = "Caisse OK";
            EtatCaisse.UseCompatibleTextRendering = true;
            // 
            // buttonPlayStop
            // 
            buttonPlayStop.AutoEllipsis = true;
            buttonPlayStop.BackColor = Color.Lime;
            buttonPlayStop.Location = new Point(5, 12);
            buttonPlayStop.Name = "buttonPlayStop";
            buttonPlayStop.Size = new Size(75, 23);
            buttonPlayStop.TabIndex = 31;
            buttonPlayStop.Text = "Play";
            buttonPlayStop.UseVisualStyleBackColor = false;
            buttonPlayStop.Click += buttonPlayStop_Click;
            // 
            // BoutonEnregistrer
            // 
            BoutonEnregistrer.AutoEllipsis = true;
            BoutonEnregistrer.BackColor = Color.White;
            BoutonEnregistrer.Location = new Point(86, 12);
            BoutonEnregistrer.Name = "BoutonEnregistrer";
            BoutonEnregistrer.Size = new Size(75, 23);
            BoutonEnregistrer.TabIndex = 32;
            BoutonEnregistrer.Text = "Enregistrer";
            BoutonEnregistrer.UseVisualStyleBackColor = false;
            BoutonEnregistrer.Click += BoutonEnregistrerCsv_Click;
            // 
            // buttonChargerCSV
            // 
            buttonChargerCSV.AutoEllipsis = true;
            buttonChargerCSV.BackColor = Color.White;
            buttonChargerCSV.Location = new Point(1, 36);
            buttonChargerCSV.Name = "buttonChargerCSV";
            buttonChargerCSV.Size = new Size(83, 23);
            buttonChargerCSV.TabIndex = 33;
            buttonChargerCSV.Text = "Charger CSV";
            buttonChargerCSV.UseVisualStyleBackColor = false;
            buttonChargerCSV.Click += buttonChargerCSV_Click;
            // 
            // numericXmaxPlastic
            // 
            numericXmaxPlastic.Increment = new decimal(new int[] { 25, 0, 0, 0 });
            numericXmaxPlastic.Location = new Point(932, 65);
            numericXmaxPlastic.Maximum = new decimal(new int[] { 15000, 0, 0, 0 });
            numericXmaxPlastic.Minimum = new decimal(new int[] { 15000, 0, 0, int.MinValue });
            numericXmaxPlastic.Name = "numericXmaxPlastic";
            numericXmaxPlastic.Size = new Size(69, 23);
            numericXmaxPlastic.TabIndex = 37;
            numericXmaxPlastic.ValueChanged += numericXmaxPlastic_ValueChanged;
            // 
            // numericXminPlastic
            // 
            numericXminPlastic.Increment = new decimal(new int[] { 25, 0, 0, 0 });
            numericXminPlastic.Location = new Point(932, 94);
            numericXminPlastic.Maximum = new decimal(new int[] { 15000, 0, 0, 0 });
            numericXminPlastic.Minimum = new decimal(new int[] { 15000, 0, 0, int.MinValue });
            numericXminPlastic.Name = "numericXminPlastic";
            numericXminPlastic.Size = new Size(69, 23);
            numericXminPlastic.TabIndex = 36;
            numericXminPlastic.ValueChanged += numericXminPlastic_ValueChanged;
            // 
            // numericYminPlastic
            // 
            numericYminPlastic.Increment = new decimal(new int[] { 25, 0, 0, 0 });
            numericYminPlastic.Location = new Point(820, 94);
            numericYminPlastic.Maximum = new decimal(new int[] { 15000, 0, 0, 0 });
            numericYminPlastic.Minimum = new decimal(new int[] { 15000, 0, 0, int.MinValue });
            numericYminPlastic.Name = "numericYminPlastic";
            numericYminPlastic.Size = new Size(69, 23);
            numericYminPlastic.TabIndex = 35;
            numericYminPlastic.ValueChanged += numericYminPlastic_ValueChanged;
            // 
            // numericYmaxPlastic
            // 
            numericYmaxPlastic.Increment = new decimal(new int[] { 25, 0, 0, 0 });
            numericYmaxPlastic.Location = new Point(820, 65);
            numericYmaxPlastic.Maximum = new decimal(new int[] { 15000, 0, 0, 0 });
            numericYmaxPlastic.Minimum = new decimal(new int[] { 15000, 0, 0, int.MinValue });
            numericYmaxPlastic.Name = "numericYmaxPlastic";
            numericYmaxPlastic.Size = new Size(69, 23);
            numericYmaxPlastic.TabIndex = 34;
            numericYmaxPlastic.ValueChanged += numericYmaxPlastic_ValueChanged;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.BackColor = Color.Transparent;
            label5.Location = new Point(782, 69);
            label5.Name = "label5";
            label5.Size = new Size(39, 15);
            label5.TabIndex = 38;
            label5.Text = "Y Max";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.BackColor = Color.Transparent;
            label6.Location = new Point(783, 96);
            label6.Name = "label6";
            label6.Size = new Size(38, 15);
            label6.TabIndex = 39;
            label6.Text = "Y Min";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.BackColor = Color.Transparent;
            label7.Location = new Point(895, 69);
            label7.Name = "label7";
            label7.Size = new Size(39, 15);
            label7.TabIndex = 40;
            label7.Text = "X Max";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.BackColor = Color.Transparent;
            label8.Location = new Point(896, 96);
            label8.Name = "label8";
            label8.Size = new Size(38, 15);
            label8.TabIndex = 41;
            label8.Text = "X Min";
            // 
            // numericNbPointMin
            // 
            numericNbPointMin.Increment = new decimal(new int[] { 25, 0, 0, 0 });
            numericNbPointMin.Location = new Point(570, 120);
            numericNbPointMin.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numericNbPointMin.Name = "numericNbPointMin";
            numericNbPointMin.Size = new Size(100, 23);
            numericNbPointMin.TabIndex = 42;
            numericNbPointMin.ValueChanged += numericNbPointMin_ValueChanged;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(486, 124);
            label9.Name = "label9";
            label9.Size = new Size(78, 15);
            label9.TabIndex = 43;
            label9.Text = "NB Point Min";
            // 
            // ButtonTrain
            // 
            ButtonTrain.AutoEllipsis = true;
            ButtonTrain.BackColor = Color.White;
            ButtonTrain.Location = new Point(762, 3);
            ButtonTrain.Name = "ButtonTrain";
            ButtonTrain.Size = new Size(83, 23);
            ButtonTrain.TabIndex = 47;
            ButtonTrain.Text = "Train IA";
            ButtonTrain.UseVisualStyleBackColor = false;
            ButtonTrain.Click += ButtonTrain_Click;
            // 
            // buttonTestIA
            // 
            buttonTestIA.AutoEllipsis = true;
            buttonTestIA.BackColor = Color.White;
            buttonTestIA.Location = new Point(851, 3);
            buttonTestIA.Name = "buttonTestIA";
            buttonTestIA.Size = new Size(83, 23);
            buttonTestIA.TabIndex = 48;
            buttonTestIA.Text = "Test IA";
            buttonTestIA.UseVisualStyleBackColor = false;
            buttonTestIA.Click += buttonTestIA_Click;
            // 
            // NumericSignalStrengthCoeff
            // 
            NumericSignalStrengthCoeff.DecimalPlaces = 2;
            NumericSignalStrengthCoeff.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            NumericSignalStrengthCoeff.Location = new Point(414, 569);
            NumericSignalStrengthCoeff.Maximum = new decimal(new int[] { 15000, 0, 0, 0 });
            NumericSignalStrengthCoeff.Minimum = new decimal(new int[] { 15000, 0, 0, int.MinValue });
            NumericSignalStrengthCoeff.Name = "NumericSignalStrengthCoeff";
            NumericSignalStrengthCoeff.Size = new Size(88, 23);
            NumericSignalStrengthCoeff.TabIndex = 49;
            NumericSignalStrengthCoeff.ValueChanged += NumericSignalStrengthCoeff_ValueChanged;
            // 
            // NumericSignalStrengthCenterSingularity
            // 
            NumericSignalStrengthCenterSingularity.Increment = new decimal(new int[] { 25, 0, 0, 0 });
            NumericSignalStrengthCenterSingularity.Location = new Point(508, 569);
            NumericSignalStrengthCenterSingularity.Maximum = new decimal(new int[] { 15000, 0, 0, 0 });
            NumericSignalStrengthCenterSingularity.Minimum = new decimal(new int[] { 15000, 0, 0, int.MinValue });
            NumericSignalStrengthCenterSingularity.Name = "NumericSignalStrengthCenterSingularity";
            NumericSignalStrengthCenterSingularity.Size = new Size(88, 23);
            NumericSignalStrengthCenterSingularity.TabIndex = 50;
            NumericSignalStrengthCenterSingularity.ValueChanged += NumericSignalStrengthCenterSingularity_ValueChanged;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.BackColor = Color.Transparent;
            label10.Location = new Point(434, 554);
            label10.Name = "label10";
            label10.Size = new Size(36, 15);
            label10.TabIndex = 51;
            label10.Text = "Coeff";
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.BackColor = Color.Transparent;
            label11.Location = new Point(495, 554);
            label11.Name = "label11";
            label11.Size = new Size(111, 15);
            label11.TabIndex = 52;
            label11.Text = "Centre Singularité X";
            // 
            // checkBoxSignalStrengthCoeff
            // 
            checkBoxSignalStrengthCoeff.AutoSize = true;
            checkBoxSignalStrengthCoeff.Checked = true;
            checkBoxSignalStrengthCoeff.CheckState = CheckState.Checked;
            checkBoxSignalStrengthCoeff.Location = new Point(358, 573);
            checkBoxSignalStrengthCoeff.Name = "checkBoxSignalStrengthCoeff";
            checkBoxSignalStrengthCoeff.Size = new Size(55, 19);
            checkBoxSignalStrengthCoeff.TabIndex = 53;
            checkBoxSignalStrengthCoeff.Text = "Coeff";
            checkBoxSignalStrengthCoeff.UseVisualStyleBackColor = true;
            checkBoxSignalStrengthCoeff.CheckedChanged += checkBoxSignalStrengthCoeff_CheckedChanged;
            // 
            // textBoxCheminExport
            // 
            textBoxCheminExport.Location = new Point(167, 13);
            textBoxCheminExport.Name = "textBoxCheminExport";
            textBoxCheminExport.Size = new Size(305, 23);
            textBoxCheminExport.TabIndex = 55;
            textBoxCheminExport.DoubleClick += TextBoxCheminExport_DoubleClick;
            // 
            // buttonGenererDataset
            // 
            buttonGenererDataset.AutoEllipsis = true;
            buttonGenererDataset.BackColor = Color.White;
            buttonGenererDataset.Location = new Point(86, 36);
            buttonGenererDataset.Name = "buttonGenererDataset";
            buttonGenererDataset.Size = new Size(100, 23);
            buttonGenererDataset.TabIndex = 56;
            buttonGenererDataset.Text = "Générer dataset";
            buttonGenererDataset.UseVisualStyleBackColor = false;
            buttonGenererDataset.Click += buttonGenererDataset_Click;
            // 
            // EtatCaisseIA
            // 
            EtatCaisseIA.AccessibleDescription = "";
            EtatCaisseIA.AccessibleName = "EtatCaisse";
            EtatCaisseIA.AutoSize = true;
            EtatCaisseIA.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            EtatCaisseIA.ForeColor = Color.Green;
            EtatCaisseIA.Location = new Point(940, 3);
            EtatCaisseIA.Name = "EtatCaisseIA";
            EtatCaisseIA.Size = new Size(78, 27);
            EtatCaisseIA.TabIndex = 57;
            EtatCaisseIA.Text = "Caisse OK";
            EtatCaisseIA.UseCompatibleTextRendering = true;
            // 
            // Courbe
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1017, 595);
            Controls.Add(EtatCaisseIA);
            Controls.Add(buttonGenererDataset);
            Controls.Add(textBoxCheminExport);
            Controls.Add(checkBoxSignalStrengthCoeff);
            Controls.Add(label11);
            Controls.Add(label10);
            Controls.Add(NumericSignalStrengthCenterSingularity);
            Controls.Add(NumericSignalStrengthCoeff);
            Controls.Add(buttonTestIA);
            Controls.Add(ButtonTrain);
            Controls.Add(label9);
            Controls.Add(numericNbPointMin);
            Controls.Add(numericXmaxPlastic);
            Controls.Add(numericXminPlastic);
            Controls.Add(numericYminPlastic);
            Controls.Add(numericYmaxPlastic);
            Controls.Add(buttonChargerCSV);
            Controls.Add(BoutonEnregistrer);
            Controls.Add(buttonPlayStop);
            Controls.Add(EtatCaisse);
            Controls.Add(checkBoxColorAuto);
            Controls.Add(NumericSignalStrengthMaxFilet);
            Controls.Add(NumericSignalStrengthMinFilet);
            Controls.Add(NumericSignalStrengthMaxPlastic);
            Controls.Add(NumericSignalStrengthMinPlastic);
            Controls.Add(NumericSignalStrengthMaxPoulet);
            Controls.Add(NumericSignalStrengthMinPoulet);
            Controls.Add(checkBoxFilterFilet);
            Controls.Add(checkBoxFilterPlastic);
            Controls.Add(checkBoxFilterPoulet);
            Controls.Add(checkBoxAutoScale);
            Controls.Add(label3);
            Controls.Add(label4);
            Controls.Add(numericYmax);
            Controls.Add(numericYmin);
            Controls.Add(label1);
            Controls.Add(label2);
            Controls.Add(numericXmax);
            Controls.Add(numericXmin);
            Controls.Add(checkBoxSignalStrengthAuto);
            Controls.Add(labelSignalStrengthMax);
            Controls.Add(labelSignalStrengthMin);
            Controls.Add(NumericSignalStrengthMax);
            Controls.Add(NumericSignalStrengthMin);
            Controls.Add(labelMax);
            Controls.Add(labelMin);
            Controls.Add(NumericAngleMax);
            Controls.Add(NumericAngleMin);
            Controls.Add(CourbeLidar1);
            Controls.Add(label5);
            Controls.Add(label6);
            Controls.Add(label7);
            Controls.Add(label8);
            Name = "Courbe";
            Text = "Caisse déboitée";
            ((System.ComponentModel.ISupportInitialize)NumericAngleMin).EndInit();
            ((System.ComponentModel.ISupportInitialize)NumericAngleMax).EndInit();
            ((System.ComponentModel.ISupportInitialize)NumericSignalStrengthMin).EndInit();
            ((System.ComponentModel.ISupportInitialize)NumericSignalStrengthMax).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericXmax).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericXmin).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericYmax).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericYmin).EndInit();
            ((System.ComponentModel.ISupportInitialize)NumericSignalStrengthMaxPoulet).EndInit();
            ((System.ComponentModel.ISupportInitialize)NumericSignalStrengthMinPoulet).EndInit();
            ((System.ComponentModel.ISupportInitialize)NumericSignalStrengthMaxPlastic).EndInit();
            ((System.ComponentModel.ISupportInitialize)NumericSignalStrengthMinPlastic).EndInit();
            ((System.ComponentModel.ISupportInitialize)NumericSignalStrengthMaxFilet).EndInit();
            ((System.ComponentModel.ISupportInitialize)NumericSignalStrengthMinFilet).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericXmaxPlastic).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericXminPlastic).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericYminPlastic).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericYmaxPlastic).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericNbPointMin).EndInit();
            ((System.ComponentModel.ISupportInitialize)NumericSignalStrengthCoeff).EndInit();
            ((System.ComponentModel.ISupportInitialize)NumericSignalStrengthCenterSingularity).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ScottPlot.WinForms.FormsPlot CourbeLidar1;
        public NumericUpDown NumericAngleMin;
        public NumericUpDown NumericAngleMax;
        private Label labelMin;
        private Label labelMax;
        private NumericUpDown NumericSignalStrengthMin;
        private NumericUpDown NumericSignalStrengthMax;
        private Label labelSignalStrengthMin;
        private Label labelSignalStrengthMax;
        private CheckBox checkBoxSignalStrengthAuto;
        private Label label1;
        private Label label2;
        private NumericUpDown numericXmax;
        private NumericUpDown numericXmin;
        private Label label3;
        private Label label4;
        private NumericUpDown numericYmax;
        private NumericUpDown numericYmin;
        private CheckBox checkBoxAutoScale;
        private CheckBox checkBoxFilterPoulet;
        private CheckBox checkBoxFilterPlastic;
        private CheckBox checkBoxFilterFilet;
        private NumericUpDown NumericSignalStrengthMaxPoulet;
        private NumericUpDown NumericSignalStrengthMinPoulet;
        private NumericUpDown NumericSignalStrengthMaxPlastic;
        private NumericUpDown NumericSignalStrengthMinPlastic;
        private NumericUpDown NumericSignalStrengthMaxFilet;
        private NumericUpDown NumericSignalStrengthMinFilet;
        private CheckBox checkBoxColorAuto;
        private Label EtatCaisse;
        private Button buttonPlayStop;
        private Button BoutonEnregistrer;
        private Button button1;
        private Button buttonChargerCSV;
        private NumericUpDown numericXmaxPlastic;
        private NumericUpDown numericXminPlastic;
        private NumericUpDown numericYminPlastic;
        private NumericUpDown numericYmaxPlastic;
        private Label label5;
        private Label label6;
        private Label label7;
        private Label label8;
        private NumericUpDown numericNbPointMin;
        private Label label9;
        private Button ButtonTrain;
        private Button buttonTestIA;
        private NumericUpDown NumericSignalStrengthCoeff;
        private NumericUpDown NumericSignalStrengthCenterSingularity;
        private Label label10;
        private Label label11;
        private CheckBox checkBoxSignalStrengthCoeff;
        private TextBox textBoxCheminExport;
        private Button buttonGenererDataset;
        private Label EtatCaisseIA;
    }
}