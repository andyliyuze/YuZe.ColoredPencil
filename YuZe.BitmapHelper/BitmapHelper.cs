using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace YuZe.ColoredPencil.Helper
{
    public static class BitmapHelper
    {
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
            var systems = GetSystemColors();
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
                    if (minColor.Name == "DarkSlateGray")
                    {
                        var min = FindColorMin(pixel, systems);
                        var max = FindColorMax(pixel, systems);
                        var mix = Color.FromArgb((min.R + max.R) / 2, (min.G + max.G) / 2, (min.B + max.B) / 2);
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
            var w = 100;
            for (int i = 0; i < img.Width; i += w)
            {
                for (int j = 0; j < img.Height; j += w)
                {
                    var height = w;
                    if (j + w > img.Height)
                    {
                        height = img.Height - j;
                    }
                    var width = w;
                    if (i + w > img.Width)
                    {
                        width = img.Width - i;
                    }
                    var copyBitmap = img.Clone(new Rectangle(i, j, width, height), PixelFormat.Undefined);
                    string dic = "spilt3/spilt" + i + "_" + j;
                    if (!Directory.Exists(dic))
                    {
                        Directory.CreateDirectory(dic);
                    }
                    if (dic == "spilt3/spilt400_2500")
                    {

                    }
                    var clost = GetRBG(copyBitmap);
                    if (clost.clost.Count != clost.imgRGB.Count)
                    {

                    }
                    var bit = new Bitmap(100, 100, PixelFormat.Format24bppRgb);
                    var g = Graphics.FromImage(bit);

                    foreach (var item in clost.clost)
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
}
