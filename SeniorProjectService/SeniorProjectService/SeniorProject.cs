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
    public class Service
    {
        public static SerialPort _serialPort;
        public static bool _continue;
        public static HashSet<ForeignNode> remoteNodeAddresses = new HashSet<ForeignNode>();

        public static ForeignNode BROADCAST = new ForeignNode(0xFFFF, "Broadcast");
        
        public static void Main()
        {
            StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
            Thread readThread = new Thread(Read);
            Thread sysTrayThread = new Thread(MenuControl.SystemTrayIcon);

            remoteNodeAddresses.Add(BROADCAST);

            sysTrayThread.Start();

            // Create a new SerialPort object with default settings.
            _serialPort = new SerialPort();


            // Allow the user to set the appropriate properties.
            _serialPort.PortName = "COM3";//SetPortName(_serialPort.PortName);
            _serialPort.Encoding = Encoding.UTF8;

            // Set the read/write timeouts
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;

            _serialPort.Open();
            _serialPort.DtrEnable = true;
            _serialPort.RtsEnable = true;
            _continue = true;
            readThread.Start();

            /*
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
            */

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
                                remoteNodeAddresses.Add(new ForeignNode(incoming.GetRemoteAddress()));
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
    }
}
