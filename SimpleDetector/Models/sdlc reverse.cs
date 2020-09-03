using SimpleDetector.Helpers;
using System;
using System.Linq;

namespace SimpleDetector.Models
{
    class SdlcReverse : CRC
    {
        ushort[] crcTable = new ushort[256];
        ushort polynomial;
        public SdlcReverse()
        {
            this.polynomial = 0x881;
            this.keyLength = 2;
        }
        public override byte[] Encode(byte[] data, bool checking)
        {
            InitializeCrcTable();
            ushort crc = 0;
            for (int i = 0; i < data.Length; ++i)
            {
                byte index = (byte)(crc ^ data[i]);
                crc = (ushort)((crc >> 8) ^ crcTable[index]);
            }
            byte[] encoded = BitConverter.GetBytes(crc).Reverse().ToArray();
            var result = encoded.Concat(data).ToArray();
            if (checking == false)
            {
                this.Original = BitHelper.ReverseBytesToBitArray(data); ;
                this.Encoded = BitHelper.ReverseBytesToBitArray(result);
            }
            return encoded.Concat(data).ToArray();
        }

        public override void InitializeCrcTable()
        {
            ushort value;
            for (ushort i = 0; i < crcTable.Length; ++i)
            {
                value = i;
                for (byte j = 0; j < 8; ++j)
                {
                    if ((value & 1) == 1)
                    {
                        value = (ushort)((value >> 1) ^ polynomial);
                    }
                    else
                    {
                        value >>= 1;
                    }
                }
                crcTable[i] = value;
            }
        }
    }
}
