namespace EAVEntities
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
            this.ctlResults = new System.Windows.Forms.TextBox();
            this.ctlGoButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ctlResults
            // 
            this.ctlResults.Location = new System.Drawing.Point(12, 12);
            this.ctlResults.Multiline = true;
            this.ctlResults.Name = "ctlResults";
            this.ctlResults.Size = new System.Drawing.Size(1059, 412);
            this.ctlResults.TabIndex = 0;
            // 
            // ctlGoButton
            // 
            this.ctlGoButton.Location = new System.Drawing.Point(525, 471);
            this.ctlGoButton.Name = "ctlGoButton";
            this.ctlGoButton.Size = new System.Drawing.Size(75, 23);
            this.ctlGoButton.TabIndex = 1;
            this.ctlGoButton.Text = "Go";
            this.ctlGoButton.UseVisualStyleBackColor = true;
            this.ctlGoButton.Click += new System.EventHandler(this.ctlGoButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1083, 529);
            this.Controls.Add(this.ctlGoButton);
            this.Controls.Add(this.ctlResults);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox ctlResults;
        private System.Windows.Forms.Button ctlGoButton;
    }
}

