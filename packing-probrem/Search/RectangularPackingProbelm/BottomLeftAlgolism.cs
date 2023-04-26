using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using packing_probrem.domain.Extentions;
using packing_probrem.domain.RectangularPackingProbelm;

namespace packing_probrem.Search.RectangularPackingProbelm
{
    class BottomLeftAlgolism : IAlgolism
    {
        private Section MotherSection { get; }
        private bool UseHeight { get; }

        public BottomLeftAlgolism(Section mother, bool useHeight)
        {
            MotherSection = mother;
            UseHeight = useHeight;
        }

        public int Cal(IReadOnlyList<Box> boxes) => Calculate(boxes).score;

        public IReadOnlyList<Rect> GetPushed(IReadOnlyList<Box> boxes) => Calculate(boxes).pushed;

        private (int score, IReadOnlyList<Rect> pushed) Calculate(IReadOnlyList<Box> boxes)
        {
            var pushed = new List<Rect>();

            if (boxes == null || boxes.Count == 0)
                return (0, pushed);

            var stables = MotherSection.StablePoints;

            //Console.WriteLine("\nStart BL Cal");

            foreach (var box in boxes)
            {
                stables = GetBLStablePoints(stables, pushed);

                if (stables.Count < 1) // 1つも安定点が無い(もうこれ以上置けるところはない)
                    break;

                var sorted = stables.SortedPoints();

                foreach (var point in sorted)
                {
                    var rect = new Rect(point.X, point.X + box.Width, point.Y + box.Height, point.Y);

                    if (!MotherSection.IsAppendable(rect, UseHeight))
                    {
                        //Console.WriteLine($"Rect Cant Append {rect}");
                        continue;
                    }
                    if (IsOverlapPushed(pushed, rect))
                    {
                        //Console.WriteLine($"Rect is Overlap {rect}");
                        continue;
                    }

                    // Console.WriteLine($"Pushed Rect {rect}");
                    pushed.Add(rect);
                    break;
                }
            }

            var score = GetMaxHeight(pushed);
            //Console.WriteLine($"end cal: {score}");
            return (score, pushed);
        }

        internal IReadOnlyList<Point> GetBLStablePoints(IReadOnlyList<Point> stables, IReadOnlyList<Rect> pushedRects)
        {
            if (pushedRects.Count == 0)
                return stables;

            var result = new List<Point>(stables);

            /*for (int i = 0; i < pushedRects.Count; i++)
            {
                var pushed = pushedRects[i];
                result.AddNotDuplication(new Point(pushed.Right, MotherSection.Bottom));
                result.AddNotDuplication(new Point(MotherSection.Left, pushed.Top));

                for (int j = 0; j < pushedRects.Count; j++)
                {
                    var repushed = pushedRects[j];

                    if (repushed == pushed)
                        continue;

                    if (pushed.Top >= repushed.Top)
                        result.AddNotDuplication(new Point(pushed.Right, repushed.Top));
                    if (pushed.Right >= repushed.Right)
                        result.AddNotDuplication(new Point(repushed.Right, pushed.Top));
                }
            }*/

            var newRect = pushedRects[pushedRects.Count - 1];

            // result.AddNotDuplication(new Point(newRect.Right, MotherSection.Bottom));
            // result.AddNotDuplication(new Point(MotherSection.Left, newRect.Top));
            AddNotFilled(result, new Point(newRect.Right, MotherSection.Bottom), pushedRects);
            AddNotFilled(result, new Point(MotherSection.Left, newRect.Top), pushedRects);

            for (int i = 0; i < pushedRects.Count - 1; i++)
            {
                var pushed = pushedRects[i];

                if (pushed.Top >= newRect.Top)
                    //result.AddNotDuplication(new Point(pushed.Right, newRect.Top));
                    AddNotFilled(result, new Point(pushed.Right, newRect.Top), pushedRects);
                if (pushed.Right >= newRect.Right)
                    //result.AddNotDuplication(new Point(newRect.Right, pushed.Top));
                    AddNotFilled(result, new Point(newRect.Right, pushed.Top), pushedRects);

                if (newRect.Top >= pushed.Top)
                    //result.AddNotDuplication(new Point(newRect.Right, pushed.Top));
                    AddNotFilled(result, new Point(newRect.Right, pushed.Top), pushedRects);
                if (newRect.Right >= pushed.Right)
                    //result.AddNotDuplication(new Point(pushed.Right, newRect.Top));
                    AddNotFilled(result, new Point(pushed.Right, newRect.Top), pushedRects);
            }

            return result;
        }

        private bool IsOverlapPushed(IReadOnlyList<Rect> pusheds, Rect rect)
        {
            foreach (var pushed in pusheds)
            {
                if (pushed.IsOverlap(rect))
                    return true;
            }
            return false;
        }

        /*private bool IsFilled(Point point)
        {
            return PushedRects
                .Any(_ => 
                    _.Left < point.X &&
                    point.X < _.Right &&
                    _.Bottom < point.Y &&
                    point.Y < _.Top);
        }*/

        private List<Point> AddNotFilled(List<Point> list, Point point, IReadOnlyList<Rect> pushed)
        {
            if (!IsFilled(point, pushed))
                return list.AddNotDuplication(point);
            return list;
        }
        private bool IsFilled(Point point, IReadOnlyList<Rect> pushed) => pushed.Any(_ => _.IsOverlap(point));

        private int GetMaxHeight(IReadOnlyList<Rect> pushedRects)
        {
            return pushedRects.Max(_ => _.Top);
        }
    }
}
