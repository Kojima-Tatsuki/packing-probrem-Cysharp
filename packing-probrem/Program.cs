using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using packing_probrem.ConsoleDrawer;
using packing_probrem.domain.Extentions;
using packing_probrem.domain.RectangularPackingProbelm;
using packing_probrem.Search;

namespace packing_probrem
{
    class Program
    {
        static void Main(string[] args)
        {
            // フォルダ作成
            if (!Directory.Exists(Environment.CurrentDirectory + "\\probrems"))
                Directory.CreateDirectory(Environment.CurrentDirectory + "\\probrems");
            if (!Directory.Exists(Environment.CurrentDirectory + "\\result"))
                Directory.CreateDirectory(Environment.CurrentDirectory + "\\result");

            var boxCount = int.Parse(Console.ReadLine());

            /*string readFilePath(int num) => $"\\probrems\\pushRect-{num}.csv";

            var br = new BoxReader();
            IReadOnlyList<Box> loadedBoxes = new List<Box>();

            if (File.Exists(Environment.CurrentDirectory + readFilePath(boxCount)))
                loadedBoxes = br.ReadBoxesFromFile(Environment.CurrentDirectory + readFilePath(boxCount));
            else
                loadedBoxes = new BoxGenereter().Create(boxCount, (5, 17), (7, 15));

            br.WriteBoxesToFile(Environment.CurrentDirectory + readFilePath(boxCount), loadedBoxes);

            Console.WriteLine("Loaded Boxes");
            for (int i = 0; i < loadedBoxes.Count; i++)
                Console.WriteLine($"[{i}]: {loadedBoxes[i]}");*/

            var section = new Section(new(33, 16));
            Console.WriteLine($"Section {section}");

            var doer = new Doer(false, section);

            const int LoopCount = 10;

            List<Dictionary<string, int>> lst = new List<Dictionary<string, int>>(LoopCount);

            for (int i = 0; i < LoopCount; i++)
                lst.Add(new Dictionary<string, int>());

            lst = lst.AsParallel()
                .Select((l, i) => (l, i))
                .Select(tuple => doer.Do(new BoxGenereter().Create(boxCount, (6, 18), (8, 16)), tuple.i))
                .ToList();

            // 平均値の出力

            using var sw = new StreamWriter(
                append: true,
                path: Environment.CurrentDirectory + $"\\result\\pushed-{boxCount}.txt");

            sw.WriteLine($"Result\n{DateTime.Now}\n");

            var re = new Dictionary<string, List<int>>();

            foreach (var l in lst)
            {
                foreach (var d in l)
                {
                    if (re.ContainsKey(d.Key))
                        re[d.Key].Add(d.Value);
                    else
                        re[d.Key] = new List<int> { d.Value };
                }
            }

            foreach (var item in re)
            {
                int sum = 0, best = -1, warst = -1;

                foreach (var current in item.Value)
                {
                    sum += current;
                    if (current < best || best == -1)
                        best = current;
                    if (warst < current || warst == -1)
                        warst = current;
                }

                sw.WriteLine($"{item.Key}\nAve: {(float)sum / item.Value.Count}, Best: {best}, Wearst: {warst}\n");
            }
        }
    }

    internal class Doer
    {
        private bool DrawView { get; init; }

        private Section Section { get; init; }

        private TimeSpan? Span { get; init; }

        public Doer(bool drawView, Section section, TimeSpan? timeSpan = null)
        {
            DrawView = drawView;
            Section = section;
            Span = timeSpan;
        }

        public Dictionary<string, int> Do(IReadOnlyList<Box> loadedBoxes, int index = -1)
        {
            var bl = new BottomLeftAlgolism(Section, false);

            var searchs = new List<ISearch>()
            {
                new LocalSearch(bl),
                new TabuSearch(bl),
                new RandomPartialNeighborhoodSearch(bl, 1.0f, RandomPartialNeighborhoodSearch.RatioType.Fix),
                //new RandomPartialNeighborhoodSearch(bl, 0.9f, RandomPartialNeighborhoodSearch.RatioType.Fix),
                //new RandomPartialNeighborhoodSearch(bl, 0.8f, RandomPartialNeighborhoodSearch.RatioType.Fix),
                //new RandomPartialNeighborhoodSearch(bl, 0.7f, RandomPartialNeighborhoodSearch.RatioType.Fix),
                //new RandomPartialNeighborhoodSearch(bl, 0.6f, RandomPartialNeighborhoodSearch.RatioType.Fix),
                //new RandomPartialNeighborhoodSearch(bl, 0.5f, RandomPartialNeighborhoodSearch.RatioType.Fix),
                //new RandomPartialNeighborhoodSearch(bl, 0.4f, RandomPartialNeighborhoodSearch.RatioType.Fix),
                new RandomPartialNeighborhoodSearch(bl, 0.3f, RandomPartialNeighborhoodSearch.RatioType.Fix),
                new RandomPartialNeighborhoodSearch(bl, 0.2f, RandomPartialNeighborhoodSearch.RatioType.Fix),
                //new RandomPartialNeighborhoodSearch(bl, 0.1f, RandomPartialNeighborhoodSearch.RatioType.Fix),
                //new RandomPartialNeighborhoodSearch(bl, 0.05f, RandomPartialNeighborhoodSearch.RatioType.Fix),
                //new RandomPartialNeighborhoodSearch(bl, 0.01f, RandomPartialNeighborhoodSearch.RatioType.Fix),
                new RandomPartialNeighborhoodSearch(bl, 0.3f, RandomPartialNeighborhoodSearch.RatioType.LinerUpdate),
                new RandomPartialNeighborhoodSearch(bl, 0.2f, RandomPartialNeighborhoodSearch.RatioType.LinerUpdate),
                new RandomPartialNeighborhoodSearch(bl, 0.1f, RandomPartialNeighborhoodSearch.RatioType.LinerUpdate),
                new RandomPartialNeighborhoodSearch(bl, 0.05f, RandomPartialNeighborhoodSearch.RatioType.LinerUpdate),
                new RandomPartialNeighborhoodSearch(bl, 0.01f, RandomPartialNeighborhoodSearch.RatioType.LinerUpdate),
                new RandomPartialNeighborhoodSearch(bl, 0.005f, RandomPartialNeighborhoodSearch.RatioType.LinerUpdate),
                new RandomPartialNeighborhoodSearch(bl, 0.3f, RandomPartialNeighborhoodSearch.RatioType.ExponentialUpdate),
                new RandomPartialNeighborhoodSearch(bl, 0.2f, RandomPartialNeighborhoodSearch.RatioType.ExponentialUpdate),
                new RandomPartialNeighborhoodSearch(bl, 0.1f, RandomPartialNeighborhoodSearch.RatioType.ExponentialUpdate),
                new RandomPartialNeighborhoodSearch(bl, 0.05f, RandomPartialNeighborhoodSearch.RatioType.ExponentialUpdate),
                new RandomPartialNeighborhoodSearch(bl, 0.01f, RandomPartialNeighborhoodSearch.RatioType.ExponentialUpdate),
                new RandomPartialNeighborhoodSearch(bl, 0.005f, RandomPartialNeighborhoodSearch.RatioType.ExponentialUpdate),
            };

            if (index != -1)
                Console.WriteLine($"\n[{index}] == Start Search ==");
            else Console.WriteLine($"\n == Start Search ==");

            List<(SearchResult result, string name)> results;

            var startDate = DateTime.Now;

            results = searchs
                .AsParallel()
                //.WithDegreeOfParallelism(2)
                .Select(s =>
                {
                    if (index != -1)
                        Console.WriteLine($"[{index}] Start {s.ToString()}");
                    else Console.WriteLine($"Start {s.ToString()}");

                    var result = (result: s.Search(loadedBoxes, TimeSpan.FromMinutes(90)), name: s.ToString());

                    if (index != -1)
                        Console.WriteLine($"[{index}] End {result.name}, score: {result.result.Score}");
                    else Console.WriteLine($"End {result.name}, score: {result.result.Score}");

                    return result;
                })
                .ToList();

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

            // コンソール出力

            if (index != -1)
                Console.WriteLine($"[{index}] == End Calculate ==\n");
            else Console.WriteLine($" == End Calculate ==\n");

            var lsrs = results
                .Select(res => new ResultConstoler(res.result, bl, Section, res.name))
                .ToList();

            // ファイル書き込み

            var writer = new ResultWriter();
            writer.Write(
                filePath: Environment.CurrentDirectory + $"\\result\\pushed-{loadedBoxes.Count}.txt",
                startDate: startDate,
                constolers: lsrs);

            var bwriter = new BoxReader();
            bwriter.WriteBoxesToFile(
                filePath: Environment.CurrentDirectory + $"\\probrems\\pushedRect-{loadedBoxes.Count}.txt",
                loadedBoxes);

            return results.ToDictionary(pair => pair.name, pair => pair.result.Score);
        }
    }

    internal class ResultConstoler
    {
        public string SearchAlgolismName { get; }
        public SearchResult Result { get; }
        public IReadOnlyList<Rect> Pushed { get; }
        public IReadOnlyList<domain.RectangularPackingProbelm.Point> Points { get; }
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
            Console.WriteLine("\n");
        }

        public void Write(string filePath, DateTime startDate, ResultConstoler constoler)
        {
            var command = new WriteCommand(
                filePath,
                startDate,
                constoler.Result.Score,
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
            for (int i = 0; i < def.Result.Order.Count; i++)
                sw.WriteLine($"[{i}]: {def.Result.Order[i]}");

            foreach (var cons in constolers)
            {
                sw.WriteLine();
                sw.WriteLine(cons.SearchAlgolismName);
                sw.WriteLine($"Best Score: {cons.Result.Score}");

                for (int i = 0; i < cons.Result.Scores.Count; i++)
                    sw.WriteLine($"[{i}]: {cons.Result.Scores[i]}");

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
            public Section MotherSection { get; }
            public IReadOnlyList<Box> PutBox { get; }
            public IReadOnlyList<int> Scores { get; }
            public IReadOnlyList<Rect> PushedRects { get; }
            public IReadOnlyList<domain.RectangularPackingProbelm.Point> BLStable { get; }

            internal WriteCommand(string path, DateTime startDate, int score, Section section, IReadOnlyList<Box> boxes, IReadOnlyList<int> scores, IReadOnlyList<Rect> rects, IReadOnlyList<domain.RectangularPackingProbelm.Point> bls) 
            {
                filePath = path;
                StartDate = startDate;
                Score = score;
                MotherSection = section;
                PutBox = boxes;
                Scores = scores;
                PushedRects = rects;
                BLStable = bls;
            }
        }
    }
}
