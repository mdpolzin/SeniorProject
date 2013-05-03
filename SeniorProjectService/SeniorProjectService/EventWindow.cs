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
    public partial class EventWindow : Form
    {
        ForeignNode node;
        public EventWindow(ForeignNode fn)
        {
            InitializeComponent();

            List<string> eventNames = new List<string>();

            node = fn;

            foreach (Event e in node.GetEvents())
            {
                if (e.Triggerable)
                    eventNames.Add(e.Name);
            }

            ux_EventNames.DataSource = eventNames;
            ux_EventNames.SelectedIndex = -1;
        }

        private void ux_EventNames_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ux_EventNames.SelectedIndex < 0)
            {
                ux_TriggerButton.Enabled = false;
                ux_option1Input.Visible = false;
                ux_option1Text.Visible = false;
                ux_option2Input.Visible = false;
                ux_option2Text.Visible = false;
                ux_EventInfo.Visible = false;
                ux_EventInfo.Text = "";
                return;
            }
            Event ev = node.GetEvents().Single<Event>(temp => temp.Name == ux_EventNames.SelectedItem.ToString());

            ux_TriggerButton.Enabled = true;
            ux_EventInfo.Visible = true;
            if (ev.Option1)
            {
                ux_option1Input.Visible = true;
                ux_option1Text.Visible = true;
                ux_option1Text.Text = ev.GetOption1Description();
            }
            if (ev.Option2)
            {
                ux_option2Input.Visible = true;
                ux_option2Text.Visible = true;
                ux_option2Text.Text = ev.GetOption2Description();
            }
            ux_EventInfo.Text = "";

            PopulateEventInfo(ev);
        }

        private void ux_TriggerButton_MouseClick(object sender, MouseEventArgs e)
        {
            Event ev = node.GetEvents().Single<Event>(temp => temp.Name == ux_EventNames.SelectedItem.ToString());

            if ((ev.Option1 && ux_option1Input.Text.Length == 0) || (ev.Option2 && ux_option2Input.Text.Length == 0))
            {
                MessageBox.Show("You must fill out every option presented",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1);

                return;
            }
        }

        private void PopulateEventInfo(Event ev)
        {
            ux_EventInfo.Text += "ID:\t" + ev.ID + "\r\n";
            ux_EventInfo.Text += "Name:\t" + ev.Name + "\r\n";
            ux_EventInfo.Text += "About:\t" + ev.Description + "\r\n";
            ux_EventInfo.Text += "Thrown:\t" + ev.Count + "\r\n";
            if (ev.Option1)
                ux_EventInfo.Text += ev.GetOption1Description() + ":\t" + ev.GetOption1Value() + "\r\n";
            if (ev.Option2)
                ux_EventInfo.Text += ev.GetOption2Description() + ":\t" + ev.GetOption2Value() + "\r\n";
        }
    }
}
