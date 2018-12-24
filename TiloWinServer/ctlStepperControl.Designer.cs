namespace TiloWinServer
{
    partial class ctlStepperControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelSTEP = new System.Windows.Forms.Label();
            this.textBoxSpeedSTEP = new System.Windows.Forms.TextBox();
            this.checkBoxDirSTEP = new System.Windows.Forms.CheckBox();
            this.checkBoxEnabledSTEP = new System.Windows.Forms.CheckBox();
            this.labelSpeedMode = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelSTEP
            // 
            this.labelSTEP.AutoSize = true;
            this.labelSTEP.Location = new System.Drawing.Point(4, 2);
            this.labelSTEP.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelSTEP.Name = "labelSTEP";
            this.labelSTEP.Size = new System.Drawing.Size(85, 17);
            this.labelSTEP.TabIndex = 9;
            this.labelSTEP.Text = "STEP Name";
            // 
            // textBoxSpeedSTEP
            // 
            this.textBoxSpeedSTEP.Location = new System.Drawing.Point(20, 70);
            this.textBoxSpeedSTEP.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxSpeedSTEP.Name = "textBoxSpeedSTEP";
            this.textBoxSpeedSTEP.Size = new System.Drawing.Size(82, 22);
            this.textBoxSpeedSTEP.TabIndex = 8;
            this.textBoxSpeedSTEP.Text = "50000";
            this.textBoxSpeedSTEP.TextChanged += new System.EventHandler(this.textBoxSpeedSTEP_TextChanged);
            // 
            // checkBoxDirSTEP
            // 
            this.checkBoxDirSTEP.AutoSize = true;
            this.checkBoxDirSTEP.Location = new System.Drawing.Point(20, 47);
            this.checkBoxDirSTEP.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxDirSTEP.Name = "checkBoxDirSTEP";
            this.checkBoxDirSTEP.Size = new System.Drawing.Size(48, 21);
            this.checkBoxDirSTEP.TabIndex = 7;
            this.checkBoxDirSTEP.Text = "Dir";
            this.checkBoxDirSTEP.UseVisualStyleBackColor = true;
            this.checkBoxDirSTEP.CheckedChanged += new System.EventHandler(this.checkBoxDirSTEP_CheckedChanged);
            // 
            // checkBoxEnabledSTEP
            // 
            this.checkBoxEnabledSTEP.AutoSize = true;
            this.checkBoxEnabledSTEP.Location = new System.Drawing.Point(20, 26);
            this.checkBoxEnabledSTEP.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxEnabledSTEP.Name = "checkBoxEnabledSTEP";
            this.checkBoxEnabledSTEP.Size = new System.Drawing.Size(82, 21);
            this.checkBoxEnabledSTEP.TabIndex = 6;
            this.checkBoxEnabledSTEP.Text = "Enabled";
            this.checkBoxEnabledSTEP.UseVisualStyleBackColor = true;
            this.checkBoxEnabledSTEP.CheckedChanged += new System.EventHandler(this.checkBoxEnabledSTEP_CheckedChanged);
            // 
            // labelSpeedMode
            // 
            this.labelSpeedMode.AutoSize = true;
            this.labelSpeedMode.Location = new System.Drawing.Point(106, 73);
            this.labelSpeedMode.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelSpeedMode.Name = "labelSpeedMode";
            this.labelSpeedMode.Size = new System.Drawing.Size(49, 17);
            this.labelSpeedMode.TabIndex = 10;
            this.labelSpeedMode.Text = "Cycles";
            // 
            // ctlStepperControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.labelSpeedMode);
            this.Controls.Add(this.labelSTEP);
            this.Controls.Add(this.textBoxSpeedSTEP);
            this.Controls.Add(this.checkBoxDirSTEP);
            this.Controls.Add(this.checkBoxEnabledSTEP);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "ctlStepperControl";
            this.Size = new System.Drawing.Size(158, 101);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelSTEP;
        private System.Windows.Forms.TextBox textBoxSpeedSTEP;
        private System.Windows.Forms.CheckBox checkBoxDirSTEP;
        private System.Windows.Forms.CheckBox checkBoxEnabledSTEP;
        private System.Windows.Forms.Label labelSpeedMode;
    }
}
