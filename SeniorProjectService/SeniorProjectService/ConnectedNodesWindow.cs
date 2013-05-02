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
    public partial class ConnectedNodesWindow : Form
    {
        public ConnectedNodesWindow()
        {
            InitializeComponent();

            foreach (ForeignNode fn in Service.remoteNodeList)
            {
                if (fn.GetName() == Service.BROADCAST.GetName())
                    continue;

                ux_ConnectedNodesTextBox.Text += fn.GetName() + ":\r\n";
                ux_ConnectedNodesTextBox.Text += "\tBrand:\t" + fn.GetBrand() + "\r\n";
                ux_ConnectedNodesTextBox.Text += "\tAlias:\t" + fn.GetAlias() + "\r\n";
                ux_ConnectedNodesTextBox.Text += "\tAddress:\t" + fn.GetHexAddress() + "\r\n";
                ux_ConnectedNodesTextBox.Text += "\tEvents:\r\n";
                foreach (Event e in fn.GetEvents())
                {
                    ux_ConnectedNodesTextBox.Text += "\t\tID:\t" + e.ID + "\r\n";
                    ux_ConnectedNodesTextBox.Text += "\t\tName:\t" + e.Name + "\r\n";
                    ux_ConnectedNodesTextBox.Text += "\t\tAbout:\t" + e.Description + "\r\n";
                    ux_ConnectedNodesTextBox.Text += "\t\tThrown:\t" + e.Count + "\r\n";
                    if(e.Option1)
                        ux_ConnectedNodesTextBox.Text += "\t\t" + e.GetOption1Description() + ":\t" + e.GetOption1Value() + "\r\n";
                    if (e.Option2)
                        ux_ConnectedNodesTextBox.Text += "\t\t" + e.GetOption2Description() + ":\t" + e.GetOption2Value() + "\r\n";
                    ux_ConnectedNodesTextBox.Text += "\r\n";
                }
            }
        }
    }
}
