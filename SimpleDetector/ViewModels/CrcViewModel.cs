using SimpleDetector.Helpers;
using SimpleDetector.Models;
using ReactiveUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleDetector.ViewModels
{
    public enum CrcType
    {
        [Description("CRC 16")]
        Crc16,
        [Description("CRC 32")]
        Crc32,
        [Description("CRC-ITU")]
        CrcITU,
        [Description("CRC SDLC REVERSE")]
        CrcSdlcReverse
    }
    public class CrcViewModel : ReactiveObject
    {
        #region privs
        CRC crc;
        Crc16 crc16;
        CRCITU crcItu;
        SdlcReverse sdlc;
        Crc32 crc32;
        CrcType selectedCrc = CrcType.Crc16;
        byte[] inputBytes;
        byte[] encodedBytes;
        string inputData;
        string disturbed;
        string encoded;
        string input;
        string originalCrc;
        string disturbedCrc;
        string controlCrc;
        BitStatus selectedBit;
        #endregion

        #region props
        public List<CrcType> CrcTypes { get; set; }
        public CrcType SelectedCrc
        {
            get { return selectedCrc; }
            set
            {
                switch (value)
                {
                    case CrcType.Crc16:
                        crc = crc16;
                        break;
                    case CrcType.CrcITU:
                        crc = crcItu;
                        break;
                    case CrcType.CrcSdlcReverse:
                        crc = sdlc;
                        break;
                    case CrcType.Crc32:
                        crc = crc32;
                        break;
                }
                this.RaiseAndSetIfChanged(ref selectedCrc, value);
            }
        }
        public List<BitStatus> Stats { get; private set; }
        public ObservableCollection<BitStatus> DisturbedBits { get; set; }
        public string InputData
        {
            get { return inputData; }
            set { this.RaiseAndSetIfChanged(ref inputData, value); }
        }
        public string Input
        {
            get { return input; }
            private set { this.RaiseAndSetIfChanged(ref input, value); }
        }
        public string Encoded
        {
            get { return encoded; }
            set { this.RaiseAndSetIfChanged(ref encoded, value); }
        }
        public string Disturbed
        {
            get { return disturbed; }
            set { this.RaiseAndSetIfChanged(ref disturbed, value); }
        }
        public string Decoded { get; set; }

        public BitStatus SelectedBit
        {
            get { return selectedBit; }
            set
            {
                if (value != null)
                {
                    if (value.Status == ErroCode.Null)
                        value.Status = ErroCode.ErrorNotDetected;
                    else if(value.Status == ErroCode.ErrorNotDetected)
                        value.Status = ErroCode.Null;
                    value.Value = !value.Value;
                    DisturbedBits.FirstOrDefault(x => x == value).Status = value.Status;
                }
                Disturbed = String.Concat(DisturbedBits.Select(x => x.IsSeparator == true ? ' ' : x.Value ? '1' : '0'));
                //this.RaiseAndSetIfChanged(ref selectedBit, value);
                this.RaisePropertyChanged(nameof(DisturbedBits));
            }
        }
        public int NumberOfOkBits { get; set; } = -1;
        public int NumberOfDetectedErrors { get; set; } = -1;
        public int NumberOfNotDetectedErrors { get; set; } = -1;
        public int NumberOfErrorControlBits { get; set; } = -1;
        public int NumberOfControlBits { get; set; } = -1;

        public string OriginalCrc
        {
            get { return originalCrc; }
            set { this.RaiseAndSetIfChanged(ref originalCrc, value); }
        }
        public string DisturbedCrc
        {
            get { return disturbedCrc; }
            set { this.RaiseAndSetIfChanged(ref disturbedCrc, value); }
        }

        public string ControlCrc
        {
            get { return controlCrc; }
            set { this.RaiseAndSetIfChanged(ref controlCrc, value); }
        }
        #endregion

        #region Commands
        public ReactiveCommand EncodeCmd { get; private set; }
        public ReactiveCommand DecodeCmd { get; private set; }
        #endregion

        public CrcViewModel()
        {
            InputData = "123456";
            CrcTypes = CrcTypes = new List<CrcType>() { CrcType.Crc16, CrcType.Crc32, CrcType.CrcITU, CrcType.CrcSdlcReverse };
            crc16 = new Crc16();
            crc32 = new Crc32();
            crcItu = new CRCITU();
            sdlc = new SdlcReverse();
            this.RaisePropertyChanged(nameof(CrcTypes));
            crc = crc16;

            EncodeCmd = ReactiveCommand.Create(() =>
            {
                inputBytes = StringToBytes(InputData);
                Input = BytesToBinary(inputBytes);
                encodedBytes = crc.Encode(inputBytes);
                OriginalCrc = BitHelper.ByteToHex(encodedBytes.Take(crc.keyLength).ToArray());
                Encoded = BytesToBinary(encodedBytes);
                Disturbed = Encoded;
                DisturbedBits = GetDisturbedBits();
                this.RaisePropertyChanged(nameof(DisturbedBits));
            }, this.WhenAnyValue(i => i.InputData, i => !string.IsNullOrEmpty(i) && !i.Any(x => (int)x > 127)));

            DecodeCmd = ReactiveCommand.Create(() =>
            {
                var disturbedBytes = ExtractDisturbetBits();
                var decoded = crc.Decode(disturbedBytes);
                byte[] newCrc;
                crc.Check(disturbedBytes,out newCrc);
                Decoded = BytesToBinary(decoded);
                Stats = GetStats();
                CalculateNumberOfErrors();
                this.RaisePropertyChanged(nameof(Decoded));
                this.RaisePropertyChanged(nameof(Stats));
                DisturbedCrc = BitHelper.ByteToHex(disturbedBytes.Take(crc.keyLength).ToArray());
                ControlCrc = BitHelper.ByteToHex(newCrc);
            }, this.WhenAnyValue(e => e.Encoded, e => !string.IsNullOrEmpty(e)));
        }

        private void CalculateNumberOfErrors()
        {
            NumberOfOkBits = Stats.Where(x => x.Status == ErroCode.Ok).Count();
            NumberOfNotDetectedErrors = Stats.Where(x => x.Status == ErroCode.ErrorNotDetected).Count();
            NumberOfDetectedErrors = Stats.Any(x => x.Status == ErroCode.ErrorDetected) ? 1 : 0;
            NumberOfControlBits = Stats.Where(x => x.Status == ErroCode.ControlBit).Count();
            NumberOfErrorControlBits = Stats.Where(x => x.Status == ErroCode.ControllBitError).Count();
            this.RaisePropertyChanged(nameof(NumberOfOkBits));
            this.RaisePropertyChanged(nameof(NumberOfDetectedErrors));
            this.RaisePropertyChanged(nameof(NumberOfNotDetectedErrors));
            this.RaisePropertyChanged(nameof(NumberOfControlBits));
            this.RaisePropertyChanged(nameof(NumberOfErrorControlBits));
        }

        private byte[] ExtractDisturbetBits()
        {
            BitArray temp = new BitArray(DisturbedBits.Where(b => b.IsSeparator == false).Select(b => b.Value).ToArray());
            byte[] bytes = new byte[DisturbedBits.Where(b => b.IsSeparator == false).Count() / 8];
            temp.CopyTo(bytes, 0);
            return BitHelper.ReverseBytes(bytes);
        }

        private ObservableCollection<BitStatus> GetDisturbedBits()
        {
            var list = new ObservableCollection<BitStatus>();
            for (int i = 0; i < crc.Encoded.Length; i++)
            {
                list.Add(new BitStatus()
                {
                    Value = crc.Encoded[i],
                    Status = ErroCode.Null
                });
                if ((i + 1) % 8 == 0 && i != 0)
                    list.Add(new BitStatus()
                    {
                        IsSeparator = true
                    });
            }
            return list;
        }

        private List<BitStatus> GetStats()
        {
            var stats = new List<BitStatus>();
            for (int i = 0; i < crc.ErrorCodes.Length; i++)
            {

                stats.Add(new BitStatus()
                {
                    Value = crc.Disturbed[i],
                    Status = crc.ErrorCodes[i],
                });
                if ((i + 1) % 8 == 0 && i != 0)
                    stats.Add(new BitStatus()
                    {
                        Status = ErroCode.Blank,
                        IsSeparator = true
                    });
            }
            return stats;
        }

        private string BytesToBinary(byte[] data)
        {
            if (data == null)
                return string.Empty;
            string binary = string.Empty;
            for (int i = 0; i < data.Length; i++)
            {
                binary += (Convert.ToString(data[i], 2).PadLeft(8, '0') + " ");
            }
            return binary;
        }

        private byte[] StringToBytes(string input)
        {
            byte[] inputBytes = new byte[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                inputBytes[i] = (byte)input[i];
            }
            return inputBytes;
        }

        private byte[] BinaryToBytes(string input)
        {
            input = input.Replace(" ", string.Empty);
            int numOfBytes = input.Length / 8;
            byte[] bytes = new byte[numOfBytes];
            for (int i = 0; i < numOfBytes; ++i)
            {
                bytes[i] = Convert.ToByte(input.Substring(8 * i, 8), 2);
            }
            return bytes;
        }


    }
}
