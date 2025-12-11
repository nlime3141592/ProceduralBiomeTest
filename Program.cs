using System;

namespace nl
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            DiamondSquare ds = new DiamondSquare(
                8,
                0.1f,
                252.0f, 62.0f, 133.0f,
                9.0f, 107.0f, 239.0f
            );

            ds.Create();
            ds.Save();
        }
    }
}