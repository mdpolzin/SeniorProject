namespace SeniorProjectService
{
    partial class EventWindow
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
            this.ux_EventNames = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ux_option1Text = new System.Windows.Forms.Label();
            this.ux_option1Input = new System.Windows.Forms.TextBox();
            this.ux_option2Text = new System.Windows.Forms.Label();
            this.ux_option2Input = new System.Windows.Forms.TextBox();
            this.ux_TriggerButton = new System.Windows.Forms.Button();
            this.ux_EventInfo = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // ux_EventNames
            // 
            this.ux_EventNames.FormattingEnabled = true;
            this.ux_EventNames.Location = new System.Drawing.Point(62, 13);
            this.ux_EventNames.Name = "ux_EventNames";
            this.ux_EventNames.Size = new System.Drawing.Size(210, 21);
            this.ux_EventNames.TabIndex = 0;
            this.ux_EventNames.SelectedIndexChanged += new System.EventHandler(this.ux_EventNames_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Events:";
            // 
            // ux_option1Text
            // 
            this.ux_option1Text.AutoSize = true;
            this.ux_option1Text.Location = new System.Drawing.Point(9, 42);
            this.ux_option1Text.Name = "ux_option1Text";
            this.ux_option1Text.Size = new System.Drawing.Size(35, 13);
            this.ux_option1Text.TabIndex = 2;
            this.ux_option1Text.Text = "label2";
            this.ux_option1Text.Visible = false;
            // 
            // ux_option1Input
            // 
            this.ux_option1Input.Location = new System.Drawing.Point(12, 58);
            this.ux_option1Input.Name = "ux_option1Input";
            this.ux_option1Input.Size = new System.Drawing.Size(260, 20);
            this.ux_option1Input.TabIndex = 3;
            this.ux_option1Input.Visible = false;
            // 
            // ux_option2Text
            // 
            this.ux_option2Text.AutoSize = true;
            this.ux_option2Text.Location = new System.Drawing.Point(12, 85);
            this.ux_option2Text.Name = "ux_option2Text";
            this.ux_option2Text.Size = new System.Drawing.Size(35, 13);
            this.ux_option2Text.TabIndex = 4;
            this.ux_option2Text.Text = "label2";
            this.ux_option2Text.Visible = false;
            // 
            // ux_option2Input
            // 
            this.ux_option2Input.Location = new System.Drawing.Point(12, 102);
            this.ux_option2Input.Name = "ux_option2Input";
            this.ux_option2Input.Size = new System.Drawing.Size(260, 20);
            this.ux_option2Input.TabIndex = 5;
            this.ux_option2Input.Visible = false;
            // 
            // ux_TriggerButton
            // 
            this.ux_TriggerButton.Location = new System.Drawing.Point(197, 246);
            this.ux_TriggerButton.Name = "ux_TriggerButton";
            this.ux_TriggerButton.Size = new System.Drawing.Size(75, 23);
            this.ux_TriggerButton.TabIndex = 6;
            this.ux_TriggerButton.Text = "Trigger";
            this.ux_TriggerButton.UseVisualStyleBackColor = true;
            this.ux_TriggerButton.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ux_TriggerButton_MouseClick);
            // 
            // ux_EventInfo
            // 
            this.ux_EventInfo.Location = new System.Drawing.Point(12, 144);
            this.ux_EventInfo.Multiline = true;
            this.ux_EventInfo.Name = "ux_EventInfo";
            this.ux_EventInfo.ReadOnly = true;
            this.ux_EventInfo.Size = new System.Drawing.Size(260, 96);
            this.ux_EventInfo.TabIndex = 7;
            this.ux_EventInfo.Visible = false;
            // 
            // EventWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 281);
            this.Controls.Add(this.ux_EventInfo);
            this.Controls.Add(this.ux_TriggerButton);
            this.Controls.Add(this.ux_option2Input);
            this.Controls.Add(this.ux_option2Text);
            this.Controls.Add(this.ux_option1Input);
            this.Controls.Add(this.ux_option1Text);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ux_EventNames);
            this.Name = "EventWindow";
            this.Text = "Trigger Event";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox ux_EventNames;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label ux_option1Text;
        private System.Windows.Forms.TextBox ux_option1Input;
        private System.Windows.Forms.Label ux_option2Text;
        private System.Windows.Forms.TextBox ux_option2Input;
        private System.Windows.Forms.Button ux_TriggerButton;
        private System.Windows.Forms.TextBox ux_EventInfo;
    }
}