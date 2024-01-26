using System;
using System.Collections.Generic;
using System.Text;
using packing_probrem.domain.RectangularPackingProbelm;

namespace packing_probrem.Search.RectangularPackingProbelm
{
    interface IAlgolism
    {
        int Cal(IReadOnlyList<Box> rects);

        IReadOnlyList<Rect> GetPushed(IReadOnlyList<Box> rects);
    }

    class SearchResult
    {
        public int Score { get; }
        public IReadOnlyList<Box> Order { get; }
        public IReadOnlyList<int> Scores { get; }
        public int Iterations { get; }

        public SearchResult(int score, IReadOnlyList<Box> order, IReadOnlyList<int> scores, int iterations)
        {
            Score = score;
            Order = order;
            Scores = scores;
            Iterations = iterations;
         }
    }

    interface ISearch
    {
        SearchResult Search(IReadOnlyList<Box> boxes, TimeSpan? timeSpan = null);

        string ToString();
    }
}
