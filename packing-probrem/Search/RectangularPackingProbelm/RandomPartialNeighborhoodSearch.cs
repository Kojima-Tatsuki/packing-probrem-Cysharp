using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using packing_probrem.domain.RectangularPackingProbelm;
using packing_probrem.Search.Extentions;

namespace packing_probrem.Search.RectangularPackingProbelm
{
    // ランダム部分近傍探索法
    internal class RandomPartialNeighborhoodSearch : ISearch
    {
        private IAlgolism Algolism { get; init; }
        private Random Random { get; init; }

        private float PartialRatio { get; init; }
        private float MinPartialRatio { get; init; }
        private RatioType RatioPatern { get; init; }

        /// <param name="partial">0 ~ 1</param>
        public RandomPartialNeighborhoodSearch(IAlgolism algolism, float partial, RatioType type)
        {
            Algolism = algolism;
            PartialRatio = partial;
            MinPartialRatio = 0.005f;
            RatioPatern = type;

            Random = new Random();
        }

        public SearchResult Search(IReadOnlyList<Box> init, TimeSpan? timeSpan)
        {
            var bestScore = Algolism.Cal(init);
            var bestOrder = init;

            var currentOrder = bestOrder;
            var currentScore = bestScore;

            var scores = new List<int>() { bestScore };

            var searchTime = timeSpan ?? TimeSpan.FromSeconds(init.ToList().Count);
            var startTime = DateTime.Now;
            var loopCount = 0;

            while (DateTime.Now.Subtract(startTime) < searchTime)
            {
                var timeRatio = DateTime.Now.Subtract(startTime).TotalSeconds / searchTime.TotalSeconds;

                var ratio = RatioPatern switch
                {
                    RatioType.Fix => PartialRatio,
                    RatioType.LinerUpdate => (float)timeRatio * (PartialRatio - MinPartialRatio) + MinPartialRatio,
                    RatioType.ExponentialUpdate => MinPartialRatio * MathF.Pow(PartialRatio / MinPartialRatio, (float)timeRatio),
                    _ => throw new Exception("Invalid RatioType")
                };

                var changed = GetNaightborhoodBest(currentOrder, ratio);

                currentOrder = changed.order;
                currentScore = changed.score;

                if (currentScore < bestScore)
                {
                    bestScore = currentScore;
                    bestOrder = currentOrder;
                    scores.Add(bestScore);
                }

                loopCount++;
            }

            return new SearchResult(bestScore, bestOrder, scores, loopCount);
        }

        /// <summary>部分近傍内で最も良い結果を返す</summary>
        private (int score, IReadOnlyList<Box> order) GetNaightborhoodBest(IReadOnlyList<Box> rects, float peartialRatio)
        {
            var bestResult = int.MaxValue;
            var bestOrders = new List<IReadOnlyList<Box>>();

            for (int i = 0; i < rects.Count; i++)
            {
                for (int k = i + 1; k < rects.Count; k++)
                {
                    var raito = (float)Random.NextDouble();

                    if (peartialRatio <= raito)
                        continue;

                    var order = rects.ChangeOrder(i, k);
                    var calResult = Algolism.Cal(order);

                    if (calResult < bestResult)
                    {
                        bestResult = calResult;
                        bestOrders = new List<IReadOnlyList<Box>> { order };
                    }
                    else if (bestResult == calResult)
                        bestOrders.Add(order);
                }
            }

            if (bestOrders.Count == 0)
                return (Algolism.Cal(rects), rects);

            var resultOrder = bestOrders[Random.Next(0, bestOrders.Count)];

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

            return $"RPNS {PartialRatio} {type}";
        }

        public enum RatioType
        {
            Fix, // 固定
            LinerUpdate, // 線形
            ExponentialUpdate, // 指数
        }
    }
}
