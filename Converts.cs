using System;
using System.Text;
using static System.Console;
using static System.Convert;

namespace ISM.DES
{
    public class Converts
    {
        #region Public Static Methods

        public static string FromTextToHex(string text)
        {
            var hexstring = string.Empty;
            foreach (char word in text)
            {
                hexstring += string.Format("{0:X}", ToInt32(word));
            }

            return hexstring;
        }

        public static string FromHexToText(string hexstring)
        {
            var text = new StringBuilder(hexstring.Length / 2);

            for (int i = 0; i < (hexstring.Length / 2); i++)
            {
                string word = hexstring.Substring(i * 2, 2);
                text.Append((char)ToInt32(word, 16));
            }

            return text.ToString();
        }

        public static string FromBinaryToText(string binarystring)
        {
            var text = new StringBuilder(binarystring.Length / 8);

            for (int i = 0; i < (binarystring.Length / 8); i++)
            {
                var word = binarystring.Substring(i * 8, 8);
                text.Append((char)ToInt32(word, 2));
            }

            return text.ToString();
        }

        public static string FromTextToBinary(string text)
        {
            var binarystring = new StringBuilder(text.Length * 8);

            foreach (char word in text)
            {
                int binary = word;
                int factor = 128;

                for (int i = 0; i < 8; i++)
                {
                    if (binary >= factor)
                    {
                        binary -= factor;
                        binarystring.Append("1");
                    }
                    else
                    {
                        binarystring.Append("0");
                    }

                    factor /= 2;
                }
            }

            return binarystring.ToString();
        }

        public static string FromDecimalToBinary(int binary)
        {
            if (binary < 0)
            {
                WriteLine("It requires a integer greater than 0.");
                return null;
            }

            string binarystring = "";
            int factor = 128;

            for (int i = 0; i < 8; i++)
            {
                if (binary >= factor)
                {
                    binary -= factor;
                    binarystring += "1";
                }
                else
                {
                    binarystring += "0";
                }
                factor /= 2;
            }

            return binarystring;
        }

        public static byte FromBinaryToByte(string binary)
        {
            byte value = 0;
            int factor = 128;

            for (int i = 0; i < 8; i++)
            {
                if (binary[i] == '1')
                {
                    value += (byte)factor;
                }

                factor /= 2;
            }

            return value;
        }

        public static string FromHexToBinary(string hexstring)
        {
            var binarystring = string.Empty;

            try
            {
                for (int i = 0; i < hexstring.Length; i++)
                {
                    int hex = ToInt32(hexstring[i].ToString(), 16);

                    int factor = 8;

                    for (int j = 0; j < 4; j++)
                    {
                        if (hex >= factor)
                        {
                            hex -= factor;
                            binarystring += "1";
                        }
                        else
                        {
                            binarystring += "0";
                        }
                        factor /= 2;
                    }
                }
            }
            catch (Exception e)
            {
                WriteLine(e.Message + " - wrong hex integer format.");
            }

            return binarystring;
        }

        #endregion
    }
}
