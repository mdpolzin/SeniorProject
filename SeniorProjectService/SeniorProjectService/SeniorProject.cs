using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SeniorProjectService
{
    public class Service
    {
        public static SerialPort _serialPort;
        public static bool _continue;
        public static HashSet<ForeignNode> remoteNodeList = new HashSet<ForeignNode>();

        public static ForeignNode BROADCAST = new ForeignNode(0xFFFF, "Broadcast");
        
        public static void Main()
        {
            DeserializeData();
            StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
            Thread readThread = new Thread(Read);
            Thread sysTrayThread = new Thread(MenuControl.SystemTrayIcon);

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

            sysTrayThread.Join();
            readThread.Join();
            _serialPort.Close();

            SerializeData();
        }

        private static void SerializeData()
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = new FileStream("ServiceInformation.dat", FileMode.OpenOrCreate);

            try
            {
                bf.Serialize(fs, remoteNodeList);
            }
            finally
            {
                fs.Close();
            }
        }

        private static void DeserializeData()
        {
            if (File.Exists("ServiceInformation.dat"))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream fs = new FileStream("ServiceInformation.dat", FileMode.Open);

                try
                {

                    // Deserialize the hashtable from the file and  
                    // assign the reference to the local variable.
                    remoteNodeList = (HashSet<ForeignNode>)bf.Deserialize(fs);
                }
                catch (SerializationException)
                {
                    remoteNodeList = new HashSet<ForeignNode>();
                    remoteNodeList.Add(BROADCAST);
                }
                finally
                {
                    fs.Close();
                }
            }
            else
            {
                remoteNodeList.Add(BROADCAST);
            }
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
                            if (!incoming.GetIsTxResponse() && !remoteNodeList.Any<ForeignNode>(node => node.GetAddress() == incoming.GetRemoteAddress()))
                            {
                                remoteNodeList.Add(new ForeignNode(incoming.GetRemoteAddress()));
                            }

                            List<int> data = incoming.GetMessage();
                            ForeignNode contactingNode = remoteNodeList.Single<ForeignNode>(node => node.GetAddress() == incoming.GetRemoteAddress());

                            switch (data[0])
                            {
                                /* Responding to ping. Rest of message is the name of the unit. */
                                case 0x01:
                                    string name = "";
                                    for (int i = 1; i < data.Count; i++)
                                    {
                                        if ((char)data[i] != '\0')
                                            name += (char)data[i];
                                    }
                                    contactingNode.SetName(name);

                                    //If there is more than one of the same type of device, give each device a numbered alias as well to identify them
                                    int count = 1;
                                    foreach (ForeignNode fn in remoteNodeList)
                                    {
                                        if (fn == contactingNode)
                                            continue;
                                        if (fn.GetName() == contactingNode.GetName())
                                        {
                                            count++;
                                        }
                                    }
                                    contactingNode.SetAlias(contactingNode.GetName() + count.ToString());
                                    break;

                                /* Responding to ping. Rest of message is the brand of the unit. */
                                case 0x03:
                                    string brand = "";
                                    for (int i = 1; i < data.Count; i++)
                                    {
                                        if ((char)data[i] != '\0')
                                            brand += (char)data[i];
                                    }
                                    contactingNode.SetBrand(brand);
                                    break;

                                default:
                                    break;
                            }
                        }
                    }
                }
                catch (TimeoutException) { }
            }
        }
    }
}
