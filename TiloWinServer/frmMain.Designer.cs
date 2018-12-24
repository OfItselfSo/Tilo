namespace TiloWinServer
{
    partial class frmMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.buttonSendData = new System.Windows.Forms.Button();
            this.textBoxDataTrace = new System.Windows.Forms.TextBox();
            this.checkBoxEnabledSTEPALL = new System.Windows.Forms.CheckBox();
            this.stepperControlSTEP0 = new TiloWinServer.ctlStepperControl();
            this.label1 = new System.Windows.Forms.Label();
            this.stepperControlSTEP1 = new TiloWinServer.ctlStepperControl();
            this.stepperControlSTEP2 = new TiloWinServer.ctlStepperControl();
            this.stepperControlSTEP5 = new TiloWinServer.ctlStepperControl();
            this.stepperControlSTEP4 = new TiloWinServer.ctlStepperControl();
            this.stepperControlSTEP3 = new TiloWinServer.ctlStepperControl();
            this.checkBoxAutoSend = new System.Windows.Forms.CheckBox();
            this.radioButtonCycles = new System.Windows.Forms.RadioButton();
            this.radioButtonHz = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonSendData
            // 
            this.buttonSendData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSendData.Location = new System.Drawing.Point(587, 161);
            this.buttonSendData.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonSendData.Name = "buttonSendData";
            this.buttonSendData.Size = new System.Drawing.Size(153, 30);
            this.buttonSendData.TabIndex = 0;
            this.buttonSendData.Text = "Force Send Data";
            this.buttonSendData.UseVisualStyleBackColor = true;
            this.buttonSendData.Click += new System.EventHandler(this.buttonSendData_Click);
            // 
            // textBoxDataTrace
            // 
            this.textBoxDataTrace.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxDataTrace.Location = new System.Drawing.Point(12, 257);
            this.textBoxDataTrace.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textBoxDataTrace.Multiline = true;
            this.textBoxDataTrace.Name = "textBoxDataTrace";
            this.textBoxDataTrace.ReadOnly = true;
            this.textBoxDataTrace.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxDataTrace.Size = new System.Drawing.Size(731, 161);
            this.textBoxDataTrace.TabIndex = 1;
            // 
            // checkBoxEnabledSTEPALL
            // 
            this.checkBoxEnabledSTEPALL.AutoSize = true;
            this.checkBoxEnabledSTEPALL.Location = new System.Drawing.Point(12, 15);
            this.checkBoxEnabledSTEPALL.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxEnabledSTEPALL.Name = "checkBoxEnabledSTEPALL";
            this.checkBoxEnabledSTEPALL.Size = new System.Drawing.Size(162, 21);
            this.checkBoxEnabledSTEPALL.TabIndex = 6;
            this.checkBoxEnabledSTEPALL.Text = "All Steppers Enabled";
            this.checkBoxEnabledSTEPALL.UseVisualStyleBackColor = true;
            this.checkBoxEnabledSTEPALL.CheckedChanged += new System.EventHandler(this.checkBoxEnabledSTEPALL_CheckedChanged);
            // 
            // stepperControlSTEP0
            // 
            this.stepperControlSTEP0.Location = new System.Drawing.Point(12, 43);
            this.stepperControlSTEP0.Margin = new System.Windows.Forms.Padding(5);
            this.stepperControlSTEP0.Name = "stepperControlSTEP0";
            this.stepperControlSTEP0.Size = new System.Drawing.Size(162, 101);
            this.stepperControlSTEP0.SpeedMode = "Cycles";
            this.stepperControlSTEP0.StepDir = ((uint)(0u));
            this.stepperControlSTEP0.StepEnabled = ((uint)(0u));
            this.stepperControlSTEP0.StepSpeed = ((uint)(50000u));
            this.stepperControlSTEP0.TabIndex = 7;
            this.stepperControlSTEP0.Title = "STEP Name";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(659, 233);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 17);
            this.label1.TabIndex = 8;
            this.label1.Text = "Diagnostics";
            // 
            // stepperControlSTEP1
            // 
            this.stepperControlSTEP1.Location = new System.Drawing.Point(196, 43);
            this.stepperControlSTEP1.Margin = new System.Windows.Forms.Padding(5);
            this.stepperControlSTEP1.Name = "stepperControlSTEP1";
            this.stepperControlSTEP1.Size = new System.Drawing.Size(162, 101);
            this.stepperControlSTEP1.SpeedMode = "Cycles";
            this.stepperControlSTEP1.StepDir = ((uint)(0u));
            this.stepperControlSTEP1.StepEnabled = ((uint)(0u));
            this.stepperControlSTEP1.StepSpeed = ((uint)(50000u));
            this.stepperControlSTEP1.TabIndex = 9;
            this.stepperControlSTEP1.Title = "STEP Name";
            // 
            // stepperControlSTEP2
            // 
            this.stepperControlSTEP2.Location = new System.Drawing.Point(382, 43);
            this.stepperControlSTEP2.Margin = new System.Windows.Forms.Padding(5);
            this.stepperControlSTEP2.Name = "stepperControlSTEP2";
            this.stepperControlSTEP2.Size = new System.Drawing.Size(162, 101);
            this.stepperControlSTEP2.SpeedMode = "Cycles";
            this.stepperControlSTEP2.StepDir = ((uint)(0u));
            this.stepperControlSTEP2.StepEnabled = ((uint)(0u));
            this.stepperControlSTEP2.StepSpeed = ((uint)(50000u));
            this.stepperControlSTEP2.TabIndex = 10;
            this.stepperControlSTEP2.Title = "STEP Name";
            // 
            // stepperControlSTEP5
            // 
            this.stepperControlSTEP5.Location = new System.Drawing.Point(382, 149);
            this.stepperControlSTEP5.Margin = new System.Windows.Forms.Padding(5);
            this.stepperControlSTEP5.Name = "stepperControlSTEP5";
            this.stepperControlSTEP5.Size = new System.Drawing.Size(162, 101);
            this.stepperControlSTEP5.SpeedMode = "Cycles";
            this.stepperControlSTEP5.StepDir = ((uint)(0u));
            this.stepperControlSTEP5.StepEnabled = ((uint)(0u));
            this.stepperControlSTEP5.StepSpeed = ((uint)(50000u));
            this.stepperControlSTEP5.TabIndex = 13;
            this.stepperControlSTEP5.Title = "STEP Name";
            // 
            // stepperControlSTEP4
            // 
            this.stepperControlSTEP4.Location = new System.Drawing.Point(196, 149);
            this.stepperControlSTEP4.Margin = new System.Windows.Forms.Padding(5);
            this.stepperControlSTEP4.Name = "stepperControlSTEP4";
            this.stepperControlSTEP4.Size = new System.Drawing.Size(162, 101);
            this.stepperControlSTEP4.SpeedMode = "Cycles";
            this.stepperControlSTEP4.StepDir = ((uint)(0u));
            this.stepperControlSTEP4.StepEnabled = ((uint)(0u));
            this.stepperControlSTEP4.StepSpeed = ((uint)(50000u));
            this.stepperControlSTEP4.TabIndex = 12;
            this.stepperControlSTEP4.Title = "STEP Name";
            // 
            // stepperControlSTEP3
            // 
            this.stepperControlSTEP3.Location = new System.Drawing.Point(12, 149);
            this.stepperControlSTEP3.Margin = new System.Windows.Forms.Padding(5);
            this.stepperControlSTEP3.Name = "stepperControlSTEP3";
            this.stepperControlSTEP3.Size = new System.Drawing.Size(162, 101);
            this.stepperControlSTEP3.SpeedMode = "Cycles";
            this.stepperControlSTEP3.StepDir = ((uint)(0u));
            this.stepperControlSTEP3.StepEnabled = ((uint)(0u));
            this.stepperControlSTEP3.StepSpeed = ((uint)(50000u));
            this.stepperControlSTEP3.TabIndex = 11;
            this.stepperControlSTEP3.Title = "STEP Name";
            // 
            // checkBoxAutoSend
            // 
            this.checkBoxAutoSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxAutoSend.AutoSize = true;
            this.checkBoxAutoSend.Checked = true;
            this.checkBoxAutoSend.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAutoSend.Location = new System.Drawing.Point(587, 135);
            this.checkBoxAutoSend.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxAutoSend.Name = "checkBoxAutoSend";
            this.checkBoxAutoSend.Size = new System.Drawing.Size(148, 21);
            this.checkBoxAutoSend.TabIndex = 14;
            this.checkBoxAutoSend.Text = "AutoSend Enabled";
            this.checkBoxAutoSend.UseVisualStyleBackColor = true;
            // 
            // radioButtonCycles
            // 
            this.radioButtonCycles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.radioButtonCycles.AutoSize = true;
            this.radioButtonCycles.Checked = true;
            this.radioButtonCycles.Location = new System.Drawing.Point(598, 80);
            this.radioButtonCycles.Name = "radioButtonCycles";
            this.radioButtonCycles.Size = new System.Drawing.Size(70, 21);
            this.radioButtonCycles.TabIndex = 15;
            this.radioButtonCycles.TabStop = true;
            this.radioButtonCycles.Text = "Cycles";
            this.radioButtonCycles.UseVisualStyleBackColor = true;
            this.radioButtonCycles.CheckedChanged += new System.EventHandler(this.radioButtonCycles_CheckedChanged);
            // 
            // radioButtonHz
            // 
            this.radioButtonHz.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.radioButtonHz.AutoSize = true;
            this.radioButtonHz.Location = new System.Drawing.Point(598, 101);
            this.radioButtonHz.Name = "radioButtonHz";
            this.radioButtonHz.Size = new System.Drawing.Size(63, 21);
            this.radioButtonHz.TabIndex = 16;
            this.radioButtonHz.Text = "Hertz";
            this.radioButtonHz.UseVisualStyleBackColor = true;
            this.radioButtonHz.CheckedChanged += new System.EventHandler(this.radioButtonHz_CheckedChanged);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(584, 59);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 17);
            this.label2.TabIndex = 17;
            this.label2.Text = "Speed Mode";
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(755, 421);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.radioButtonHz);
            this.Controls.Add(this.radioButtonCycles);
            this.Controls.Add(this.checkBoxAutoSend);
            this.Controls.Add(this.stepperControlSTEP5);
            this.Controls.Add(this.stepperControlSTEP4);
            this.Controls.Add(this.stepperControlSTEP3);
            this.Controls.Add(this.stepperControlSTEP2);
            this.Controls.Add(this.stepperControlSTEP1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.stepperControlSTEP0);
            this.Controls.Add(this.checkBoxEnabledSTEPALL);
            this.Controls.Add(this.textBoxDataTrace);
            this.Controls.Add(this.buttonSendData);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Location = new System.Drawing.Point(0, 0);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "frmMain";
            this.Text = "Tilo WinServer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonSendData;
        private System.Windows.Forms.TextBox textBoxDataTrace;
        private System.Windows.Forms.CheckBox checkBoxEnabledSTEPALL;
        private ctlStepperControl stepperControlSTEP0;
        private System.Windows.Forms.Label label1;
        private ctlStepperControl stepperControlSTEP1;
        private ctlStepperControl stepperControlSTEP2;
        private ctlStepperControl stepperControlSTEP5;
        private ctlStepperControl stepperControlSTEP4;
        private ctlStepperControl stepperControlSTEP3;
        private System.Windows.Forms.CheckBox checkBoxAutoSend;
        private System.Windows.Forms.RadioButton radioButtonCycles;
        private System.Windows.Forms.RadioButton radioButtonHz;
        private System.Windows.Forms.Label label2;
    }
}

