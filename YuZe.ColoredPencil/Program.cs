using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using YuZe.ColoredPencil.Helper;

namespace YuZe.ColoredPencil
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BitmapHelper.Splt(new System.Drawing.Bitmap(@"G:\BaiduNetdiskDownload\P10424-172645.jpg"));
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