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

        public TabuSearch(IAlgolism algolism)
        {
            Algolism = algolism;
        }

        public SearchResult Search(IReadOnlyList<Box> init)
        {
            Console.WriteLine("Start Tabu Search");

            var tabuList = new List<IReadOnlyList<Box>>() { init };

            var bestScore = Algolism.Cal(init);
            var bestOrder = init;

            var changed = IsIncludeMores(init, tabuList);

            var scores = new List<int> { bestScore.score };

            int i = 1, currentScoreLoops = 0;

            while (changed.isInclude)
            {
                if (bestScore.score > changed.score) {
                    currentScoreLoops = 0;
                    scores.Add(changed.score);
                }
                else if (currentScoreLoops >= 200)
                    break;

                bestScore.score = changed.score;
                bestOrder = changed.orders.First();

                // Console.WriteLine($"[{i}]: score {bestScore.score}, {changed.orders.Count}, current: {currentScoreLoops}");

                tabuList.Add(bestOrder);
                changed = IsIncludeMores(bestOrder, tabuList);
                i++;
                currentScoreLoops++;
            }

            return new SearchResult(bestScore.score, bestOrder, scores);
        }

        private (bool isInclude, int score, IReadOnlyCollection<IReadOnlyList<Box>> orders) IsIncludeMores(
            IReadOnlyList<Box> rects, 
            IReadOnlyCollection<IReadOnlyList<Box>> tabus)
        {
            var bestResult = Algolism.Cal(rects);
            var bestOrders = new List<IReadOnlyList<Box>>();

            for (int i = 0; i < rects.Count - 2; i++)
            {
                for (int k = i + 1; k < rects.Count - 1; k++)
                {
                    var order = rects.ChangeOrder(i, k);
                    // 並び変えた後の配列がタブーリストと一致しているなら探索しない
                    if (tabus.Any(tabu => tabu.SequenceEqual(order)))
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

        public override string ToString()
        {
            return "Tabu Search";
        }
    }
}
