using BigGustave;
using System;
using System.Diagnostics;
using System.IO;

namespace nl
{
    public class DiamondSquare
    {
        private int _resolution;
        private int _size;
        private float[,] _map;

        // color 0
        private float r0;
        private float g0;
        private float b0;

        // color 1
        private float r1;
        private float g1;
        private float b1;

        private Random _prng;
        private float _gaussScale;
        private float _h;

        private float _vMin;
        private float _vMax;

        public DiamondSquare(
            int resolution,
            float gaussScale,
            float h,
            float r0, float g0, float b0,
            float r1, float g1, float b1)
        {
            Debug.Assert(resolution > 0 && resolution < 12, "Invalid image size.");
            Debug.Assert(gaussScale >= 0.0f && gaussScale <= 1.0f);
            Debug.Assert(h > 0.0f && h < 1.0f);

            _resolution = resolution;
            _size = (1 << _resolution) + 1;
 
            _map = new float[_size, _size];

            this.r0 = r0;
            this.g0 = g0;
            this.b0 = b0;

            this.r1 = r1;
            this.g1 = g1;
            this.b1 = b1;

            _prng = new Random();
            _gaussScale = gaussScale;
            _h = h;

            _vMin = float.MaxValue;
            _vMax = float.MinValue;
        }

        public void Create()
        {
            int s = _size - 1;

            _map[0,0] = _prng.NextSingle();
            _map[0,s] = _prng.NextSingle();
            _map[s,0] = _prng.NextSingle();
            _map[s,s] = _prng.NextSingle();

            int scale = 1 << _resolution;
            float depth = 1.0f;
            float depthMultiplier = MathF.Pow(2, -_h);

            while (scale > 1)
            {
                int nextScale = scale / 2;

                CreateDiamond(nextScale, scale, depth);
                CreateSquare(0, nextScale, depth);

                scale = nextScale;
                depth *= depthMultiplier;
            }
        }

        private void CreateDiamond(int offset, int scale, float depth)
        {
            for (int x = offset; x < _size; x += scale)
            {
                for (int y = offset; y < _size; y += scale)
                {
                    _map[x, y] = GetDiamondValue(x, y, offset) + GaussRandom() * _gaussScale * depth;
                }
            }
        }

        private void CreateSquare(int offset, int scale, float depth)
        {
            for (int x = offset; x < _size; x += scale)
            {
                for (int y = 0; y < _size; y += scale)
                {
                    _map[x, y] = GetSquareValue(x, y, scale) + GaussRandom() * _gaussScale * depth;
                    _map[y, x] = GetSquareValue(y, x, scale) + GaussRandom() * _gaussScale * depth;
                }
            }
        }

        private float GetDiamondValue(int x, int y, int scale)
        {
            float count = 0.0f;
            float value = 0.0f;
            float v;

            if (TryGetValue(out v, x + scale, y + scale)) { count += 1.0f; value += v; }
            if (TryGetValue(out v, x + scale, y - scale)) { count += 1.0f; value += v; }
            if (TryGetValue(out v, x - scale, y + scale)) { count += 1.0f; value += v; }
            if (TryGetValue(out v, x - scale, y - scale)) { count += 1.0f; value += v; }

            return value / count;
        }

        private float GetSquareValue(int x, int y, int scale)
        {
            float count = 0.0f;
            float value = 0.0f;
            float v;

            if (TryGetValue(out v, x + scale, y)) { count += 1.0f; value += v; }
            if (TryGetValue(out v, x - scale, y)) { count += 1.0f; value += v; }
            if (TryGetValue(out v, x, y + scale)) { count += 1.0f; value += v; }
            if (TryGetValue(out v, x, y - scale)) { count += 1.0f; value += v; }

            return value / count;
        }

        private bool TryGetValue(out float v, int x, int y)
        {
            if (x < 0 || x >= _size || y < 0 || y >= _size)
            {
                v = 0.0f;
                return false;
            }

            v = _map[x, y];
            return true;
        }

        private void CompareMin(float v)
        {
            if (v < _vMin)
            {
                _vMin = v;
            }
        }

        private void CompareMax(float v)
        {
            if (v > _vMax)
            {
                _vMax = v;
            }
        }

        private float GaussRandom()
        {
            float u1 = 1.0f - _prng.NextSingle();
            float u2 = 1.0f - _prng.NextSingle();

            float randStdNormal = MathF.Sqrt(-2.0f * MathF.Log(u1)) * MathF.Sin(2.0f * MathF.PI * u2);

            return randStdNormal;
        }

        public string Save()
        {
            DateTime time = DateTime.Now;
            int YYYY = time.Year;
            int MM = time.Month;
            int DD = time.Day;
            int hh = time.Hour;
            int mm = time.Minute;
            int ss = time.Second;

            string dateString = string.Format("{0}-{1:D02}-{2:D02}-{3:D02}-{4:D02}-{5:D02}", YYYY, MM, DD, hh, mm, ss);
            string path = $@"C:\Test\{dateString}.png";

            PngBuilder builder = PngBuilder.Create(_size, _size, false);

            _vMin = float.MaxValue;
            _vMax = float.MinValue;

            for (int x = 0; x < _size; ++x)
            {
                for (int y = 0; y < _size; ++y)
                {
                    CompareMin(_map[x, y]);
                    CompareMax(_map[x, y]);
                }
            }

            for (int x = 0; x < _size; ++x)
            {
                for (int y = 0; y < _size; ++y)
                {
                    builder.SetPixel(GetPixel(x, y), x, y);
                }
            }

            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                builder.Save(fs);
            }

            return path;
        }

        private Pixel GetPixel(int x, int y)
        {
            float v = 0.0f;

            v = GetNorm(_map[x, y]);
            v = Clamp(v, 0.0f, 1.0f) * 255.0f;

            float r = v;
            float g = v;
            float b = v;

            // float r = r0 + (r1 - r0) * MathF.Max(0.0f, MathF.Min(1.0f, _map[x, y].value));
            // float g = g0 + (g1 - g0) * MathF.Max(0.0f, MathF.Min(1.0f, _map[x, y].value));
            // float b = b0 + (b1 - b0) * MathF.Max(0.0f, MathF.Min(1.0f, _map[x, y].value));

            return new Pixel((byte)r, (byte)g, (byte)b);
        }

        private float Clamp(float v, float min, float max)
        {
            return MathF.Max(min, MathF.Min(max, v));
        }

        private float GetNorm(float v)
        {
            return (v - _vMin) / (_vMax - _vMin);
        }

        private bool IsSingleBit(int value)
        {
            int state = 0;

            for (int i = 0; i < sizeof(int) * 8; ++i)
            {
                state += (value & 0x1);
            }

            return state == 1;
        }
    }
}