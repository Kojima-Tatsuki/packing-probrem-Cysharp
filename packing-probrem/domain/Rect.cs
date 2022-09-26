using System;
using System.Collections.Generic;
using System.Text;

namespace packing_probrem.domain
{
    class Rect
    {
        public int Left { get; }
        public int Right { get; }
        public int Top { get; }
        public int Bottom { get; }

        public int Width => Right - Left;
        public int Height => Top - Bottom;

        public int Area => Width * Height;

        public Rect(int left, int right, int top, int bottom)
        {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }

        public bool Equals(Rect obj)
        {
            if (Left == obj.Left && Right == obj.Right && Top == obj.Top && Bottom == obj.Bottom)
                return true;

            return false;
        }

        public bool IsOverlap(Point point)
        {
            return Left < point.X && point.X < Right && Top > point.Y && point.Y > Bottom;
        }

        public bool IsOverlap(Rect rect)
        {
            return Math.Max(Left, rect.Left) < Math.Min(Right, rect.Right) && Math.Max(Bottom, rect.Bottom) < Math.Min(Top, rect.Top);
        }

        public override string ToString()
        {
            return $"[{Left}, {Right}], [{Top}, {Bottom}]";
        }
    }
}
