using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace packing_probrem.domain
{
    class Point : IComparable<Point>, IEquatable<Point>
    {
        public int X { get; }
        public int Y { get; }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override string ToString() => $"{X}, {Y}";

        public int CompareTo(Point other)
        {
            if (Y < other.Y)
                return -1;
            else if (Y == other.Y)
            {
                if (X < other.X)
                    return -1;
                else if (X == other.X)
                    return 0;
                else
                    return 1;
            }
            else
                return 1;

        }

        public bool Equals(Point other) => X == other.X && Y == other.Y;
    }

    class PointCompaer : IComparer<Point>
    {
        public int Compare(Point x, Point y)
        {
            return x.CompareTo(y);
        }
    }
}

namespace packing_probrem.domain.Extentions
{
    static class PointExtention
    {
        public static IReadOnlyList<Point> SortedPoints(this IReadOnlyList<Point> points)
        {
            var result = new List<Point>(points);
            result.Sort(new PointCompaer());
            return result;
        }

        public static List<Point> AddNotDuplication(this List<Point> list, Point point)
        {
            if (list.Exists(_ => _.Equals(point)))
                return list;
            list.Add(point);
            return list;
        }
    }
}
