﻿using System.Drawing;

namespace DwarfCastles
{
    public static class Program 
    {
        public static void Main()
        {
            ResourceMasterList.LoadAllResources();
            var game = new GameManager(MapGenerator.GenerateMap(new Point(25,25)), new Gui());
        }
    }
}