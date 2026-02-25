namespace WinFormsApp1.Controls
{
    partial class LidarDisplayControl
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // LidarDisplayControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "LidarDisplayControl";
            this.Size = new System.Drawing.Size(400, 350);
            this.ResumeLayout(false);
        }
    }
}