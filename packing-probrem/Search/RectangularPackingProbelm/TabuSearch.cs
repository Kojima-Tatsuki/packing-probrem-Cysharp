using System;
using System.Collections.Generic;
using System.Linq;
using packing_probrem.domain.RectangularPackingProbelm;
using packing_probrem.Search.Extentions;

namespace packing_probrem.Search.RectangularPackingProbelm
{
    class TabuSearch : ISearch
    {
        private readonly IAlgolism Algolism;
        private readonly Random Random;

        private int? TabuListLength { get; init; }

        public TabuSearch(IAlgolism algolism, int? tabuListLength = null)
        {
            Algolism = algolism;
            Random = new Random();

            TabuListLength = tabuListLength;
        }

        public SearchResult Search(IReadOnlyList<Box> init, TimeSpan? timeSpan)
        {
            var tabuList = new TabuList(TabuListLength ?? (int)Math.Sqrt(init.Count));

            var tabuBoxes = init.Select((box, index) => new TabuBox(index, box)).ToList();
            var boxDictionary = tabuBoxes
                .ToDictionary(pair => pair.Index, pair => pair);

            var bestScore = Algolism.Cal(tabuBoxes);
            var bestOrder = tabuBoxes;

            var currentScore = bestScore;
            var currentOrder = bestOrder;

            var scores = new List<int> { bestScore };

            var searchTime = timeSpan ?? TimeSpan.FromSeconds(init.ToList().Count);
            var startTime = DateTime.Now;
            var loopCount = 0;

            while (DateTime.Now.Subtract(startTime) < searchTime)
            {
                var changed = IsIncludeMores(currentOrder, tabuList);

                // タブリストを満たす解がないなら終了
                if (!changed.IsInclude)
                    break;

                currentOrder = changed.Order.ToList();
                currentScore = changed.Score;
                tabuList = changed.TabuList;
                
                // スコアの更新
                if (changed.Score < bestScore)
                {
                    bestScore = changed.Score;
                    scores.Add(changed.Score);
                }

                loopCount++;
            }

            return new SearchResult(bestScore, bestOrder, scores, loopCount);
        }

        private IncludeMoreResult IsIncludeMores(IReadOnlyList<TabuBox> init, TabuList tabuList)
        {
            var bestScore = int.MaxValue;
            var bestOrders = new List<TabuOrder>();

            for (int i = 0; i < init.Count; i++)
            {
                for (int k = i + 1; k < init.Count; k++)
                {
                    // 並び変えた後の配列がタブーリストと一致しているなら探索しない
                    if (tabuList.IsTabu(init[i], init[k]))
                        continue;

                    var order = init.ChangeOrder(i, k);
                    var score = Algolism.Cal(order);
                    var orderModel = new TabuOrder(order, new Pair(init[i].Index, init[k].Index));

                    if (score < bestScore)
                    {
                        bestOrders = new List<TabuOrder> { orderModel };
                        bestScore = score;
                    }
                    else if (score == bestScore)
                        bestOrders.Add(orderModel);
                }
            }

            var isInclude = bestOrders.Count != 0;
            if (!isInclude)
                return new(isInclude, bestScore, init, tabuList);

            var resultTabuOrder = bestOrders[Random.Next(0, bestOrders.Count)];
            tabuList.AddTabuList(resultTabuOrder.Pair);

            return new(isInclude, bestScore, resultTabuOrder.Order, tabuList);
        }

        private class IncludeMoreResult
        {
            public bool IsInclude { get; init; }
            public int Score { get; init; }
            public IReadOnlyList<TabuBox> Order { get; init; }
            public TabuList TabuList { get; init; }

            public IncludeMoreResult(bool isIncloude, int score, IReadOnlyList<TabuBox> order, TabuList tabuList)
            {
                IsInclude = isIncloude;
                Score = score;
                Order = order;
                TabuList = tabuList;
            }
        }

        private class TabuList
        {
            public int Length { get; init; }
            private Queue<Pair> BoxPairs { get; init; }
            private Queue<int> IntQueue { get; init; }

            public TabuList(int length)
            {
                Length = length; // 平方根を取る
                BoxPairs = new Queue<Pair>(Length);
                IntQueue = new Queue<int>(Length * 2);
            }

            public void AddTabuList(Pair pair)
            {
                if (BoxPairs.Count >= Length)
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
                    First = b;
                    Second = a;
                }
            }
        }

        private record TabuBox : Box
        {
            public int Index { get; init; }

            public TabuBox(int index, Box original) : base(original)
            {
                Index = index;
            }
        }

        private record TabuOrder(IReadOnlyList<TabuBox> Order, Pair Pair);

        public override string ToString()
        {
            return $"Tabu Search-{TabuListLength}";
        }
    }
}
