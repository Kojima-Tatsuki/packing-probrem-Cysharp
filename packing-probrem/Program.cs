using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using packing_probrem.ConsoleDrawer;
using packing_probrem.domain;
using packing_probrem.domain.Extentions;
using packing_probrem.Search;

namespace packing_probrem
{
    class Program
    {
        static void Main(string[] args)
        {
            string readFilePath(int num) => $"\\probrems\\pushRect-{num}.csv";

            var br = new BoxReader();
            IReadOnlyList<Box> loadedBoxes = new List<Box>();

            var boxCount = 20;
            if (File.Exists(Environment.CurrentDirectory + readFilePath(boxCount)))
                loadedBoxes = br.ReadBoxesFromFile(Environment.CurrentDirectory + readFilePath(boxCount));
            else
                loadedBoxes = new BoxGenereter().Create(boxCount, (3, 10), (5, 10));

            br.WriteBoxesToFile(Environment.CurrentDirectory + readFilePath(boxCount), loadedBoxes);

            Console.WriteLine("Loaded Boxes");
            for (int i = 0; i < loadedBoxes.Count; i++)
                Console.WriteLine($"[{i}]: {loadedBoxes[i]}");

            var section = new Section(new Box(24, 16));

            Console.WriteLine($"Section {section}");

            var bl = new BottomLeftAlgolism(section, false);

            var searchs = new List<ISearch>()
            {
                new LocalSearch(bl),
                new TabuSearch(bl),
                new RandomPartialNeighborhoodSearch(bl, 0.95f),
                new RandomPartialNeighborhoodSearch(bl, 0.9f),
                new RandomPartialNeighborhoodSearch(bl, 0.85f),
                new RandomPartialNeighborhoodSearch(bl, 0.8f)
            };

            Console.WriteLine("Start Search");

            var results = searchs
                .Select(s => (result: s.Search(loadedBoxes), name: s.ToString()))
                .ToList();

            // 図形描画

            foreach (var result in results)
            {
                var cal = bl.Cal(result.result.Order);
                var drawer = new SquareDrawer(section.Width, result.result.Score);

                foreach (var rect in cal.pushed)
                    drawer.SetRect(rect);

                drawer.DrawAllSquare();
            }

            // コンソール出力

            Console.WriteLine("\nEnd Calculate");

            var lsrs = results
                .Select(res => new ResultConstoler(res.result, bl, section, res.name))
                .ToList();

            // ファイル書き込み

            var writer = new ResultWriter();
            writer.Write(
                filePath: Environment.CurrentDirectory + $"\\result\\pushed-{loadedBoxes.Count}.txt",
                constolers: lsrs);
        }
    }

    internal class ResultConstoler
    {
        public string SearchAlgolismName { get; }
        public SearchResult Result { get; }
        public IReadOnlyList<Rect> Pushed { get; }
        public IReadOnlyList<domain.Point> Points { get; }
        public Section Section { get; }

        public ResultConstoler(SearchResult result, BottomLeftAlgolism algolism, Section section, string searchAlgolismName)
        {
            Result = result;
            Section = section;
            Pushed = algolism.Cal(result.Order).pushed;
            Points = algolism.GetBLStablePoints(section.StablePoints, Pushed);
            SearchAlgolismName = searchAlgolismName;

            WriteOnConsole();
        }

        private void WriteOnConsole()
        {
            Console.WriteLine("\n");
            Console.WriteLine($"Score: {Result.Score}");
            for (int i = 0; i < Result.Scores.Count; i++)
                Console.WriteLine($"[{i}]: {Result.Scores[i]}");

            Console.WriteLine("Order");
            for (int i = 0; i < Pushed.Count; i++)
                Console.WriteLine($"[{i}]: {Pushed[i]}");

            Console.WriteLine("BL");
            for (int i = 0; i < Points.Count; i++)
                Console.WriteLine($"[{i}]: {Points[i]}");
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
        }

        public void Write(string filePath, ResultConstoler constoler)
        {
            var command = new WriteCommand(
                filePath,
                constoler.Result.Score,
                constoler.Section,
                constoler.Result.Order,
                constoler.Result.Scores,
                constoler.Pushed,
                constoler.Points);
            Write(command);
        }

        public void Write(string filePath, IReadOnlyList<ResultConstoler> constolers)
        {
            using var sw = new StreamWriter(filePath, append: true, encoding: Encoding.UTF8);

            var def = constolers.FirstOrDefault();

            sw.WriteLine($"Section: {def.Section}");
            sw.WriteLine($"Put Boxes, count: {def.Result.Order.Count}");
            for (int i = 0; i < def.Result.Order.Count; i++)
                sw.WriteLine($"[{i}]: {def.Result.Order[i]}");
            Console.WriteLine("\n");

            foreach (var cons in constolers)
            {
                sw.WriteLine(cons.SearchAlgolismName);
                sw.WriteLine($"Best Score: {cons.Result.Score}");

                for (int i = 0; i < cons.Result.Scores.Count; i++)
                    sw.WriteLine($"[{i}]: {cons.Result.Scores[i]}");

                sw.WriteLine($"PushedRects, count: {cons.Pushed.Count}");
                for (int i = 0; i < cons.Pushed.Count; i++)
                    sw.WriteLine($"[{i}]: {cons.Pushed[i]}");
                sw.WriteLine("\n");
            }
        }

        internal class WriteCommand
        {
            public string filePath { get; }
            public int Score { get; }
            public Section MotherSection { get; }
            public IReadOnlyList<Box> PutBox { get; }
            public IReadOnlyList<int> Scores { get; }
            public IReadOnlyList<Rect> PushedRects { get; }
            public IReadOnlyList<domain.Point> BLStable { get; }

            internal WriteCommand(string path, int score, Section section, IReadOnlyList<Box> boxes, IReadOnlyList<int> scores, IReadOnlyList<Rect> rects, IReadOnlyList<domain.Point> bls) 
            {
                filePath = path;
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
