using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace SeniorProjectService
{
    public class MenuControl
    {
        private static NotifyIcon trayIcon;
        private static ContextMenu trayMenu;

        /// <summary>
        /// Operates System Tray Icon and Menus in a separate thread
        /// </summary>
        public static void SystemTrayIcon()
        {
            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Send Message", OnSend);
            trayMenu.MenuItems.Add("Ping Nodes", OnPing);
            trayMenu.MenuItems.Add("Investigate Nodes", OnInvestigate);
            trayMenu.MenuItems.Add("-");
            trayMenu.MenuItems.Add("Exit", OnExit);

            trayIcon = new NotifyIcon();
            trayIcon.Text = "SmartHome Service";
            trayIcon.Icon = new Icon(SystemIcons.Application, 40, 40);

            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;
            Application.Run();
        }

        private static void OnInvestigate(object sender, EventArgs e)
        {
            ConnectedNodesWindow cnw = new ConnectedNodesWindow();

            cnw.WindowState = FormWindowState.Normal;
            cnw.Visible = true;
        }

        private static void OnPing(object sender, EventArgs e)
        {
            List<byte> data = new List<byte>();
            data.Add(0x00);
            XbeeTx64Bit transmit = new XbeeTx64Bit(data);
            transmit.Send(Service._serialPort);
        }

        private static void OnSend(object sender, EventArgs e)
        {
            Window window = new Window();

            window.WindowState = FormWindowState.Normal;
            window.Visible = true;
        }

        private static void OnExit(object sender, EventArgs e)
        {
            trayIcon.Dispose();
            Service._continue = false;
            Application.Exit();
        }
    }
}
