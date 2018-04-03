using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NoxHandle.Models
{
    public static class ImageProcess
    {
        public static void DrawRect(this Bitmap bmp, Point pt, int width, int height)
        {

        }

        public static void Binalize(this Bitmap bmp, byte threshold)
        {
            Open(bmp, pixels =>
            {
                for (int i = 0; i < pixels.Length; i += 4)
                {
                    byte b = pixels[i + 0];
                    byte g = pixels[i + 1];
                    byte r = pixels[i + 2];
                    byte a = pixels[i + 3];

                    byte avg = (byte)((r + g + b) / 3);
                    if (threshold < avg)
                    {
                        r = g = b = 255;
                    }
                    else
                    {
                        r = g = b = 0;
                    }

                    pixels[i + 0] = b;
                    pixels[i + 1] = g;
                    pixels[i + 2] = r;
                    pixels[i + 3] = a;
                }
            });
        }

        public static void Open(this Bitmap bmp, Action<byte[]> procedure)
        {
            BitmapData bmpData = bmp.LockBits(
                new Rectangle(new Point(0, 0), new Size(bmp.Width, bmp.Height)),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);

            if (bmpData.Stride < 0)
            {
                bmp.UnlockBits(bmpData);
                throw new Exception();
            }

            IntPtr scan0 = bmpData.Scan0;
            byte[] pixels = new byte[bmpData.Stride * bmp.Height];
            Marshal.Copy(scan0, pixels, 0, pixels.Length);
            procedure?.Invoke(pixels);
            Marshal.Copy(pixels, 0, scan0, pixels.Length);
            bmp.UnlockBits(bmpData);
        }
    }
}
