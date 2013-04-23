using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace COM_Test
{
    class Program
    {
        static SerialPort _serialPort;
        static bool _continue;
        public static void Main()
        {
            string message;
            StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
            Thread readThread = new Thread(Read);

            // Create a new SerialPort object with default settings.
            _serialPort = new SerialPort();


            // Allow the user to set the appropriate properties.
            _serialPort.PortName = SetPortName(_serialPort.PortName);
            _serialPort.BaudRate = SetPortBaudRate(_serialPort.BaudRate);
            _serialPort.Parity = SetPortParity(_serialPort.Parity);
            _serialPort.DataBits = SetPortDataBits(_serialPort.DataBits);
            _serialPort.StopBits = SetPortStopBits(_serialPort.StopBits);
            _serialPort.Handshake = SetPortHandshake(_serialPort.Handshake);
            _serialPort.Encoding = Encoding.UTF8;


            // Set the read/write timeouts
            _serialPort.ReadTimeout = 1500;
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
            }

            readThread.Join();
            _serialPort.Close();
        }

        public static void Read()
        {
            while (_continue)
            {
                try
                {
                    int message = ReadByte();

                    if (message == 0x7E)
                    {
                        int length;

                        int MSB = ReadByte();
                        int LSB = ReadByte();
                        length = (MSB << 8) | LSB;

                        int ID = ReadByte();

                        ulong source = 0;
                        int sourceBytes = 0;

                        if (ID == 0x81)
                        {
                            sourceBytes = 2;
                            //source = (ReadByte() << 8) | ReadByte();
                        }
                        else if (ID == 0x80)
                        {
                            sourceBytes = 8;
                        }

                        for (int x = 0; x < sourceBytes; x++)
                        {
                            source = (source << 8) | (uint)ReadByte();
                        }

                        int RSSI = ReadByte();
                        int options = ReadByte();

                        List<int> data = new List<int>();

                        for (int i = 0; i < length - (3 + sourceBytes); i++)
                        {
                            data.Add(ReadByte());
                        }

                        int checksum = ReadByte();

                        Console.WriteLine("Message received. Source: {0:X}", source);
                        Console.Write("Message: ");
                        foreach (char i in data)
                        {
                            Console.Write("{0}", i);
                        }
                        Console.WriteLine();

                    }
                }
                catch (TimeoutException) { }
            }
        }

        public static int ReadByte()
        {
            int readByte;

            readByte = _serialPort.ReadByte();
            if (readByte == 0x7D)
            {
                readByte = _serialPort.ReadByte() ^ 0x20;
            }

            return readByte;
        }

        public static string SetPortName(string defaultPortName)
        {
            string portName;

            Console.WriteLine("Available Ports:");
            foreach (string s in SerialPort.GetPortNames())
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("COM port({0}): ", defaultPortName);
            portName = Console.ReadLine();

            if (portName == "")
            {
                portName = defaultPortName;
            }
            return portName;
        }

        public static int SetPortBaudRate(int defaultPortBaudRate)
        {
            string baudRate;

            Console.Write("Baud Rate({0}): ", defaultPortBaudRate);
            baudRate = Console.ReadLine();

            if (baudRate == "")
            {
                baudRate = defaultPortBaudRate.ToString();
            }

            return int.Parse(baudRate);
        }

        public static Parity SetPortParity(Parity defaultPortParity)
        {
            string parity;

            Console.WriteLine("Available Parity options:");
            foreach (string s in Enum.GetNames(typeof(Parity)))
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Parity({0}):", defaultPortParity.ToString());
            parity = Console.ReadLine();

            if (parity == "")
            {
                parity = defaultPortParity.ToString();
            }

            return (Parity)Enum.Parse(typeof(Parity), parity);
        }

        public static int SetPortDataBits(int defaultPortDataBits)
        {
            string dataBits;

            Console.Write("Data Bits({0}): ", defaultPortDataBits);
            dataBits = Console.ReadLine();

            if (dataBits == "")
            {
                dataBits = defaultPortDataBits.ToString();
            }

            return int.Parse(dataBits);
        }

        public static StopBits SetPortStopBits(StopBits defaultPortStopBits)
        {
            string stopBits;

            Console.WriteLine("Available Stop Bits options:");
            foreach (string s in Enum.GetNames(typeof(StopBits)))
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Stop Bits({0}):", defaultPortStopBits.ToString());
            stopBits = Console.ReadLine();

            if (stopBits == "")
            {
                stopBits = defaultPortStopBits.ToString();
            }

            return (StopBits)Enum.Parse(typeof(StopBits), stopBits);
        }

        public static Handshake SetPortHandshake(Handshake defaultPortHandshake)
        {
            string handshake;

            Console.WriteLine("Available Handshake options:");
            foreach (string s in Enum.GetNames(typeof(Handshake)))
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Handshake({0}):", defaultPortHandshake.ToString());
            handshake = Console.ReadLine();

            if (handshake == "")
            {
                handshake = defaultPortHandshake.ToString();
            }

            return (Handshake)Enum.Parse(typeof(Handshake), handshake);
        }
    }
}
