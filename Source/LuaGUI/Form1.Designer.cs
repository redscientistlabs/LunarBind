namespace LuaGUI
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.bTest = new System.Windows.Forms.Button();
            this.bStart = new System.Windows.Forms.Button();
            this.bExecute = new System.Windows.Forms.Button();
            this.bDispose = new System.Windows.Forms.Button();
            this.tbScript = new System.Windows.Forms.TextBox();
            this.bAbort = new System.Windows.Forms.Button();
            this.tbStashkey1 = new System.Windows.Forms.TextBox();
            this.bSetStashkey = new System.Windows.Forms.Button();
            this.nmStashkey = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nmStashkey)).BeginInit();
            this.SuspendLayout();
            // 
            // bTest
            // 
            this.bTest.Location = new System.Drawing.Point(20, 636);
            this.bTest.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.bTest.Name = "bTest";
            this.bTest.Size = new System.Drawing.Size(131, 28);
            this.bTest.TabIndex = 0;
            this.bTest.Text = "Execute 60 Test";
            this.bTest.UseVisualStyleBackColor = true;
            this.bTest.Click += new System.EventHandler(this.bTest_Click);
            // 
            // bStart
            // 
            this.bStart.Location = new System.Drawing.Point(17, 63);
            this.bStart.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.bStart.Name = "bStart";
            this.bStart.Size = new System.Drawing.Size(100, 28);
            this.bStart.TabIndex = 1;
            this.bStart.Text = "Start";
            this.bStart.UseVisualStyleBackColor = true;
            this.bStart.Click += new System.EventHandler(this.bStart_Click);
            // 
            // bExecute
            // 
            this.bExecute.Enabled = false;
            this.bExecute.Location = new System.Drawing.Point(17, 98);
            this.bExecute.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.bExecute.Name = "bExecute";
            this.bExecute.Size = new System.Drawing.Size(100, 28);
            this.bExecute.TabIndex = 2;
            this.bExecute.Text = "Execute";
            this.bExecute.UseVisualStyleBackColor = true;
            this.bExecute.Click += new System.EventHandler(this.bExecute_Click);
            // 
            // bDispose
            // 
            this.bDispose.Enabled = false;
            this.bDispose.Location = new System.Drawing.Point(16, 134);
            this.bDispose.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.bDispose.Name = "bDispose";
            this.bDispose.Size = new System.Drawing.Size(100, 28);
            this.bDispose.TabIndex = 3;
            this.bDispose.Text = "Dispose";
            this.bDispose.UseVisualStyleBackColor = true;
            this.bDispose.Click += new System.EventHandler(this.bDispose_Click);
            // 
            // tbScript
            // 
            this.tbScript.AcceptsTab = true;
            this.tbScript.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbScript.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbScript.Location = new System.Drawing.Point(324, 48);
            this.tbScript.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbScript.Multiline = true;
            this.tbScript.Name = "tbScript";
            this.tbScript.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbScript.Size = new System.Drawing.Size(419, 617);
            this.tbScript.TabIndex = 4;
            this.tbScript.Text = resources.GetString("tbScript.Text");
            this.tbScript.WordWrap = false;
            // 
            // bAbort
            // 
            this.bAbort.Enabled = false;
            this.bAbort.Location = new System.Drawing.Point(125, 98);
            this.bAbort.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.bAbort.Name = "bAbort";
            this.bAbort.Size = new System.Drawing.Size(100, 28);
            this.bAbort.TabIndex = 5;
            this.bAbort.Text = "Abort";
            this.bAbort.UseVisualStyleBackColor = true;
            this.bAbort.Click += new System.EventHandler(this.bAbort_Click);
            // 
            // tbStashkey1
            // 
            this.tbStashkey1.AcceptsTab = true;
            this.tbStashkey1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbStashkey1.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbStashkey1.Location = new System.Drawing.Point(752, 48);
            this.tbStashkey1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbStashkey1.Multiline = true;
            this.tbStashkey1.Name = "tbStashkey1";
            this.tbStashkey1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbStashkey1.Size = new System.Drawing.Size(444, 616);
            this.tbStashkey1.TabIndex = 7;
            this.tbStashkey1.Text = "function executeMain()\r\n\tprint(\'(lua SIDE SCR) exec every frame\')\r\nend\r\n\r\nRegiste" +
    "rHook(executeMain, \'Execute\')";
            this.tbStashkey1.WordWrap = false;
            // 
            // bSetStashkey
            // 
            this.bSetStashkey.Enabled = false;
            this.bSetStashkey.Location = new System.Drawing.Point(188, 325);
            this.bSetStashkey.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.bSetStashkey.Name = "bSetStashkey";
            this.bSetStashkey.Size = new System.Drawing.Size(100, 28);
            this.bSetStashkey.TabIndex = 8;
            this.bSetStashkey.Text = "Set";
            this.bSetStashkey.UseVisualStyleBackColor = true;
            this.bSetStashkey.Click += new System.EventHandler(this.bSetStashkey_Click);
            // 
            // nmStashkey
            // 
            this.nmStashkey.Location = new System.Drawing.Point(20, 329);
            this.nmStashkey.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.nmStashkey.Maximum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.nmStashkey.Name = "nmStashkey";
            this.nmStashkey.Size = new System.Drawing.Size(160, 22);
            this.nmStashkey.TabIndex = 9;
            this.nmStashkey.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(748, 28);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(141, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Stashkey Script index 1";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(320, 28);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "Master Script";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 309);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(40, 16);
            this.label3.TabIndex = 12;
            this.label3.Text = "Index";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 217);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(273, 32);
            this.label4.TabIndex = 13;
            this.label4.Text = "Select Stashkey Script (Only index 1 defined)\r\nSelecting again resets stashkey sc" +
    "ript";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(16, 28);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(150, 16);
            this.label5.TabIndex = 14;
            this.label5.Text = "Script Manual Execution";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(16, 261);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(195, 32);
            this.label6.TabIndex = 15;
            this.label6.Text = "Index 0 is initially selected when\r\nstarting with the start button";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1213, 681);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.nmStashkey);
            this.Controls.Add(this.bSetStashkey);
            this.Controls.Add(this.tbStashkey1);
            this.Controls.Add(this.bAbort);
            this.Controls.Add(this.tbScript);
            this.Controls.Add(this.bDispose);
            this.Controls.Add(this.bExecute);
            this.Controls.Add(this.bStart);
            this.Controls.Add(this.bTest);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MinimumSize = new System.Drawing.Size(1035, 593);
            this.Name = "Form1";
            this.Text = "Scriptcore Test GUI";
            ((System.ComponentModel.ISupportInitialize)(this.nmStashkey)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button bTest;
        private System.Windows.Forms.Button bStart;
        private System.Windows.Forms.Button bExecute;
        private System.Windows.Forms.Button bDispose;
        private System.Windows.Forms.TextBox tbScript;
        private System.Windows.Forms.Button bAbort;
        private System.Windows.Forms.TextBox tbStashkey1;
        private System.Windows.Forms.Button bSetStashkey;
        private System.Windows.Forms.NumericUpDown nmStashkey;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
    }
}

