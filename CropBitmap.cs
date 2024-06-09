
namespace WinUI.Image.Helpers
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices.WindowsRuntime;
    using System.Threading.Tasks;
    using Windows.Foundation;
    using Microsoft.UI.Xaml.Media.Imaging;
    using System.Diagnostics;

    public class CropBitmap
    {
        public static async Task<WriteableBitmap> GetCroppedBitmapAsync(WriteableBitmap originalImage,
            Point startPoint, Size corpSize, double scale)
        {
            if (double.IsNaN(scale) || double.IsInfinity(scale))
            {
                scale = 1;
            }

            uint startPointX = (uint)Math.Floor(startPoint.X * scale);
            uint startPointY = (uint)Math.Floor(startPoint.Y * scale);
            uint height = (uint)Math.Floor(corpSize.Height * scale);
            uint width = (uint)Math.Floor(corpSize.Width * scale);

            // Ensure that originalImage is not null and contains valid dimensions
            if (originalImage == null && width <= 0 && height <= 0 &&
                startPointX + width >= (uint)originalImage.PixelWidth &&
                startPointY + height >= (uint)originalImage.PixelHeight)
            {
                Debug.WriteLine("Invalid image dimensions or null originalImage.");
                return null;

            }

            byte[] originalPixels = originalImage.PixelBuffer.ToArray();
            uint originalWidth = (uint)originalImage.PixelWidth;
            uint originalHeight = (uint)originalImage.PixelHeight;
            if (startPointX + width > originalWidth)
            {
                startPointX = originalWidth - width;
            }

            if (startPointY + height > originalHeight)
            {
                startPointY = originalHeight - height;
            }

            byte[] croppedPixels = GetPixelData(originalPixels, startPointX, startPointY, width, height,
                originalWidth, originalHeight);

            WriteableBitmap croppedBitmap = new WriteableBitmap((int)width, (int)height);
            using (Stream stream = croppedBitmap.PixelBuffer.AsStream())
            {
                await stream.WriteAsync(croppedPixels, 0, croppedPixels.Length);
            }

            return croppedBitmap;
        }

        private static byte[] GetPixelData(byte[] originalPixels, uint startPointX, uint startPointY,
            uint width, uint height, uint originalWidth, uint originalHeight)
        {

            uint stride = originalWidth * 4;

            byte[] croppedPixels = new byte[width * height * 4];
            try
            {
                for (uint y = 0; y < height; y++)
                {
                    for (uint x = 0; x < width; x++)
                    {
                        // Ensure that the index doesn't exceed the length of originalPixels
                        if ((startPointY + y) * stride + (startPointX + x) * 4 < originalPixels.Length)
                        {
                            uint originalIndex = (startPointY + y) * stride + (startPointX + x) * 4;
                            uint croppedIndex = y * width * 4 + x * 4;

                            Array.Copy(originalPixels, originalIndex, croppedPixels, croppedIndex, 4);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return croppedPixels;
        }
    }
}


