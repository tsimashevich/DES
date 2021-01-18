using System;
using System.Text;
using static System.Console;
using static System.Convert;

namespace ISM.DES
{
    public class DESCrypto
    {
        #region Private Fields

        private const int LENGTH_OF_DES_BLOCKS = 64;

        #endregion

        #region Public Methods

        public string Encode(string inputString, string key) =>
            MainAlgorithm(true, inputString, key.ToUpper());

        public string Decode(string inputString, string key) =>
            MainAlgorithm(false, inputString, key.ToUpper());

        #endregion

        #region Private Methods

        private string MainAlgorithm(bool isEncode, string text, string key) =>
            MainAlgorithm(isEncode, text, key, false);

        private string MainAlgorithm(bool isEncode, string text, string key, bool IsTextBinary)
        {
            #region Get 16 sub-keys using key

            var hexKey = Converts.FromTextToHex(key);
            var binaryKey = Converts.FromHexToBinary(hexKey);
            var keyPlus = DoPermutation(binaryKey, Rearranging.KeyPosition);
            var C0 = TextHelper.SetLeftHalvesKey(keyPlus);
            var D0 = TextHelper.SetRightHalvesKey(keyPlus);
            var keys = SetAllKeys(C0, D0);

            #endregion

            #region Encrypt/Decrypt process

            var binaryText = !IsTextBinary ? Converts.FromTextToBinary(text) : text;
            binaryText = TextHelper.setTextMutipleOf64Bits(binaryText);

            var cipherTextBuilder = new StringBuilder(binaryText.Length);

            for (int i = 0; i < (binaryText.Length / LENGTH_OF_DES_BLOCKS); i++)
            {
                var permutatedText = DoPermutation(
                    binaryText.Substring(i * LENGTH_OF_DES_BLOCKS, LENGTH_OF_DES_BLOCKS),
                    Rearranging.FirstChangingIP);
                var L0 = TextHelper.SetLeftHalvesKey(permutatedText);
                var R0 = TextHelper.SetRightHalvesKey(permutatedText);
                var finalText = FinalEncription(L0, R0, keys, !isEncode);

                if (!isEncode && LENGTH_OF_DES_BLOCKS * (i + 1) == binaryText.Length)
                {
                    #region It's correct subtracted '0' that have added for set text multiple of 64bit (just for DECODING)

                    var lastText = new StringBuilder(finalText.TrimEnd('0'));
                    var count = finalText.Length - lastText.Length;

                    if ((count % 8) != 0)
                        count = 8 - (count % 8);

                    var appendText = string.Empty;
                    for (int k = 0; k < count; k++)
                        appendText += "0";

                    cipherTextBuilder.Append(lastText.ToString() + appendText);

                    #endregion
                }
                else
                {
                    cipherTextBuilder.Append(finalText);
                }
            }

            #endregion

            return Converts.FromBinaryToText(cipherTextBuilder.ToString());
        }

        #region Permutation

        private string DoPermutation(string text, int[] order)
        {
            var PermutatedText = new StringBuilder(order.Length);

            for (int i = 0; i < order.Length; i++)
                PermutatedText.Append(text[order[i] - 1]);

            return PermutatedText.ToString();
        }

        //For SBoxes Transformation
        private string DoPermutation(string text, int[,] order)
        {
            int rowIndex = ToInt32(text[0].ToString() + text[text.Length - 1].ToString(), 2);
            int colIndex = ToInt32(text.Substring(1, 4), 2);

            return Converts.FromDecimalToBinary(order[rowIndex, colIndex]);
        }

        #endregion

        private string LeftShift(string text, int count)
        {
            if (count < 1)
            {
                WriteLine("The count of leftshift is must more than 1 time.");
                return null;
            }

            var shifted = new StringBuilder(text.Length);
            shifted.Append(text.Substring(count) + text.Substring(0, count));

            return shifted.ToString();
        }

        private Keys SetAllKeys(string C0, string D0)
        {
            var keys = new Keys();
            keys.Cn[0] = C0;
            keys.Dn[0] = D0;

            for (int i = 1; i < keys.Cn.Length; i++)
            {
                keys.Cn[i] = LeftShift(keys.Cn[i - 1], Rearranging.RoundShift[i]);
                keys.Dn[i] = LeftShift(keys.Dn[i - 1], Rearranging.RoundShift[i]);
                keys.Kn[i - 1] = DoPermutation(keys.Cn[i] + keys.Dn[i], Rearranging.PermutationPos);
            }

            return keys;
        }

        private string FinalEncription(string L0, string R0, Keys keys, bool IsReverse)
        {
            string Ln = "", Rn = "", Ln_1 = L0, Rn_1 = R0;
            var i = 0;

            if (IsReverse == true)
                i = 15;

            while (IsEnough(i, IsReverse))
            {
                Ln = Rn_1;
                Rn = XOR(Ln_1, f(Rn_1, keys.Kn[i]));

                Ln_1 = Ln;
                Rn_1 = Rn;

                if (IsReverse == false)
                    i += 1;
                else
                    i -= 1;
            }

            return DoPermutation(Rn + Ln, Rearranging.LastChanging);
        }

        private bool IsEnough(int i, bool IsReverse) =>
            (IsReverse == false) ? i < 16 : i >= 0;

        private string f(string Rn_1, string Kn) =>
            P(SboxTransformation(XOR(ESelection(Rn_1), Kn)));

        private string P(string text) =>
            DoPermutation(text, Rearranging.P);

        private string SboxTransformation(string text)
        {
            var TransformedText = new StringBuilder(32);

            for (int i = 0; i < 8; i++)
            {
                var temp = text.Substring(i * 6, 6);
                TransformedText.Append(DoPermutation(temp, Rearranging.S[i]));
            }

            return TransformedText.ToString();
        }

        private string ESelection(string Rn_1) =>
            DoPermutation(Rn_1, Rearranging.E);

        private string XOR(string text1, string text2)
        {
            if (text1.Length != text2.Length)
            {
                WriteLine("Two data blocks for XOR are must get same size.");
                return null;
            }

            var XORTextBuilder = new StringBuilder(text1.Length);

            for (int i = 0; i < text1.Length; i++)
            {
                if (text1[i] != text2[i])
                    XORTextBuilder.Append("1");
                else
                    XORTextBuilder.Append("0");
            }

            return XORTextBuilder.ToString();
        }

        #endregion

        #region Private Classes

        private class Keys
        {
            public string[] Cn = new string[17];
            public string[] Dn = new string[17];
            public string[] Kn = new string[16];
        }

        private class Rearranging
        {
            // First rearranging with input 64 bit block
            public static readonly int[] FirstChangingIP =
            {
                58, 50, 42, 34, 26, 18, 10, 2,
                60, 52, 44, 36, 28, 20, 12, 4,
                62, 54, 46, 38, 30, 22, 14, 6,
                64, 56, 48, 40, 32, 24, 16, 8,
                57, 49, 41, 33, 25, 17,  9, 1,
                59, 51, 43, 35, 27, 19, 11, 3,
                61, 53, 45, 37, 29, 21, 13, 5,
                63, 55, 47, 39, 31, 23, 15, 7
            };

            // Extension table E
            public static readonly int[] E =
            {
                32,  1,  2,  3,  4,  5,
                 4,  5,  6,  7,  8,  9,
                 8,  9, 10, 11, 12, 13,
                12, 13, 14, 15, 16, 17,
                16, 17, 18, 19, 20, 21,
                20, 21, 22, 23, 24, 25,
                24, 25, 26, 27, 28, 29,
                28, 29, 30, 31, 32,  1
            };

            // Table of replacements (S1..S8)
            public static readonly int[][,] S = new int[][,]
            {
                new int[,]
                {
                    { 14,  4, 13,  1,  2, 15, 11,  8,  3, 10,  6, 12,  5,  9,  0,  7 },
                    {  0, 15,  7,  4, 14,  2, 13,  1, 10,  6, 12, 11,  9,  5,  3,  8 },
                    {  4,  1, 14,  8, 13,  6,  2, 11, 15, 12,  9,  7,  3, 10,  5,  0 },
                    { 15, 12,  8,  2,  4,  9,  1,  7,  5, 11,  3, 14, 10,  0,  6, 13 }
                },
                new int[,]
                {
                    { 15,  1,  8, 14,  6, 11,  3,  4,  9,  7,  2, 13, 12,  0,  5, 10 },
                    {  3, 13,  4,  7, 15,  2,  8, 14, 12,  0,  1, 10,  6,  9, 11,  5 },
                    {  0, 14,  7, 11, 10,  4, 13,  1,  5,  8, 12,  6,  9,  3,  2, 15 },
                    { 13,  8, 10,  1,  3, 15,  4,  2, 11,  6,  7, 12,  0,  5, 14,  9 }
                },
                new int[,]
                {
                    { 10,  0,  9, 14,  6,  3, 15,  5,  1, 13, 12,  7, 11,  4,  2,  8 },
                    { 13,  7,  0,  9,  3,  4,  6, 10,  2,  8,  5, 14, 12, 11, 15,  1 },
                    { 13,  6,  4,  9,  8, 15,  3,  0, 11,  1,  2, 12,  5, 10, 14,  7 },
                    {  1, 10, 13,  0,  6,  9,  8,  7,  4, 15, 14,  3, 11,  5,  2, 12 }
                },
                new int[,]
                {
                    {  7, 13, 14,  3,  0,  6,  9, 10,  1,  2,  8,  5, 11, 12,  4, 15 },
                    { 13,  8, 11,  5,  6, 15,  0,  3,  4,  7,  2, 12,  1, 10, 14,  9 },
                    { 10,  6,  9,  0, 12, 11,  7, 13, 15,  1,  3, 14,  5,  2,  8,  4 },
                    {  3, 15,  0,  6, 10,  1, 13,  8,  9,  4,  5, 11, 12,  7,  2, 14 }
                },
                new int[,]
                {
                    {  2, 12,  4,  1,  7, 10, 11,  6,  8,  5,  3, 15, 13,  0, 14,  9 },
                    { 14, 11,  2, 12,  4,  7, 13,  1,  5,  0, 15, 10,  3,  9,  8,  6 },
                    {  4,  2,  1, 11, 10, 13,  7,  8, 15,  9, 12,  5,  6,  3,  0, 14 },
                    { 11,  8, 12,  7,  1, 14,  2, 13,  6, 15,  0,  9, 10,  4,  5,  3 }
                },
                new int[,]
                {
                    { 12,  1, 10, 15,  9,  2,  6,  8,  0, 13,  3,  4, 14,  7,  5, 11 },
                    { 10, 15,  4,  2,  7, 12,  9,  5,  6,  1, 13, 14,  0, 11,  3,  8 },
                    {  9, 14, 15,  5,  2,  8, 12,  3,  7,  0,  4, 10,  1, 13, 11,  6 },
                    {  4,  3,  2, 12,  9,  5, 15, 10, 11, 14,  1,  7,  6,  0,  8, 13 }
                },
                new int[,]
                {
                    {  4, 11,  2, 14, 15,  0,  8, 13,  3, 12,  9,  7,  5, 10,  6,  1 },
                    { 13,  0, 11,  7,  4,  9,  1, 10, 14,  3,  5, 12,  2, 15,  8,  6 },
                    {  1,  4, 11, 13, 12,  3,  7, 14, 10, 15,  6,  8,  0,  5,  9,  2 },
                    {  6, 11, 13,  8,  1,  4, 10,  7,  9,  5,  0, 15, 14,  2,  3, 12 }
                },
                new int[,]
                {
                    { 13,  2,  8,  4,  6, 15, 11,  1, 10,  9,  3, 14,  5,  0, 12,  7 },
                    {  1, 15, 13,  8, 10,  3,  7,  4, 12,  5,  6, 11,  0, 14,  9,  2 },
                    {  7, 11,  4,  1,  9, 12, 14,  2,  0,  6, 10, 13, 15,  3,  5,  8 },
                    {  2,  1, 14,  7,  4, 10,  8, 13, 15, 12,  9,  0,  3,  5,  6, 11 }
                }
            };

            // 16 times changing table
            public static readonly int[] P =
            {
                16,  7, 20, 21, 29, 12, 28, 17,
                 1, 15, 23, 26,  5, 18, 31, 10,
                 2,  8, 24, 14, 32, 27,  3,  9,
                19, 13, 30,  6, 22, 11,  4, 25
            };

            // Key extencion
            public static readonly int[] KeyPosition =
            {
                57, 49, 41, 33, 25, 17,  9,
                 1, 58, 50, 42, 34, 26, 18,
                10,  2, 59, 51, 43, 35, 27,
                19, 11,  3, 60, 52, 44, 36,
                63, 55, 47, 39, 31, 23, 15,
                 7, 62, 54, 46, 38, 30, 22,
                14,  6, 61, 53, 45, 37, 29,
                21, 13,  5, 28, 20, 12,  4
            };

            public static readonly int[] RoundShift =
            {
                0, 1, 1, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 1
            };

            public static readonly int[] PermutationPos =
            {
                14, 17, 11, 24,  1,  5,
                 3, 28, 15,  6, 21, 10,
                23, 19, 12,  4, 26,  8,
                16,  7, 27, 20, 13,  2,
                41, 52, 31, 37, 47, 55,
                30, 40, 51, 45, 33, 48,
                44, 49, 39, 56, 34, 53,
                46, 42, 50, 36, 29, 32
            };

            public static readonly int[] LastChanging =
            {
                40, 8, 48, 16, 56, 24, 64, 32,
                39, 7, 47, 15, 55, 23, 63, 31,
                38, 6, 46, 14, 54, 22, 62, 30,
                37, 5, 45, 13, 53, 21, 61, 29,
                36, 4, 44, 12, 52, 20, 60, 28,
                35, 3, 43, 11, 51, 19, 59, 27,
                34, 2, 42, 10, 50, 18, 58, 26,
                33, 1, 41,  9, 49, 17, 57, 25
            };
        }

        #endregion
    }
}
