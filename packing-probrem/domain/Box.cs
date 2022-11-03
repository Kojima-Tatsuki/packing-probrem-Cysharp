using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace packing_probrem.domain
{
    /// <summary>座標を持たない箱</summary>
    class Box : IEquatable<Box>
    {
        public int Width { get; }
        public int Height { get; }

        public Box(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public override string ToString() => $"({Width}, {Height})";

        public bool Equals(Box other)
        {
            if (Width == other.Width && Height == other.Height)
                return true;
            return false;
        }
    }

    class BoxGenereter
    {
        public IReadOnlyList<Box> Create(int count, (int min, int max) width, (int min, int max) height)
        {
            var result = new List<Box>();

            var rand = new Random();

            for (int i = 0; i < count; i++)
            {
                var box = new Box(rand.Next(width.min, width.max), rand.Next(height.min, height.max));
                result.Add(box);
            }

            return result;
        }
    }

    class BoxReader
    {
        public IReadOnlyList<Box> ReadBoxesFromFile(string filePath)
        {
            var result = new List<Box>();

            using var sr = new StreamReader(filePath);

            var boxCount = int.Parse(sr.ReadLine());
            for (int i = 0; i < boxCount; i++)
            {
                var line = sr.ReadLine().Split(',');
                var box = new Box(int.Parse(line[0]), int.Parse(line[1]));

                result.Add(box);
            }

            return result;
        }

        public void WriteBoxesToFile(string filePath, IReadOnlyList<Box> boxes)
        {
            using var sw = new StreamWriter(filePath);

            sw.WriteLine(boxes.Count);
            foreach (var box in boxes)
            {
                sw.WriteLine($"{box.Width},{box.Height}");
            }
        }
    }
}
