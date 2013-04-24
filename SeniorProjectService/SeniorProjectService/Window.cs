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
    public partial class Window : Form
    {
        public Window()
        {
            InitializeComponent();
            List<string> source = new List<string>();

            foreach (ForeignNode addr in Service.remoteNodeAddresses)
            {
                source.Add(addr.GetName());
            }

            ux_AddressDropDown.DataSource = source;
        }

        private void ux_SendButton_Click(object sender, EventArgs e)
        {
            ulong address;
            if (ux_AddressDropDown.Text == "")
            {
                address = 0xFFFF;
            }
            else
            {

                if (ux_AddressDropDown.Text.Contains("Unknown"))
                {
                    //Get address portion of string, e.g. "Unknown name. 0x001234567890ABCD"
                    string addr = ux_AddressDropDown.Text.Split(' ')[2];
                    address = Service.remoteNodeAddresses.Single<ForeignNode>(node => node.GetHexAddress() == addr).GetAddress();
                }
                else
                {
                    address = Service.remoteNodeAddresses.Single<ForeignNode>(node => node.GetName() == ux_AddressDropDown.Text).GetAddress();
                }
            }

            List<byte> byteMessage = new List<byte>();
            foreach (char c in ux_TextBox.Text)
            {
                byteMessage.Add((byte)c);
            }

            XbeeTx64Bit transmit = new XbeeTx64Bit(byteMessage, address);

            transmit.Send(Service._serialPort);

            this.Visible = false;
        }
    }
}
