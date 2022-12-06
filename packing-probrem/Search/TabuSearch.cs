using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using packing_probrem.domain;
using packing_probrem.Search.Extentions;

namespace packing_probrem.Search
{
    class TabuSearch : ISearch
    {
        private readonly IAlgolism Algolism;

        private readonly Random Random;

        public TabuSearch(IAlgolism algolism)
        {
            Algolism = algolism;
            Random = new Random();
        }

        public SearchResult Search(IReadOnlyList<Box> init)
        {
            Console.WriteLine("Start Tabu Search");

            var tabuList = new List<IReadOnlyList<Box>>() { init };
            var boxDictionary = init
                .Select((box, index) => (box, index))
                .ToDictionary(pair => pair.index, pair => pair);

            var bestScore = Algolism.Cal(init);
            var bestOrder = init;

            var changed = IsIncludeMores(init, tabuList);

            var scores = new List<int> { bestScore.score };

            int i = 1, currentScoreLoops = 0;

            while (changed.isInclude)
            {
                // スコアの更新
                if (bestScore.score > changed.score) {
                    currentScoreLoops = 0;
                    scores.Add(changed.score);
                }
                else if (currentScoreLoops >= 200) // スコアの更新から200回の実行で終了
                    break;

                bestScore.score = changed.score;
                bestOrder = changed.orders[Random.Next(0, changed.orders.Count)];

                // Console.WriteLine($"[{i}]: score {bestScore.score}, {changed.orders.Count}, current: {currentScoreLoops}");

                tabuList.Add(bestOrder); // タブーリストに追加
                changed = IsIncludeMores(bestOrder, tabuList);
                i++;
                currentScoreLoops++;
            }

            Console.WriteLine("End Tabu Search");
            return new SearchResult(bestScore.score, bestOrder, scores);
        }

        private (bool isInclude, int score, IReadOnlyList<IReadOnlyList<Box>> orders) IsIncludeMores(
            IReadOnlyList<Box> rects, 
            TabuList tabus)
        {
            var bestResult = Algolism.Cal(rects);
            var bestOrders = new List<IReadOnlyList<Box>>();

            var tabuPairs = tabus.GetIndexPairs();

            for (int i = 0; i < rects.Count - 2; i++)
            {
                for (int k = i + 1; k < rects.Count - 1; k++)
                {
                    var order = rects.ChangeOrder(i, k);
                    // 並び変えた後の配列がタブーリストと一致しているなら探索しない
                    if (tabuPairs.Any(pair => pair.Equals(new(i, k))))
                        continue;

                    var calResult = Algolism.Cal(order);

                    if (calResult.score < bestResult.score)
                    {
                        // Console.WriteLine("より良い解が見つかったよ！！");
                        bestOrders = new List<IReadOnlyList<Box>> { order };
                        bestResult = calResult;
                    }
                    else if(calResult.score == bestResult.score)
                        bestOrders.Add(order);
                }
            }

            var isChangeable = bestOrders.Count > 0;

            return (isChangeable, bestResult.score, bestOrders);
        }

        private class TabuList
        {
            public int Length { get; init; }
            private Queue<Pair> BoxPairs { get; init; }

            public TabuList(int boxCount)
            {
                Length = (int)Math.Sqrt(boxCount); // 平方根を取る
                BoxPairs = new Queue<Pair>(Length);
            }

            public void AddTabuList(int a, int b)
            {
                Pair pair;
                if (a < b)
                    pair = new(a, b);
                else
                    pair = new(b, a);

                BoxPairs.Enqueue(pair);
            }

            /// <summary>first < second を満たす</summary>
            public IReadOnlyCollection<Pair> GetIndexPairs() => BoxPairs.ToList();

            public record Pair(int First, int Second);
        }

        public override string ToString()
        {
            return "Tabu Search";
        }
    }
}
