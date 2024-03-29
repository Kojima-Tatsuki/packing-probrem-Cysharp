﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using packing_probrem.domain.RectangularPackingProbelm;
using packing_probrem.Search.Extentions;

namespace packing_probrem.Search.RectangularPackingProbelm
{
    // 局所探索
    class LocalSearch : ISearch
    {
        private readonly IAlgolism Algolism;

        public LocalSearch(IAlgolism algolism)
        {
            Algolism = algolism;
        }

        public SearchResult Search(IReadOnlyList<Box> init, TimeSpan? timeSpan)
        {
            var bestScore = Algolism.Cal(init);
            var bestOrder = init;
            var searchTime = timeSpan ?? TimeSpan.FromSeconds(init.ToList().Count);
            var startTime = DateTime.Now;

            var scores = new List<int> { bestScore };

            int loopCount = 0;

            while (DateTime.Now.Subtract(startTime) < searchTime)
            {
                var changed = IsIncludeMore(bestOrder);

                if (!changed.isInclude)
                    break;

                bestScore = changed.score;
                bestOrder = changed.order;
                scores.Add(changed.score);
                loopCount++;
            }
            return new SearchResult(bestScore, bestOrder, scores, loopCount);
        }

        // より良い解が存在するか
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

                    if (calResult < bestResult)
                    {
                        bestOrder = order;
                        bestResult = calResult;
                        result = true;
                    }
                }
            }

            return (result, bestResult, bestOrder);
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
        public static IReadOnlyList<T> ChangeOrder<T>(this IReadOnlyList<T> list, int oldIndex, int newIndex)
        {
            var newOrder = list.ToList();
            var tmp = newOrder[oldIndex];
            newOrder[oldIndex] = newOrder[newIndex];
            newOrder[newIndex] = tmp;
            return newOrder;

            /*var result = new List<T>(list);
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

            return result;*/
        }
    }
}
