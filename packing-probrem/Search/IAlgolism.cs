using System;
using System.Collections.Generic;
using System.Text;
using packing_probrem.domain;

namespace packing_probrem.Search
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
     
        public SearchResult(int score, IReadOnlyList<Box> order, IReadOnlyList<int> scores)
        {
            Score = score;
            Order = order;
            Scores = scores;
        }
    }

    interface ISearch
    {
        SearchResult Search(IReadOnlyList<Box> boxes);

        string ToString();
    }

    interface IMemoryedAlgolism
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="boxes">今回調べる並び</param>
        /// <param name="original">類似の並び</param>
        /// <param name="index">変更の開始点</param>
        /// <returns></returns>
        SearchResult Cal(IReadOnlyList<Box> boxes, IReadOnlyList<Rect> original, int index);
    }
}
