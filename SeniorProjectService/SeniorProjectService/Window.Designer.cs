namespace SeniorProjectService
{
    partial class Window
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
            this.ux_TextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ux_AddressDropDown = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.ux_SendButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ux_TextBox
            // 
            this.ux_TextBox.Location = new System.Drawing.Point(12, 30);
            this.ux_TextBox.Multiline = true;
            this.ux_TextBox.Name = "ux_TextBox";
            this.ux_TextBox.Size = new System.Drawing.Size(260, 75);
            this.ux_TextBox.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 112);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(92, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Known Addresses";
            // 
            // ux_AddressDropDown
            // 
            this.ux_AddressDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ux_AddressDropDown.FormattingEnabled = true;
            this.ux_AddressDropDown.Location = new System.Drawing.Point(111, 112);
            this.ux_AddressDropDown.MaxDropDownItems = 100;
            this.ux_AddressDropDown.Name = "ux_AddressDropDown";
            this.ux_AddressDropDown.Size = new System.Drawing.Size(160, 21);
            this.ux_AddressDropDown.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(69, 11);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Text to Send";
            // 
            // ux_SendButton
            // 
            this.ux_SendButton.Location = new System.Drawing.Point(197, 139);
            this.ux_SendButton.Name = "ux_SendButton";
            this.ux_SendButton.Size = new System.Drawing.Size(75, 23);
            this.ux_SendButton.TabIndex = 4;
            this.ux_SendButton.Text = "Send";
            this.ux_SendButton.UseVisualStyleBackColor = true;
            this.ux_SendButton.Click += new System.EventHandler(this.ux_SendButton_Click);
            // 
            // Window
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 174);
            this.Controls.Add(this.ux_SendButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ux_AddressDropDown);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ux_TextBox);
            this.Name = "Window";
            this.Text = "Send Message";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox ux_TextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox ux_AddressDropDown;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button ux_SendButton;
    }
}