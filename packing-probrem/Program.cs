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
            // Console.WriteLine("Hello World!");

            var readFilePath = "\\probrems\\pushRect.csv";

            var br = new BoxReader();

            var loadedBoxes = br.ReadBoxesFromFile(Environment.CurrentDirectory + readFilePath);
            loadedBoxes = new BoxGenereter().Create(80, (3, 13), (5, 13));

            Console.WriteLine("Loaded Boxes");
            for (int i = 0; i < loadedBoxes.Count; i++)
            {
                Console.WriteLine($"[{i}]: {loadedBoxes[i]}");
            }

            var section = new Section(new Box(24, 16));

            Console.WriteLine($"Section {section}");

            var bl = new BottomLeftAlgolism(section, false);

            var ls = new LocalSearch(bl);

            Console.WriteLine("Start Search");

            var result = ls.Search(loadedBoxes);

            var drawer = new SquareDrawer(section.Width, result.score);

            var cal = bl.Cal(result.order);

            foreach (var rect in cal.pushed)
            {
                drawer.SetRect(rect);
            }

            drawer.DrawAllSquare();

            Console.WriteLine("\nEnd Calculate");

            Console.WriteLine($"Score: {result.score}");
            for (int i = 0; i < result.scores.Count; i++)
            {
                Console.WriteLine($"[{i}]: {result.scores[i]}");
            }

            Console.WriteLine("Order");
            for (int i = 0; i < cal.pushed.Count; i++)
            {
                Console.WriteLine($"[{i}]: {cal.pushed[i]}");
            }

            Console.WriteLine("BL");
            var st = bl.GetBLStablePoints(section.StablePoints, cal.pushed).SortedPoints();
            for (int i = 0; i < st.Count; i++)
            {
                Console.WriteLine($"[{i}]: {st[i]}");
            }

            var writer = new ResultWriter();
            writer.Write(new ResultWriter.WriteCommand(
                path: Environment.CurrentDirectory + $"\\result\\pushed-{loadedBoxes.Count}.txt",
                score: result.score,
                section: section,
                boxes: loadedBoxes,
                scores: result.scores,
                rects: cal.pushed,
                bls: st
                ));
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
