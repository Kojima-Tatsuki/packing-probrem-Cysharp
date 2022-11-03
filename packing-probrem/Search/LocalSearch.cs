using System;
using System.Collections.Generic;
using System.Text;
using packing_probrem.domain;
using packing_probrem.Search.Extentions;

namespace packing_probrem.Search
{
    class LocalSearch : ISearch
    {
        private readonly IAlgolism Algolism;

        public LocalSearch(IAlgolism algolism)
        {
            Algolism = algolism;
        }

        public SearchResult Search(IReadOnlyList<Box> init)
        {
            Console.WriteLine("Start Local Search");

            var bestScore = Algolism.Cal(init).score;
            var bestOrder = init;

            var changed = IsIncludeMore(init);

            var scores = new List<int>();
            scores.Add(bestScore);

            var i = 1;

            while (changed.isInclude)
            {
                bestScore = changed.score;
                bestOrder = changed.order;
                scores.Add(changed.score);

                //Console.WriteLine($"[{i}]: score {bestScore}");

                changed = IsIncludeMore(bestOrder);
            }

            return new SearchResult(bestScore, bestOrder, scores);
        }

        private (bool isInclude, int score, IReadOnlyList<Box> order) IsIncludeMore(IReadOnlyList<Box> rects)
        {
            var bestResult = Algolism.Cal(rects);
            var bestOrder = rects;

            var result = false;

            for (int i = 0; i < rects.Count - 2; i++)
            {
                for (int k = i + 1; k < rects.Count - 1; k++)
                {
                    var order = rects.ChangeOrder(i, k);
                    var calResult = Algolism.Cal(order);

                    if (calResult.score < bestResult.score)
                    {
                        bestOrder = order;
                        bestResult = calResult;
                        result = true;
                    }
                }
            }

            return (result, bestResult.score, bestOrder);
        }

        public override string ToString()
        {
            return "Lorcal Search";
        }
    }
}

namespace packing_probrem.Search.Extentions
{
    /// <summary>Listの拡張メソッドクラス</summary>
    /// <remarks> https://takap-tech.com/entry/2017/09/15/083300 </remarks>
    public static class ListExtention
    {
        /// <summary>指定した位置の要素を指定したインデックス位置に変更します。</summary>
        public static List<T> ChangeOrder<T>(this IReadOnlyList<T> list, int oldIndex, int newIndex)
        {
            var result = new List<T>(list);
            if (newIndex > result.Count)
                throw new ArgumentOutOfRangeException(nameof(newIndex));

            if (oldIndex == newIndex)
                return result;

            var item = result[oldIndex];
            result.RemoveAt(oldIndex);

            if (newIndex > result.Count)
                result.Add(item);
            else
                result.Insert(newIndex, item);

            return result;
        }
    }
}
