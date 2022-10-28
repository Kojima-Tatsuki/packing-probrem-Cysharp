using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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

            var ls = new LocalSearch(bl);

            Console.WriteLine("Start Search");

            var result = ls.Search(loadedBoxes);

            // 図形描画

            var cal = bl.Cal(result.Order);
            var drawer = new SquareDrawer(section.Width, result.Score);

            foreach (var rect in cal.pushed)
                drawer.SetRect(rect);

            drawer.DrawAllSquare();

            // コンソール出力

            Console.WriteLine("\nEnd Calculate");

            var lsr = new ResultConstoler(result, bl, section);

            // ファイル書き込み

            var writer = new ResultWriter();
            writer.Write(
                filePath: Environment.CurrentDirectory + $"\\result\\pushed-{loadedBoxes.Count}.txt",
                constoler: lsr);
        }
    }

    internal class ResultConstoler
    {
        public SearchResult Result { get; }
        public IReadOnlyList<Rect> Pushed { get; }
        public IReadOnlyList<domain.Point> Points { get; }
        public Section Section { get; }

        public ResultConstoler(SearchResult result, BottomLeftAlgolism algolism, Section section)
        {
            Result = result;
            Section = section;
            Pushed = algolism.Cal(result.Order).pushed;
            Points = algolism.GetBLStablePoints(section.StablePoints, Pushed);

            WriteOnConsole();
        }

        private void WriteOnConsole()
        {
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
            sw.WriteLine($"PushedRects, coun: {command.PushedRects.Count}");
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
