using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;
using NepBot;
using NepBot.Data;
using NepBot.Resources.Database;
using NepBot.Resources.Extensions;
using ImageResizer;
using ImageResizer.Collections;
using System.Text;
using FontStyle = System.Drawing.FontStyle;
using System.Net.Http;

namespace ConvenienceMethods
{
    public class GraphicsMethods
    {
        public float ConvertToPoint(int pixels)
        {
            //Instructions f = new Instructions()
            return 0.75f * (float)pixels;
        }

        private Font GetAdjustedFont(Graphics g, ref string graphicString, Font originalFont, int containerWidth, int maxFontSize, int minFontSize, bool smallestOnFail)
        {
            Font font = null;
            int num = 0;
            char[] array = graphicString.ToCharArray();
            bool flag = graphicString.Length > 10;
            if (flag)
            {
                bool flag2 = array.Length >= 20;
                if (flag2)
                {
                    for (int i = array.Length / 2 - 1; i < array.Length; i++)
                    {
                        bool flag3 = array[i] == ' ';
                        if (flag3)
                        {
                            num = i;
                            break;
                        }
                    }
                }
                else
                {
                    for (int j = graphicString.Length / 2 - 1; j < array.Length; j++)
                    {
                        bool flag4 = array[j] == ' ';
                        if (flag4)
                        {
                            num = j;
                            break;
                        }
                    }
                }
                bool flag5 = num >= 25;
                if (flag5)
                {
                    graphicString = graphicString.Insert(num, "\n");
                }
            }
            bool flag6 = false;
            decimal d = (containerWidth < 2000) ? 0.80m : 2m;
            minFontSize = 1;
            string text = graphicString;
            for (int k = maxFontSize; k > minFontSize; k--)
            {
                font = new Font(originalFont.Name, (float)k, originalFont.Style);
                int value = (int)g.MeasureString(text, font).Width;
                bool flag7 = containerWidth * d > value;
                if (flag7)
                {
                    flag6 = true;
                    break;
                }
            }
            bool flag8 = flag6;
            Font result;
            if (flag8)
            {
                result = font;
            }
            else if (smallestOnFail)
            {
                graphicString = "Failed with smallest on fail";
                result = font;
            }
            else
            {
                result = originalFont;
            }
            return result;
        }

        public string CardLayout(List<ACard> playerCards, List<ACard> dealerCards, string playerName)
        {
            string filename = Assembly.GetEntryAssembly().Location.Replace("bin\\Debug\\NepBot.exe", "Data\\blank-bitmap-bg.png");
            Bitmap bitmap = (Bitmap)Image.FromFile(filename);
            bitmap = new Bitmap(bitmap, new System.Drawing.Size(1500, 550));
            Font font = new Font("Impact", 50f);
            string result;
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                ImageData imageData = new ImageData();
                List<Bitmap> list = new List<Bitmap>();
                List<Bitmap> list2 = new List<Bitmap>();
                Rectangle rectangle = new Rectangle(0, 0, bitmap.Width, 230);
                Rectangle rectangle2 = new Rectangle(0, 250, bitmap.Width, 230);
                Rectangle layoutRect = new Rectangle(0, 325, bitmap.Width, 230);
                StringFormat stringFormat = new StringFormat();
                stringFormat.Alignment = StringAlignment.Near;
                stringFormat.LineAlignment = StringAlignment.Far;
                foreach (ACard acard in playerCards)
                {
                    list2.Add((Bitmap)this.ScaleImage((Bitmap)Image.FromFile(acard.CardImage), rectangle2.Width, rectangle2.Height));
                }
                foreach (ACard acard2 in dealerCards)
                {
                    list.Add((Bitmap)this.ScaleImage((Bitmap)Image.FromFile(acard2.CardImage), rectangle2.Width, rectangle2.Height));
                }
                int num = 0;
                int num2 = 1;
                foreach (Bitmap bitmap2 in list2)
                {
                    bool flag = num == 0;
                    if (flag)
                    {
                        graphics.DrawImage(bitmap2, rectangle2.Left, rectangle2.Top, bitmap2.Width, bitmap2.Height);
                    }
                    else
                    {
                        graphics.DrawImage(bitmap2, rectangle2.Left + num, rectangle2.Top, bitmap2.Width, bitmap2.Height);
                    }
                    num = (bitmap2.Width + 5) * num2;
                    num2++;
                }
                num = 0;
                num2 = 1;
                foreach (Bitmap bitmap3 in list)
                {
                    bool flag2 = num == 0;
                    if (flag2)
                    {
                        graphics.DrawImage(bitmap3, rectangle.Left, rectangle.Top, bitmap3.Width, bitmap3.Height);
                    }
                    else
                    {
                        graphics.DrawImage(bitmap3, rectangle.Left + num, rectangle.Top, bitmap3.Width, bitmap3.Height);
                    }
                    num = (bitmap3.Width + 5) * num2;
                    num2++;
                }
                GraphicsPath graphicsPath = new GraphicsPath();
                graphicsPath.AddString(playerName + "'s blackjack game", font.FontFamily, 1, font.Size, layoutRect, stringFormat);
                graphics.DrawPath(new Pen(Color.Black, font.Size / 5f)
                {
                    LineJoin = LineJoin.Round
                }, graphicsPath);
                graphics.FillPath(new SolidBrush(Color.Yellow), graphicsPath);
                graphics.Flush();
                string text = Program.DataPath("heysup", "jpg");
                bitmap.Save(text);
                bitmap.Dispose();
                result = text;
            }
            return result;
        }

        public Bitmap ProfileArt(string userName, Bitmap userAvatar, UserData userData)
        {
            Bitmap bitmap = (Bitmap)Image.FromFile(Program.DataPath("prototype-profile", "jpg"));
            Bitmap result;
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                string filename = Program.DataPath("exp-bar", "jpg");
                Bitmap bitmap2 = (Bitmap)Image.FromFile(filename);
                Bitmap bitmap3 = (Bitmap)Image.FromFile(filename);
                Bitmap bitmap4 = (Bitmap)Image.FromFile(filename);
                GraphicsPath graphicsPath = new GraphicsPath();
                int num = 115;
                Rectangle rectangle = new Rectangle(0, 0, bitmap.Width - 25, bitmap.Height - 25);
                Font font = new Font("Consolas", 25f);
                graphicsPath.AddString(userName, font.FontFamily, 1, 22f, new Rectangle(rectangle.Left + num, rectangle.Top + 20, rectangle.Width, rectangle.Height), StringFormat.GenericDefault);
                graphicsPath.AddString("Total Posts: " + userData.TotalPosts.ToString(), font.FontFamily, 1, 16f, new Rectangle(rectangle.Left + num, rectangle.Top + 50, rectangle.Width, rectangle.Height), StringFormat.GenericDefault);
                graphicsPath.AddString("Total Pudding: " + userData.Pudding.ToString(), font.FontFamily, 1, 16f, new Rectangle(rectangle.Left + num, rectangle.Top + 250, rectangle.Width, rectangle.Height), StringFormat.GenericDefault);
                graphicsPath.AddString("Highest Word Count: " + userData.WordCountRecord.ToString(), font.FontFamily, 1, 16f, new Rectangle(rectangle.Left + num, rectangle.Top + 200, rectangle.Width, rectangle.Height), StringFormat.GenericDefault);
                graphicsPath.AddString("Roleplay", font.FontFamily, 1, 15f, new Rectangle(rectangle.Left + num, rectangle.Top + 70, rectangle.Width, rectangle.Height), StringFormat.GenericDefault);
                graphicsPath.AddString(string.Concat(new object[]
                {
                    "Level: ",
                    userData.CasualLevel,
                    " (",
                    userData.CurrentCasualExp,
                    " / ",
                    userData.ReqExp(userData.CasualLevel),
                    ")"
                }), font.FontFamily, 1, 13f, new Rectangle(rectangle.Left + 280, rectangle.Top + 75, rectangle.Width, rectangle.Height), StringFormat.GenericDefault);
                graphicsPath.AddString("Literate Roleplay", font.FontFamily, 1, 15f, new Rectangle(rectangle.Left + num, rectangle.Top + 110, rectangle.Width, rectangle.Height), StringFormat.GenericDefault);
                graphicsPath.AddString(string.Concat(new object[]
                {
                    "Level: ",
                    userData.ParaLevel,
                    " (",
                    userData.CurrentParaExp,
                    " / ",
                    userData.ReqExp(userData.ParaLevel),
                    ")"
                }), font.FontFamily, 1, 13f, new Rectangle(rectangle.Left + 280, rectangle.Top + 113, rectangle.Width, rectangle.Height), StringFormat.GenericDefault);
                graphicsPath.AddString("Non-Roleplay", font.FontFamily, 1, 15f, new Rectangle(rectangle.Left + num, rectangle.Top + 150, rectangle.Width, rectangle.Height), StringFormat.GenericDefault);
                graphicsPath.AddString(string.Concat(new object[]
                {
                    "Level: ",
                    userData.NonLevel,
                    " (",
                    userData.CurrentNonExp,
                    " / ",
                    userData.ReqExp(userData.NonLevel),
                    ")"
                }), font.FontFamily, 1, 13f, new Rectangle(rectangle.Left + 280, rectangle.Top + 153, rectangle.Width, rectangle.Height), StringFormat.GenericDefault);
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.DrawPath(new Pen(Color.Black, 3f)
                {
                    LineJoin = LineJoin.Round
                }, graphicsPath);
                graphics.FillPath(new SolidBrush(Color.White), graphicsPath);
                bool flag = userAvatar != null;
                if (flag)
                {
                    userAvatar = (Bitmap)this.ScaleImage(userAvatar, 84, 84);
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    graphics.DrawImage(userAvatar, rectangle.Left + 20, rectangle.Top + 20, userAvatar.Width, userAvatar.Height);
                }
                decimal fg = userData.ReqExp(userData.CasualLevel);
                graphics.DrawImage(bitmap2, rectangle.Left + num, rectangle.Top + 90, (int)barSize(bitmap2, (decimal)userData.ReqExp(userData.CasualLevel), (decimal)userData.CurrentCasualExp), bitmap2.Height);
                graphics.DrawImage(bitmap3, rectangle.Left + num, rectangle.Top + 130, (int)barSize(bitmap3, (decimal)userData.ReqExp(userData.ParaLevel), (decimal)userData.CurrentParaExp), bitmap3.Height);
                graphics.DrawImage(bitmap4, rectangle.Left + num, rectangle.Top + 170, (int)barSize(bitmap4, (decimal)userData.ReqExp(userData.NonLevel), (decimal)userData.CurrentNonExp), bitmap4.Height);
                result = bitmap;
            }
            return result;
        }

        private decimal barSize(Bitmap bmp, decimal max, decimal cur)
        {
            decimal num = cur / max;
            return (bmp.Width * num) * .95m;
        }

        public Image ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            double val = (double)maxWidth / (double)image.Width;
            double val2 = (double)maxHeight / (double)image.Height;
            double num = Math.Min(val, val2);
            int width = (int)((double)image.Width * num);
            int height = (int)((double)image.Height * num);
            Bitmap bitmap = new Bitmap(width, height);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.DrawImage(image, 0, 0, width, height);
            }
            return bitmap;
        }

        /// <summary>
        /// Used to crop/resize and place an image over another image.
        /// </summary>
        /// <param name="imgToResize">The image that will be cropped/resized and placed over another image.</param>
        /// <param name="destinationSize">The size the cropped/resized image should be in the final photo.</param>
        /// <param name="posX">Position of the x axis.</param>
        /// <param name="posY">Position of the y axis.</param>
        /// <param name="filePathToWriteOver">The background image.</param>
        /// <returns></returns>
        public Image MemeCaption(Image imgToResize, System.Drawing.Size destinationSize, float posX, float posY, string filePathToWriteOver, int offsetX = 0, int offsetY = 0)
        {
            try
            {
                var originalWidth = imgToResize.Width;
                var originalHeight = imgToResize.Height;

                //how many units are there to make the original length
                var hRatio = (float)originalHeight / destinationSize.Height;
                var wRatio = (float)originalWidth / destinationSize.Width;

                //get the shorter side
                var ratio = Math.Min(hRatio, wRatio);

                var hScale = Convert.ToInt32(destinationSize.Height * ratio) + offsetY;
                var wScale = Convert.ToInt32(destinationSize.Width * ratio) + offsetX;

                //start cropping from the center
                var startX = (originalWidth - wScale) / 2;
                var startY = (originalHeight - hScale) / 2;

                //crop the image from the specified location and size
                var sourceRectangle = new Rectangle(startX, startY, wScale, hScale);

                Image src = Image.FromFile(filePathToWriteOver);

                //the future size of the image

                //fill-in the whole bitmap
                var destinationRectangle = new Rectangle((int)(posX), (int)(src.Height * posY), destinationSize.Width, destinationSize.Height);

                //generate the new image
                using (var g = Graphics.FromImage(src))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(imgToResize, destinationRectangle, sourceRectangle, GraphicsUnit.Pixel);
                }

                return src;
            }
            catch (Exception i)
            {
                Console.WriteLine(string.Format("From MemeCaption>>> {0}\n{1}\n{2}\n{3}\n{4}\n{5}\n{6}\n{7}", new object[]
                {
                    i.Message,
                    i.TargetSite,
                    i.Source,
                    i.InnerException,
                    i.StackTrace,
                    i.HResult,
                    i.Data,
                    i.HelpLink
                }), false, null, null);
            }
            return null;
        }

        public Bitmap InsertTextOnImage(string fontName, float fontSize, Bitmap bitmap, string text, bool isBottomText, bool doublePanelBottomPosition = false, bool isGif = false)
        {
            decimal divisionFucker = 14m;
            if (isGif && (bitmap.Height > 200 || bitmap.Width > 200))
            {
                fontName = "Rosario";
                divisionFucker = 25;
            }
            if (bitmap.Width <= 125)
                divisionFucker = 14m;

            //Bitmap bmm = new Bitmap(bitmap.Width, bitmap.Height);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                try
                {
                    float num = (float)(bitmap.Width / divisionFucker);
                    bool flag = bitmap.Width >= 1000;
                    if (flag)
                    {
                        num = num - (num * .15f);
                    }
                    Font font = new Font(fontName, num);
                    StringFormat stringFormat = new StringFormat();
                    //int num2 = (!isBottomText && doublePanelBottomPosition) ? ((int)((float)bitmap.Height / 3f)) : 15;
                    stringFormat.Alignment = StringAlignment.Center;
                    if (isBottomText)
                    {
                        stringFormat.LineAlignment = StringAlignment.Far;
                    }
                    if (doublePanelBottomPosition)
                    {
                        stringFormat.LineAlignment = StringAlignment.Near;
                    }
                    //Rectangle layoutRect = new Rectangle(new System.DrawingCore.Point((int)(this.ConvertToPoint(bitmap.Width) * 0.03f), 
                    //  (!isBottomText) ? num2 : ((int)((float)bitmap.Height / 4f))), 
                    //new System.DrawingCore.Size((int)(this.ConvertToPoint(bitmap.Width) * 1.3f), 
                    //(int)this.ConvertToPoint(bitmap.Height)));
                    Rectangle layoutRect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);

                    font = new Font(fontName, num, FontStyle.Bold);
                    GraphicsPath graphicsPath = new GraphicsPath();
                    graphicsPath.AddString(text, font.FontFamily, 1, font.Size, layoutRect, stringFormat);
                    Pen pen = new Pen(Color.Black, font.Size / 5f);
                    pen.LineJoin = LineJoin.Round;
                    graphics.SmoothingMode = (bitmap.Width > 500 || bitmap.Height > 500) ? SmoothingMode.HighQuality : SmoothingMode.Default;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.DrawPath(pen, graphicsPath);
                    graphics.FillPath(new SolidBrush(Color.Yellow), graphicsPath);
                    graphics.Flush();

                }
                catch (Exception m)
                {
                    ExtensionMethods.WriteToLog(ExtensionMethods.LogType.ErrorLog, m, "Autobot");
                }
            }
            return bitmap;
        }
    }
}

public class GifInfo
{
    [System.Runtime.InteropServices.DllImport("gdi32.dll")]
    public static extern bool DeleteObject(IntPtr hObject);
    private IList<Image> frames;
    public byte[] f = new byte[] { };
    public Image reconstructedGif;
    public readonly string originalDelay;
    public IList<Image> Frames
    {
        get
        {
            return this.frames;
        }
        set
        {
            this.frames = value;
        }
    }

    public int FrameCount
    {
        get
        {
            return frames.Count;
        }
    }

    public string WriteByteArray(byte[] array)
    {
        StringBuilder sb = new StringBuilder();

        foreach (var g in array)
            sb.Append(g).Append(" ");

        return sb.ToString();
    }

    public Byte[] Ff()
    {
        //GifInfo info = new GifInfo(stream);
        for (int i = 0; i < this.Frames.Count - 1; i++)
        {
            string v = (i < 10) ? "0" + i.ToString() : i.ToString();
            string frameFilePath = Path.Combine(Path.GetDirectoryName(@"E:\Backups\Desktop Files\Gifs\redgiflesgo.gif"), $"{v} - {Path.GetFileNameWithoutExtension(@"E:\Backups\Desktop Files\Gifs")}.gif");//E:\Backups\Desktop Files\Gifs
        }
        System.Windows.Media.Imaging.GifBitmapEncoder gEnc = new GifBitmapEncoder();
        for (int i = 0; i < Frames.Count; i++)
        {
            using (Bitmap bm = (Bitmap)Frames[i])
            {
                IntPtr bmp = bm.GetHbitmap();
                BitmapSource src = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    bmp,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                var p = BitmapFrame.Create(src);
                gEnc.Frames.Add(p);
                DeleteObject(bmp); // recommended, handle memory leak
            }
        }
        var newBytes = new List<byte>();
        byte[] nz = null;
        using (var ms = new MemoryStream())
        {
            gEnc.Save(ms);
            ms.Position = 0;
            gEnc = null;
            byte[] fileBytes = ms.ToArray();
            var applicationExtension = new byte[] { 33, 255, 11, 78, 69, 84, 83, 67, 65, 80, 69, 50, 46, 48, 3, 1, 0, 0, 0 };
            newBytes.AddRange(fileBytes.Take(13));
            newBytes.AddRange(applicationExtension);
            newBytes.AddRange(fileBytes.Skip(13));
            for (int i = 0; i < newBytes.Count; i++)
            {
                if (newBytes[i] == 33 && newBytes[i + 1] == 249 && newBytes[i + 2] == 4)
                {
                    newBytes[i] = f[0];
                    newBytes[i + 1] = f[1];
                    newBytes[i + 2] = f[2];
                    newBytes[i + 3] = f[3];
                    newBytes[i + 4] = f[4];
                    newBytes[i + 5] = f[5];
                }
            }
        }
        return newBytes.ToArray();
    }

    public byte[] StringToByteArray(String hex)
    {
        int NumberChars = hex.Length;
        byte[] bytes = new byte[NumberChars / 2];
        for (int i = 0; i < NumberChars; i += 2)
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        return bytes;
    }

    public string ByteArrayToString(byte[] ba)
    {
        return BitConverter.ToString(ba).Replace("-", "");
    }

    public string[] ByteArrayToHexaArray(byte[] ba)
    {
        return BitConverter.ToString(ba).Split('-');
    }

    public string GrabControlDelay(string[] hexaString)
    {
        //21-F9-04
        string found = string.Empty;
        for (int i = 0; i < hexaString.Length; i++)
        {
            if (hexaString[i] == "21" && hexaString[i + 1] == "F9" && hexaString[i + 2] == "04")
            {
                found = string.Concat(hexaString[i], hexaString[i + 1], hexaString[i + 2], hexaString[i + 3], hexaString[i + 4], hexaString[i + 5]);
                break;
            }
        }
        return found;
    }

    public string suffixPlace = string.Empty;
    public double BytesToString(long byteCount)
    {
        string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
        if (byteCount == 0)
            return 0;
        long bytes = Math.Abs(byteCount);
        int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
        double num = Math.Round(bytes / Math.Pow(1024, place), 2);
        suffixPlace = suf[place];
        return (Math.Sign(byteCount) * num);
    }

    public string FileTooBig(Image image, double zx)
    {
        Console.WriteLine($"{zx}{suffixPlace} {image.Width} x {image.Height}");
        if (image.Height > 800 || image.Width > 800)
        {
            if (zx > 2.5d && suffixPlace == "MB" || suffixPlace == "GB")
            {
                //image.Dispose();
                debugMsg = $"File size is too big. Size is {zx}{suffixPlace}. Width is {image.Width}, height is {image.Height}. Keep the height and width under 800 and size under 3mb. Sizes up to 5mb can work if the width and height are less than 600. This is due to Discord's upload size limit.";
                image.Dispose();
                return debugMsg;
            }
        }
        return string.Empty;
    }

    public string debugMsg = string.Empty;

    public GifInfo(Image image)
    {
        if (image.RawFormat.Equals(ImageFormat.Gif))
        {
            frames = new List<Image>();

            if (ImageAnimator.CanAnimate(image))
            {
                //Get frames  
                var dimension = new FrameDimension(image.FrameDimensionsList[0]);
                int frameCount = image.GetFrameCount(dimension);

                for (int i = 0; i < frameCount; i++)
                {
                    image.SelectActiveFrame(dimension, i);
                    var frame = image.Clone() as Image;
                    frames.Add(frame);
                }
                ImageConverter _imageConverter = new ImageConverter();
                f = (byte[])_imageConverter.ConvertTo(image, typeof(byte[]));

                for (int i = 0; i < f.Length; i++)
                {
                    if (f[i] == 33 && f[i + 1] == 249 && f[i + 2] == 4)
                    {
                        f = new byte[] { f[i], f[i + 1], f[i + 2], f[i + 3], f[i + 4], f[i + 5] };
                        //33 249 4 4 7 0
                        break;
                    }
                }

            }
        }
        else
        {
            throw new FormatException("Not valid GIF image format");
        }
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();
    }
}