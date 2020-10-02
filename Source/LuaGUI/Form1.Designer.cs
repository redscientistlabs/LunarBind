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
            this.SuspendLayout();
            // 
            // bTest
            // 
            this.bTest.Location = new System.Drawing.Point(13, 13);
            this.bTest.Name = "bTest";
            this.bTest.Size = new System.Drawing.Size(75, 23);
            this.bTest.TabIndex = 0;
            this.bTest.Text = "Full Test";
            this.bTest.UseVisualStyleBackColor = true;
            this.bTest.Click += new System.EventHandler(this.bTest_Click);
            // 
            // bStart
            // 
            this.bStart.Location = new System.Drawing.Point(13, 102);
            this.bStart.Name = "bStart";
            this.bStart.Size = new System.Drawing.Size(75, 23);
            this.bStart.TabIndex = 1;
            this.bStart.Text = "Start";
            this.bStart.UseVisualStyleBackColor = true;
            this.bStart.Click += new System.EventHandler(this.bStart_Click);
            // 
            // bExecute
            // 
            this.bExecute.Enabled = false;
            this.bExecute.Location = new System.Drawing.Point(13, 131);
            this.bExecute.Name = "bExecute";
            this.bExecute.Size = new System.Drawing.Size(75, 23);
            this.bExecute.TabIndex = 2;
            this.bExecute.Text = "Execute";
            this.bExecute.UseVisualStyleBackColor = true;
            this.bExecute.Click += new System.EventHandler(this.bExecute_Click);
            // 
            // bDispose
            // 
            this.bDispose.Enabled = false;
            this.bDispose.Location = new System.Drawing.Point(12, 160);
            this.bDispose.Name = "bDispose";
            this.bDispose.Size = new System.Drawing.Size(75, 23);
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
            this.tbScript.Location = new System.Drawing.Point(253, 13);
            this.tbScript.Multiline = true;
            this.tbScript.Name = "tbScript";
            this.tbScript.Size = new System.Drawing.Size(495, 425);
            this.tbScript.TabIndex = 4;
            this.tbScript.Text = resources.GetString("tbScript.Text");
            // 
            // bAbort
            // 
            this.bAbort.Location = new System.Drawing.Point(94, 131);
            this.bAbort.Name = "bAbort";
            this.bAbort.Size = new System.Drawing.Size(75, 23);
            this.bAbort.TabIndex = 5;
            this.bAbort.Text = "Abort";
            this.bAbort.UseVisualStyleBackColor = true;
            this.bAbort.Click += new System.EventHandler(this.bAbort_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(764, 450);
            this.Controls.Add(this.bAbort);
            this.Controls.Add(this.tbScript);
            this.Controls.Add(this.bDispose);
            this.Controls.Add(this.bExecute);
            this.Controls.Add(this.bStart);
            this.Controls.Add(this.bTest);
            this.MinimumSize = new System.Drawing.Size(780, 489);
            this.Name = "Form1";
            this.Text = "Form1";
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
    }
}

