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
        private readonly float MinPartialRatio;
        private readonly RatioType RatioPatern;

        private readonly Random Random;

        private readonly int IterMax;

        /// <param name="partial">0 ~ 1</param>
        public RandomPartialNeighborhoodSearch(IAlgolism algolism, float partial, RatioType type)
        {
            Algolism = algolism;
            PartialRatio = partial;
            RatioPatern = type;

            IterMax = 100;

            Random = new Random();
        }

        public SearchResult Search(IReadOnlyList<Box> init)
        {
            Console.WriteLine($"Start PRNS, {PartialRatio}");

            var bestScore = Algolism.Cal(init);
            var bestOrder = init;

            var scores = new List<int>() { bestScore };

            var changed = GetNaightborhoodBest(init, 0);

            for (int i = 1; i < IterMax; i++)
            {
                if (changed.score < bestScore)
                {
                    bestScore = changed.score;
                    bestOrder = changed.order;
                    scores.Add(bestScore);
                }

                //Console.WriteLine($"[{i}]: current score {bestScore}");
                changed = GetNaightborhoodBest(changed.order, i);
            }

            Console.WriteLine($"End PRNS, {PartialRatio}, score:  {bestScore}");
            return new SearchResult(bestScore, bestOrder, scores);
        }

        /// <summary>部分近傍内で最も良い結果を返す</summary>
        private (int score, IReadOnlyList<Box> order) GetNaightborhoodBest(IReadOnlyList<Box> rects, int itr)
        {
            var bestResult = -1;
            var bestOrders = new List<IReadOnlyList<Box>> { rects };

            for (int i = 0; i < rects.Count - 2; i++)
            {
                for (int k = i + 1; k < rects.Count - 1; k++)
                {
                    float ratio = RatioPatern switch
                    {
                        RatioType.Fix => (float)Random.NextDouble(),
                        RatioType.LinerUpdate => i / IterMax * (PartialRatio - MinPartialRatio) + MinPartialRatio,
                        RatioType.ExponentialUpdate => MinPartialRatio * MathF.Pow(PartialRatio / MinPartialRatio, i / IterMax),
                        _ => (float)Random.NextDouble()
                    };

                    if (PartialRatio <= ratio)
                        continue;

                    var order = rects.ChangeOrder(i, k);
                    var calResult = Algolism.Cal(order);

                    if (calResult < bestResult || bestResult == -1)
                    {
                        bestResult = calResult;
                        bestOrders = new List<IReadOnlyList<Box>> { order };
                    }
                    else if (bestResult == calResult)
                        bestOrders.Add(order);
                }
            }

            var resultOrder = bestOrders[Random.Next(0, bestOrders.Count)];

            if (bestResult == -1)
                return (Algolism.Cal(rects), resultOrder);

            return (bestResult, resultOrder);
        }

        public override string ToString()
        {
            var type = RatioPatern switch
            {
                RatioType.Fix => "Fix",
                RatioType.LinerUpdate => "Liner",
                RatioType.ExponentialUpdate => "Exp",
                _ => "Fix"
            };

            return $"RandomPartialNeighborhoodSearch {PartialRatio} {type}";
        }

        public enum RatioType
        {
            Fix, // 固定
            LinerUpdate, // 線形
            ExponentialUpdate, // 指数
        }
    }
}
