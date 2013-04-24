using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeniorProjectService
{
    public class XbeeRx64Bit
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
        /// <summary>
        /// Parse an entire incoming message per Xbee documentation. The Frame delimiter has already been read from the stream.
        /// </summary>
        /// <param name="_serialPort">COM port to read the stream from</param>
        /// <returns>Returns true if the cheksums match</returns>
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

        /// <summary>
        /// Returns the unformatted List comprising the Data
        /// </summary>
        /// <returns>Data to be returned</returns>
        public List<int> GetMessage()
        {
            return data;
        }

        /// <summary>
        /// Returns the source address
        /// </summary>
        /// <returns>Source address</returns>
        public ulong GetRemoteAddress()
        {
            return source;
        }

        /*********************************PRIVATE FUNCTIONS*********************************/
        /// <summary>
        /// Calculates the checksum from the components of the bitstream
        /// </summary>
        /// <returns>Returns the checksum</returns>
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

        /// <summary>
        /// Reads a byte from the specified COM stream. It will detect an escape byte and properly format the following byte.
        /// </summary>
        /// <param name="_serialPort">COM stream from which we are reading</param>
        /// <returns>Returns the properly formatted next byte in the stream</returns>
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

    public class XbeeTx64Bit
    {
        /* Constants */
        const byte API_ID = 0x00;
        const byte OPTION_NONE = 0x00;
        const byte OPTION_DISABLE_ACK = 0X01;
        const byte OPTION_SEND_WITH_BROADCAST_ID = 0x04;
        const byte FRAME_DELIMITER = 0X7E;
        const ulong BROADCAST_ADDRESS = 0xFFFF;

        /* Static outgoing ID */
        static byte msgID = 0;

        ulong destination;
        byte options;
        List<byte> data = new List<byte>();

        int length;

        byte checksum;

        List<byte> byteStream = new List<byte>();

        /// <summary>
        /// Constructor for a 64 bit Xbee Tx message
        /// </summary>
        /// <param name="_data">Data to send across. Limit 100 bytes</param>
        /// <param name="_destination">Destination address. Default is BROADCAST_ADDRESS</param>
        /// <param name="_options">Options for this packet. Default is OPTION_NONE</param>
        public XbeeTx64Bit(List<byte> _data, ulong _destination = BROADCAST_ADDRESS, byte _options = OPTION_NONE)
        {
            data = _data;
            destination = _destination;
            options = _options;

            byteStream.Add(FRAME_DELIMITER);
            length = data.Count + 11;

            //Add MSB followed by LSB to the byte stream
            byteStream.Add((byte)((length >> 8) & 0xFF));
            byteStream.Add((byte)(length & 0xFF));

            //Add API ID to the byte stream
            byteStream.Add(API_ID);

            //Add msg ID to the byte stream
            byteStream.Add(msgID++);

            //Add Destination Address to the byte stream
            for (int i = 7; i >= 0; i--)
            {
                byteStream.Add((byte)((destination >> i * 8) & 0xFF));
            }

            byteStream.Add(options);

            byteStream.AddRange(data);

            byteStream.Add(CalculateChecksum());

            for (int i = 1; i < byteStream.Count; i++)
            {
                if (byteStream[i] == 0x7E || byteStream[i] == 0x7D || byteStream[i] == 0x11 || byteStream[i] == 0x13)
                {
                    byteStream[i] = (byte)(byteStream[i] | 0x20);
                    byteStream.Insert(i, (byte)0x7D);
                    i++;
                }
            }
        }

        private byte CalculateChecksum()
        {
            checksum = 0;

            for (int i = 3; i < byteStream.Count; i++)
            {
                checksum += byteStream[i];
            }
            checksum = (byte)(checksum & 0xFF);
            checksum = (byte)(0xFF - checksum);

            return checksum;
        }

        public void Send(SerialPort _serialPort)
        {
            _serialPort.Write(byteStream.ToArray(), 0, byteStream.Count);
        }
    }
}
