namespace SeniorProjectService
{
    partial class AuditNode
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
            this.ux_NodeList = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ux_RenameNode = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.ux_UpdateButton = new System.Windows.Forms.Button();
            this.ux_TriggerEvent = new System.Windows.Forms.Button();
            this.ux_NodeInfo = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // ux_NodeList
            // 
            this.ux_NodeList.FormattingEnabled = true;
            this.ux_NodeList.Location = new System.Drawing.Point(68, 13);
            this.ux_NodeList.Name = "ux_NodeList";
            this.ux_NodeList.Size = new System.Drawing.Size(260, 21);
            this.ux_NodeList.TabIndex = 0;
            this.ux_NodeList.SelectedIndexChanged += new System.EventHandler(this.ux_NodeList_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Nodes:";
            // 
            // ux_RenameNode
            // 
            this.ux_RenameNode.Location = new System.Drawing.Point(68, 41);
            this.ux_RenameNode.Name = "ux_RenameNode";
            this.ux_RenameNode.Size = new System.Drawing.Size(260, 20);
            this.ux_RenameNode.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 44);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Rename:";
            // 
            // ux_UpdateButton
            // 
            this.ux_UpdateButton.Enabled = false;
            this.ux_UpdateButton.Location = new System.Drawing.Point(252, 304);
            this.ux_UpdateButton.Name = "ux_UpdateButton";
            this.ux_UpdateButton.Size = new System.Drawing.Size(75, 23);
            this.ux_UpdateButton.TabIndex = 4;
            this.ux_UpdateButton.Text = "Update";
            this.ux_UpdateButton.UseVisualStyleBackColor = true;
            this.ux_UpdateButton.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ux_UpdateButton_MouseClick);
            // 
            // ux_TriggerEvent
            // 
            this.ux_TriggerEvent.Enabled = false;
            this.ux_TriggerEvent.Location = new System.Drawing.Point(165, 304);
            this.ux_TriggerEvent.Name = "ux_TriggerEvent";
            this.ux_TriggerEvent.Size = new System.Drawing.Size(81, 23);
            this.ux_TriggerEvent.TabIndex = 5;
            this.ux_TriggerEvent.Text = "Trigger Event";
            this.ux_TriggerEvent.UseVisualStyleBackColor = true;
            this.ux_TriggerEvent.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ux_TriggerEvent_MouseClick);
            // 
            // ux_NodeInfo
            // 
            this.ux_NodeInfo.Location = new System.Drawing.Point(15, 74);
            this.ux_NodeInfo.Multiline = true;
            this.ux_NodeInfo.Name = "ux_NodeInfo";
            this.ux_NodeInfo.ReadOnly = true;
            this.ux_NodeInfo.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.ux_NodeInfo.Size = new System.Drawing.Size(312, 224);
            this.ux_NodeInfo.TabIndex = 6;
            // 
            // AuditNode
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(340, 339);
            this.Controls.Add(this.ux_NodeInfo);
            this.Controls.Add(this.ux_TriggerEvent);
            this.Controls.Add(this.ux_UpdateButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ux_RenameNode);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ux_NodeList);
            this.Name = "AuditNode";
            this.Text = "Audit Node";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox ux_NodeList;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox ux_RenameNode;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button ux_UpdateButton;
        private System.Windows.Forms.Button ux_TriggerEvent;
        private System.Windows.Forms.TextBox ux_NodeInfo;
    }
}