using System;
using System.Collections.Generic;
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
        
        private char[,] visibleChars;
        private ConsoleColor[,] visibleCharsColorsForeground;
        private ConsoleColor[,] visibleCharsColorsBackground;
        private bool[,] charSet;

        public Gui()
        {
            CameraOffset = new Point();
            CameraSize = new Point(25, 25);
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            for (int i = 0; i < CameraSize.Y; i++)
            {
                Console.SetCursorPosition(0,i);
                Console.Write(string.Concat(Enumerable.Repeat(" ", Console.WindowWidth)));
            }
        }

        private void SetUpNewDraw()
        {
            
        }

        private void PrepareDraw(char c, int x, int y, ConsoleColor foreground, ConsoleColor background)
        {
            
        }

        public void Draw(Map map, MenuManager menus, InputManager input)
        {
            Console.CursorVisible = false;

            visibleChars = new char[CameraSize.X, CameraSize.Y];
            visibleCharsColorsForeground = new ConsoleColor[CameraSize.X, CameraSize.Y];
            visibleCharsColorsBackground = new ConsoleColor[CameraSize.X, CameraSize.Y];
            charSet = new bool[CameraSize.X, CameraSize.Y];
            foreach (var e in map.Entities)
            {
                if (map.InBounds(Point.Add(e.Pos, new Size(CameraOffset))))
                {
                    var RelativePoint = new Point(e.Pos.X + CameraOffset.X, e.Pos.Y + CameraOffset.Y);
                    if (charSet[RelativePoint.X, RelativePoint.Y])
                    {
                        continue;
                    }
                    visibleCharsColorsBackground[RelativePoint.X, RelativePoint.Y] = e.BackgroundColor;
                    visibleCharsColorsForeground[RelativePoint.X, RelativePoint.Y] = e.ForegroundColor;
                    visibleChars[RelativePoint.X, RelativePoint.Y] = e.Ascii;
                    if (e is Actor)
                    {
                        charSet[RelativePoint.X, RelativePoint.Y] = true;
                    }
                }
            }

            for (int i = 0; i < visibleChars.GetLength(0); i++)
            {
                for (int j = 0; j < visibleChars.GetLength(1); j++)
                {
                    Console.SetCursorPosition(i * 2, j - CameraOffset.Y);
                    if (visibleChars[i, j] != '\0')
                    {
                        Console.BackgroundColor = visibleCharsColorsBackground[i, j];
                        Console.ForegroundColor = visibleCharsColorsForeground[i, j];
                        Console.Write(visibleChars[i, j]);
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.Write(' ');
                    }
                    else
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write(map.InBounds(new Point(i, j)) ? '.' : ' ');
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.Write(' ');
                    }
                }
            }
            DrawMenu(menus, input);
        }

        public void DrawMenu(MenuManager menu, InputManager input)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            for (int i = 0; i < CameraSize.Y; i++)
            {
                Console.SetCursorPosition(CameraSize.X * 2, i);
                Console.Write("|");
            }

            int FreeSpace = Console.WindowWidth - CameraSize.X * 2 - 3;
            int Start = CameraSize.X * 2 + 2;
 
            IList<string> correctedLines = new List<string>();

            foreach (var s in menu.GetMenuDisplay().Split('\n'))
            {
                foreach (var splitString in Split(s, FreeSpace))
                {
                    correctedLines.Add(splitString);
                }
            }
            // Clear the menu as to ensure there is no overlapped Lines
            Console.ForegroundColor = ConsoleColor.Black;
            for (int i = 0; i < CameraSize.Y; i++)
            {
                Console.SetCursorPosition(Start, i);
                Console.Write(string.Concat(Enumerable.Repeat(" ", FreeSpace)));
            }

            Console.ForegroundColor = ConsoleColor.White;
            var line = 0;
            foreach (var s in correctedLines)
            {
                Console.SetCursorPosition(Start, line);
                Console.Write(s);
                line++;
            }

            Console.BackgroundColor = ConsoleColor.Magenta;

            if (menu.State == 1)
            {
                Console.SetCursorPosition(input.CursorPosition.X * 2, input.CursorPosition.Y);
                Console.Write(' ');
            }
            else if(menu.State == 2)
            {
                var r = MenuManager.FixedRectangle(menu.FirstPoint, input.CursorPosition);
                for (int i = r.X * 2; i <= r.Right * 2; i++)
                {
                    Console.SetCursorPosition(i, r.Y);
                    Console.Write(' ');
                    Console.SetCursorPosition(i, r.Bottom);
                    Console.Write(' ');
                }

                for (int i = r.Y; i <= r.Bottom; i++)
                {
                    Console.SetCursorPosition(r.X * 2, i);
                    Console.Write(' ');
                    Console.SetCursorPosition(r.Right * 2, i);
                    Console.Write(' ');
                }
            }

        }

        private static IEnumerable<string> Split(string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize + 1)
                .Select(i => str.Substring(i * chunkSize, Math.Min(str.Length, chunkSize)));
        }
    }
}