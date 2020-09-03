using SimpleDetector.Converters;
using SimpleDetector.Helpers;
using SimpleDetector.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SimpleDetector
{
    /// <summary>
    /// Interaction logic for Page3.xaml
    /// </summary>
    public partial class Page3 : Page
    {
        string dana, decoded, bitowa, tmp1, tmp2;
        byte[] tablicaBitowa = new byte[1000];
        char[] tablicaZnakow = new char[1000];
        byte[] bity = new byte[1000];

        Hamming hamming;

        public ObservableCollection<BitStatus> DisturbedBits { get; set; }
        int[] input;
        int[] encoded;
        int[] recived;

        public Page3()
        {
            InitializeComponent();
            inputTxt.Text = "123";
            hamming = new Hamming();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            tmp1 = "";
            tmp2 = "";
            dana = inputTxt.Text;

            tablicaBitowa = BitHelper.GetBytes(dana);
            for (int i = 0; i < tablicaBitowa.Length; i++)
            {
                tmp1 += tablicaBitowa[i];
            }
            input = BitHelper.BytesToInts(tablicaBitowa);
            encoded = hamming.GenerateCode(input);
            disturbedTxt.Text = BitHelper.IntsToString(hamming.Disturbed);
            DisturbedBits = GetDisturbedBits();
            DisturbedBitsBox.ItemsSource = DisturbedBits;

            tmp1 = BitHelper.IntsToString(encoded);
            encodedTxt.Text = tmp1;
            bitowa = Formatuj(tablicaBitowa);
            tablicaZnakow = bitowa.ToCharArray();
            binaryInputTxt.Text = FormatCharArray(tablicaZnakow);
            decoded = GetString(tablicaBitowa);
            recivedTxt.Text = decoded;
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DisturbedBitsBox.SelectedIndex = -1;
        }

        private void DecodeBtn_Click(object sender, RoutedEventArgs e)
        {
            int? errorPosition;
            string correctMessage;
            int[] controlBits;
            recived = hamming.Receive(
                DisturbedBits.Where(x => !x.IsSeparator).Select(x => x.Value ? 1 : 0).ToArray(),
                out errorPosition, out correctMessage, out controlBits);
            string recivedBits = BitHelper.IntsToString(recived);
            recivedBitsTxt.Text = recivedBits;
            Debug.WriteLine($"correct message: {correctMessage}");
            byte[] recivedBytes = BinaryToBytes(recivedBits);
            recivedTxt.Text = GetString(recivedBytes);
            if (errorPosition == null)
                errorsNumberTxt.Text = "Brak błędów";
            else
                errorsNumberTxt.Text = errorPosition.Value.ToString();

            var errors = new List<BitStatus>();
            foreach (var bit in DisturbedBits)
                errors.Add(new BitStatus()
                {
                    Value = bit.Value,
                    IsSeparator = bit.IsSeparator,
                    Status = bit.Status == ErroCode.Null ? ErroCode.Ok : bit.Status
                });

            okBitsLbl.Content = new NumberToLabelConverter().Convert(errors.Where(x => x.Status == ErroCode.Ok && !x.IsSeparator).Count(), typeof(string), null, Thread.CurrentThread.CurrentCulture);
            controlBitsLbl.Content = new NumberToLabelConverter().Convert(errors.Where(x => x.Status == ErroCode.ControlBit).Count(), typeof(string), null, Thread.CurrentThread.CurrentCulture);

            if (errorPosition != null)
            {
                errors.Reverse();
                fixedBitsLbl.Content = string.Empty;
                fixedControlLbl.Content = string.Empty;

                if (errors.Where(x => !x.IsSeparator).ToArray()[errorPosition.Value - 1].Status == ErroCode.ControllBitError)
                {
                    errors.Where(x => !x.IsSeparator).ToArray()[errorPosition.Value - 1].Status = ErroCode.ControlBitFixed;
                    fixedControlLbl.Content = new NumberToLabelConverter().Convert(1, typeof(string), null, Thread.CurrentThread.CurrentCulture);
                }
                else
                {
                    errors.Where(x => !x.IsSeparator).ToArray()[errorPosition.Value - 1].Status = ErroCode.ErrorFixed;
                    fixedBitsLbl.Content = new NumberToLabelConverter().Convert(1, typeof(string), null, Thread.CurrentThread.CurrentCulture);
                }
                errors.Where(x => !x.IsSeparator).ToArray()[errorPosition.Value - 1].Value ^= true;
                errors.Reverse();
                notDetectdLbl.Content = new NumberToLabelConverter().Convert(errors.Where(x => x.Status == ErroCode.ErrorNotDetected).Count(), typeof(string), null, Thread.CurrentThread.CurrentCulture);
            }
            ErrorsBx.ItemsSource = errors;

            if (controlBits.Where(x => x != 0).Count() > 0)
            {
                controBitsTxt.Text = string.Join(", ", controlBits.Where(x => x != 0).Select(x => $"p{x}"));
            }
        }

        private void DisturbedBitsBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = DisturbedBitsBox.SelectedItem as BitStatus;
            if (selected != null)
            {
                if (selected?.Status == ErroCode.Null)
                    selected.Status = ErroCode.ErrorNotDetected;
                else if (selected?.Status == ErroCode.ControlBit)
                    selected.Status = ErroCode.ControllBitError;
                else if (selected?.Status == ErroCode.ControllBitError)
                    selected.Status = ErroCode.ControlBit;
                else
                    selected.Status = ErroCode.Null;
                selected.Value = !selected.Value;
                DisturbedBits.FirstOrDefault(x => x == selected).Status = selected.Status;
            }
            disturbedTxt.Text = String.Concat(DisturbedBits.Select(x => x.IsSeparator == true ? ' ' : x.Value ? '1' : '0'));
        }

        private string FormatCharArray(char[] data)
        {
            string tmp2 = string.Empty;
            for (int i = 0; i < data.Length; i++)
            {
                if (i > 0 && ((i % 8) - 7) == 0)
                {
                    tmp2 = tmp2 + data[i] + " ";
                }
                else
                    tmp2 += data[i];
            }
            return tmp2;
        }

        private static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            string str;
            //System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            //chars = Encoding.Default.GetString(bytes);
            str = Encoding.Default.GetString(bytes);
            return str;
            //return new string(chars);
        }

        private static string Formatuj(byte[] data)
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

        private ObservableCollection<BitStatus> GetDisturbedBits()
        {
            var list = new List<BitStatus>();
            for (int i = 0; i < hamming.Disturbed.Length; i++)
            {
                list.Add(new BitStatus()
                {
                    Value = hamming.Disturbed[i] == 1 ? true : false,
                    Status = ErroCode.Null,
                    Label = $"d{ hamming.Disturbed.Length - i}"
                });
                if ((i + 1) % 8 == 0 && i != 0)
                    list.Add(new BitStatus()
                    {
                        IsSeparator = true
                    });

            }
            int j = 0;
            list.Reverse();
            while (Math.Pow(2, j) - 1 < list.Where(x => !x.IsSeparator).ToList().Count)
            {
                list.Where(x => !x.IsSeparator).ToList()[(int)Math.Pow(2, j) - 1].Status = ErroCode.ControlBit;
                list.Where(x => !x.IsSeparator).ToList()[(int)Math.Pow(2, j) - 1].Label = $"p{(int)Math.Pow(2, j)}";
                j++;
            }
            list.Reverse();
            if (list.FirstOrDefault() != null && list.FirstOrDefault().IsSeparator)
                list.RemoveAt(0);
            return new ObservableCollection<BitStatus>(list);
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