using MySql;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
        public const byte TRIGGER_BYTE = 0x08;
        public const byte THROW_BYTE = 0x09;
        public const byte VERSION_BYTE = 0x0B;

        public static SerialPort _serialPort;
        public static bool _continue;
        public static HashSet<ForeignNode> remoteNodeList = new HashSet<ForeignNode>();

        public static ForeignNode BROADCAST = new ForeignNode(0xFFFF, "Broadcast");

        public static void Main()
        {
            _continue = true;
            DataSqlConnection.mutex.WaitOne();
            DataSqlConnection.Connect();
            DataSqlConnection.mutex.ReleaseMutex();
            StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
            Thread readThread = new Thread(Read);
            Thread sysTrayThread = new Thread(MenuControl.SystemTrayIcon);
            Thread consumeThread = new Thread(Consume);

            consumeThread.Start();
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
            readThread.Start();

            sysTrayThread.Join();
            readThread.Join();
            consumeThread.Join();
            _serialPort.Close();

            DataSqlConnection.mutex.WaitOne();
            DataSqlConnection.CommandNonQuery("UPDATE nodes SET Registered=0");

            DataSqlConnection.Close();
            DataSqlConnection.mutex.ReleaseMutex();
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

        public static void Consume()
        {
            while (_continue)
            {
                DataSqlConnection.mutex.WaitOne();
                string msgs = DataSqlConnection.CommandReturnLine(String.Format(
                    "SELECT * FROM messages_to_nodes"));
                DataSqlConnection.mutex.ReleaseMutex();

                if (msgs != "")
                {
                    string[] msgsArr = msgs.Split('\n');
                    foreach (string msg in msgsArr)
                    {
                        if (msg == "")
                            continue;

                        string[] msgData = msg.Split('|');
                        int NodeID = Convert.ToInt32(msgData[0]);
                        int EventID = Convert.ToInt32(msgData[1]);
                        string op1 = msgData[2];
                        string op2 = msgData[3];
                        int MsgID = Convert.ToInt32(msgData[4]);

                        XbeeTx64Bit transmit;

                        DataSqlConnection.mutex.WaitOne();
                        string ret = DataSqlConnection.CommandReturnLine(String.Format(
                            "SELECT ForeignAddress, Registered FROM nodes WHERE ID = {0};",
                            NodeID));
                        DataSqlConnection.mutex.ReleaseMutex();

                        if (ret == "")
                            continue;

                        DataSqlConnection.mutex.WaitOne();
                        DataSqlConnection.CommandNonQuery(String.Format(
                            "DELETE FROM messages_to_nodes WHERE MessageID={0}",
                            MsgID));
                        DataSqlConnection.mutex.ReleaseMutex();

                        string forAddr = ret.Split('|')[0];
                        string reg = ret.Split('|')[1];

                        if (reg != "1")
                            continue;

                        ulong addr = Convert.ToUInt64(forAddr);

                        List<byte> data = new List<byte>();
                        data.Add(TRIGGER_BYTE);
                        byte id1 = (byte)((EventID & 0x300) >> 8);
                        byte id2 = (byte)(EventID & 0xFF);
                        data.Add(id1);
                        data.Add(id2);

                        if (op1 != "")
                        {
                            data.Add((byte)op1.Length);
                            foreach (char c in op1)
                            {
                                data.Add((byte)c);
                            }
                        }
                        if (op2 != "")
                        {
                            data.Add((byte)op2.Length);
                            foreach (char c in op2)
                            {
                                data.Add((byte)c);
                            }
                        }

                        transmit = new XbeeTx64Bit(data, addr);
                        transmit.Send(_serialPort);
                    }
                }

                Thread.Sleep(250);
            }
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

                                    DataSqlConnection.mutex.WaitOne();
                                    string ret = DataSqlConnection.CommandReturnLine(String.Format(
                                        "SELECT Version FROM nodes WHERE ForeignAddress = {0}",
                                        contactingNode.GetAddress()));
                                    DataSqlConnection.mutex.ReleaseMutex();
                                    if (ret == "")
                                    {
                                        d.Add(MISMATCH_BYTE);
                                        contactingNode.ResetNode();
                                    }
                                    else
                                    {
                                        int databaseVersion = Convert.ToInt32(ret.Split('\n')[0].Split('|')[0]);

                                        if (version != databaseVersion)
                                        {
                                            d.Add(MISMATCH_BYTE);
                                            contactingNode.ResetNode();

                                            DataSqlConnection.mutex.WaitOne();
                                            DataSqlConnection.CommandNonQuery(String.Format(
                                                "UPDATE nodes SET Version = {0} WHERE ForeignAddress = {1}",
                                                version,
                                                contactingNode.GetAddress()));
                                            DataSqlConnection.mutex.ReleaseMutex();
                                        }
                                        else
                                        {
                                            DataSqlConnection.mutex.WaitOne();
                                            DataSqlConnection.CommandNonQuery(String.Format(
                                                "UPDATE nodes SET Registered = 1 WHERE ForeignAddress = {0}",
                                                contactingNode.GetAddress()));

                                            d.Add(MATCH_BYTE);
                                            string node = DataSqlConnection.CommandReturnLine(String.Format(
                                                "SELECT * FROM nodes WHERE ForeignAddress = {0}",
                                                contactingNode.GetAddress())).Split('\n')[0];
                                            DataSqlConnection.mutex.ReleaseMutex();

                                            string[] nodeInfo = node.Split('|');

                                            contactingNode.SetNodeID(Convert.ToInt32(nodeInfo[0]));
                                            contactingNode.SetName(nodeInfo[1]);
                                            contactingNode.SetAlias(nodeInfo[2]);
                                            contactingNode.SetBrand(nodeInfo[3]);
                                            contactingNode.SetAddress(Convert.ToUInt64(nodeInfo[4]));
                                            contactingNode.SetHexAddress(nodeInfo[5]);
                                            contactingNode.SetRegistered(true);
                                            //nodeInfo[7] is IsForeign which we don't care about
                                            contactingNode.SetVersion(Convert.ToByte(nodeInfo[8]));

                                            DataSqlConnection.mutex.WaitOne();
                                            nodeInfo = DataSqlConnection.CommandReturnLine(String.Format(
                                                "SELECT * FROM events WHERE NodeId = {0}",
                                                contactingNode.GetNodeID())).Split('\n');
                                            DataSqlConnection.mutex.ReleaseMutex();

                                            foreach (string s in nodeInfo)
                                            {
                                                if(s == "")
                                                    continue;

                                                string[] eventData = s.Split('|');

                                                Event contactingEvent = new Event(eventData[2], eventData[3], Convert.ToInt32(eventData[1]), Convert.ToInt32(eventData[4]) == 1);

                                                DataSqlConnection.mutex.WaitOne();
                                                string optionsData = DataSqlConnection.CommandReturnLine(
                                                    String.Format("SELECT * FROM options WHERE NodeID = {0} AND EventID = {1}",
                                                    contactingNode.GetNodeID(),
                                                    contactingEvent.ID));
                                                DataSqlConnection.mutex.ReleaseMutex();

                                                if (optionsData == "")
                                                {
                                                    contactingEvent.Option1 = false;
                                                    contactingEvent.Option2 = false;
                                                }
                                                else
                                                {
                                                    string[] ops = optionsData.Split('\n');
                                                    string[] opsVals;

                                                    opsVals = ops[0].Split('|');
                                                    contactingEvent.Option1 = true;
                                                    contactingEvent.SetOption1(opsVals[3]);
                                                    contactingEvent.SetOption1Value(opsVals[4]);

                                                    if (ops.Length == 3)
                                                    {
                                                        opsVals = ops[1].Split('|');
                                                        contactingEvent.Option2 = true;
                                                        contactingEvent.SetOption2(opsVals[3]);
                                                        contactingEvent.SetOption2Value(opsVals[4]);
                                                    }
                                                }

                                                contactingNode.AddEvent(contactingEvent);
                                            }
                                        }
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

                                    AddNodeToDatabase(contactingNode);

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

                                    AddEventToDatabase(contactingNode, e);

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

                                    AddOptionToDatabase(contactingNode, currEvent, 1, opDescription);

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

                                        AddOptionToDatabase(contactingNode, currEvent, 1, opDescription);
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

                                    ThrowEvent(contactingNode, thisEvent, op1Str, op2Str);
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

        private static void ThrowEvent(ForeignNode contactingNode, Event thisEvent, string op1Str, string op2Str)
        {
            DataSqlConnection.mutex.WaitOne();
            DataSqlConnection.CommandNonQuery(String.Format(
                "INSERT INTO messages_from_nodes (NodeID, EventID, Option1Val, Option2Val) VALUES({0}, {1}, \"{2}\", \"{3}\")",
                contactingNode.GetNodeID(),
                thisEvent.ID,
                op1Str,
                op2Str));

            DataSqlConnection.CommandNonQuery(String.Format(
                "UPDATE events SET Count=Count+1 WHERE NodeID={0} AND EventID={1}",
                contactingNode.GetNodeID(),
                thisEvent.ID));

            if (op1Str != "")
            {
                DataSqlConnection.CommandNonQuery(String.Format(
                    "UPDATE options SET Value=\"{0}\" WHERE NodeID={1} AND EventID={2} AND OptionNum=1",
                    op1Str,
                    contactingNode.GetNodeID(),
                    thisEvent.ID));
            }
            if (op2Str != "")
            {
                DataSqlConnection.CommandNonQuery(String.Format(
                    "UPDATE options SET Value=\"{0}\" WHERE NodeID={1} AND EventID={2} AND OptionNum=2",
                    op2Str,
                    contactingNode.GetNodeID(),
                    thisEvent.ID));
            }

            string chainData = DataSqlConnection.CommandReturnLine(String.Format(
                "SELECT ThrowNodeID, ThrowEventID, ThrowOp1, ThrowOp2, ThrowRemaining FROM event_chains WHERE TriggerNodeID = {0} AND TriggerEventID = {1}",
                contactingNode.GetNodeID(),
                thisEvent.ID));

            if (chainData != "")
            {
                string[] chainInfo = chainData.Split('\n');
                foreach (string thisChain in chainInfo)
                {
                    if (thisChain == "")
                        continue;
                    string[] chain = thisChain.Split('|');
                    int throwNodeID = Convert.ToInt32(chain[0]);
                    int throwEventID = Convert.ToInt32(chain[1]);
                    string throwOp1 = chain[2];
                    string throwOp2 = chain[3];
                    int throwRemaining = Convert.ToInt32(chain[4]);

                    if (throwRemaining == 1)
                    {
                        DataSqlConnection.CommandNonQuery(String.Format(
                            "DELETE FROM event_chains WHERE TriggerNodeID={0} AND TriggerEventID={1} AND ThrowNodeID={2} AND ThrowEventID={3}",
                            contactingNode.GetNodeID(),
                            thisEvent.ID,
                            throwNodeID,
                            throwEventID));
                    }
                    else if (throwRemaining > 1)
                    {
                        DataSqlConnection.CommandNonQuery(String.Format(
                            "UPDATE event_chains SET ThrowRemaining = ThrowRemaining - 1 WHERE TriggerNodeID={0} AND TriggerEventID={1} AND ThrowNodeID={2} AND ThrowEventID={3}",
                            contactingNode.GetNodeID(),
                            thisEvent.ID,
                            throwNodeID,
                            throwEventID));
                    }

                    string ret = DataSqlConnection.CommandReturnLine(String.Format(
                        "SELECT ForeignAddress, Registered FROM nodes WHERE ID = {0};",
                        throwNodeID));

                    if (ret != "")
                    {
                        XbeeTx64Bit transmit;

                        string forAddr = ret.Split('|')[0];
                        string reg = ret.Split('|')[1];

                        if (reg != "1")
                            continue;

                        ulong addr = Convert.ToUInt64(forAddr);

                        List<byte> data = new List<byte>();
                        data.Add(TRIGGER_BYTE);
                        byte id1 = (byte)((throwEventID & 0x300) >> 8);
                        byte id2 = (byte)(throwEventID & 0xFF);
                        data.Add(id1);
                        data.Add(id2);

                        if (throwOp1 != "")
                        {
                            data.Add((byte)throwOp1.Length);
                            foreach (char c in throwOp1)
                            {
                                data.Add((byte)c);
                            }
                        }
                        if (throwOp2 != "")
                        {
                            data.Add((byte)throwOp2.Length);
                            foreach (char c in throwOp2)
                            {
                                data.Add((byte)c);
                            }
                        }

                        transmit = new XbeeTx64Bit(data, addr);
                        transmit.Send(_serialPort);
                    }
                }
            }

            DataSqlConnection.mutex.ReleaseMutex();
        }

        private static void AddOptionToDatabase(ForeignNode contactingNode, Event currEvent, int opNum, string opDescription)
        {
            DataSqlConnection.mutex.WaitOne();
            string ret = DataSqlConnection.CommandReturnLine(String.Format("SELECT * FROM options WHERE NodeId = {0} AND EventId = {1} AND OptionNum = {2};",
                contactingNode.GetNodeID(),
                currEvent.ID,
                opNum));

            string query;

            if (ret == "")
            {
                query = String.Format(
                    "INSERT INTO options VALUES({0}, {1}, {2}, \"{3}\", null)",
                    contactingNode.GetNodeID(),
                    currEvent.ID,
                    opNum,
                    opDescription);
            }
            else
            {
                query = String.Format(
                    "UPDATE options SET Description = \"{3}\", Value = \"\" WHERE NodeID = {0} AND EventID = {1} AND OptionNum = {2}",
                    contactingNode.GetNodeID(),
                    currEvent.ID,
                    opNum,
                    opDescription);
            }

            DataSqlConnection.CommandNonQuery(query);
            DataSqlConnection.mutex.ReleaseMutex();
        }

        private static void AddEventToDatabase(ForeignNode contactingNode, Event e)
        {
            DataSqlConnection.mutex.WaitOne();
            string ret = DataSqlConnection.CommandReturnLine(String.Format("SELECT * FROM events WHERE NodeId = {0} AND EventId = {1};",
                contactingNode.GetNodeID(),
                e.ID));

            string query;

            int t = 0;
            if (e.Triggerable)
                t = 1;

            if (ret == "")
            {
                query = String.Format(
                "INSERT INTO events VALUES({0}, {1}, \"{2}\", \"{3}\", {4}, 0)",
                contactingNode.GetNodeID(),
                e.ID,
                e.Name,
                e.Description,
                t);
            }
            else
            {
                query = String.Format(
                "UPDATE events SET Name = \"{0}\", Description = \"{1}\", Triggerable = {2}, Count = 0 WHERE NodeId = \"{3}\" AND EventId = \"{4}\"",
                e.Name,
                e.Description,
                t,
                contactingNode.GetNodeID(),
                e.ID);
            }

            DataSqlConnection.CommandNonQuery(query);
            DataSqlConnection.mutex.ReleaseMutex();
        }

        private static void AddNodeToDatabase(ForeignNode contactingNode)
        {
            DataSqlConnection.mutex.WaitOne();
            string ret = DataSqlConnection.CommandReturnLine(String.Format("SELECT ID FROM nodes WHERE ForeignAddress = {0};", contactingNode.GetAddress()));

            if (ret == "")
            {
                DataSqlConnection.CommandNonQuery(String.Format(
                    "INSERT INTO nodes (Name, Alias, Brand, ForeignAddress, AddressHexRep, Registered, IsForeign, Version) " +
                    "VALUES(\"{0}\", \"{1}\", \"{2}\", {3}, \"{4}\", 1, 1, {5})",
                    contactingNode.GetName(),
                    contactingNode.GetAlias(),
                    contactingNode.GetBrand(),
                    contactingNode.GetAddress(),
                    contactingNode.GetHexAddress(),
                    contactingNode.GetVersion()));

                string s = DataSqlConnection.CommandReturnLine(String.Format(
                    "SELECT ID FROM nodes WHERE ForeignAddress = {0};",
                    contactingNode.GetAddress()));

                contactingNode.SetNodeID(Convert.ToInt32(s.Split('\n')[0].Split('|')[0]));

                contactingNode.SetAlias(contactingNode.GetName() + contactingNode.GetNodeID());

                DataSqlConnection.CommandNonQuery(String.Format(
                    "UPDATE nodes SET Alias = \"{0}\", Registered = '1' WHERE ID = {1}",
                    contactingNode.GetAlias(),
                    contactingNode.GetNodeID()));
            }
            else
            {
                contactingNode.SetNodeID(Convert.ToInt32(ret.Split('\n')[0].Split('|')[0]));

                contactingNode.SetAlias(contactingNode.GetName() + contactingNode.GetNodeID());

                DataSqlConnection.CommandNonQuery(String.Format(
                    "UPDATE nodes SET Name = \"{0}\", Alias = \"{1}\", Brand = \"{2}\", ForeignAddress = {3}, AddressHexRep = \"{4}\", Registered = 1, IsForeign = 1, Version = {5} WHERE ID = {6}",
                    contactingNode.GetName(),
                    contactingNode.GetAlias(),
                    contactingNode.GetBrand(),
                    contactingNode.GetAddress(),
                    contactingNode.GetHexAddress(),
                    contactingNode.GetVersion(),
                    contactingNode.GetNodeID()));
            }
            DataSqlConnection.mutex.ReleaseMutex();
        }
    }

    public class DataSqlConnection
    {
        private static bool connected = false;
        public static MySqlConnection thisConnection;
        public static Mutex mutex = new Mutex();

        public static void Connect()
        {
            try
            {
                thisConnection = new MySqlConnection(@"server = 127.0.0.1; database = SeniorProject; uid = root; pwd = Cpgkzxa3");
                thisConnection.Open();
                connected = true;
            }
            catch (SqlException) { }
        }

        public static void CommandNonQuery(string query)
        {
            if (connected)
            {
                MySqlCommand command = thisConnection.CreateCommand();
                command.CommandText = query;
                command.ExecuteNonQuery();
            }
        }

        public static string CommandReturnLine(string query)
        {
            if(connected)
            {
                string response = "";
                MySqlCommand command = thisConnection.CreateCommand();
                command.CommandText = query;
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    for (int i = 0; i < reader.VisibleFieldCount; i++)
                    {
                        response += reader[i] + "|";
                    }
                    response += "\n";
                }

                reader.Close();

                return response;
            }

            return "";
        }

        public static void Close()
        {
            thisConnection.Close();
        }
    }
}