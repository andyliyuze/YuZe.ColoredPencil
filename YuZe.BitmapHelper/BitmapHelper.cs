using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace YuZe.ColoredPencil.Helper
{
    public static class BitmapHelper
    {
        public static List<Color> MyColors = new List<Color>();

        public static string[] Colors = {

            "#ffffff","#fffcd1","#fff3c1","#fde1b9","#fbe2c4","#fff000","#ffe001","#dcc400","#d8ad00","#f8b713","#f49d36","#ef8200",

            "#f29c8f","#e7340b","#e70044","#e6012c","#d9082f","#d70b17","#e70020","#c80815","#ac3e03","#ac3e03","#922a11","#9f141b",

            "#f7c8dc","#ed86b5","#ea5098","#b64a96","#a44998","#b50081","#8f0171","#b879b0","#a674af","#9a7bb6","#7c4799","#541b86",

            "#b8e2f8","#86cabf","#61c1b5","#01afec","#019fe9","#0168b7","#0081ab","#004ea1","#003585","#063190","#352f8f","#234455",

            "#dadf00","#abcd05","#90c320","#91a100","#798602","#3eb034","#12a83a","#009946","#008e44","#007445","#006835","#00522e",

            "#c8c8c8","#b6b6b6","#b9bdbc","#707070","#617985","#3f3939","#c99d3a","#b37400","#a85102","#6a2c05","#411401","#221715",
        };

        /// <summary>
        /// 越接近白色值约接近0
        /// 越接近黑色值约接近255
        /// 如果没有对应色值，可以将R，G，B值统一加值
        /// R，G，B越大颜色越浅
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public static unsafe byte[] ConvertTo8Byte(Bitmap img)
        {
            byte[] result = new byte[img.Width * img.Height];
            int n = 0;
            BitmapData data = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly,
                                           PixelFormat.Format24bppRgb);
            var bp = (byte*)data.Scan0.ToPointer();

            for (int i = 0; i != data.Height; i++)
            {
                for (int j = 0; j != data.Width; j++)
                {
                    result[n] = bp[i * data.Stride + j * 3];
                    n++;

                }
            }
            img.UnlockBits(data);

            return result;

        }
        /// <summary>
        /// 求一个颜色的接近色，应该RBG同时加一个值，或者同时减一个值
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public static (List<Color> imgRGB, List<Color> clost) GetRBG(Bitmap img)
        {
            var systems = GetMyColors();
            var clost = new List<Color>();

            var rgbs = new List<Color>();
            var names = new List<string>();

            var w = (int)(img.Width / 5);
            for (int i = 0; i < img.Width; i += w)
            {
                if (i > img.Width) { break; }
                for (int j = 0; j < img.Height; j += w)
                {
                    if (j > img.Height) { break; }
                    Color pixel = img.GetPixel(i, j);
                    Color minColor = Color.Empty;
                    double minDis = 999;
                    foreach (var thisColor in systems)
                    {
                        if (thisColor.Name == "#ff01afec")
                        {

                        }
                        var dis = DistanceOf(pixel.RGB2HSV(), thisColor.RGB2HSV());
                        if (dis < minDis)
                        {
                            minDis = dis;
                            minColor = thisColor;

                        }
                    }
                    if (minDis < 3)
                    {

                    }
                    if (rgbs.Any(a => a == pixel))
                    {
                        continue;
                    }
                    clost.Add(minColor);
                    names.Add(minColor.Name);
                    rgbs.Add(pixel);
                }
            }
            names = names.ToList();

            for (int i = 0; i < rgbs.Count; i++)
            {
                var item = rgbs[i];
                var name = names[i];
                var closeitem = clost[i];
                Console.WriteLine($"{name}  rgb({item.R } {item.G} {item.B})  rgb({closeitem.R } {closeitem.G} {closeitem.B})");
            }


            Console.WriteLine($"---------------");

            return (rgbs, clost);
        }

        private static double R = 100;
        private static double angle = 30;
        private static double h = R * Math.Cos(angle / 180 * Math.PI);
        private static double r = R * Math.Sin(angle / 180 * Math.PI);

        public static void Splt(Bitmap img)
        {
            var index = 1;
            var w = 100;
            var h = w;
            for (int i = 0; i < img.Width; i += w)
            {
                for (int j = 0; j < img.Height; j += h)
                {
                    var height = h;
                    if (j + h > img.Height)
                    {
                        height = img.Height - j;
                    }
                    var width = w;
                    if (i + w > img.Width)
                    {
                        width = img.Width - i;
                    }
                    var copyBitmap = img.Clone(new Rectangle(i, j, width, height), PixelFormat.Undefined);

                    string dic = "spilt_2/spilt_" + index;
                    if (!Directory.Exists(dic))
                    {
                        Directory.CreateDirectory(dic);
                    }

                    var clost = GetRBG(copyBitmap);

                    var bit = new Bitmap(100, 100, PixelFormat.Format24bppRgb);
                    var g = Graphics.FromImage(bit);

                    foreach (var item in clost.imgRGB)
                    {
                        var color = item;
                        g.FillRectangle(new SolidBrush(color), new Rectangle(0, 0, 100, 100));//这句实现填充矩形的功能
                        g.Save();
                        var colorName = color.Name.Substring(2, color.Name.Length - 2);
                        string colorFileName = dic + "/" + color.Name + ".jpeg";
                        bit.Save(colorFileName, ImageFormat.Jpeg);
                    }

                    string filename = dic + "/" + "image_" + i + "_" + j + ".jpeg";
                    copyBitmap.Save(Path.Combine(filename), ImageFormat.Jpeg);

                    index++;
                }
            }
        }

        /// 返回两个颜色在HSV颜色空间的距离
        public static double DistanceOf(HSV hsv1, HSV hsv2)
        {
            double x1 = r * hsv1.V * hsv1.S * Math.Cos(hsv1.H / 180 * Math.PI);
            double y1 = r * hsv1.V * hsv1.S * Math.Sin(hsv1.H / 180 * Math.PI);
            double z1 = h * (1 - hsv1.V);
            double x2 = r * hsv2.V * hsv2.S * Math.Cos(hsv2.H / 180 * Math.PI);
            double y2 = r * hsv2.V * hsv2.S * Math.Sin(hsv2.H / 180 * Math.PI);
            double z2 = h * (1 - hsv2.V);
            double dx = x1 - x2;
            double dy = y1 - y2;
            double dz = z1 - z2;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public static double DistanceOfV2(Color color1, Color color2)
        {
            var dis = Math.Sqrt((color1.R - color2.R) ^ 2 + (color1.G - color2.G) ^ 2 + (color1.B - color2.B) ^ 2);

            return dis;
        }

        public static Color FindColorMin(Color color1, List<Color> colors)
        {
            var select = colors.Where(a => a.R <= color1.R && a.G <= color1.G && a.B <= color1.B && a != color1).ToList();
            double mindis = 999;
            var mincolor = Color.Empty;
            foreach (var item in select)
            {
                var dis = DistanceOfV2(color1, item);
                if (dis < mindis)
                {
                    mindis = dis;
                    mincolor = item;
                }
            }
            return mincolor;
        }

        public static Color FindColorMax(Color color1, List<Color> colors)
        {
            var select = colors.Where(a => a.R >= color1.R && a.G >= color1.G && a.B >= color1.B && a != color1).ToList();
            var minR = select.Min(a => a.R);

            double mindis = 999;
            var mincolor = Color.Empty;
            //mincolor = select.FirstOrDefault(a => a.R == minR);
            //return mincolor;
            foreach (var item in select)
            {
                var dis = DistanceOfV2(color1, item);
                if (dis < mindis)
                {
                    mindis = dis;
                    mincolor = item;
                }
            }
            return mincolor;
        }

        public static List<Color> GetSystemColors()
        {
            var type = typeof(Color);
            var pros = type.GetProperties();
            var color = Color.Empty;
            var systems = new List<Color>();
            var colors = pros.Where(a =>
            {
                return a.PropertyType == typeof(Color);
            }).ToList();
            foreach (var item in colors)
            {
                var thisColor = (Color)item.GetValue(color);
                systems.Add(thisColor);
            }
            return systems;
        }

        public static List<Color> GetMyColors()
        {
            if (MyColors.Any())
            {
                return MyColors;
            }
            var systems = new List<Color>();

            foreach (var item in Colors)
            {
                var thisColor = ColorTranslator.FromHtml(item);
                systems.Add(thisColor);
            }
            MyColors = systems;
            return systems;
        }

        public static void PaintLine(Bitmap bit)
        {
            var w = 100;
            var paintWidth = 0;
            var paintHeight = 0;
            var g = Graphics.FromImage(bit);
            //创建黑色pen对象
            var pen = new Pen(Color.FromArgb(158, 158, 158), 2f);
            while (paintWidth + 100 <= bit.Width)
            {
                paintWidth += w;
                var p1 = new Point(paintWidth, 0);
                var p2 = new Point(paintWidth, bit.Height);
                g.DrawLine(pen, p1, p2);

            }
            while (paintHeight + 100 <= bit.Height)
            {
                paintHeight += w;
                var p1 = new Point(0, paintHeight);
                var p2 = new Point(bit.Width, paintHeight);
                g.DrawLine(pen, p1, p2);
            }
            var index = 1;
            for (int i = 0; i < bit.Width; i += w)
            {
                for (int j = 0; j < bit.Height; j += w)
                {
                    g.DrawString(index.ToString(), new Font("微软雅黑", 12), new SolidBrush(Color.White), i, j);
                    index++;
                }
            }

            bit.Save(Path.Combine("line.jpeg"), ImageFormat.Jpeg);
        }

        //获取笔盒颜色
        //让用户自己输入每行有几个格子，然后程序除格子数，得到单位长度，然后高度也是按照该长度

        /// <summary>
        /// 图像灰度化
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public static Bitmap ToGray(Bitmap bmp)
        {
            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    //获取该点的像素的RGB的颜色
                    Color color = bmp.GetPixel(i, j);
                    //利用公式计算灰度值
                    int gray = (int)(color.R * 0.3 + color.G * 0.59 + color.B * 0.11);
                    Color newColor = Color.FromArgb(gray, gray, gray);
                    bmp.SetPixel(i, j, newColor);
                }
            }

            return bmp;
        }

        public static unsafe Bitmap ConvertTo8BitBitmap(Bitmap img, string destFile)
        {
            var bit = new Bitmap(img.Width, img.Height, PixelFormat.Format8bppIndexed);
            BitmapData data = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly,
                                           PixelFormat.Format24bppRgb);
            var bp = (byte*)data.Scan0.ToPointer();
            BitmapData data2 = bit.LockBits(new Rectangle(0, 0, bit.Width, bit.Height), ImageLockMode.ReadWrite,
                                            PixelFormat.Format8bppIndexed);
            var bp2 = (byte*)data2.Scan0.ToPointer();
            for (int i = 0; i != data.Height; i++)
            {
                for (int j = 0; j != data.Width; j++)
                {
                    //0.3R+0.59G+0.11B
                    float value = 0.11F * bp[i * data.Stride + j * 3] + 0.59F * bp[i * data.Stride + j * 3 + 1] +
                                  0.3F * bp[i * data.Stride + j * 3 + 2];
                    bp2[i * data2.Stride + j] = (byte)value;
                }
            }
            img.UnlockBits(data);
            bit.UnlockBits(data2);

            ColorPalette palette = bit.Palette;
            for (int i = 0; i != palette.Entries.Length; i++)
            {
                palette.Entries[i] = Color.FromArgb(i, i, i);
            }

            bit.Palette = palette;

            img = new Bitmap(bit);
            bit.Dispose();
            return img;
        }

        public static unsafe Bitmap ConvertTo1Bpp3(Bitmap bmp)
        {
            var bit = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format8bppIndexed);
            var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var bp = (byte*)data.Scan0.ToPointer();

            int average = 0;
            for (int i = 0; i != data.Height; i++)
            {
                for (int j = 0; j != data.Width; j++)
                {
                    average += bp[i * data.Stride + j * 3];
                }
            }

            average = (int)average / (bmp.Width * bmp.Height);

            var data2 = bit.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            var bp2 = (byte*)data.Scan0.ToPointer();

            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    //获取该点的像素的RGB的颜色                
                    var b = bp[i * data.Stride + j * 3];
                    int value = 255 - b;
                    if (value > average)
                    {
                    }
                    var newColor = value > average ? 0 : 255;
                    bp2[i * data2.Stride + j] = (byte)(newColor * 3);
                    //SetPixel(i, j, new IntPtr(bp2), data2, Color.FromArgb(newColor, newColor, newColor));
                }
            }
            bmp.UnlockBits(data);
            bit.UnlockBits(data2);

            ColorPalette palette = bit.Palette;
            for (int i = 0; i != palette.Entries.Length; i++)
            {
                palette.Entries[i] = Color.FromArgb(i, i, i);
            }

            bit.Palette = palette;
            bmp = new Bitmap(bit);
            //bit.Dispose();

            bit.Save(@"G:\其他项目\YuZe.ColoredPencil\YuZe.ColoredPencil\new\6.jpeg", ImageFormat.Jpeg);
            return bmp;
        }

        public static void SetPixel(int x, int y, IntPtr Iptr, BitmapData bitmapData, Color c)
        {
            var Depth = 8;
            unsafe
            {
                byte* ptr = (byte*)Iptr;
                ptr = ptr + bitmapData.Stride * y;
                ptr += Depth * x / 8;
                if (Depth == 32)
                {
                    ptr[3] = c.A;
                    ptr[2] = c.R;
                    ptr[1] = c.G;
                    ptr[0] = c.B;
                }
                else if (Depth == 24)
                {
                    ptr[2] = c.R;
                    ptr[1] = c.G;
                    ptr[0] = c.B;
                }
                else if (Depth == 8)
                {
                    ptr[2] = c.R;
                    ptr[1] = c.G;
                    ptr[0] = c.B;
                }
            }
        }


        /// <summary>
        /// 图像灰度反转
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public static Bitmap GrayReverse(Bitmap bmp)
        {
            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    //获取该点的像素的RGB的颜色
                    Color color = bmp.GetPixel(i, j);
                    Color newColor = Color.FromArgb(255 - color.R, 255 - color.G, 255 - color.B);
                    bmp.SetPixel(i, j, newColor);
                }
            }
            return bmp;
        }

        /// <summary>
        /// 图像二值化1：取图片的平均灰度作为阈值，低于该值的全都为0，高于该值的全都为255
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public static Bitmap ConvertTo1Bpp1(Bitmap bmp)
        {
            int average = 0;
            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    Color color = bmp.GetPixel(i, j);
                    average += color.B;

                }
            }
            average = (int)average / (bmp.Width * bmp.Height);

            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    //获取该点的像素的RGB的颜色
                    Color color = bmp.GetPixel(i, j);
                    int value = 255 - color.B;
                    Color newColor = value > average ? Color.FromArgb(0, 0, 0) : Color.FromArgb(255, 255, 255);
                    bmp.SetPixel(i, j, newColor);
                }
            }
            return bmp;
        }

        /// <summary>
        /// 图像二值化2
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public static Bitmap ConvertTo1Bpp2(Bitmap img)
        {
            int w = img.Width;
            int h = img.Height;
            Bitmap bmp = new Bitmap(w, h, PixelFormat.Format1bppIndexed);
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, PixelFormat.Format1bppIndexed);
            for (int y = 0; y < h; y++)
            {
                byte[] scan = new byte[(w + 7) / 8];
                for (int x = 0; x < w; x++)
                {
                    Color c = img.GetPixel(x, y);
                    if (c.GetBrightness() >= 0.5) scan[x / 8] |= (byte)(0x80 >> (x % 8));
                }
                Marshal.Copy(scan, 0, (IntPtr)((int)data.Scan0 + data.Stride * y), scan.Length);
            }
            return bmp;
        }

    }
}
public static class ColorExtension
{
    public static HSV RGB2HSV(this Color color)
    {
        return RGB2HSV(color.R, color.G, color.B);
    }
    /// RGB转换HSV
    private static HSV RGB2HSV(int red, int green, int blue)
    {
        double r = ((double)red / 255.0);
        double g = ((double)green / 255.0);
        double b = ((double)blue / 255.0);

        double max = Math.Max(r, Math.Max(g, b));
        double min = Math.Min(r, Math.Min(g, b));

        var hue = 0.0;
        if (max == r && g >= b)
        {
            if (max - min == 0) hue = 0.0;
            else hue = 60 * (g - b) / (max - min);
        }
        else if (max == r && g < b)
        {
            hue = 60 * (g - b) / (max - min) + 360;
        }
        else if (max == g)
        {
            hue = 60 * (b - r) / (max - min) + 120;
        }
        else if (max == b)
        {
            hue = 60 * (r - g) / (max - min) + 240;
        }

        var sat = (max == 0) ? 0.0 : (1.0 - ((double)min / (double)max));
        var bri = max;
        return new HSV(hue, sat, bri);
    }

}


public struct HSV
{
    public HSV(double hue, double sat, double bri)
    {
        this.H = hue;
        this.S = sat;
        this.V = bri;
    }

    public double H { get; set; }

    public double S { get; set; }

    public double V { get; set; }
}

public struct RGB
{
    public short R { get; set; }

    public short G { get; set; }

    public short B { get; set; }
}

