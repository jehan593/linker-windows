using System;
using System.IO;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Linker.Util
{
    public static class IconExtractor
    {
        public static BitmapSource ExtractIcon(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return null;
            try
            {
                var icon = System.Drawing.Icon.ExtractAssociatedIcon(path);
                if (icon == null) return null;

                using (icon)
                {
                    var bitmap = Imaging.CreateBitmapSourceFromHIcon(
                        icon.Handle,
                        new System.Windows.Int32Rect(0, 0, icon.Width, icon.Height),
                        BitmapSizeOptions.FromEmptyOptions());
                    bitmap.Freeze();
                    return bitmap;
                }
            }
            catch (Exception e) when (e is IOException || e is System.ComponentModel.Win32Exception || e is ArgumentException)
            {
                return null;
            }
        }

        private static BitmapSource _placeholder;

        public static BitmapSource GetPlaceholder()
        {
            if (_placeholder != null) return _placeholder;

            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                dc.DrawRectangle(
                    new SolidColorBrush(Theme.NordColors.Nord3),
                    null,
                    new System.Windows.Rect(0, 0, 32, 32));
            }
            var rtb = new RenderTargetBitmap(32, 32, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(dv);
            rtb.Freeze();
            _placeholder = rtb;
            return _placeholder;
        }
    }
}
