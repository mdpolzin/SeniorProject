using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SeniorProjectService
{
    public class Program
    {
        static SerialPort _serialPort;
        static bool _continue;
        static HashSet<ulong> remoteNodeAddresses = new HashSet<ulong>();
        private static NotifyIcon trayIcon;
        private static ContextMenu trayMenu;

        public static void Main()
        {
            string message;
            StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
            Thread readThread = new Thread(Read);
            Thread sysTrayThread = new Thread(SystemTrayIcon);
            sysTrayThread.Start();

            // Create a new SerialPort object with default settings.
            _serialPort = new SerialPort();


            // Allow the user to set the appropriate properties.
            _serialPort.PortName = SetPortName(_serialPort.PortName);
            _serialPort.Encoding = Encoding.UTF8;

            // Set the read/write timeouts
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;

            _serialPort.Open();
            _serialPort.DtrEnable = true;
            _serialPort.RtsEnable = true;
            _continue = true;
            readThread.Start();

            Console.WriteLine("Type QUIT to exit");

            while (_continue)
            {
                message = Console.ReadLine();

                if (stringComparer.Equals("quit", message))
                {
                    _continue = false;
                }

                List<byte> byteMessage = new List<byte>();
                foreach (char c in message)
                {
                    byteMessage.Add((byte)c);
                }
                XbeeTx64Bit transmit = new XbeeTx64Bit(byteMessage);

                transmit.Send(_serialPort);
            }

            sysTrayThread.Join();
            readThread.Join();
            _serialPort.Close();
        }

        public static string SetPortName(string defaultPortName)
        {
            string portName;

            Console.WriteLine("Available Ports:");
            foreach (COMPortInfo comPort in COMPortInfo.GetCOMPortsInfo())
            {
                Console.WriteLine(string.Format("{0} – {1}", comPort.Name, comPort.Description));
            }

            Console.Write("COM port({0}): ", defaultPortName);
            portName = Console.ReadLine();

            if (portName == "")
            {
                portName = defaultPortName;
            }
            return portName;
        }


        /// <summary>
        /// Reads from a separate thread
        /// </summary>
        public static void Read()
        {
            while (_continue)
            {
                try
                {
                    int message = _serialPort.ReadByte();

                    if (message == 0x7E)
                    {
                        XbeeRx incoming = new XbeeRx();
                        if (incoming.ParseIncomingMessage(_serialPort))
                        {
                            if (!incoming.GetIsTxResponse())
                            {
                                remoteNodeAddresses.Add(incoming.GetRemoteAddress());
                            }
                            Console.Write("Message: ");
                            foreach (int i in incoming.GetMessage())
                            {
                                Console.Write((char)i);
                            }
                            Console.WriteLine();
                        }
                    }
                }
                catch (TimeoutException) { }
            }
        }

        /// <summary>
        /// Operates System Tray Icon and Menu in a separate thread
        /// </summary>
        public static void SystemTrayIcon()
        {
            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Exit", OnExit);
            trayMenu.MenuItems.Add("-");
            trayMenu.MenuItems.Add("Stuff");

            trayIcon = new NotifyIcon();
            trayIcon.Text = "MyTrayApp";
            trayIcon.Icon = new Icon(SystemIcons.Application, 40, 40);

            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;
            Application.Run();
        }



        private static void OnExit(object sender, EventArgs e)
        {
            trayIcon.Dispose();
            _continue = false;
            Application.Exit();
        }
    }
}
