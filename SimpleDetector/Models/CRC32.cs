using SimpleDetector.Helpers;
using System;
using System.Linq;

namespace SimpleDetector.Models
{
    public class Crc32 : CRC
    {
        UInt32[] crcTable = new UInt32[256];
        UInt32 polynomial;
        public Crc32()
        {
            this.polynomial = 0xedb88320u;
            this.keyLength = 4;
        }

        public override byte[] Encode(byte[] data, bool checking = false)
        {
            InitializeCrcTable();
            UInt32 crc = 0xffffffffu;
            for (int i = 0; i < data.Length; ++i)
            {
                byte index = (byte)(crc ^ data[i]);
                crc = (UInt32)((crc >> 8) ^ crcTable[index]);
            }
            var encoded = UInt32ToBigEndianBytes(~crc);
            if (checking == false)
            {
                this.Original = BitHelper.ReverseBytesToBitArray(data); ;
                Encoded = BitHelper.ReverseBytesToBitArray(encoded.Concat(data).ToArray());
            }
            return encoded.Concat(data).ToArray();
        }

        public override void InitializeCrcTable()
        {
            for (var i = 0; i < 256; i++)
            {
                UInt32 entry = (UInt32)i;
                for (var j = 0; j < 8; j++)
                    if ((entry & 1) == 1)
                        entry = (entry >> 1) ^ polynomial;
                    else
                        entry = entry >> 1;
                crcTable[i] = entry;
            }
        }

        byte[] UInt32ToBigEndianBytes(UInt32 uint32)
        {
            var result = BitConverter.GetBytes(uint32);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(result);

            return result;
        }
    }
}
