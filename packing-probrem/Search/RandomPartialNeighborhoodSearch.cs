using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using packing_probrem.domain;
using packing_probrem.Search.Extentions;

namespace packing_probrem.Search
{
    // ランダム部分近傍探索法
    internal class RandomPartialNeighborhoodSearch : ISearch
    {
        private readonly IAlgolism Algolism;

        private readonly float PartialRatio;

        private readonly Random Random;

        /// <param name="partial">0 ~ 1</param>
        public RandomPartialNeighborhoodSearch(IAlgolism algolism, float partial)
        {
            Algolism = algolism;
            PartialRatio = partial;

            Random = new Random();
        }

        public SearchResult Search(IReadOnlyList<Box> init)
        {
            Console.WriteLine($"Start PRNS, {PartialRatio}");

            var bestScore = Algolism.Cal(init).score;
            var bestOrder = init;

            var scores = new List<int>() { bestScore };

            var changed = GetNaightborhoodBest(init);

            int i = 0;

            while (i < 200)
            {
                if (changed.score < bestScore)
                {
                    bestScore = changed.score;
                    bestOrder = changed.order;
                    scores.Add(bestScore);
                    i = 0;
                }

                //Console.WriteLine($"[{i}]: current score {bestScore}");
                changed = GetNaightborhoodBest(changed.order);
                i++;
            }

            return new SearchResult(bestScore, bestOrder, scores);
        }

        /// <summary>部分近傍内で最も良い結果を返す</summary>
        private (int score, IReadOnlyList<Box> order) GetNaightborhoodBest(IReadOnlyList<Box> rects)
        {
            var bestResult = -1;
            var bestOrders = new List<IReadOnlyList<Box>>();

            for (int i = 0; i < rects.Count - 2; i++)
            {
                for (int k = i + 1; k < rects.Count - 1; k++)
                {
                    var rait = (float)Random.NextDouble();

                    if (PartialRatio <= rait)
                        continue;

                    var order = rects.ChangeOrder(i, k);
                    var calResult = Algolism.Cal(order);

                    if (calResult.score < bestResult || bestResult == -1)
                    {
                        bestResult = calResult.score;
                        bestOrders = new List<IReadOnlyList<Box>> { order };
                    }
                    else if (bestResult == calResult.score)
                        bestOrders.Add(order);
                }
            }

            var resultOrder = bestOrders[Random.Next(0, bestOrders.Count)];

            return (bestResult, resultOrder);
        }

        public override string ToString()
        {
            return $"RandomPartialNeighborhoodSearch {PartialRatio}";
        }
    }
}
