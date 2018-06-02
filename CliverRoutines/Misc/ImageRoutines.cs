//********************************************************************************************
//Author: Sergey Stoyan
//        sergey.stoyan@gmail.com
//        http://www.cliversoft.com
//********************************************************************************************
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace Cliver
{
    public static class ImageRoutines
    {
        public static Bitmap GetScaled(Image image, Size max_size)
        {
            var ratio = Math.Min((double)max_size.Width / image.Width, (double)max_size.Height / image.Height);
            var i = new Bitmap((int)(image.Width * ratio), (int)(image.Height * ratio));
            using (var graphics = Graphics.FromImage(i))
            {
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.DrawImage(image, 0, 0, i.Width, i.Height);
            }
            return i;
        }

        public static Bitmap GetScaled(Image image, float ratio)
        {
            var i = new Bitmap((int)(image.Width * ratio), (int)(image.Height * ratio));
            using (var graphics = Graphics.FromImage(i))
            {
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.DrawImage(image, 0, 0, i.Width, i.Height);
            }
            return i;
        }

        public static Bitmap GetCroppedByColor(Image image, Color color)
        {
            Bitmap b = new Bitmap(image);
            int y = b.Height, height = 0, x = b.Width, width = 0;
            for (int h = 0; h < b.Height; h++)
            {
                bool another_color_found = false;
                int wl = 0;
                for (; wl < b.Width; wl++)
                {
                    Color c = b.GetPixel(wl, h);
                    if (c != color)
                    {
                        another_color_found = true;
                        if (x > wl)
                            x = wl;
                        break;
                    }
                }
                for (int wr = b.Width - 1; wr > wl; wr--)
                {
                    Color c = b.GetPixel(wr, h);
                    if (c != color)
                    {
                        another_color_found = true;
                        if (width <= wr)
                            width = wr + 1;
                        break;
                    }
                }
                if (another_color_found)
                {
                    if (height <= h)
                        height = h + 1;
                    if (y > h)
                        y = h;
                }
            }
            if (y >= b.Height)
                return new Bitmap(0, 0);
            b = new Bitmap(width - x, height - y);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.DrawImage(image, x, y, width, height);
            }
            //b.Save("2.png");
            return b;
        }

        public static Bitmap GetGreyScale(Bitmap b)
        {
            Bitmap b2 = new Bitmap(b);
            for (int x = 0; x < b.Width; x++)
            {
                for (int y = 0; y < b.Height; y++)
                {
                    Color c = b.GetPixel(x, y);
                    int gc = (c.R + c.G + c.B) / 3;
                    Color c2 = Color.FromArgb(c.A, gc, gc, gc);
                    b2.SetPixel(x, y, c2);
                }
            }
            return b2;
        }

        public static Bitmap GetInverted(Bitmap b)
        {
            Bitmap b2 = new Bitmap(b);
            for (int x = 0; x < b.Width; x++)
            {
                for (int y = 0; y < b.Height; y++)
                {
                    Color c = b.GetPixel(x, y);
                    Color c2 = Color.FromArgb(c.A, 255 - c.R, 255 - c.G, 255 - c.B);
                    b2.SetPixel(x, y, c2);
                }
            }
            return b2;
        }

        public static System.Windows.Media.ImageSource ToImageSource(this System.Drawing.Icon icon)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                System.Windows.Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
        }

        //public static Bitmap GetResizedImage(Image image, float factor)
        //{
        //    var image2 = new Bitmap((int)(image.Width * factor), (int)(image.Height * factor));
        //    using (var graphics = Graphics.FromImage(image2))
        //    {
        //        graphics.CompositingMode = CompositingMode.SourceCopy;
        //        graphics.CompositingQuality = CompositingQuality.HighQuality;
        //        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        //        graphics.SmoothingMode = SmoothingMode.HighQuality;
        //        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

        //        using (var wrapMode = new ImageAttributes())
        //        {
        //            wrapMode.SetWrapMode(WrapMode.TileFlipXY);
        //            graphics.DrawImage(image, new Rectangle(0, 0, image2.Width, image2.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
        //        }
        //    }
        //    return image2;
        //}
    }
}