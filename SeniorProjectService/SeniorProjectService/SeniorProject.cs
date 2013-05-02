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
        public const byte PING_BYTE = 0x00;
        public const byte NAME_BYTE = 0x01;
        public const byte MISMATCH_BYTE = 0x02;
        public const byte BRAND_BYTE = 0X03;
        public const byte MATCH_BYTE = 0x04;
        public const byte EVENT_BYTE = 0X05;
        public const byte POWER_OFF_BYTE = 0x06;
        public const byte OPTION_BYTE = 0x07;
        public const byte THROW_BYTE = 0x09;
        public const byte VERSION_BYTE = 0x0B;

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

            foreach (ForeignNode fn in remoteNodeList)
            {
                fn.SetRegistered(false);
            }

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

                            int byteCounter;

                            switch (data[0])
                            {
                                case VERSION_BYTE:
                                    int version = data[1];
                                    XbeeTx64Bit transmit;
                                    List<byte> d = new List<byte>();
                                    if ((byte)version == contactingNode.GetVersion())
                                    {
                                        d.Add(MATCH_BYTE);
                                        contactingNode.SetRegistered(true);
                                    }
                                    else
                                    {
                                        d.Add(MISMATCH_BYTE);
                                        contactingNode.ResetNode();
                                    }
                                    contactingNode.SetVersion((byte)version);
                                    transmit = new XbeeTx64Bit(d, contactingNode.GetAddress());
                                    transmit.Send(_serialPort);
                                    break;

                                /* Responding to ping. Rest of message is the name of the unit. */
                                case NAME_BYTE:
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
                                    contactingNode.SetRegistered(true);
                                    break;

                                /* Responding to ping. Rest of message is the brand of the unit. */
                                case BRAND_BYTE:
                                    string brand = "";
                                    for (int i = 1; i < data.Count; i++)
                                    {
                                        if ((char)data[i] != '\0')
                                            brand += (char)data[i];
                                    }
                                    contactingNode.SetBrand(brand);
                                    break;

                                /* Responding to ping. Rest of message is a distinct Event. */
                                case EVENT_BYTE:
                                    byteCounter = 1;
                                    int nameLen = data[byteCounter++];
                                    string eventName = "";
                                    for (int i = 0; i < nameLen; i++)
                                    {
                                        if ((char)data[byteCounter] != '\0')
                                            eventName += (char)data[byteCounter];
                                        byteCounter++;
                                    }

                                    int descLen = data[byteCounter++];
                                    string description = "";
                                    for (int i = 0; i < descLen; i++)
                                    {
                                        if ((char)data[byteCounter] != '\0')
                                            description += (char)data[byteCounter];
                                        byteCounter++;
                                    }

                                    ushort doubleByte = (ushort)(data[byteCounter++] << 8);
                                    doubleByte = (ushort)(doubleByte | data[byteCounter++]);

                                    int eventID = doubleByte & 0x3FF;
                                    bool triggerable = (doubleByte & 0x8000) > 0;
                                    bool option1Exists = (doubleByte & 0x800) > 0;
                                    bool option2Exists = (doubleByte & 0x400) > 0;

                                    Event e = new Event(eventName, description, eventID, triggerable);

                                    e.Option1 = option1Exists;
                                    e.Option2 = option2Exists;

                                    contactingNode.AddEvent(e);
                                    break;

                                case OPTION_BYTE:
                                    byteCounter = 1;

                                    int eventId = (ushort)(data[byteCounter++] << 8);
                                    eventId = (ushort)(eventId | data[byteCounter++]);

                                    int opDescLen = data[byteCounter++];
                                    string opDescription = "";
                                    for (int i = 0; i < opDescLen; i++)
                                    {
                                        if ((char)data[byteCounter] != '\0')
                                            opDescription += (char)data[byteCounter];
                                        byteCounter++;
                                    }

                                    Event currEvent = contactingNode.GetEvents().Single<Event>(c => c.ID == eventId);
                                    currEvent.SetOption1(opDescription);

                                    if (currEvent.Option2)
                                    {
                                        opDescLen = data[byteCounter++];
                                        opDescription = "";
                                        for (int i = 0; i < opDescLen; i++)
                                        {
                                            if ((char)data[byteCounter] != '\0')
                                                opDescription += (char)data[byteCounter];
                                            byteCounter++;
                                        }
                                    }

                                    break;

                                case THROW_BYTE:
                                    byteCounter = 1;
                                    if (!contactingNode.GetRegistered())
                                    {
                                        break;
                                    }

                                    int eventNum = (data[byteCounter++] << 7) | data[byteCounter++];
                                    Event thisEvent = contactingNode.GetEvents().Single<Event>(temp => temp.ID == eventNum);
                                    string op1Str = "";
                                    string op2Str = "";

                                    if (thisEvent.Option1)
                                    {
                                        int op1len = data[byteCounter++];
                                        for (int i = 0; i < op1len; i++)
                                        {
                                            op1Str += (char)data[byteCounter++];
                                        }
                                    }
                                    if (thisEvent.Option2)
                                    {
                                        int op2len = data[byteCounter++];
                                        for (int i = 0; i < op2len; i++)
                                        {
                                            op2Str += (char)data[byteCounter++];
                                        }
                                    }
                                    contactingNode.ThrowEvent(eventNum, op1Str, op2Str);
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