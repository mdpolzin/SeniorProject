namespace SeniorProjectService
{
    partial class ConnectedNodesWindow
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
            this.ux_ConnectedNodesTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // ux_ConnectedNodesTextBox
            // 
            this.ux_ConnectedNodesTextBox.Location = new System.Drawing.Point(13, 13);
            this.ux_ConnectedNodesTextBox.Multiline = true;
            this.ux_ConnectedNodesTextBox.Name = "ux_ConnectedNodesTextBox";
            this.ux_ConnectedNodesTextBox.ReadOnly = true;
            this.ux_ConnectedNodesTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.ux_ConnectedNodesTextBox.Size = new System.Drawing.Size(351, 385);
            this.ux_ConnectedNodesTextBox.TabIndex = 0;
            // 
            // ConnectedNodesWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(376, 410);
            this.Controls.Add(this.ux_ConnectedNodesTextBox);
            this.Name = "ConnectedNodesWindow";
            this.Text = "ConnectedNodes";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox ux_ConnectedNodesTextBox;
    }
}