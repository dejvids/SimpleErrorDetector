using SimpleDetector.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleDetector.Models
{
    public class Parity : ICodesIdentifier
    {
        public int Errors { get; private set; }
        public BitArray Original { get; set; }
        public BitArray Encoded { get; set; }
        public BitArray Decoded { get; set; }
        public BitArray Disturbed { get; set; }
        public ErroCode[] ErrorCodes { get; set; }
        public Parity()
        {

        }
        public byte[] encode(byte[] inputBytes)
        {
            byte[] outputBytes = new byte[inputBytes.Length];
            ErrorCodes = new ErroCode[inputBytes.Length * 8];
            this.Original = BitHelper.ReverseBytesToBitArray(inputBytes);
            string str = string.Empty;
            foreach (byte b in inputBytes)
                str += b.ToString("x2").ToLower();
            Console.WriteLine("Parity is {0}", str);

            for (int j = 0; j < inputBytes.Length; j++)
            {
                BitArray bits = new BitArray(new byte[] { inputBytes[j] });
                int counter = 0;
                for (int i = 0; i < bits.Length; i++)
                {
                    if (bits[i])
                        counter++;
                }
                if (counter % 2 == 1)
                {
                    bits[7] = true;
                }
                ErrorCodes[j * 8 ] = ErroCode.ControlBit;
                bits.CopyTo(outputBytes, j);

            }
            Encoded = BitHelper.ReverseBytesToBitArray(outputBytes);
            this.Disturbed = new BitArray(Encoded);
            return outputBytes;
        }


        public byte[] decode(byte[] data)
        {
            Disturbed = new BitArray(BitHelper.ReverseBytesToBitArray(data));
            Errors = 0;
            byte[] output = new byte[data.Length];
            ResetErrorCodes();
            for (int i = 0; i < data.Length; i++)
            {
                BitArray bits = new BitArray(new byte[] { data[i] });
                short counter = 0;
                for (int j = 0; j < bits.Length; j++)
                {
                    if (bits[j])
                        counter++;
                }
                if (counter % 2 == 0)
                {
                    bits[7] = false;
                }
                else
                {
                    Errors++;
                    MarkErrorByte(i);

                }
                bits.CopyTo(output, i);
            }
            Decoded = BitHelper.ReverseBytesToBitArray(output);
            FindNotDetectedErrors();
            return output;
        }

        private void ResetErrorCodes()
        {
            for (int i = 0; i < ErrorCodes.Length; i++)
            {
                if (ErrorCodes[i] != ErroCode.ControlBit && ErrorCodes[i] != ErroCode.ControllBitError)
                    ErrorCodes[i] = ErroCode.Ok;
                else
                    ErrorCodes[i] = ErroCode.ControlBit;
            }
        }

        private void MarkErrorByte(int i)
        {
            for (int j = 1; j <8 ; j++)
            {
                ErrorCodes[i * 8 + j] = ErroCode.ErrorDetected;
            }
        }

        private void FindNotDetectedErrors()
        {
            for (int i = 0; i < Original.Length; i++)
            {
                if(Encoded[i] != Disturbed[i] && (ErrorCodes[i] ==ErroCode.Ok || ErrorCodes[i] == ErroCode.ControlBit))
                {
                    if(ErrorCodes[i] == ErroCode.Ok)
                        ErrorCodes[i] = ErroCode.ErrorNotDetected;
                    else
                        ErrorCodes[i] = ErroCode.ControllBitError;
                }
            }
        }
    }
}
