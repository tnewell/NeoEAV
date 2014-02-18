using System.Data.Entity;
using System.Windows.Forms;

using NeoEAV.Objects;
using NeoEAV.Data.DataClasses;
using NeoEAV.Windows.UI;


namespace NeoEAVWindows
{
    partial class NeoEAVForm
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
            this.ctlGoButton = new System.Windows.Forms.Button();
            this.ctlValue = new NeoEAV.Windows.UI.EAVTextBox();
            this.ctlProject = new NeoEAV.Windows.UI.EAVProjectContextControl();
            this.ctlSubject = new NeoEAV.Windows.UI.EAVSubjectContextControl();
            this.ctlContainer = new NeoEAV.Windows.UI.EAVContainerContextControl();
            this.ctlInstance = new NeoEAV.Windows.UI.EAVInstanceContextControl();
            this.ctlAttribute = new NeoEAV.Windows.UI.EAVAttributeContextControl();
            this.ctlProject.SuspendLayout();
            this.ctlSubject.SuspendLayout();
            this.ctlContainer.SuspendLayout();
            this.ctlInstance.SuspendLayout();
            this.SuspendLayout();
            // 
            // ctlGoButton
            // 
            this.ctlGoButton.Location = new System.Drawing.Point(301, 327);
            this.ctlGoButton.Name = "ctlGoButton";
            this.ctlGoButton.Size = new System.Drawing.Size(75, 23);
            this.ctlGoButton.TabIndex = 1;
            this.ctlGoButton.Text = "Go";
            this.ctlGoButton.UseVisualStyleBackColor = true;
            this.ctlGoButton.Click += new System.EventHandler(this.ctlGoButton_Click);
            // 
            // ctlValue
            // 
            this.ctlValue.Location = new System.Drawing.Point(2, 2);
            this.ctlValue.Name = "ctlValue";
            this.ctlValue.RawValue = "";
            this.ctlValue.Size = new System.Drawing.Size(200, 20);
            this.ctlValue.TabIndex = 0;
            // 
            // ctlProject
            // 
            this.ctlProject.ContextKey = null;
            this.ctlProject.Controls.Add(this.ctlSubject);
            this.ctlProject.DataSource = null;
            this.ctlProject.Location = new System.Drawing.Point(12, 12);
            this.ctlProject.Name = "ctlProject";
            this.ctlProject.Size = new System.Drawing.Size(269, 144);
            this.ctlProject.TabIndex = 0;
            // 
            // ctlSubject
            // 
            this.ctlSubject.ContextKey = null;
            this.ctlSubject.Controls.Add(this.ctlContainer);
            this.ctlSubject.DataSource = null;
            this.ctlSubject.Location = new System.Drawing.Point(2, 2);
            this.ctlSubject.Name = "ctlSubject";
            this.ctlSubject.Size = new System.Drawing.Size(250, 112);
            this.ctlSubject.TabIndex = 0;
            // 
            // ctlContainer
            // 
            this.ctlContainer.ContextKey = null;
            this.ctlContainer.Controls.Add(this.ctlInstance);
            this.ctlContainer.DataSource = null;
            this.ctlContainer.Location = new System.Drawing.Point(2, 2);
            this.ctlContainer.Name = "ctlContainer";
            this.ctlContainer.Size = new System.Drawing.Size(237, 87);
            this.ctlContainer.TabIndex = 0;
            // 
            // ctlInstance
            // 
            this.ctlInstance.ContextKey = null;
            this.ctlInstance.Controls.Add(this.ctlAttribute);
            this.ctlInstance.DataSource = null;
            this.ctlInstance.Location = new System.Drawing.Point(2, 2);
            this.ctlInstance.Name = "ctlInstance";
            this.ctlInstance.Size = new System.Drawing.Size(224, 66);
            this.ctlInstance.TabIndex = 0;
            // 
            // ctlAttribute
            // 
            this.ctlAttribute.ContextKey = null;
            this.ctlAttribute.Controls.Add(this.ctlValue);
            this.ctlAttribute.DataSource = null;
            this.ctlAttribute.Location = new System.Drawing.Point(2, 2);
            this.ctlAttribute.Name = "ctlAttribute";
            this.ctlAttribute.Size = new System.Drawing.Size(211, 45);
            this.ctlAttribute.TabIndex = 0;
            // 
            // NeoEAVForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(772, 611);
            this.Controls.Add(this.ctlGoButton);
            this.Controls.Add(this.ctlProject);
            this.Name = "NeoEAVForm";
            this.Text = "NeoEAV Test Form";
            this.ctlProject.ResumeLayout(false);
            this.ctlSubject.ResumeLayout(false);
            this.ctlContainer.ResumeLayout(false);
            this.ctlInstance.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion


        private EAVProjectContextControl ctlProject;
        private EAVSubjectContextControl ctlSubject;
        private EAVContainerContextControl ctlContainer;
        private EAVInstanceContextControl ctlInstance;
        private EAVAttributeContextControl ctlAttribute;
        private EAVTextBox ctlValue;

        private System.Windows.Forms.Button ctlGoButton;
    }
}

