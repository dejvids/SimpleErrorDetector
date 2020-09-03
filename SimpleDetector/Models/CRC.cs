using SimpleDetector.Helpers;
using System;
using System.Collections;
using System.Linq;

namespace SimpleDetector.Models
{

    public abstract class CRC : ICodesIdentifier
    {
        public int keyLength;
        public BitArray Original { get; set; }
        public BitArray Encoded { get; set; }
        public BitArray Decoded { get; set; }
        public BitArray Disturbed { get; set; }

        public ErroCode[] ErrorCodes { get; private set; }


        public abstract byte[] Encode(byte[] data, bool checking = false);

        public abstract void InitializeCrcTable();

        public byte[] Decode(byte[] data)
        {
            byte[] decoded = new byte[data.Length - keyLength];
            decoded = data.Skip(keyLength).ToArray();
            var decodedBits = new byte[decoded.Length + keyLength];
            data.Take(keyLength).ToArray().CopyTo(decodedBits, 0);
            decoded.CopyTo(decodedBits, keyLength);
            //for(int i=0; i < decodedBits.Length; i++)
            //{
            //    decodedBits[i] = Reverse(decodedBits[i]);
            //}
            this.Decoded = BitHelper.ReverseBytesToBitArray(decodedBits);
            ErrorCodes = SetErrorCodes(data);
            return decoded;
        }

        private ErroCode[] SetErrorCodes(byte[] data)
        {
            byte[] crc;
            bool isErrorDetected = !Check(data, out crc);
            this.Disturbed = new BitArray(BitHelper.ReverseBytes(crc.Concat(data.Skip(keyLength)).ToArray()));
            var codes = new ErroCode[Encoded.Count];
            for (int i = 0; i < keyLength * 8; i++)
            {
                if (Decoded[i] == Encoded[i])
                    codes[i] = ErroCode.ControlBit;
                else
                    codes[i] = ErroCode.ControllBitError;
            }
            for (int i = 0; i < Decoded.Count - keyLength * 8; i++)
            {
                if (isErrorDetected)
                    codes[i + keyLength * 8] = ErroCode.ErrorDetected;
                else
                {
                    if (Original[i] == Decoded[i + keyLength * 8])
                    {
                        codes[i + keyLength * 8] = ErroCode.Ok;
                    }
                    else
                    {
                        codes[i + keyLength * 8] = ErroCode.ErrorNotDetected;
                    }
                }
            }
            return codes;
        }

        public bool Check(byte[] encoded, out byte[] newCrc)
        {
            byte[] encodedCrc = encoded.Take(keyLength).ToArray();
            byte[] data = encoded.Skip(keyLength).ToArray();
            byte[] controlData = this.Encode(data, true).Take(keyLength).ToArray();
            newCrc = controlData;
            for (int i = 0; i < encodedCrc.Length; i++)
                if (encodedCrc[i] != controlData[i])
                    return false;
            return true;
        }



    }
}
