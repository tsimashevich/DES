using static System.Console;

namespace ISM.DES
{
    public class TextHelper
    {
        #region Public Methods

        public static string setTextMutipleOf64Bits(string text)
        {
            if ((text.Length % 64) != 0)
            {
                int maxLength = 0;
                maxLength = ((text.Length / 64) + 1) * 64;
                text = text.PadRight(maxLength, '0');
            }

            return text;
        }

        public static string SetLeftHalvesKey(string text) =>
            SetHalvesKey(true, text);

        public static string SetRightHalvesKey(string text) =>
            SetHalvesKey(false, text);

        public static string SetHalvesKey(bool IsLeft, string text)
        {
            if ((text.Length % 8) != 0)
            {
                WriteLine("The key is not multiple of 8bit.");
                return null;
            }

            int midindex = (text.Length / 2) - 1;
            var result = string.Empty;

            if (IsLeft)
                result = text.Substring(0, midindex + 1);
            else
                result = text.Substring(midindex + 1);

            return result;
        }

        #endregion
    }
}
