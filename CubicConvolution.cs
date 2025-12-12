using System;

namespace nl
{
    public class CubicConvolution
    {
        public static float[,] Scaling(float[,] origin, int ow, int oh, int tw, int th)
        {
            float[,] target = new float[tw, th];

            for (int x = 0; x < tw; ++x)
            {
                for (int y = 0; y < th; ++y)
                {
                    float tu = (float)x / (float)(tw - 1);
                    float tv = (float)y / (float)(th - 1);

                    int cx = (int)MathF.Floor(tu * (float)ow);
                    int cy = (int)MathF.Floor(tv * (float)oh);

                    float ou = (float)cx / (float)(ow - 1);
                    float ov = (float)cy / (float)(oh - 1);

                    float du = tu - ou;
                    float dv = tv - ov;

                    float v0 = Interpolate(
                        GetColor(origin, ow, oh, cx - 1, cy - 1),
                        GetColor(origin, ow, oh, cx + 0, cy - 1),
                        GetColor(origin, ow, oh, cx + 1, cy - 1),
                        GetColor(origin, ow, oh, cx + 2, cy - 1),
                        du);

                    float v1 = Interpolate(
                        GetColor(origin, ow, oh, cx - 1, cy + 0),
                        GetColor(origin, ow, oh, cx + 0, cy + 0),
                        GetColor(origin, ow, oh, cx + 1, cy + 0),
                        GetColor(origin, ow, oh, cx + 2, cy + 0),
                        du);

                    float v2 = Interpolate(
                        GetColor(origin, ow, oh, cx - 1, cy + 1),
                        GetColor(origin, ow, oh, cx + 0, cy + 1),
                        GetColor(origin, ow, oh, cx + 1, cy + 1),
                        GetColor(origin, ow, oh, cx + 2, cy + 1),
                        du);

                    float v3 = Interpolate(
                        GetColor(origin, ow, oh, cx - 1, cy + 2),
                        GetColor(origin, ow, oh, cx + 0, cy + 2),
                        GetColor(origin, ow, oh, cx + 1, cy + 2),
                        GetColor(origin, ow, oh, cx + 2, cy + 2),
                        du);

                    target[x, y] = Interpolate(v0, v1, v2, v3, dv);
                }
            }

            return target;
        }

        private static float GetColor(float[,] origin, int ow, int oh, int x, int y)
        {
            if (x < 0)
                x = 0;
            if (x >= ow)
                x = ow - 1;
            if (y < 0)
                y = 0;
            if (y >= oh)
                y = oh - 1;
            
            return origin[x, y];
        }

        // d: 두 번째 픽셀과 실수 좌표 사이의 거리, 0~1 사이의 정규화된 값
        // a == -1인 경우.
        private static float Interpolate(float v0, float v1, float v2, float v3, float d)
        {
            float p0 = v1;
            float p1 = -v0 + v2;
            float p2 = 2.0f * (v0 - v1) + v2 - v3;
            float p3 = -v0 + v1 - v2 + v3;

            return p0 + d * (p1 + d * (p2 + d * p3));
        }
    }
}