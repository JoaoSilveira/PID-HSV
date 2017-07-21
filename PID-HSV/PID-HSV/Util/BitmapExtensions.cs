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

        //public static void CopyBitmap(this Bitmap bitmap, ImageBase img, Filter filter)
        //{
        //    unsafe
        //    {
        //        var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
        //            ImageLockMode.ReadWrite, bitmap.PixelFormat);

        //        var bytesPerPixel = System.Drawing.Image.GetPixelFormatSize(bitmap.PixelFormat) / 8;
        //        var heightInPixels = bitmapData.Height;
        //        var widthInBytes = bitmapData.Width * bytesPerPixel;
        //        var ptrFirstPixel = (byte*)bitmapData.Scan0;
        //        var pointer = (byte*)img.Buffer.ToPointer();

        //        Parallel.For(0, heightInPixels, y =>
        //        {
        //            var currentLine = ptrFirstPixel + y * bitmapData.Stride;
        //            var currLinePointer = pointer + (heightInPixels - y - 1) * img.LineStride;

        //            for (var x = 0; x < widthInBytes; x = x + bytesPerPixel)
        //            {
        //                var bytes = Converter.ConvertBack(new[] {
        //                    (byte) (currLinePointer[x] + filter.Hue),
        //                    (byte) Math.Max(currLinePointer[x + 1] * filter.Saturation, 255),
        //                    currLinePointer[x + 2]
        //                }, null, null, null) as byte[];

        //                currentLine[x] = bytes[0];
        //                currentLine[x + 1] = bytes[1];
        //                currentLine[x + 2] = bytes[2];
        //            }
        //        });
        //        bitmap.UnlockBits(bitmapData);
        //    }
        //}

        public static Bitmap ToBitmap(this ImageBase img)
        {
            var bitmap = new Bitmap(img.Width, img.Height, PixelFormat.Format24bppRgb);

            bitmap.CopyBitmap(img);

            return bitmap;
        }
    }
}