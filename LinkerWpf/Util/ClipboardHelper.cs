using System;
using System.Windows;

namespace Linker.Util
{
    public static class ClipboardHelper
    {
        public static bool TryCopy(string text)
        {
            try
            {
                Clipboard.SetText(text);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
