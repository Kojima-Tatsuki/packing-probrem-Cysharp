using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using packing_probrem.ConsoleDrawer;
using packing_probrem.domain.RectangularPackingProbelm;
using packing_probrem.Search.RectangularPackingProbelm;

namespace packing_probrem
{
    internal class RectangularPackingProblem
    {
        public void Main()
        {
            // フォルダ作成
            if (!Directory.Exists(Environment.CurrentDirectory + "\\probrems"))
                Directory.CreateDirectory(Environment.CurrentDirectory + "\\probrems");
            if (!Directory.Exists(Environment.CurrentDirectory + "\\result"))
                Directory.CreateDirectory(Environment.CurrentDirectory + "\\result");

            Console.WriteLine("詰め込む箱の数を指定してください: ");
            var boxCount = int.Parse(Console.ReadLine());

            var section = new Section(new(33, 16));
            Console.WriteLine($"Section {section}");
            var date = DateTime.Now;

            var searchTime = TimeSpan.FromMinutes(10);
            var doer = new Doer(false, section, date, searchTime);

            const int LoopCount = 1;

            var searchResultDictList = new List<Dictionary<string, SearchResult>>(LoopCount);

            for (int i = 0; i < LoopCount; i++)
                searchResultDictList.Add(doer.Do(new BoxGenereter().Create(boxCount, (6, 18), (8, 16)), i));

            #region 平均値のファイル出力
            using var sw = new StreamWriter(
                append: true,
                path: Environment.CurrentDirectory + $"\\result\\pushed-{boxCount}-{date.ToString("yyyyMMdd-HHmmss")}.txt");

            sw.WriteLine($"Result\nSearchTime[sec]: {searchTime.TotalSeconds}");

            var searchResults = searchResultDictList
                .SelectMany(dic => dic)
                .GroupBy(pair => pair.Key)
                .ToDictionary(group => group.Key, group => group.Select(pair => pair.Value).ToList());

            foreach (var item in searchResults)
            {
                int sum = 0, best = int.MaxValue, warst = int.MinValue, sumIter = 0;

                foreach (var searchResult in item.Value)
                {
                    var score = searchResult.Score;
                    sum += score;
                    sumIter += searchResult.Iterations;
                    if (score < best)
                        best = score;
                    if (warst < score)
                        warst = score;
                }

                sw.WriteLine($"{item.Key}, Ave: {(float)sum / item.Value.Count}, Best: {best}, Wearst: {warst}, Iter: {(float)sumIter / item.Value.Count}");
            }
            #endregion
        }
    }

    internal class Doer
    {
        private bool DrawView { get; init; }

        private Section Section { get; init; }
        private TimeSpan? TimeSpan { get; init; }
        private DateTime CalcDate { get; init; }

        public Doer(bool drawView, Section section, DateTime calcDate, TimeSpan? timeSpan = null)
        {
            DrawView = drawView;
            Section = section;
            TimeSpan = timeSpan;
            CalcDate = calcDate;
        }

        public Dictionary<string, SearchResult> Do(IReadOnlyList<Box> loadedBoxes, int index = 0)
        {
            var bl = new BottomLeftAlgolism(Section, false);

            var searchs = new List<ISearch>()
            {
                new LocalSearch(bl),
                new TabuSearch(bl),
                new TabuSearch(bl, 3),
                new TabuSearch(bl, 5),
                new TabuSearch(bl, 7),
                new TabuSearch(bl, 11),
                new TabuSearch(bl, 13),
                new TabuSearch(bl, 17),
                new TabuSearch(bl, 23),
                new TabuSearch(bl, 29),
                new TabuSearch(bl, 37),
                new TabuSearch(bl, 47),
                /*new RandomPartialNeighborhoodSearch(bl, 1.0f, RandomPartialNeighborhoodSearch.RatioType.Fix),
                new RandomPartialNeighborhoodSearch(bl, 0.5f, RandomPartialNeighborhoodSearch.RatioType.Fix),
                new RandomPartialNeighborhoodSearch(bl, 0.3f, RandomPartialNeighborhoodSearch.RatioType.Fix),
                new RandomPartialNeighborhoodSearch(bl, 0.2f, RandomPartialNeighborhoodSearch.RatioType.Fix),
                new RandomPartialNeighborhoodSearch(bl, 0.1f, RandomPartialNeighborhoodSearch.RatioType.Fix),
                new RandomPartialNeighborhoodSearch(bl, 0.05f, RandomPartialNeighborhoodSearch.RatioType.Fix),
                new RandomPartialNeighborhoodSearch(bl, 0.03f, RandomPartialNeighborhoodSearch.RatioType.Fix),
                new RandomPartialNeighborhoodSearch(bl, 0.02f, RandomPartialNeighborhoodSearch.RatioType.Fix),
                new RandomPartialNeighborhoodSearch(bl, 0.01f, RandomPartialNeighborhoodSearch.RatioType.Fix),
                new RandomPartialNeighborhoodSearch(bl, 0.4f, RandomPartialNeighborhoodSearch.RatioType.LinerUpdate),
                new RandomPartialNeighborhoodSearch(bl, 0.35f, RandomPartialNeighborhoodSearch.RatioType.LinerUpdate),
                new RandomPartialNeighborhoodSearch(bl, 0.3f, RandomPartialNeighborhoodSearch.RatioType.LinerUpdate),
                new RandomPartialNeighborhoodSearch(bl, 0.25f, RandomPartialNeighborhoodSearch.RatioType.LinerUpdate),
                new RandomPartialNeighborhoodSearch(bl, 0.2f, RandomPartialNeighborhoodSearch.RatioType.LinerUpdate),
                new RandomPartialNeighborhoodSearch(bl, 0.15f, RandomPartialNeighborhoodSearch.RatioType.LinerUpdate),
                new RandomPartialNeighborhoodSearch(bl, 0.1f, RandomPartialNeighborhoodSearch.RatioType.LinerUpdate),
                new RandomPartialNeighborhoodSearch(bl, 0.07f, RandomPartialNeighborhoodSearch.RatioType.LinerUpdate),
                new RandomPartialNeighborhoodSearch(bl, 0.05f, RandomPartialNeighborhoodSearch.RatioType.LinerUpdate),
                new RandomPartialNeighborhoodSearch(bl, 0.03f, RandomPartialNeighborhoodSearch.RatioType.LinerUpdate),
                new RandomPartialNeighborhoodSearch(bl, 0.02f, RandomPartialNeighborhoodSearch.RatioType.LinerUpdate),
                new RandomPartialNeighborhoodSearch(bl, 0.015f, RandomPartialNeighborhoodSearch.RatioType.LinerUpdate),
                new RandomPartialNeighborhoodSearch(bl, 0.01f, RandomPartialNeighborhoodSearch.RatioType.LinerUpdate),
                new RandomPartialNeighborhoodSearch(bl, 0.005f, RandomPartialNeighborhoodSearch.RatioType.LinerUpdate),
                new RandomPartialNeighborhoodSearch(bl, 0.4f, RandomPartialNeighborhoodSearch.RatioType.ExponentialUpdate),
                new RandomPartialNeighborhoodSearch(bl, 0.35f, RandomPartialNeighborhoodSearch.RatioType.ExponentialUpdate),
                new RandomPartialNeighborhoodSearch(bl, 0.3f, RandomPartialNeighborhoodSearch.RatioType.ExponentialUpdate),
                new RandomPartialNeighborhoodSearch(bl, 0.25f, RandomPartialNeighborhoodSearch.RatioType.ExponentialUpdate),
                new RandomPartialNeighborhoodSearch(bl, 0.2f, RandomPartialNeighborhoodSearch.RatioType.ExponentialUpdate),
                new RandomPartialNeighborhoodSearch(bl, 0.15f, RandomPartialNeighborhoodSearch.RatioType.ExponentialUpdate),
                new RandomPartialNeighborhoodSearch(bl, 0.1f, RandomPartialNeighborhoodSearch.RatioType.ExponentialUpdate),
                new RandomPartialNeighborhoodSearch(bl, 0.07f, RandomPartialNeighborhoodSearch.RatioType.ExponentialUpdate),
                new RandomPartialNeighborhoodSearch(bl, 0.05f, RandomPartialNeighborhoodSearch.RatioType.ExponentialUpdate),
                new RandomPartialNeighborhoodSearch(bl, 0.03f, RandomPartialNeighborhoodSearch.RatioType.ExponentialUpdate),
                new RandomPartialNeighborhoodSearch(bl, 0.02f, RandomPartialNeighborhoodSearch.RatioType.ExponentialUpdate),
                new RandomPartialNeighborhoodSearch(bl, 0.015f, RandomPartialNeighborhoodSearch.RatioType.ExponentialUpdate),
                new RandomPartialNeighborhoodSearch(bl, 0.01f, RandomPartialNeighborhoodSearch.RatioType.ExponentialUpdate),
                new RandomPartialNeighborhoodSearch(bl, 0.005f, RandomPartialNeighborhoodSearch.RatioType.ExponentialUpdate),*/
            };

            Console.WriteLine($"\n[{index}] == Start Search ==");
            var startDate = DateTime.Now;

            var results = searchs
                .AsParallel()
                .WithDegreeOfParallelism(6)
                .Select(s =>
                {
                    // Console.WriteLine($"[{index}] Start {s.ToString()}");
                    var result = (result: s.Search(loadedBoxes, TimeSpan), name: s.ToString());
                    Console.WriteLine($"[{index}] End {result.name}, Score: {result.result.Score}, Iter: {result.result.Iterations}");
                    return result;
                })
                .ToList();

            Console.WriteLine($"[{index}] == End Calculate ==\n");

            // 図形描画
            if (DrawView)
            {
                foreach (var result in results)
                {
                    var pushed = bl.GetPushed(result.result.Order);
                    var drawer = new SquareDrawer(Section.Width, result.result.Score);

                    foreach (var rect in pushed)
                        drawer.SetRect(rect);

                    drawer.DrawAllSquare();
                }
            }

            var lsrs = results
                .Select(res => new ResultConstoler(res.result, bl, Section, res.name))
                .ToList();

            // ファイル書き込み
            var writer = new ResultWriter();
            writer.Write(
                filePath: Environment.CurrentDirectory + $"\\result\\pushed-{loadedBoxes.Count}-{CalcDate.ToString("yyyyMMdd-HHmmss")}.txt",
                startDate: startDate,
                constolers: lsrs);

            var bwriter = new BoxReader();
            bwriter.WriteBoxesToFile(
                filePath: Environment.CurrentDirectory + $"\\probrems\\pushedRect-{loadedBoxes.Count}.txt",
                loadedBoxes);

            return results.ToDictionary(pair => pair.name, pair => pair.result);
        }
    }
    
    internal class ResultConstoler
    {
        public string SearchAlgolismName { get; }
        public SearchResult Result { get; }
        public IReadOnlyList<Rect> Pushed { get; }
        public IReadOnlyList<Point> Points { get; }
        public Section Section { get; }

        public ResultConstoler(SearchResult result, BottomLeftAlgolism algolism, Section section, string searchAlgolismName)
        {
            Result = result;
            Section = section;
            Pushed = algolism.GetPushed(result.Order);
            Points = algolism.GetBLStablePoints(section.StablePoints, Pushed);
            SearchAlgolismName = searchAlgolismName;

            WriteOnConsole();
        }

        private void WriteOnConsole()
        {
            Console.WriteLine($"{SearchAlgolismName}, Score: {Result.Score}");
            for (int i = 0; i < Result.Scores.Count; i++)
                Console.WriteLine($"[{i}]: {Result.Scores[i]}");
            /*
            Console.WriteLine("Order");
            for (int i = 0; i < Pushed.Count; i++)
                Console.WriteLine($"[{i}]: {Pushed[i]}");

            Console.WriteLine("BL");
            for (int i = 0; i < Points.Count; i++)
                Console.WriteLine($"[{i}]: {Points[i]}");*/
        }
    }

    internal class ResultWriter
    {
        public void Write(WriteCommand command)
        {
            using var sw = new StreamWriter(
                path: command.filePath,
                append: true,
                encoding: Encoding.UTF8);
            sw.WriteLine($"Wtite date: {DateTime.Now}");
            sw.WriteLine($"Start date: {command.StartDate}");
            sw.WriteLine($"Section: {command.MotherSection}");
            sw.WriteLine($"Put Boxes, count: {command.PutBox.Count}");
            for (int i = 0; i < command.PutBox.Count; i++)
            {
                sw.WriteLine($"[{i}]: {command.PutBox[i]}");
            }
            sw.WriteLine($"Best Score: {command.Score}");
            for (int i = 0; i < command.Scores.Count; i++)
            {
                sw.WriteLine($"[{i}]: {command.Scores[i]}");
            }
            sw.WriteLine($"PushedRects, count: {command.PushedRects.Count}");
            for (int i = 0; i < command.PushedRects.Count; i++)
            {
                sw.WriteLine($"[{i}]: {command.PushedRects[i]}");
            }
            sw.WriteLine($"Iter Count: {command.IterCount}");
            Console.WriteLine("\n");
        }

        public void Write(string filePath, DateTime startDate, ResultConstoler constoler)
        {
            var command = new WriteCommand(
                filePath,
                startDate,
                constoler.Result.Score,
                constoler.Result.Iterations,
                constoler.Section,
                constoler.Result.Order,
                constoler.Result.Scores,
                constoler.Pushed,
                constoler.Points);
            Write(command);
        }

        public void Write(string filePath, DateTime startDate, IReadOnlyList<ResultConstoler> constolers)
        {
            using var sw = new StreamWriter(filePath, append: true, encoding: Encoding.UTF8);

            sw.WriteLine($"Wtite date: {DateTime.Now}");

            var def = constolers.FirstOrDefault();
            sw.WriteLine($"Start date: {startDate}");
            sw.WriteLine($"Section: {def.Section}");
            sw.WriteLine($"Put Boxes, count: {def.Result.Order.Count}");
            /*for (int i = 0; i < def.Result.Order.Count; i++)
                sw.WriteLine($"[{i}]: {def.Result.Order[i]}");*/

            foreach (var cons in constolers)
            {
                sw.WriteLine($"{cons.SearchAlgolismName}, Best Score: {cons.Result.Score}, Iter: {cons.Result.Iterations}");

                /*for (int i = 0; i < cons.Result.Scores.Count; i++)
                    sw.WriteLine($"[{i}]: {cons.Result.Scores[i]}");*/

                /*sw.WriteLine($"PushedRects, count: {cons.Pushed.Count}");
                for (int i = 0; i < cons.Pushed.Count; i++)
                    sw.WriteLine($"[{i}]: {cons.Pushed[i]}");*/
            }

            sw.WriteLine("\n");
        }

        internal class WriteCommand
        {
            public string filePath { get; }
            public DateTime StartDate { get; }
            public int Score { get; }
            public int IterCount { get; }
            public Section MotherSection { get; }
            public IReadOnlyList<Box> PutBox { get; }
            public IReadOnlyList<int> Scores { get; }
            public IReadOnlyList<Rect> PushedRects { get; }
            public IReadOnlyList<Point> BLStable { get; }

            internal WriteCommand(string path, DateTime startDate, int score, int iterCount, Section section, IReadOnlyList<Box> boxes, IReadOnlyList<int> scores, IReadOnlyList<Rect> rects, IReadOnlyList<domain.RectangularPackingProbelm.Point> bls)
            {
                filePath = path;
                StartDate = startDate;
                Score = score;
                IterCount = iterCount;
                MotherSection = section;
                PutBox = boxes;
                Scores = scores;
                PushedRects = rects;
                BLStable = bls;
            }
        }
    }
}
