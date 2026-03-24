using System;
using System.Drawing;
using System.Windows.Forms;
using WinFormsApp1.Models;
using WinFormsApp1.Services;

namespace WinFormsApp1.Forms
{
    public class ModbusConfigForm : Form
    {
        private ModbusConfigManager _manager;
        private ModbusConfig _config;
        private ModbusService _service;

        private CheckBox chkActiver;
        private DataGridView dgvCartes;
        private Button btnAjouter, btnSupprimer, btnSauvegarder;
        private Panel panelVoyant;
        private Label lblVoyantTexte;
        private System.Windows.Forms.Timer timerSupervision;
        private Label lblSupervision;

        public ModbusConfigForm(ModbusService service, ModbusConfigManager manager, ModbusConfig config)
        {
            _service = service;
            _manager = manager;
            _config = config;
            InitializeUI();
            LoadToGrid();
            StartSupervisionTimer();
        }

        private void InitializeUI()
        {
            this.Text = "Configuration Modbus TCP";
            this.Size = new Size(900, 530);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(245, 245, 245);

            // === Activation globale ===
            chkActiver = new CheckBox
            {
                Text = "Activer Modbus TCP",
                Checked = _config.ActivationGlobale,
                Location = new Point(20, 15),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };
            this.Controls.Add(chkActiver);

            // === DataGridView cartes ===
            dgvCartes = new DataGridView
            {
                Location = new Point(20, 50),
                Size = new Size(840, 270),
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
            };

            dgvCartes.Columns.Add(new DataGridViewTextBoxColumn { Name = "Nom",        HeaderText = "Nom",          DataPropertyName = "Nom",        Width = 100 });
            dgvCartes.Columns.Add(new DataGridViewTextBoxColumn { Name = "IpAddress",  HeaderText = "IP",           DataPropertyName = "IpAddress",  Width = 130 });
            dgvCartes.Columns.Add(new DataGridViewTextBoxColumn { Name = "Port",       HeaderText = "Port",         DataPropertyName = "Port",       Width = 60  });
            dgvCartes.Columns.Add(new DataGridViewTextBoxColumn { Name = "SlaveId",    HeaderText = "Slave ID",     DataPropertyName = "SlaveId",    Width = 70  });
            dgvCartes.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Enabled",   HeaderText = "Actif",        DataPropertyName = "Enabled",    Width = 50  });
            dgvCartes.Columns.Add(new DataGridViewTextBoxColumn { Name = "BitDem",     HeaderText = "Bit Démarrer", DataPropertyName = "BitEntreeDemarrerTest", Width = 90 });
            dgvCartes.Columns.Add(new DataGridViewTextBoxColumn { Name = "BitCache",   HeaderText = "Bit Cache",    DataPropertyName = "BitEntreeCachePresent", Width = 80 });
            dgvCartes.Columns.Add(new DataGridViewTextBoxColumn { Name = "BitNormal",  HeaderText = "Bit Normal",   DataPropertyName = "BitSortieNormal",  Width = 80 });
            dgvCartes.Columns.Add(new DataGridViewTextBoxColumn { Name = "BitDeboite", HeaderText = "Bit Déboîté", DataPropertyName = "BitSortieDeboite", Width = 85 });
            dgvCartes.Columns.Add(new DataGridViewTextBoxColumn { Name = "BitEnCours", HeaderText = "Bit EnCours",  DataPropertyName = "BitSortieTestEnCours", Width = 85 });

            this.Controls.Add(dgvCartes);

            // === Boutons ===
            btnAjouter = new Button
            {
                Text = "➕ Ajouter carte",
                Location = new Point(20, 330),
                Size = new Size(150, 35),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.ForestGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAjouter.Click += (s, e) =>
            {
                _config.Cartes.Add(new ModbusCardConfig { Nom = $"Carte {_config.Cartes.Count + 1}" });
                LoadToGrid();
            };
            this.Controls.Add(btnAjouter);

            btnSupprimer = new Button
            {
                Text = "🗑 Supprimer",
                Location = new Point(180, 330),
                Size = new Size(150, 35),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.Firebrick,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSupprimer.Click += (s, e) =>
            {
                if (dgvCartes.SelectedRows.Count > 0)
                {
                    int idx = dgvCartes.SelectedRows[0].Index;
                    if (idx < _config.Cartes.Count) _config.Cartes.RemoveAt(idx);
                    LoadToGrid();
                }
            };
            this.Controls.Add(btnSupprimer);

            btnSauvegarder = new Button
            {
                Text = "💾 Sauvegarder",
                Location = new Point(700, 330),
                Size = new Size(160, 35),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSauvegarder.Click += BtnSauvegarder_Click;
            this.Controls.Add(btnSauvegarder);

            // === Voyant visuel ===
            var lblVoyantTitre = new Label
            {
                Text = "Voyant résultat :",
                Location = new Point(20, 378),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };
            this.Controls.Add(lblVoyantTitre);

            panelVoyant = new Panel
            {
                Location = new Point(160, 373),
                Size = new Size(40, 40),
                BackColor = Color.Gray
            };
            // Rendre le voyant circulaire
            var gp = new System.Drawing.Drawing2D.GraphicsPath();
            gp.AddEllipse(0, 0, 40, 40);
            panelVoyant.Region = new Region(gp);
            this.Controls.Add(panelVoyant);

            lblVoyantTexte = new Label
            {
                Text = "—",
                Location = new Point(210, 381),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.Gray
            };
            this.Controls.Add(lblVoyantTexte);

            // === Supervision temps réel ===
            lblSupervision = new Label
            {
                Text = "Supervision : inactif",
                Location = new Point(20, 428),
                Size = new Size(840, 25),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.DimGray
            };
            this.Controls.Add(lblSupervision);
        }

        private void LoadToGrid()
        {
            dgvCartes.Rows.Clear();
            foreach (var c in _config.Cartes)
            {
                dgvCartes.Rows.Add(c.Nom, c.IpAddress, c.Port, c.SlaveId, c.Enabled,
                    c.BitEntreeDemarrerTest, c.BitEntreeCachePresent,
                    c.BitSortieNormal, c.BitSortieDeboite, c.BitSortieTestEnCours);
            }
        }

        private void BtnSauvegarder_Click(object sender, EventArgs e)
        {
            // Relire la grille → _config
            _config.ActivationGlobale = chkActiver.Checked;
            _config.Cartes.Clear();
            foreach (DataGridViewRow row in dgvCartes.Rows)
            {
                _config.Cartes.Add(new ModbusCardConfig
                {
                    Nom = row.Cells["Nom"].Value?.ToString() ?? "",
                    IpAddress = row.Cells["IpAddress"].Value?.ToString() ?? "192.168.0.10",
                    Port = int.TryParse(row.Cells["Port"].Value?.ToString(), out int p) ? p : 502,
                    SlaveId = int.TryParse(row.Cells["SlaveId"].Value?.ToString(), out int s) ? s : 1,
                    Enabled = row.Cells["Enabled"].Value is bool b && b,
                    BitEntreeDemarrerTest = int.TryParse(row.Cells["BitDem"].Value?.ToString(), out int bd) ? bd : 0,
                    BitEntreeCachePresent = int.TryParse(row.Cells["BitCache"].Value?.ToString(), out int bc) ? bc : 1,
                    BitSortieNormal = int.TryParse(row.Cells["BitNormal"].Value?.ToString(), out int bn) ? bn : 0,
                    BitSortieDeboite = int.TryParse(row.Cells["BitDeboite"].Value?.ToString(), out int bdeb) ? bdeb : 1,
                    BitSortieTestEnCours = int.TryParse(row.Cells["BitEnCours"].Value?.ToString(), out int bec) ? bec : 2,
                });
            }
            _manager.Save(_config);
            MessageBox.Show("Configuration Modbus sauvegardée !", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void StartSupervisionTimer()
        {
            timerSupervision = new System.Windows.Forms.Timer { Interval = 300 };
            timerSupervision.Tick += (s, e) => UpdateVoyant();
            timerSupervision.Start();
        }

        private void UpdateVoyant()
        {
            if (_service == null) return;
            if (_service.SortieNormal)
            {
                panelVoyant.BackColor = Color.LimeGreen;
                lblVoyantTexte.Text = "✅ NORMAL";
                lblVoyantTexte.ForeColor = Color.ForestGreen;
            }
            else if (_service.SortieDeboite)
            {
                panelVoyant.BackColor = Color.Red;
                lblVoyantTexte.Text = "❌ DÉBOÎTÉ";
                lblVoyantTexte.ForeColor = Color.Firebrick;
            }
            else
            {
                panelVoyant.BackColor = Color.Gray;
                lblVoyantTexte.Text = "—";
                lblVoyantTexte.ForeColor = Color.Gray;
            }

            lblSupervision.Text = _config.ActivationGlobale
                ? $"Supervision active — {_config.Cartes.FindAll(c => c.Enabled).Count} carte(s) activée(s)"
                : "Supervision : Modbus désactivé";
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            timerSupervision?.Stop();
            base.OnFormClosed(e);
        }
    }
}
