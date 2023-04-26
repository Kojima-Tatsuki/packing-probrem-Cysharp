using System;
using System.Collections.Generic;
using System.Text;

namespace packing_probrem.domain.RectangularPackingProbelm
{
    record Section : Box
    {
        public IReadOnlyList<Point> StablePoints { get; }

        public int Left { get; }
        public int Right { get; }
        public int Top { get; }
        public int Bottom { get; }

        public Section(Box section, IReadOnlyList<Point> points = null) : base(section.Width, section.Height)
        {
            if (points == null || points.Count == 0)
                StablePoints = new List<Point>() { new Point(Left, Bottom) };
            else
                StablePoints = new List<Point>(points);

            Left = 0;
            Right = section.Width;
            Top = section.Height;
            Bottom = 0;
        }

        public Rect ToRect() => new Rect(0, Width, Height, 0);

        /// <summary>母材内に入るスペースがあるかを返す</summary>
        /// <remarks>母材からはみ出していないかを返す</remarks>
        /// <param name="rect">調べる長方形</param>
        /// <param name="useSectionHeight">母材の高さを使用するか</param>
        /// <returns></returns>
        public bool IsAppendable(Rect rect, bool useSectionHeight)
        {
            if (Left > rect.Left)
                return false;
            if (Right < rect.Right)
                return false;
            if (Bottom > rect.Bottom)
                return false;
            if (Top < rect.Top && useSectionHeight)
                return false;
            return true;
        }

        public override string ToString()
        {
            return $"({Right}, {Top})";
        }
    }
}
