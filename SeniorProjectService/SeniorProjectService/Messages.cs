using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeniorProjectService
{
    class XbeeRx64Bit
    {
        /*********************************DATA MEMBERS*********************************/
        private int length;
        private int lengthMSB;
        private int lengthLSB;

        private int APIid;

        private ulong source;
        private int sourceBytes;

        private int RSSI;
        private int options;

        private List<int> data = new List<int>();

        private int checksum;
        private int internalChecksum;

        /*********************************PUBLIC FUNCTIONS*********************************/
        public bool ParseIncomingMessage(SerialPort _serialPort)
        {
            lengthMSB = ReadByte(_serialPort);
            lengthLSB = ReadByte(_serialPort);
            length = (lengthMSB << 8) | lengthLSB;

            APIid = ReadByte(_serialPort);

            source = 0;
            sourceBytes = 0;

            if (APIid == 0x81)
            {
                sourceBytes = 2;
            }
            else if (APIid == 0x80)
            {
                sourceBytes = 8;
            }

            for (int x = 0; x < sourceBytes; x++)
            {
                source = (source << 8) | (uint)ReadByte(_serialPort);
            }

            RSSI = ReadByte(_serialPort);
            options = ReadByte(_serialPort);

            for (int i = 0; i < length - (3 + sourceBytes); i++)
            {
                data.Add(ReadByte(_serialPort));
            }

            checksum = ReadByte(_serialPort);

            internalChecksum = CalculateInternalChecksum();

            return internalChecksum == checksum;
        }

        public List<int> GetMessage()
        {
            return data;
        }

        /*********************************PRIVATE FUNCTIONS*********************************/

        private int CalculateInternalChecksum()
        {
            int ret = 0;

            ret += APIid;
            for (int i = 0; i < sourceBytes; i++)
            {
                int addition = (int)(source >> (i * 8)) & 0xFF;
                ret += addition;
            }
            ret += RSSI;
            ret += options;
            foreach (int i in data)
            {
                ret += i;
            }
            ret = ret & 0xFF;
            ret = 0xFF - ret;

            return ret;
        }

        private int ReadByte(SerialPort _serialPort)
        {
            int readByte;

            readByte = _serialPort.ReadByte();
            if (readByte == 0x7D)
            {
                readByte = _serialPort.ReadByte() ^ 0x20;
            }

            return readByte;
        }
    }
}
