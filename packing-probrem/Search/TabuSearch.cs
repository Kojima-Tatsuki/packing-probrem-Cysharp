using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Channels;
using packing_probrem.domain.RectangularPackingProbelm;
using packing_probrem.Search.Extentions;
using static packing_probrem.Search.TabuSearch;

namespace packing_probrem.Search
{
    class TabuSearch : ISearch
    {
        private readonly IAlgolism Algolism;

        private readonly Random Random;

        private int IterMax { get; init; }
        private int TimeIterMax { get; init; }

        public TabuSearch(IAlgolism algolism)
        {
            Algolism = algolism;
            Random = new Random();

            IterMax = 100;
            TimeIterMax = 100000;
        }

        public SearchResult Search(IReadOnlyList<Box> init, TimeSpan? timeSpan)
        {
            var tabuList = new TabuList(init.Count);
            var maxItr = timeSpan == null ? IterMax : TimeIterMax;

            var tabuBoxes = init.Select((box, index) => new TabuBox(index, box)).ToList();
            var boxDictionary = tabuBoxes
                .ToDictionary(pair => pair.Index, pair => pair);

            var bestScore = Algolism.Cal(tabuBoxes);
            var bestOrder = tabuBoxes;
            var scores = new List<int> { bestScore };
            var startTime = DateTime.Now;

            var changed = IsIncludeMores(new Order(tabuBoxes, new(0, 0)), tabuList);

            for (int i = 0; i < maxItr && 
                timeSpan == null? true: DateTime.Now.Subtract(startTime) < timeSpan; i++)
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
                changed = IsIncludeMores(changed.Orders[index], tabuList);
            }

            return new SearchResult(bestScore, bestOrder, scores);
        }

        private IncludeMoreResult IsIncludeMores(Order init, TabuList tabus)
        {
            var bestScore = -1;
            var bestOrders = new List<Order> { init };

            for (int i = 0; i < init.Value.Count - 2; i++)
            {
                for (int k = i + 1; k < init.Value.Count - 1; k++)
                {
                    // 並び変えた後の配列がタブーリストと一致しているなら探索しない
                    if (tabus.IsTabu(init.Value[i], init.Value[k]))
                        continue;

                    var order = init.Value.ChangeOrder(i, k);
                    var score = Algolism.Cal(order);
                    var orderModel = new Order(order, new Pair(init.Value[i].Index, init.Value[k].Index));

                    if (score < bestScore || bestScore == -1)
                    {
                        // Console.WriteLine("より良い解が見つかったよ！！");
                        bestOrders = new List<Order> { orderModel };
                        bestScore = score;
                    }
                    else if(score == bestScore)
                        bestOrders.Add(orderModel);
                }
            }

            var isChangeable = bestScore != -1;

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
