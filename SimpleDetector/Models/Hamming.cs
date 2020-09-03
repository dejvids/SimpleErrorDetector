using SimpleDetector.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SimpleDetector.Models
{
    class Hamming
    {

        int[] inMessage;
        string outMessage;
        int errorIndex;
        string correctMessage;

        public Hamming()
        {    
        }

        public int[] Disturbed { get; private set; }

        public int[] GenerateCode(int[] a)
        {
            a = BitHelper.ReversIntArray(a);

            inMessage = new int[a.Length];
            inMessage = a;
            for (int i = 0; i < inMessage.Length / 2; i++)
            {
                int temp = inMessage[i];
                inMessage[i] = inMessage[inMessage.Length - i - 1];
                inMessage[inMessage.Length - i - 1] = temp;
            }
            // zainicjowanie zwracanej tablicy
            int[] b = new int[1000];
            // wyszukanie bitów parzystości
            int bitcounter = 0, parity_count = 0, j = 0, k = 0;
            while (bitcounter < a.Length)
            {
                // 2^(bit parzystosci) musi być równy aktualnej pozycji
                // +1 ponieważ indeksowanie od zera.
                if (Math.Pow(2, parity_count) == bitcounter + parity_count + 1)
                {
                    parity_count++;
                }
                else
                {
                    bitcounter++;
                }
            }
            // długość zwracanej tablicy to długość wejściowej + ilość dodawanych bitów
            b = new int[a.Length + parity_count];
            // zainicjowanie '2', aby wymusić pierwsze 2 bity, które są zawsze
            for (bitcounter = 1; bitcounter <= b.Length; bitcounter++)
            {
                if (Math.Pow(2, j) == bitcounter)
                {
                    // znalezienie indeksu dla bitu parzystości

                    b[bitcounter - 1] = 2;
                    j++;
                }
                else
                {
                    b[k + j] = a[k++];
                }
            }
            for (bitcounter = 0; bitcounter < parity_count; bitcounter++)
            {
                // ustawianie bitów parzystości na swoich miejscach

                b[((int)Math.Pow(2, bitcounter)) - 1] = GetParity(b, bitcounter);
            }

            Disturbed = b.Reverse().ToArray();
            return b.Reverse().ToArray();
        }

        public int GetParity(int[] b, int power)
        {
            int parity = 0;
            for (int i = 0; i < b.Length; i++)
            {
                if (b[i] != 2)
                {
                    // jeżeli i nie zawiera nieustalonej wartości
                    // zapamietujmy i+1
                    // zamieniamy na 
                    int k = i + 1;
                    //String s = Integer.toBinaryString(k);
                    string s = Convert.ToString(k, 2);
                    //jezeli bit na pozycji 2^(power) ma wartość 1
                    //sprawdzamy zapamiętaną wartość
                    //jeżeli jest 1 lub 0, wtedy obliczamy wartość parzystości
                    int x = ((int.Parse(s)) / ((int)Math.Pow(10, power))) % 10;
                    if (x == 1)
                    {
                        if (b[i] == 1)
                        {
                            parity = (parity + 1) % 2;
                        }
                    }
                }
            }
            return parity;
        }

        public int[] Receive(int[] a, out int? errorPositon, out string correctMessage, out int[] controlBits)
        {
            int parity_count = a.Length - inMessage.Length; // nie dodajemy bitów w przypadku błędów, tylko zamieniamy
            int power;
            List<int> decoded = new List<int>(a);
            decoded.Reverse();
            //zmienna 'parity' zawiera wartości sprawdzonych bitów parzystości
            int[] parity = new int[parity_count];
            controlBits = new int[parity_count];
            List<int> toRemove = new List<int>();
            //zmienna 'syndrome' zawiera miejsce błędu
            int syndrome = 0;
            for (power = 0; power < parity_count; power++)
            {
                // sprawdzamy taką ilość razy, ile dodanych bitów przystości
                int parityCnt = 0;
                for (int i = (int)Math.Pow(2,power)-1; i< a.Length; i+=(int)Math.Pow(2,power+1) )
                {
                    //wyrażenei Linq do obliczania jedynek w danym przedziale
                    parityCnt += a.Reverse()
                        .Skip(i)
                        .Take((int)Math.Pow(2,power))
                        .Where(x => x == 1)
                        .Count();
                }

                //sprawdzenie czy liczba jedynek jest parzysta
                if (parityCnt % 2 != 0)
                {
                    controlBits[power] = (int)Math.Pow(2, power);
                    syndrome += (int)Math.Pow(2, power);
                }
                //syndrome = parity[power] + syndrome;
                decoded[(int)Math.Pow(2, power) - 1] = 2 ;
            }

            decoded = decoded.Where(x => x != 2).ToList();

            decoded.Reverse();
             //return BitHelper.ReversIntArray(decoded.ToArray());
            int error_location = syndrome;
            //int error_location = Integer.parseInt(syndrome, 2);
            errorPositon = null;
            if (error_location != 0)
            {
                errorPositon = error_location;
                Debug.WriteLine("Błąd na pozycji " + error_location + ".");
                errorIndex = error_location - 1;

                a[error_location - 1] = (a[error_location - 1] + 1) % 2;
                Debug.WriteLine("Poprawiony ciąg to: ");
                for (int i = 0; i < a.Length; i++)
                {
                    this.correctMessage += a[a.Length - i - 1];
                    Debug.Write(a[a.Length - i - 1]);
                }
            }
            else
            {
                Debug.WriteLine("Brak błędów");

            }

            // wyciąganie wiadomości z poprawionego ciągu
            for (int i = 0; i < a.Length; i++)
            {
                outMessage += a[a.Length - i - 1];
            }
            correctMessage = outMessage;
            return decoded.ToArray();
        }
    }
}
