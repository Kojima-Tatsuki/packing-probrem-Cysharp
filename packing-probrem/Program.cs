using System;
using packing_probrem.domain;

namespace packing_probrem
{
    class Program
    {
        static void Main(string[] args)
        {
            // Console.WriteLine("Hello World!");

            var readFilePath = "\\probrems\\pushRect.csv";

            var br = new BoxReader();

            var r = br.ReadBoxesFromFile(Environment.CurrentDirectory + readFilePath);
        }
    }
}
