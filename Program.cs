using System;

namespace nl
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            DiamondSquare ds = new DiamondSquare(
                resolution: 8,
                gaussScale: 0.1f,
                h: 0.9f,
                252.0f, 62.0f, 133.0f,
                9.0f, 107.0f, 239.0f
            );

            for (int i = 0; i < 1; ++i)
                ds.Create();
            
            ds.Save();
        }
    }
}