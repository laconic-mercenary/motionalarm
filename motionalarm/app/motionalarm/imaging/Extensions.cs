namespace app.motionalarm.imaging
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// http://stackoverflow.com/questions/94456/load-a-wpf-bitmapimage-from-a-system-drawing-bitmap
    /// </summary>
    public static class ExtensionMethods
    {

        /// <summary>
        /// Generates a BitmapSource object using the raw bytes of an image that was encoded.
        /// </summary>
        /// <param name="imageData">The raw bytes of an image.</param>
        /// <param name="decodePixelWidth"></param>
        /// <param name="decodePixelHeight"></param>
        /// <returns></returns>
        public static BitmapSource createImage(this byte[] imageData)
        {
            if (imageData == null) return null;

            BitmapImage result = new BitmapImage();
            result.BeginInit();
            //if (decodePixelWidth > 0) {
            //    result.DecodePixelWidth = decodePixelWidth;
            //}
            //if (decodePixelHeight > 0) {
            //    result.DecodePixelHeight = decodePixelHeight;
            //}
            result.StreamSource = new MemoryStream(imageData);
            result.CreateOptions = BitmapCreateOptions.None;
            result.CacheOption = BitmapCacheOption.Default;
            result.EndInit();
            return result;
        }

        /// <summary>
        /// Converts an ImageSource to a byte array based on the specified image format.
        /// </summary>
        /// <param name="image">The image to transform into a raw data.</param>
        /// <param name="preferredFormat">".jpeg", ".bmp", ".png" for example.</param>
        /// <returns></returns>
        public static byte[] getEncodedImageData(this ImageSource image, string preferredFormat)
        {
            byte[] result = null;
            BitmapEncoder encoder = null;
            switch (preferredFormat.ToLower())
            {
                case ".jpg":
                case ".jpeg":
                    encoder = new JpegBitmapEncoder();
                    break;

                case ".bmp":
                    encoder = new BmpBitmapEncoder();
                    break;

                case ".png":
                    encoder = new PngBitmapEncoder();
                    break;

                case ".tif":
                case ".tiff":
                    encoder = new TiffBitmapEncoder();
                    break;

                case ".gif":
                    encoder = new GifBitmapEncoder();
                    break;

                case ".wmp":
                    encoder = new WmpBitmapEncoder();
                    break;
            }

            if (image is BitmapSource)
            {
                MemoryStream stream = new MemoryStream();
                encoder.Frames.Add(BitmapFrame.Create(image as BitmapSource));
                encoder.Save(stream);
                stream.Seek(0, SeekOrigin.Begin);
                result = new byte[stream.Length];
                BinaryReader br = new BinaryReader(stream);
                br.Read(result, 0, (int)stream.Length);
                br.Close();
                stream.Close();
            }
            return result;
        }

        /// <summary>
        /// Converts a <see cref="System.Drawing.Image"/> into a WPF <see cref="BitmapSource"/>.
        /// </summary>
        /// <param name="source">The source image.</param>
        /// <returns>A BitmapSource</returns>
        public static BitmapSource toBitmapSource(this Image source)
        {
            var bitmap = new Bitmap(source);

            var bitSrc = bitmap.toBitmapSource();

            bitmap.Dispose();

            return bitSrc;
        }

        public static System.Drawing.Bitmap toDrawingBitmap(this BitmapSource bitmapSource)
        {
            System.Drawing.Bitmap bitmap;
            using (MemoryStream outStream = new MemoryStream())
            {
                // from System.Media.BitmapImage to System.Drawing.Bitmap 
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapSource));
                enc.Save(outStream);
                bitmap = new System.Drawing.Bitmap(outStream);
            }
            return bitmap;
        }

        /// <summary>
        /// Saves a bitmap source to a jpeg file.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="path"></param>
        public static void toJpegFile(this BitmapSource source, string path)
        {
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            BitmapFrame frame = BitmapFrame.Create(source);
            FileStream stream = new FileStream(path, FileMode.Create);
            encoder.Frames.Add(frame);
            encoder.Save(stream);
            // cleanup
            stream.Close();
            stream.Dispose();
            stream = null;
            encoder.Frames.Clear();
        }

        /// <summary>
        /// Converts a <see cref="System.Drawing.Bitmap"/> into a WPF <see cref="BitmapSource"/>.
        /// </summary>
        /// <remarks>Uses GDI to do the conversion. Hence the call to the marshalled DeleteObject.
        /// </remarks>
        /// <param name="source">The source bitmap.</param>
        /// <returns>A BitmapSource</returns>
        public static BitmapSource toBitmapSource(this Bitmap source)
        {
            BitmapSource bitSrc;
            IntPtr hBitmap = source.GetHbitmap();
            try
            {
                bitSrc = Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Win32Exception)
            {
                bitSrc = null;
            }
            finally
            {
                NativeMethods.DeleteObject(hBitmap);
            }

            return bitSrc;
        }

        #region Nested type: NativeMethods

        /// <summary>
        /// FxCop requires all Marshalled functions to be in a class called NativeMethods.
        /// </summary>
        internal static class NativeMethods
        {
            [DllImport("gdi32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool DeleteObject(IntPtr hObject);
        }

        #endregion
    }
}
