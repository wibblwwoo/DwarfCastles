using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace DwarfCastles
{
    /// <summary>
    /// Authors: Alec Mills and Josh DeMoss
    ///
    /// handles drawing in console
    /// </summary>
    public class Gui
    {
        public Point CameraOffset { get; }
        public Point CameraSize { get; }

        private char[,] PastVisibleChars;
        private ConsoleColor[,] PastVisibleCharsColorsForeground;
        private ConsoleColor[,] PastVisibleCharsColorsBackground;
        private char[,] VisibleChars;
        private ConsoleColor[,] VisibleCharsColorsForeground;
        private ConsoleColor[,] VisibleCharsColorsBackground;
        private bool[,] VisibleCharOwnershipSet;

        private Queue<double> FrameTimes;

        public Gui()
        {
            CameraOffset = new Point();
            CameraSize = new Point(25, 25);

            VisibleChars = new char[Console.WindowWidth, Console.WindowHeight];
            VisibleCharsColorsForeground = new ConsoleColor[Console.WindowWidth, Console.WindowHeight];
            VisibleCharsColorsBackground = new ConsoleColor[Console.WindowWidth, Console.WindowHeight];
            VisibleCharOwnershipSet = new bool[Console.WindowWidth, Console.WindowHeight];

            FrameTimes = new Queue<double>();

            Console.CursorVisible = false;
        }

        private void SetUpNewDraw()
        {
            PastVisibleChars = VisibleChars;
            PastVisibleCharsColorsBackground = VisibleCharsColorsBackground;
            PastVisibleCharsColorsForeground = VisibleCharsColorsForeground;
            VisibleChars = new char[Console.WindowWidth, Console.WindowHeight];
            VisibleCharsColorsForeground = new ConsoleColor[Console.WindowWidth, Console.WindowHeight];
            VisibleCharsColorsBackground = new ConsoleColor[Console.WindowWidth, Console.WindowHeight];
            VisibleCharOwnershipSet = new bool[Console.WindowWidth, Console.WindowHeight];
            for (var i = 0; i < VisibleCharsColorsBackground.GetUpperBound(0); i++)
            {
                for (var j = 0; j < VisibleCharsColorsBackground.GetUpperBound(1); j++)
                {
                    VisibleChars[i, j] = ' ';
                    VisibleCharsColorsBackground[i, j] = ConsoleColor.Black;
                    VisibleCharsColorsForeground[i, j] = ConsoleColor.Black;
                }
            }
        }

        private void FinishDraw()
        {
            for (var i = 0; i < VisibleChars.GetLength(0); i++)
            {
                for (var j = 0; j < VisibleChars.GetLength(1); j++)
                {
                    // Continue if the character is the same as last draw
                    if (PastVisibleChars[i, j] == VisibleChars[i, j] &&
                        PastVisibleCharsColorsBackground[i, j] == VisibleCharsColorsBackground[i, j] &&
                        PastVisibleCharsColorsForeground[i, j] == VisibleCharsColorsForeground[i, j])
                    {
                        continue;
                    }

                    Console.SetCursorPosition(i, j);
                    Console.BackgroundColor = VisibleCharsColorsBackground[i, j];
                    Console.ForegroundColor = VisibleCharsColorsForeground[i, j];
                    Console.Write(VisibleChars[i, j]);
                }
            }

            FrameTimes.Enqueue((new TimeSpan(DateTime.Now.Ticks)).TotalMilliseconds);
        }

        private void PrepareDraw(string s, int x, int y, ConsoleColor background, ConsoleColor foreground,
            bool drawOnTop = false)
        {
            for (var i = 0; i < s.Length; i++)
            {
                PrepareDraw(s[i], x + i, y, background, foreground, drawOnTop);
            }
        }

        private void PrepareDraw(char c, int x, int y, ConsoleColor background, ConsoleColor foreground,
            bool drawOnTop = false)
        {
            if (VisibleCharOwnershipSet[x, y])
            {
                return;
            }

            VisibleChars[x, y] = c;
            VisibleCharsColorsBackground[x, y] = background;
            VisibleCharsColorsForeground[x, y] = foreground;
            VisibleCharOwnershipSet[x, y] = drawOnTop;
        }

        private void DrawFrame()
        {
            // Outer Frame
            for (var i = 0; i < Console.WindowWidth; i++)
            {
                PrepareDraw('_', i, 0, ConsoleColor.Black, ConsoleColor.White, true);
                PrepareDraw('\u00AF', i, Console.WindowHeight - 1, ConsoleColor.Black, ConsoleColor.White, true);
            }

            for (var i = 0; i < Console.WindowHeight; i++)
            {
                PrepareDraw('|', 0, i, ConsoleColor.Black, ConsoleColor.White, true);
                PrepareDraw('|', Console.WindowWidth - 1, i, ConsoleColor.Black, ConsoleColor.White, true);
            }

            // Right and Bottom of the Map Window
            for (var i = 0; i < CameraSize.Y; i++)
            {
                PrepareDraw('|', CameraSize.X * 2 + 1, i + 1, ConsoleColor.Black, ConsoleColor.White, true);
            }

            for (var i = 0; i < CameraSize.X * 2; i++)
            {
                PrepareDraw('\u00AF', i + 1, CameraSize.Y + 1, ConsoleColor.Black, ConsoleColor.White, true);
            }
        }

        private void DrawFPS()
        {

            var current = (new TimeSpan(DateTime.Now.Ticks)).TotalMilliseconds;
            while (FrameTimes.Count != 0 && FrameTimes.Peek() + 1000 < current)
            {
                Logger.Log($"Dequeueing {FrameTimes.Peek()} at {current}");
                FrameTimes.Dequeue();
            }

            PrepareDraw(FrameTimes.Count.ToString(), 0, 0, ConsoleColor.Black, ConsoleColor.Red, true);
        }

        private void DrawInput()
        {
            if (GameManager.Menu.State == 1)
            {
                VisibleCharsColorsBackground[GameManager.Input.CursorPosition.X * 2 + 1,
                    GameManager.Input.CursorPosition.Y + 1] = ConsoleColor.Magenta;
            }
            else if (GameManager.Menu.State == 2)
            {
                var r = MenuManager.FixedRectangle(GameManager.Menu.FirstPoint, GameManager.Input.CursorPosition);
                for (var i = r.X * 2 + 1; i <= r.Right * 2 + 1; i++)
                {
                    VisibleCharsColorsBackground[i, r.Y + 1] = ConsoleColor.Magenta;
                    VisibleCharsColorsBackground[i, r.Bottom + 1] = ConsoleColor.Magenta;
                }

                for (int i = r.Y; i <= r.Bottom; i++)
                {
                    VisibleCharsColorsBackground[r.X * 2 + 1, i + 1] = ConsoleColor.Magenta;
                    VisibleCharsColorsBackground[r.Right * 2 + 1, i + 1] = ConsoleColor.Magenta;
                }
            }
        }

        private void DrawMenu()
        {
            var freeSpace = Console.WindowWidth - CameraSize.X * 2 - 3;
            var start = CameraSize.X * 2 + 2;

            IList<string> correctedLines = GameManager.Menu.GetMenuDisplay().Split('\n')
                .SelectMany(s => Split(s, freeSpace)).ToList();

            for (var i = 0; i < correctedLines.Count; i++)
            {
                PrepareDraw(correctedLines[i], start, i + 1, ConsoleColor.Black, ConsoleColor.White, true);
            }
        }

        private void DrawMap()
        {
            for (var i = 0; i < CameraSize.X; i++)
            {
                for (var j = 0; j < CameraSize.Y; j++)
                {
                    PrepareDraw('.', i * 2 + 1, j + 1, ConsoleColor.Black, ConsoleColor.White);
                }
            }

            IList<Entity> snapshot = new List<Entity>();
            // Use a snapshot to ensure the List is not changed during draw by another thread
            foreach (var e in GameManager.ActiveMap.Entities)
            {
                snapshot.Add(e);
            }

            foreach (var e in snapshot)
            {
                if (Map.Within(e.Pos, new Rectangle(CameraOffset.X, CameraOffset.Y, CameraSize.X, CameraSize.Y)))
                {
                    var drawPosition = new Point((e.Pos.X - CameraOffset.X) * 2 + 1, e.Pos.Y - CameraOffset.Y + 1);
                    PrepareDraw(e.Ascii, drawPosition.X, drawPosition.Y, e.BackgroundColor, e.ForegroundColor,
                        e is Actor);
                }
            }
        }

        public void Draw()
        {
            SetUpNewDraw();
            DrawFPS();
            DrawFrame();
            DrawMap();
            DrawInput();
            DrawMenu();
            FinishDraw();
        }

        private static IEnumerable<string> Split(string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize + 1)
                .Select(i => str.Substring(i * chunkSize, Math.Min(str.Length, chunkSize)));
        }
    }
}