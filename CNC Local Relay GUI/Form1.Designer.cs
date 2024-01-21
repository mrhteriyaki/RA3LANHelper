namespace CNC_Local_Relay_GUI
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            txtPortOffset = new TextBox();
            lblPortStart = new Label();
            btnHostEnable = new Button();
            btnHostDisable = new Button();
            groupBox1 = new GroupBox();
            label1 = new Label();
            txtHostSetting = new TextBox();
            btnStart = new Button();
            btnStop = new Button();
            gbxRelay = new GroupBox();
            chkUPNP = new CheckBox();
            groupBox2 = new GroupBox();
            groupBox1.SuspendLayout();
            gbxRelay.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // txtPortOffset
            // 
            txtPortOffset.Location = new Point(75, 22);
            txtPortOffset.Name = "txtPortOffset";
            txtPortOffset.Size = new Size(70, 23);
            txtPortOffset.TabIndex = 0;
            txtPortOffset.TextChanged += txtPortOffset_TextChanged;
            // 
            // lblPortStart
            // 
            lblPortStart.AutoSize = true;
            lblPortStart.Location = new Point(10, 25);
            lblPortStart.Name = "lblPortStart";
            lblPortStart.Size = new Size(59, 15);
            lblPortStart.TabIndex = 1;
            lblPortStart.Text = "Port Start:";
            // 
            // btnHostEnable
            // 
            btnHostEnable.Location = new Point(6, 56);
            btnHostEnable.Name = "btnHostEnable";
            btnHostEnable.Size = new Size(75, 23);
            btnHostEnable.TabIndex = 2;
            btnHostEnable.Text = "Enable";
            btnHostEnable.UseVisualStyleBackColor = true;
            btnHostEnable.Click += btnHostEnable_Click;
            // 
            // btnHostDisable
            // 
            btnHostDisable.Location = new Point(87, 56);
            btnHostDisable.Name = "btnHostDisable";
            btnHostDisable.Size = new Size(75, 23);
            btnHostDisable.TabIndex = 3;
            btnHostDisable.Text = "Disable";
            btnHostDisable.UseVisualStyleBackColor = true;
            btnHostDisable.Click += btnHostDisable_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(txtHostSetting);
            groupBox1.Controls.Add(btnHostEnable);
            groupBox1.Controls.Add(btnHostDisable);
            groupBox1.Location = new Point(12, 101);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(172, 88);
            groupBox1.TabIndex = 4;
            groupBox1.TabStop = false;
            groupBox1.Text = "Hostfile";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(10, 25);
            label1.Name = "label1";
            label1.Size = new Size(42, 15);
            label1.TabIndex = 7;
            label1.Text = "Status:";
            // 
            // txtHostSetting
            // 
            txtHostSetting.Location = new Point(58, 22);
            txtHostSetting.Name = "txtHostSetting";
            txtHostSetting.ReadOnly = true;
            txtHostSetting.Size = new Size(87, 23);
            txtHostSetting.TabIndex = 4;
            // 
            // btnStart
            // 
            btnStart.Location = new Point(6, 51);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(75, 23);
            btnStart.TabIndex = 5;
            btnStart.Text = "Start Relay";
            btnStart.UseVisualStyleBackColor = true;
            btnStart.Click += btnStart_Click;
            // 
            // btnStop
            // 
            btnStop.Enabled = false;
            btnStop.Location = new Point(87, 51);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(75, 23);
            btnStop.TabIndex = 6;
            btnStop.Text = "Stop Relay";
            btnStop.UseVisualStyleBackColor = true;
            btnStop.Click += btnStop_Click;
            // 
            // gbxRelay
            // 
            gbxRelay.Controls.Add(btnStart);
            gbxRelay.Controls.Add(btnStop);
            gbxRelay.Controls.Add(txtPortOffset);
            gbxRelay.Controls.Add(lblPortStart);
            gbxRelay.Location = new Point(12, 12);
            gbxRelay.Name = "gbxRelay";
            gbxRelay.Size = new Size(172, 83);
            gbxRelay.TabIndex = 8;
            gbxRelay.TabStop = false;
            gbxRelay.Text = "Relay Control";
            // 
            // chkUPNP
            // 
            chkUPNP.AutoSize = true;
            chkUPNP.Checked = true;
            chkUPNP.CheckState = CheckState.Checked;
            chkUPNP.Location = new Point(10, 22);
            chkUPNP.Name = "chkUPNP";
            chkUPNP.Size = new Size(95, 19);
            chkUPNP.TabIndex = 9;
            chkUPNP.Text = "Enable UPNP";
            chkUPNP.UseVisualStyleBackColor = true;
            chkUPNP.CheckedChanged += chkUPNP_CheckedChanged;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(chkUPNP);
            groupBox2.Location = new Point(12, 195);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(172, 56);
            groupBox2.TabIndex = 10;
            groupBox2.TabStop = false;
            groupBox2.Text = "Auto Port Forwarding";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(200, 263);
            Controls.Add(groupBox2);
            Controls.Add(gbxRelay);
            Controls.Add(groupBox1);
            Name = "Form1";
            Text = " LAN Relay";
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            gbxRelay.ResumeLayout(false);
            gbxRelay.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TextBox txtPortOffset;
        private Label lblPortStart;
        private Button btnHostEnable;
        private Button btnHostDisable;
        private GroupBox groupBox1;
        private TextBox txtHostSetting;
        private Button btnStart;
        private Button btnStop;
        private GroupBox gbxRelay;
        private Label label1;
        private CheckBox chkUPNP;
        private GroupBox groupBox2;
    }
}