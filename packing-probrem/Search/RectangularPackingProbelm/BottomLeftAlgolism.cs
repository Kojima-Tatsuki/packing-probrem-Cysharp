using System.Collections.Generic;
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
                        continue;
                    if (IsOverlapPushed(pushed, rect))
                        continue;

                    pushed.Add(rect);
                    break;
                }
            }

            var score = GetMaxHeight(pushed);
            return (score, pushed);
        }

        internal IReadOnlyList<Point> GetBLStablePoints(IReadOnlyList<Point> stables, IReadOnlyList<Rect> pushedRects)
        {
            if (pushedRects.Count == 0)
                return stables;

            var result = new List<Point>(stables);
            var newRect = pushedRects[pushedRects.Count - 1];

            AddNotFilled(result, new Point(newRect.Right, MotherSection.Bottom), pushedRects);
            AddNotFilled(result, new Point(MotherSection.Left, newRect.Top), pushedRects);

            for (int i = 0; i < pushedRects.Count - 1; i++)
            {
                var pushed = pushedRects[i];

                if (pushed.Top >= newRect.Top)
                    AddNotFilled(result, new Point(pushed.Right, newRect.Top), pushedRects);
                if (pushed.Right >= newRect.Right)
                    AddNotFilled(result, new Point(newRect.Right, pushed.Top), pushedRects);
                if (newRect.Top >= pushed.Top)
                    AddNotFilled(result, new Point(newRect.Right, pushed.Top), pushedRects);
                if (newRect.Right >= pushed.Right)
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

        private List<Point> AddNotFilled(List<Point> list, Point point, IReadOnlyList<Rect> pushed)
        {
            if (!IsFilled(point, pushed))
                return list.AddNotDuplication(point);
            return list;
        }
        private bool IsFilled(Point point, IReadOnlyList<Rect> pushed) => pushed.Any(rect => rect.IsOverlap(point));

        private int GetMaxHeight(IReadOnlyList<Rect> pushedRects)
        {
            return pushedRects.Max(rect => rect.Top);
        }
    }
}
