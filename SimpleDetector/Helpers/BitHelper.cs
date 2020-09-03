using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleDetector.Helpers
{
    public static class BitHelper
    {
        private static byte Reverse(byte inByte)
        {
            byte result = 0x00;

            for (byte mask = 0x80; Convert.ToInt32(mask) > 0; mask >>= 1)
            {
                // shift right current result
                result = (byte)(result >> 1);

                // tempbyte = 1 if there is a 1 in the current position
                var tempbyte = (byte)(inByte & mask);
                if (tempbyte != 0x00)
                {
                    // Insert a 1 in the left
                    result = (byte)(result | 0x80);
                }
            }

            return (result);
        }
        public static BitArray ReverseBytesToBitArray(byte[] bytes)
        {
            var temp = ReverseBytes(bytes);
            var result = new BitArray(temp);
            return result;
        }

        public static byte[] ReverseBytes(byte[] bytes)
        {
            var temp = new byte[bytes.Length];
            for (int i = 0; i < bytes.Length; i++)
            {
                temp[i] = Reverse(bytes[i]);
            }
            return temp;
        }

        public static string ByteToHex(byte[] data)
        {
            string hash = string.Empty;
            foreach (byte b in data)
                hash += b.ToString("x2").ToLower();
            return "0x" + hash;
        }

        public static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            bytes = Encoding.ASCII.GetBytes(str);
            return bytes;
        }

        public static int[] BytesToInts(byte[] bytes)
        {
            List<int> converted = new List<int>();
            BitArray bits = new BitArray(bytes);
            foreach (bool b in bits)
            {
                converted.Add(b ? 1 : 0);
            }
            converted.Reverse();
            return converted.ToArray();
        }

        public static int[] ReversIntArray(int[] data)
        {
            int n = data.Length % 8 == 0 ? data.Length / 8 : data.Length / 8 + 1;
            string[] bytes = new string[n];
            for (int i = 0; i < n; i++)
            {
                string b = string.Empty;
                for (int j = 0; j < 8; j++)
                {
                    b += data[i * 8 + j];
                }
                bytes[i] = b;
            }

            bytes = bytes.Reverse().ToArray();
            List<int> output = new List<int>();
            string str = string.Join("", bytes);
            foreach (var c in str)
            {
                output.Add(Int32.Parse(c.ToString()));
            }

            return output.ToArray();
        }

        public static string IntsToString(int[] data)
        {
            string result = string.Empty;

            foreach (int bit in data)
            {
                result += bit.ToString();
            }
            for (int i = 0; i < data.Length/8; i++)
            {

            }
            return result;
        }
    }
}
