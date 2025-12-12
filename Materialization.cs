using BigGustave;
using System.Diagnostics;
using System.IO;

namespace nl
{
    // Synthesize noise image on original image. 
    public class Materialization
    {
        public static void Synthesize(
            string imgOut, string imgSource, string imgNoise,
            float min, float max,
            float rMin, float gMin, float bMin)
        {
            Debug.Assert(min >= 0.0f && min < max && max <= 1.0f);

            Png src = null;
            Png nzs = null;

            using (FileStream fs = new FileStream(imgSource, FileMode.Open))
            {
                src = PngOpener.Open(fs);
            }
            
            using (FileStream fs = new FileStream(imgNoise, FileMode.Open))
            {
                nzs = PngOpener.Open(fs);
            }

            Debug.Assert(src.Width == nzs.Width && src.Height == nzs.Height);

            PngBuilder builder = PngBuilder.Create(src.Width, src.Height, false);

            for (int x = 0; x < src.Width; ++x)
            {
                for (int y = 0; y < src.Height; ++y)
                {
                    float norm = min + (max - min) * ((float)nzs.GetPixel(x, y).R / 255.0f);

                    float r = Interpolate(rMin, (float)src.GetPixel(x, y).R, norm);
                    float g = Interpolate(gMin, (float)src.GetPixel(x, y).G, norm);
                    float b = Interpolate(bMin, (float)src.GetPixel(x, y).B, norm);
                    Pixel px = new Pixel((byte)r, (byte)g, (byte)b);

                    builder.SetPixel(px, x, y);
                }
            }

            using (FileStream fs = new FileStream(imgOut, FileMode.Create))
            {
                builder.Save(fs);
            }
        }

        private static float Interpolate(float v0, float v1, float d)
        {
            return v0 + (v1 - v0) * d;
        }
    }
}