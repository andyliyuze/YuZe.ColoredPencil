using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuZe.BitmapHelper
{
    public class MorphologySample
    {
        public void Run()
        {
            Mat gray = new Mat(@"G:\其他项目\YuZe.ColoredPencil\YuZe.ColoredPencil\P10424-172645.jpg", ImreadModes.Grayscale);
            Mat binary = new Mat();
            Mat dilate1 = new Mat();
            Mat dilate2 = new Mat();
            byte[] kernelValues = { 0, 1, 0, 1, 1, 1, 0, 1, 0 }; // cross (+)
            Mat kernel = new Mat(3, 3, MatType.CV_8UC1, kernelValues);

            // Binarize
            Cv2.Threshold(gray, binary, 0, 255, ThresholdTypes.Otsu);

            // empty kernel
            Cv2.Dilate(binary, dilate1, null);
            // + kernel
            Cv2.Dilate(binary, dilate2, kernel);

            using (new Window("binary", binary, WindowFlags.Normal))
            using (new Window("dilate (kernel = null)", dilate1, WindowFlags.Normal))
            using (new Window("dilate (kernel = +)", dilate2, WindowFlags.Normal))
            
            Cv2.WaitKey(0);
            Cv2.DestroyAllWindows();
        }
    }
}
