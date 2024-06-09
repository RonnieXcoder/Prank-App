using Microsoft.UI.Windowing;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.UI.Composition;
using Windows.System;
using Windows.UI;
using WinUI.Image.Helpers;
using Windows.Graphics.Capture;
using Windows.Devices.Display;
using Windows.Devices.Enumeration;

namespace Prank
{
    public sealed partial class FullscreenWindow : Window
    {
        private enum ScreenEffect
        {
            None,
            Freezing,
            UpsideDown,
            RandomColor,
            Earthquake,
            Snow,
            Lines,
            Puzzle,
            ScreenOfDeath
        }

        private ScreenEffect screenEffect = ScreenEffect.None;
        private DispatcherTimer timer;
        private Image image;
        private Random random = new Random();
        private Color color;
        private bool isScreenLocked = true;

        private int ScreenWidth { get; set; }
        private int ScreenHeight { get; set; }
        public FullscreenWindow(int screenWidth, int screenHeight)
        {
            this.InitializeComponent();

            ScreenWidth = screenWidth;
            ScreenHeight = screenHeight;

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WindowId WndID = Win32Interop.GetWindowIdFromWindow(hwnd);
            AppWindow appW = AppWindow.GetFromWindowId(WndID);
            OverlappedPresenter presenter = appW.Presenter as OverlappedPresenter;
            presenter.IsAlwaysOnTop = true;
            appW.SetPresenter(AppWindowPresenterKind.FullScreen);
            MainCanvas.Background = new SolidColorBrush(Colors.Black);
            this.Closed += FullscreenWindow_Closed;
            this.VisibilityChanged += FullscreenWindow_VisibilityChanged;


        }

        private void FullscreenWindow_VisibilityChanged(object sender, WindowVisibilityChangedEventArgs args)
        {
            MainCanvas.Visibility = Visibility.Visible;
        }

        private void FullscreenWindow_Closed(object sender, WindowEventArgs args)
        {
            args.Handled = isScreenLocked;
        }
        private void Grid_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.F12)
            {
                isScreenLocked = false;
                screenEffect = ScreenEffect.None;
                if (timer != null && timer.IsEnabled == true) timer.Stop();
                MainCanvas.Children.Clear();
            }
            else isScreenLocked = true;
        }
        private void Grid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (isScreenLocked == false) Close();
        }

        public void StartFreezing()
        {
            TakeScreenshot();
        }

        public void StartUpsideDown()
        {
            TakeScreenshot();
            MainCanvas.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);
            MainCanvas.RenderTransform = new RotateTransform() { Angle = 180 };
        }

        public void StartRandomColor()
        {
            screenEffect = ScreenEffect.RandomColor;
            StartTimer();
        }

        public void StartEarthquake()
        {
            TakeScreenshot();
            screenEffect = ScreenEffect.Earthquake;
            StartTimer();
        }

        public void StartSnow()
        {
            screenEffect = ScreenEffect.Snow;
        }

        public void StartLines()
        {
            TakeScreenshot();
            screenEffect = ScreenEffect.Lines;
            StartTimer();
        }

        public void StartPuzzle()
        {
            MainCanvas.Visibility = Visibility.Collapsed;
            MainCanvas.Children.Clear();
            CreateGrid();
            screenEffect = ScreenEffect.Puzzle;
            StartTimer();
        }

        private List<Image> puzzlePieces = new List<Image>();

        private async void CreateGrid()
        {
            if (MainGrid.Children.Count > 0)
            {
                MainGrid.Children.Clear();
            }

            puzzlePieces.Clear();

            MainGrid.RowDefinitions.Clear();
            MainGrid.ColumnDefinitions.Clear();

            for (int i = 0; i < 5; i++)
            {
                MainGrid.RowDefinitions.Add(new RowDefinition());
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            int cellWidth = ScreenWidth / 5;
            int cellHeight = ScreenHeight / 5;

            TakeScreenshot();

            for (int row = 0; row < 5; row++)
            {
                for (int col = 0; col < 5; col++)
                {

                    var img = new Image();
                    img.Stretch = Stretch.Fill;
                    img.HorizontalAlignment = HorizontalAlignment.Stretch;
                    img.VerticalAlignment = VerticalAlignment.Stretch;

                    WriteableBitmap croppedBitmap = await CropBitmap.GetCroppedBitmapAsync(image.Source as WriteableBitmap,
                        new Point { X = cellWidth * row, Y = cellHeight * col },
                        new Size { Width = cellWidth, Height = cellHeight }, 1.0);
                    img.Source = croppedBitmap;
                    puzzlePieces.Add(img);
                    Grid.SetRow(img, row);
                    Grid.SetColumn(img, col);

                    MainGrid.Children.Add(img);

                }

            }

        }
        public void StartScreenOfDeath()
        {
            Image image = new Image();

            image.Width = ScreenWidth;
            image.Height = ScreenHeight;

            image.Stretch = Stretch.Fill;
            BitmapImage bitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/ScreenOfDeath.png"));

            image.Source = bitmapImage;

            Canvas.SetLeft(image, 0);
            Canvas.SetTop(image, 0);

            MainCanvas.Children.Add(image);
        }
        private void StartTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(50);
            timer.Tick += Timer_Tick;
            timer.Start();
        }
        private void Timer_Tick(object sender, object e)
        {
            if (screenEffect == ScreenEffect.RandomColor)
            {
                color = Color.FromArgb(255, (byte)random.Next(256), (byte)random.Next(256),
                    (byte)random.Next(256));
                MainCanvas.Background = new SolidColorBrush(color);
            }
            else if (screenEffect == ScreenEffect.Earthquake)
            {
                double offsetX = random.Next(-5, 5);
                double offsetY = random.Next(-5, 5);
                Canvas.SetLeft(image, 0 + offsetX);
                Canvas.SetTop(image, 0 + offsetY);
            }
            else if (screenEffect == ScreenEffect.Lines)
            {
                color = Color.FromArgb(255, (byte)random.Next(256), (byte)random.Next(256),
                    (byte)random.Next(256));

                Line line = new Line
                {
                    X1 = random.NextDouble() * MainCanvas.ActualWidth,
                    Y1 = random.NextDouble() * MainCanvas.ActualHeight,
                    X2 = random.NextDouble() * MainCanvas.ActualWidth,
                    Y2 = random.NextDouble() * MainCanvas.ActualHeight,
                    Stroke = new SolidColorBrush(color),
                    StrokeThickness = 1
                };


                MainCanvas.Children.Add(line);
            }
            else if (screenEffect == ScreenEffect.Puzzle)
            {
                for (int i = 0; i < puzzlePieces.Count; i++)
                {
                    int index1 = random.Next(0, puzzlePieces.Count);
                    int index2 = random.Next(0, puzzlePieces.Count);

                    ImageSource source = puzzlePieces[index1].Source;
                    puzzlePieces[index1].Source = puzzlePieces[index2].Source;
                    puzzlePieces[index2].Source = source;
                }
            }
        }

        public void GenerateSnowflakes()
        {

            MainCanvas.Background = new SolidColorBrush(Colors.Black);

            for (int i = 0; i < 300; i++)
            {
                Ellipse snowflake = new Ellipse();
                snowflake.Width = 5;
                snowflake.Height = 5;
                snowflake.Fill = new SolidColorBrush(Colors.White);
                double x = random.NextDouble() * MainCanvas.ActualWidth;
                double y = random.NextDouble() * MainCanvas.ActualHeight;
                Canvas.SetLeft(snowflake, x);
                Canvas.SetTop(snowflake, y);
                MainCanvas.Children.Add(snowflake);

                var visual = ElementCompositionPreview.GetElementVisual(snowflake);
                var compositor = visual.Compositor;
                var opacityAnimationObject = compositor.CreateScalarKeyFrameAnimation();
                opacityAnimationObject.InsertKeyFrame(0.0f, 0.0f);
                opacityAnimationObject.InsertKeyFrame(0.5f, 1.0f);
                opacityAnimationObject.InsertKeyFrame(1.0f, 0.0f);
                opacityAnimationObject.Duration = TimeSpan.FromSeconds(random.NextDouble() + 1);
                opacityAnimationObject.IterationBehavior = AnimationIterationBehavior.Forever;
                opacityAnimationObject.Direction = Microsoft.UI.Composition.AnimationDirection.Alternate;

                visual.StartAnimation("Opacity", opacityAnimationObject);
            }

        }
        private void MainCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            if (screenEffect == ScreenEffect.Snow) GenerateSnowflakes();
        }



        public enum CopyPixelOperation : int
        {
            SRCCOPY = 0x00CC0020
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr window);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleDC(IntPtr dc);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr dc, int width, int height);

        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr dc, IntPtr obj);

        [DllImport("gdi32.dll")]
        public static extern bool BitBlt(IntPtr dcDest, int xDest, int yDest, int width, int height, IntPtr dcSrc, int xSrc, int ySrc, CopyPixelOperation rop);

        [DllImport("user32.dll")]
        public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr dc);

        [DllImport("gdi32.dll")]
        public static extern IntPtr DeleteDC(IntPtr dc);

        [DllImport("gdi32.dll")]
        public static extern IntPtr DeleteObject(IntPtr obj);

        private async void TakeScreenshot()
        {
            int screenWidth = ScreenWidth;
            int screenHeight = ScreenHeight;

            IntPtr desktopWindow = GetDesktopWindow();
            IntPtr desktopDC = GetWindowDC(desktopWindow);
            IntPtr compatibleDC = CreateCompatibleDC(desktopDC);
            IntPtr compatibleBitmap = CreateCompatibleBitmap(desktopDC, screenWidth, screenHeight);
            IntPtr previousBitmap = SelectObject(compatibleDC, compatibleBitmap);

            BitBlt(compatibleDC, 0, 0, screenWidth, screenHeight, desktopDC, 0, 0, CopyPixelOperation.SRCCOPY);
            WriteableBitmap writeableBitmap = await GetCaptureWriteableBitmap(compatibleBitmap);
            image = new Image();

            image.Source = writeableBitmap;

            Canvas.SetLeft(image, 0);
            Canvas.SetTop(image, 0);

            MainCanvas.Children.Add(image);

            SelectObject(compatibleDC, previousBitmap);
            DeleteObject(compatibleBitmap);
            DeleteDC(compatibleDC);
            ReleaseDC(desktopWindow, desktopDC);

        }

        private WriteableBitmap m_hWriteableBitmapCapture;

        [DllImport("gdi32.dll")]
        public static extern bool GetObject(IntPtr hgdiobj, int cbBuffer, out BITMAP lpvObject);

        [DllImport("gdi32.dll")]
        public static extern int GetDIBits(IntPtr hdc, IntPtr hbmp, uint uStartScan, uint cScanLines, byte[] lpvBits, ref BITMAPV5HEADER lpbi, uint uUsage);

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAP
        {
            public int bmType;
            public int bmWidth;
            public int bmHeight;
            public int bmWidthBytes;
            public ushort bmPlanes;
            public ushort bmBitsPixel;
            public IntPtr bmBits;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAPV5HEADER
        {
            public uint bV5Size;
            public int bV5Width;
            public int bV5Height;
            public ushort bV5Planes;
            public ushort bV5BitCount;
            public uint bV5Compression;
            public uint bV5SizeImage;
            public int bV5XPelsPerMeter;
            public int bV5YPelsPerMeter;
            public uint bV5ClrUsed;
            public uint bV5ClrImportant;
            public uint bV5RedMask;
            public uint bV5GreenMask;
            public uint bV5BlueMask;
            public uint bV5AlphaMask;
            public uint bV5CSType;
            public IntPtr bV5Endpoints;
            public uint bV5GammaRed;
            public uint bV5GammaGreen;
            public uint bV5GammaBlue;
            public uint bV5Intent;
            public uint bV5ProfileData;
            public uint bV5ProfileSize;
            public uint bV5Reserved;
        }

        private const uint DIB_RGB_COLORS = 0;
        private const uint BI_BITFIELDS = 3;
        public async Task<WriteableBitmap> GetCaptureWriteableBitmap(IntPtr m_hBitmapCapture)
        {
            if (m_hBitmapCapture != IntPtr.Zero)
            {
                BITMAP bm;
                GetObject(m_hBitmapCapture, Marshal.SizeOf(typeof(BITMAP)), out bm);
                int nWidth = bm.bmWidth;
                int nHeight = bm.bmHeight;
                BITMAPV5HEADER bi = new BITMAPV5HEADER();
                bi.bV5Size = (uint)Marshal.SizeOf(typeof(BITMAPV5HEADER));
                bi.bV5Width = nWidth;
                bi.bV5Height = -nHeight;
                bi.bV5Planes = 1;
                bi.bV5BitCount = 32;
                bi.bV5Compression = BI_BITFIELDS;
                bi.bV5AlphaMask = unchecked((uint)0xFF000000);
                bi.bV5RedMask = 0x00FF0000;
                bi.bV5GreenMask = 0x0000FF00;
                bi.bV5BlueMask = 0x000000FF;

                IntPtr hDC = CreateCompatibleDC(IntPtr.Zero);
                IntPtr hBitmapOld = SelectObject(hDC, m_hBitmapCapture);
                int nNumBytes = nWidth * 4 * nHeight;
                byte[] pPixels = new byte[nNumBytes];
                int nScanLines = GetDIBits(hDC, m_hBitmapCapture, 0, (uint)nHeight, pPixels, ref bi, DIB_RGB_COLORS);
                if (m_hWriteableBitmapCapture != null)
                    m_hWriteableBitmapCapture.Invalidate();
                m_hWriteableBitmapCapture = new WriteableBitmap(nWidth, nHeight);
                using (var stream = m_hWriteableBitmapCapture.PixelBuffer.AsStream())
                {
                    await stream.WriteAsync(pPixels, 0, pPixels.Length);
                }
                SelectObject(hDC, hBitmapOld);
                DeleteDC(hDC);
                return m_hWriteableBitmapCapture;
            }
            return null;
        }

    }
}