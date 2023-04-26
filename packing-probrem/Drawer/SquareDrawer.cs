using packing_probrem.domain.RectangularPackingProbelm;
using System;
using Point = System.Drawing.Point;

/// <summary>
/// コンソール描画
/// </summary>
namespace packing_probrem.ConsoleDrawer
{
    /// <summary>
    /// 四角形描画処理
    /// </summary>
    class SquareDrawer
    {
        private ConsoleColor[,] canvas;
        private readonly int width;
        private readonly int height;

        private int colorIndex = 0;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="width">幅</param>
        /// <param name="height">高さ</param>
        public SquareDrawer(int width, int height)
        {
            Console.CursorVisible = false;
            Console.BackgroundColor = ConsoleColor.Black;
            this.width = width;
            this.height = height;
            canvas = new ConsoleColor[height, width];

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    canvas[i, j] = ConsoleColor.Black;
                }
            }
        }

        /// <summary>
        /// 四角形描画
        /// </summary>
        /// <param name="color">文字色</param>
        private void DrawSquare(ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write("■");
        }

        /// <summary>
        /// すべての四角形の描画
        /// </summary>
        public void DrawAllSquare()
        {
            Console.ForegroundColor = ConsoleColor.White;
            // Console.SetCursorPosition(0, 0);

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    DrawSquare(canvas[i, j]);
                }
                Console.Write("\n");
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.CursorVisible = false;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine("\n");
        }

        /// <summary>
        /// 文字色の指定
        /// </summary>
        /// <param name="color">文字色</param>
        /// <param name="position">位置</param>
        public void SetColor(ConsoleColor color, Point position)
        {
            canvas[position.Y, position.X] = color;
        }

        public void SetRect(Rect rect)
        {
            var color = RandomConsoleColor();

            for (int i = rect.Left; i < rect.Right; i++)
            {
                for (int j = rect.Bottom; j < rect.Top; j++)
                {
                    SetColor(color, new Point(i, j));
                }
            }        
        }

        private ConsoleColor RandomConsoleColor()
        {
            colorIndex++;
            if (colorIndex < 1 || 14 < colorIndex)
                colorIndex = 1;
            return (ConsoleColor)colorIndex;
        }


    }
}
