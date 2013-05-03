using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SeniorProjectService
{
    public partial class AuditNode : Form
    {
        public AuditNode()
        {
            InitializeComponent();

            List<string> nodeNames = new List<string>();
            foreach(ForeignNode fn in Service.remoteNodeList)
            {
                if (fn.GetName() == Service.BROADCAST.GetName() || !fn.GetRegistered())
                    continue;

                nodeNames.Add(fn.GetAlias());
            }

            ux_NodeList.DataSource = nodeNames;
            ux_NodeList.SelectedIndex = -1;
        }

        private void ux_UpdateButton_MouseClick(object sender, MouseEventArgs e)
        {
            if (ux_RenameNode.Text.Length <= 0)
            {
                return;
            }

            ForeignNode fn = Service.remoteNodeList.Single<ForeignNode>(node => node.GetAlias() == ux_NodeList.SelectedItem.ToString());
            fn.SetAlias(ux_RenameNode.Text);

            List<string> nodeNames = new List<string>();
            foreach (ForeignNode FN in Service.remoteNodeList)
            {
                if (FN.GetName() == Service.BROADCAST.GetName() || !FN.GetRegistered())
                    continue;

                nodeNames.Add(FN.GetAlias());
            }

            int index = ux_NodeList.SelectedIndex;
            ux_NodeList.DataSource = nodeNames;
            ux_NodeList.SelectedIndex = index;
            PopulateNodeInfo(fn);
        }

        private void ux_NodeList_SelectedIndexChanged(object sender, EventArgs evArgs)
        {
            if (ux_NodeList.SelectedIndex < 0)
            {
                ux_UpdateButton.Enabled = false;
                ux_TriggerEvent.Enabled = false;
                ux_NodeInfo.Text = "";
                return;
            }
            ux_UpdateButton.Enabled = true;
            ux_TriggerEvent.Enabled = true;
            ux_RenameNode.Text = "";

            ForeignNode fn = Service.remoteNodeList.Single<ForeignNode>(node => node.GetAlias() == ux_NodeList.SelectedItem.ToString());

            PopulateNodeInfo(fn);
        }

        private void ux_TriggerEvent_MouseClick(object sender, MouseEventArgs e)
        {
            ForeignNode fn = Service.remoteNodeList.Single<ForeignNode>(node => node.GetAlias() == ux_NodeList.SelectedItem.ToString());

            EventWindow ew = new EventWindow(fn);

            ew.WindowState = FormWindowState.Normal;
            ew.Visible = true;
        }

        private void PopulateNodeInfo(ForeignNode fn)
        {
            ux_NodeInfo.Text = fn.GetName() + ":\r\n";
            ux_NodeInfo.Text += "\tBrand:\t" + fn.GetBrand() + "\r\n";
            ux_NodeInfo.Text += "\tAlias:\t" + fn.GetAlias() + "\r\n";
            ux_NodeInfo.Text += "\tAddress:\t" + fn.GetHexAddress() + "\r\n";
            ux_NodeInfo.Text += "\tEvents:";
            foreach (Event e in fn.GetEvents())
            {
                ux_NodeInfo.Text += "\r\n";
                ux_NodeInfo.Text += "\t\tID:\t" + e.ID + "\r\n";
                ux_NodeInfo.Text += "\t\tName:\t" + e.Name + "\r\n";
                ux_NodeInfo.Text += "\t\tAbout:\t" + e.Description + "\r\n";
                ux_NodeInfo.Text += "\t\tThrown:\t" + e.Count + "\r\n";
                if (e.Option1)
                    ux_NodeInfo.Text += "\t\t" + e.GetOption1Description() + ":\t" + e.GetOption1Value() + "\r\n";
                if (e.Option2)
                    ux_NodeInfo.Text += "\t\t" + e.GetOption2Description() + ":\t" + e.GetOption2Value() + "\r\n";
            }
        }
    }
}
