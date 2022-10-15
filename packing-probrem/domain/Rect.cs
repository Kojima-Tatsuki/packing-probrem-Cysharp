using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace packing_probrem.domain
{
    class Rect : IComparable<Rect>
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

        public Box ToBox() => new Box(Width, Height);

        public override string ToString()
        {
            return $"[{Left}, {Bottom}], [{Right}, {Top}]";
        }

        public int CompareTo(Rect other)
        {
            if (Top > other.Top)
                return 1;
            else if (Top == other.Top)
                return 0;
            else return -1;
        }
    }

    class RectCompaer : IComparer<Rect>
    {
        public int Compare(Rect x, Rect y)
        {
            return x.CompareTo(y);
        }
    }
}

namespace packing_probrem.domain.Extentions
{
    static class RectExtention 
    {
        /// <summary>
        /// Rectを高さ順に並び変える
        /// </summary>
        /// <param name="rects"></param>
        /// <returns></returns>
        public static IReadOnlyList<Rect> SortedPushed(this IReadOnlyList<Rect> rects)
        {
            var result = new List<Rect>(rects);
            result.Sort(new RectCompaer());
            return result;
        }

        public static IReadOnlyList<Box> ToBox(this IReadOnlyList<Rect> rects) 
            => rects
                .Select(_ => _.ToBox())
                .ToList();
    }
}