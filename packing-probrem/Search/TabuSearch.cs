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
            var tabuList = new List<IReadOnlyList<Box>>() { init };

            var bestScore = Algolism.Cal(init);
            var bestOrder = init;

            var changed = IsIncludeMores(init, tabuList);

            var scores = new List<int> { bestScore.score };

            Console.WriteLine($"init serach {bestScore.score}");
            Console.WriteLine($"init isChanged {changed.isInclude}");
            var i = 1;

            while (changed.isInclude)
            {
                bestScore.score = changed.score;
                bestOrder = changed.orders.First();
                scores.Add(changed.score);

                Console.WriteLine($"[{i}]: score {bestScore.score}");

                tabuList.Add(bestOrder);
                changed = IsIncludeMores(bestOrder, tabuList);
            }

            return new SearchResult(bestScore.score, bestOrder, scores);
        }

        private (bool isInclude, int score, IReadOnlyCollection<IReadOnlyList<Box>> orders) IsIncludeMores(
            IReadOnlyList<Box> rects, 
            IReadOnlyCollection<IReadOnlyList<Box>> tabus)
        {
            var bestResult = Algolism.Cal(rects);
            var bestOrders = new List<IReadOnlyList<Box>>();

            for (int i = 0; i < rects.Count - 1; i++)
            {
                for (int k = i + 1; k < rects.Count; k++)
                {
                    if (i == 0 && k == rects.Count - 1)
                        continue;

                    var order = rects.ChangeOrder(i, k);
                    // 並び変えた後の配列がタブーリストと一致しているなら探索しない
                    if (tabus.Any(tabu => tabu.Equals(order)))
                        continue;

                    var calResult = Algolism.Cal(order);

                    if (calResult.score < bestResult.score)
                    {
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
    }
}
