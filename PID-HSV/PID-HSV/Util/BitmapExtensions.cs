using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using PID_HSV.Converter;
using PID_HSV.Image;

namespace PID_HSV.Util
{
    public static class BitmapExtensions
    {
        public static void CopyBitmap(this Bitmap bitmap, ImageBase img)
        {
            unsafe
            {
                var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadWrite, bitmap.PixelFormat);

                var bytesPerPixel = System.Drawing.Image.GetPixelFormatSize(bitmap.PixelFormat) / 8;
                var heightInPixels = bitmapData.Height;
                var widthInBytes = bitmapData.Width * bytesPerPixel;
                var ptrFirstPixel = (byte*)bitmapData.Scan0;
                var pointer = (byte*)img.Buffer.ToPointer();

                Parallel.For(0, heightInPixels, y =>
                {
                    var currentLine = ptrFirstPixel + y * bitmapData.Stride;
                    var currLinePointer = pointer + (heightInPixels - y - 1) * img.LineStride;

                    for (var x = 0; x < widthInBytes; x = x + bytesPerPixel)
                    {
                        var bytes = RGBToHSVConverter.ConvertBack(
                            new[] { currLinePointer[x], currLinePointer[x + 1], currLinePointer[x + 2] });

                        currentLine[x] = bytes[0];
                        currentLine[x + 1] = bytes[1];
                        currentLine[x + 2] = bytes[2];
                    }
                });
                bitmap.UnlockBits(bitmapData);
            }
        }

        public static void CopyBitmap(this Bitmap bitmap, ImageBase img, HSVOptions filter)
        {
            unsafe
            {
                var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadWrite, bitmap.PixelFormat);

                var bytesPerPixel = System.Drawing.Image.GetPixelFormatSize(bitmap.PixelFormat) / 8;
                var heightInPixels = bitmapData.Height;
                var widthInBytes = bitmapData.Width * bytesPerPixel;
                var bmpPointer = (byte*)bitmapData.Scan0;
                var imgBsPointer = (byte*)img.Buffer.ToPointer();

                var hue = (int)(filter.Hue / 359.0 * 255);
                var sat = (int)(filter.Saturation * 255);
                var val = (int)(filter.Value * 255);

                Parallel.For(0, heightInPixels, y =>
                {
                    //for (var y = 0; y < heightInPixels; y++)
                    //{
                    var bmpCrrLine = bmpPointer + y * bitmapData.Stride;
                    var imgBsCrrLine = imgBsPointer + (heightInPixels - y - 1) * img.LineStride;

                    for (var x = 0; x < widthInBytes; x = x + bytesPerPixel)
                    {
                        var bytes = RGBToHSVConverter.ConvertBack(new[]
                        {
                                (byte) (imgBsCrrLine[x] + hue),
                                MathUtil.ClampByte(imgBsCrrLine[x + 1] + sat),
                                MathUtil.ClampByte(imgBsCrrLine[x + 2] + val)
                            });

                        bmpCrrLine[x] = bytes[0];
                        bmpCrrLine[x + 1] = bytes[1];
                        bmpCrrLine[x + 2] = bytes[2];
                    }
                    //}
                });
                bitmap.UnlockBits(bitmapData);
            }
        }

        public static Bitmap ToBitmap(this ImageBase img)
        {
            var bitmap = new Bitmap(img.Width, img.Height, PixelFormat.Format24bppRgb);

            bitmap.CopyBitmap(img);

            return bitmap;
        }
    }
}