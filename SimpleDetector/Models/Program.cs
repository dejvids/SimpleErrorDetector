using System;
using System.Text;

namespace SimpleDetector.Models
{
    class Program
    {
        //zamienia string na tablicę heksadecymalną
        public static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            //System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            bytes = Encoding.ASCII.GetBytes(str);
            return bytes;
        }
        //zamienia tablicę heksadecymanlną na string 
        public static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            string str;
            //System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            //chars = Encoding.Default.GetString(bytes);
            str = Encoding.Default.GetString(bytes);
            return str;
            //return new string(chars);
        }

        //Formats a byte[] into a binary string (010010010010100101010)
        public static string Formatuj(byte[] data)
        {
            //storage for the resulting string
            string result = string.Empty;
            //iterate through the byte[]
            foreach (byte value in data)
            {
                //storage for the individual byte
                string binarybyte = Convert.ToString(value, 2);
                //if the binarybyte is not 8 characters long, its not a proper result
                while (binarybyte.Length < 8)
                {
                    //prepend the value with a 0
                    binarybyte = "0" + binarybyte;
                }
                //append the binarybyte to the result
                result += binarybyte;
            }
            //return the result
            return result;
        }
    }
}
