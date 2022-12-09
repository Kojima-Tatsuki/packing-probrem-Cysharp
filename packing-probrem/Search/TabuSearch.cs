using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Channels;
using packing_probrem.domain;
using packing_probrem.Search.Extentions;
using static packing_probrem.Search.TabuSearch;

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

            var tabuList = new TabuList(init.Count);

            var tabuBoxes = init.Select((box, index) => new TabuBox(index, box)).ToList();
            var boxDictionary = tabuBoxes
                .ToDictionary(pair => pair.Index, pair => pair);

            var bestScore = Algolism.Cal(tabuBoxes);
            var bestOrder = tabuBoxes;
            var scores = new List<int> { bestScore };

            var changed = IsIncludeMores(tabuBoxes, tabuList);

            for (int i = 0; i < 100; i++)
            {
                // スコアの更新
                if (changed.Score < bestScore)
                {
                    // currentScoreLoops = 0;
                    bestScore = changed.Score;
                    scores.Add(changed.Score);
                }

                var index = Random.Next(0, changed.Orders.Count);

                // Console.WriteLine($"[{i}]: score {bestScore.score}, {changed.orders.Count}, current: {currentScoreLoops}");

                tabuList.AddTabuList(changed.Orders[index].index); // タブーリストに追加
                changed = IsIncludeMores(changed.Orders[index].Value, tabuList);
            }

            Console.WriteLine("End Tabu Search");
            return new SearchResult(bestScore, bestOrder, scores);
        }

        private IncludeMoreResult IsIncludeMores(IReadOnlyList<TabuBox> rects, TabuList tabus)
        {
            var bestScore = Algolism.Cal(rects);
            var bestOrders = new List<Order>();

            for (int i = 0; i < rects.Count - 2; i++)
            {
                for (int k = i + 1; k < rects.Count - 1; k++)
                {
                    // 並び変えた後の配列がタブーリストと一致しているなら探索しない
                    if (tabus.IsTabu(rects[i], rects[k]))
                        continue;

                    var order = rects.ChangeOrder(i, k);
                    var score = Algolism.Cal(order);
                    var orderModel = new Order(order, new Pair(rects[i].Index, rects[k].Index));

                    if (score < bestScore)
                    {
                        // Console.WriteLine("より良い解が見つかったよ！！");
                        bestOrders = new List<Order> { orderModel };
                        bestScore = score;
                    }
                    else if(score == bestScore)
                        bestOrders.Add(orderModel);
                }
            }

            var isChangeable = bestOrders.Count > 0;

            return new(isChangeable, bestScore, bestOrders);
        }

        private class IncludeMoreResult
        {
            public bool IsInclude { get; init; }
            public int Score { get; init; }
            public IReadOnlyList<Order> Orders { get; init; }

            public IncludeMoreResult(bool isIncloude, int score, IReadOnlyList<Order> orders)
            {
                IsInclude = isIncloude;
                Score = score;
                Orders = orders;
            }
        }

        private class TabuList
        {
            public int Length { get; init; }
            private Queue<Pair> BoxPairs { get; init; }
            private Queue<int> IntQueue { get; init; }

            public TabuList(int boxCount)
            {
                Length = (int)Math.Sqrt(boxCount); // 平方根を取る
                BoxPairs = new Queue<Pair>(Length);
                IntQueue = new Queue<int>(Length * 2);
            }

            public void AddTabuList(Pair pair)
            {
                if (BoxPairs.Count == Length)
                {
                    BoxPairs.Dequeue();
                    IntQueue.Dequeue();
                    IntQueue.Dequeue();
                }
                BoxPairs.Enqueue(pair);
                IntQueue.Enqueue(pair.First);
                IntQueue.Enqueue(pair.Second);
            }

            /// <summary>first < second を満たす</summary>
            public IReadOnlyCollection<Pair> GetIndexPairs() => BoxPairs.ToList();

            public bool IsTabu(TabuBox i, TabuBox k) => IntQueue.Any(_ => _ == i.Index || _ == k.Index);
        }

        private record Pair
        {
            public int First { get; init; }
            public int Second { get; init; }

            public Pair(int a, int b)
            {
                if (a < b)
                {
                    First = a;
                    Second = b;
                }
                else
                {
                    First= b;
                    Second= a;
                }
            }
        }

        private record TabuBox : Box
        {
            public int Index { get;init; }

            public TabuBox(int index, Box original): base(original)
            {
                Index= index;
            }
        }

        private record Order(IReadOnlyList<TabuBox> Value, Pair index);

        public override string ToString()
        {
            return "Tabu Search";
        }
    }
}
