using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using YuZe.BitmapHelper;

namespace YuZe.ColoredPencil
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new MorphologySample().Run();
            //new BinarizerSample().Run();
            var img = Helper.BitmapHelper.ConvertTo8BitBitmap(new System.Drawing.Bitmap(@"sky.jpg"), @"G:\其他项目\YuZe.ColoredPencil\YuZe.ColoredPencil\new\9.jpeg");
            var newImg = Helper.BitmapHelper.ConvertTo1Bpp3(img);
            img.Save(@"G:\其他项目\YuZe.ColoredPencil\YuZe.ColoredPencil\new\4.jpeg", System.Drawing.Imaging.ImageFormat.Jpeg);
            // BitmapHelper.PaintLine()
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
