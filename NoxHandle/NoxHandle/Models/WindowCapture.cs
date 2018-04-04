using NoxHandle.Models.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static NoxHandle.Models.WinAPI;

namespace NoxHandle.Models
{
    public class WindowCapture : IObservable<Bitmap>, IDisposable
    {
        private Subject<Bitmap> Subject { get; } = new Subject<Bitmap>();
        public TimeSpan Interval { get; }
        public string Title { get; }

        public IntPtr HWnd { get; }
        public IntPtr HDC { get; }
        public RECT Rect { get; private set; }
        public Bitmap Bitmap { get; private set; }

        public IDisposable Timer { get; }

        public static TimeSpan Fps60 => TimeSpan.FromMilliseconds(1000 / 60);
        public static TimeSpan Fps30 => TimeSpan.FromMilliseconds(1000 / 30);

        public WindowCapture(string title, TimeSpan interval)
        {
            Title = title;
            Interval = interval;

            HWnd = FindWindowEx(IntPtr.Zero, IntPtr.Zero, null, title);
            HDC = GetDCEx(HWnd, IntPtr.Zero, DeviceContextValues.Window);
            GetWindowRect(HWnd, out RECT rect);
            Rect = rect;
            Bitmap = new Bitmap(rect.GetWidth(), rect.GetHeight());

            Timer = Observable.Timer(TimeSpan.FromSeconds(0), interval).Subscribe(_ => Tick());
        }

        private void Tick()
        {
            GetWindowRect(HWnd, out RECT rect);

            if (Rect.GetWidth() != rect.GetWidth() ||
                Rect.GetHeight() != rect.GetHeight())
            {
                Rect = rect;
                Bitmap = new Bitmap(rect.GetWidth(), rect.GetHeight());
            }

            using (Graphics g = Graphics.FromImage(Bitmap))
            {
                BitBlt(
                    g.GetHdc(), 0, 0, Bitmap.Width, Bitmap.Height,
                    HDC, 0, 0, TernaryRasterOperations.SRCCOPY);
            }

            Subject.OnNext(Bitmap);
        }

        public static ImageSource ToImageSource(Bitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Png);
                ms.Position = 0;

                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = ms;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();

                return image;
            }
        }

        public IDisposable Subscribe(IObserver<Bitmap> observer)
        {
            return Subject.Subscribe(observer);
        }

        public void Dispose()
        {
            Timer.Dispose();
            Subject.OnCompleted();
        }
    }
}
