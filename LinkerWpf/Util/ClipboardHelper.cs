using System;
using WinFormsClipboard = System.Windows.Forms.Clipboard;

namespace Linker.Util
{
    public static class ClipboardHelper
    {
        public static bool TryCopy(string text)
        {
            try
            {
                // WinForms has a retrying overload for brief clipboard locks.
                WinFormsClipboard.SetDataObject(text, true, 10, 100);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
