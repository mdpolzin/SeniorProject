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
            trayMenu.MenuItems.Add("-");
            trayMenu.MenuItems.Add("Exit", OnExit);

            trayIcon = new NotifyIcon();
            trayIcon.Text = "MyTrayApp";
            trayIcon.Icon = new Icon(SystemIcons.Application, 40, 40);

            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;
            Application.Run();
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
