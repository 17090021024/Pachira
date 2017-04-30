using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace TrunkCommon
{
    public static class ImageHelper
    {
        /// <summary>
        /// 1像素空图(image/gif)
        /// </summary>
        public static readonly byte[] EmptyImage = new byte[] { 71, 73, 70, 56, 57, 97, 1, 0, 1, 0, 240, 0, 0, 0, 0, 0, 0, 0, 0, 33, 249, 4, 1, 0, 0, 0, 0, 44, 0, 0, 0, 0, 1, 0, 1, 0, 0, 2, 2, 68, 1, 0, 59 };
        /// <summary>
        /// HTML专用的base64空图像字符串(1x1 gif)
        /// </summary>
        public const string HTMLBase64EmptyImage = "data:image/gif;base64,R0lGODlhAQABAIAAAAAAAAAAACH5BAEAAAAALAAAAAABAAEAAAICRAEAOw==";
        /// <summary>
        /// 生成缩略图v4
        /// 返回生成好的image对象缩略图，请用完后手动dispose两个（原图和生成的图）！
        /// 将以Color颜色，width宽和height高填充背景,Color.Empty表示无背景色
        /// </summary>
        public static Bitmap CreateThumbImage(Image img, int width, int height, Color backgroundColor)
        {
            if (img == null) return null;
            if (width < 1) width = 1;
            if (height < 1) height = 1;
            Size size = GetThumbImageSize(img.Width, img.Height, width, height);
            Bitmap bitmap = null;
            Graphics g = null;
            bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            bitmap.SetResolution(72f, 72f);
            g = Graphics.FromImage(bitmap);
            if (backgroundColor != Color.Empty)
                g.Clear(backgroundColor);
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(img, new Rectangle((width - size.Width) / 2, (height - size.Height) / 2, size.Width, size.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel);
            g.Dispose();
            return bitmap;
        }

        /// <summary>
        /// 获取等比例缩放缩略图的最终大小
        /// </summary>
        /// <param name="srcW">原图宽</param>
        /// <param name="srcH">原图高</param>
        /// <param name="destW">目标宽</param>
        /// <param name="destH">目标高</param>
        /// <returns></returns>
        public static Size GetThumbImageSize(int srcW, int srcH, int destW, int destH)
        {
            Size size = new Size();
            float wZoom = (float)srcW / (float)destW;
            float hZoom = (float)srcH / (float)destH;
            if (wZoom > hZoom)
            {
                if (wZoom <= 1f)
                {
                    size.Width = srcW;
                    size.Height = srcH;
                }
                else
                {
                    size.Width = destW;
                    size.Height = (int)(srcH / wZoom);
                }
            }
            else
            {
                if (hZoom <= 1f)
                {
                    size.Width = srcW;
                    size.Height = srcH;
                }
                else
                {
                    size.Width = (int)(srcW / hZoom);
                    size.Height = destH;
                }
            }
            return size;
        }

        /// <summary>
        /// 获取图像编码解码器的所有相关信息
        /// </summary>
        /// <param name="mimeType">包含编码解码器的多用途网际邮件扩充协议 (MIME) 类型的字符串</param>
        /// <returns>返回图像编码解码器的所有相关信息</returns>
        public static ImageCodecInfo GetCodecInfo(string mimeType)
        {
            ImageCodecInfo[] CodecInfo = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo ici in CodecInfo)
            {
                if (ici.MimeType.Equals(mimeType, System.StringComparison.CurrentCultureIgnoreCase)) return ici;
            }
            return null;
        }
        /// <summary>
        /// 获取base64图像字符串
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetBase64Image(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return string.Empty;

            return GetBase64Image(Image.FromFile(filePath));
        }
        /// <summary>
        /// 获取base64图像字符串
        /// </summary>
        public static string GetBase64Image(Image img)
        {
            if (img == null)
                return string.Empty;

            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
                img.Dispose();
                return Convert.ToBase64String(ms.ToArray());
            }
        }
        /// <summary>
        /// 获取HTML专用的base64图像字符串
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetHTMLBase64Image(string filePath)
        {
            return string.Concat("data:image/gif;base64,", GetBase64Image(filePath));
        }
        /// <summary>
        /// 获取HTML专用的base64图像字符串
        /// </summary>
        public static string GetHTMLBase64Image(Image img)
        {
            return string.Concat("data:image/gif;base64,", GetBase64Image(img));
        }

        /// <summary>
        /// 验证数据是否为jpg、gif、bmp、png、tiff、icon的图片格式，是则返回该图片格式，否则返回null
        /// </summary>
        public static ImageFormat ValidataImageFormat(byte[] data)
        {
            if (data.Length < 5)
                return null;
            else if (data[0] == 255 && data[1] == 216)
                return ImageFormat.Jpeg;
            else if (data[0] == 66 && data[1] == 77)
                return ImageFormat.Bmp;
            else if (data[0] == 71 && data[1] == 73 && data[2] == 70 && (data[4] == 55 || data[4] == 57))//gif87||gif89
                return ImageFormat.Gif;
            else if (data[0] == 137 && data[1] == 80)
                return ImageFormat.Png;
            else if ((data[0] == 77 && data[1] == 77) || (data[0] == 73 && data[1] == 73))
                return ImageFormat.Tiff;
            else if (data[2] == 1 && data[4] == 1)
                return ImageFormat.Icon;
            else
                return null;
        }
    }//end class
}
