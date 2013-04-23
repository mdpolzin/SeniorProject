using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SeniorProjectService
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
    }
}
