using System;
using System.Collections.Generic;
using System.Text;

namespace packing_probrem.domain
{
    class Point
    {
        public int X { get; }
        public int Y { get; }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override string ToString() => $"{X}, {Y}";
    }
}
