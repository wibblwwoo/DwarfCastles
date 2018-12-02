using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Threading;
using DwarfCastles.Jobs;

namespace DwarfCastles
{
    /// <summary>
    /// Authors: Alec Mills and Josh DeMoss
    ///
    /// observer and input handler for game
    /// </summary>
    public class GameManager
    {
        public Map Map { get; }
        public Gui Gui { get; }
        private bool Running = true;
        private static ConcurrentQueue<Job> Tasks;

        public GameManager(Map map, Gui gui)
        {
            Map = map;
            Gui = gui;
            Tasks = new ConcurrentQueue<Job>();
            Run();
        }

        public void AddTask(Job task)
        {
            Tasks.Enqueue(task);
        }

        public Job GetTask()
        {
            return Tasks.TryDequeue(out var result) ? result : null;
        }

        
        public void Run()
        {
            while (Running)
            {
                Update();
                Thread.Sleep(1000);
            }
        }

        public void Update()
        {
            Logger.Log("Entering Update Method in GameManager.cs");
            foreach (Entity e in Map.Entities)
            {
                if (e is Actor)
                {
                    Actor a = (Actor) e;
                    if (a.Jobs.Count == 0)
                    {
                        Random r = new Random();
                        
                        Point nextPos = new Point();
                        do
                        {
                            nextPos = new Point(r.Next(0, Map.Size.X), r.Next(0, Map.Size.Y));
                        } while (Map.Impassables[nextPos.X,nextPos.Y]);
                        
                        Job j = new Build(nextPos, "forge");
                        a.Jobs.Enqueue(j);
                        j.Actor = a;
                    }
                    a.Update();
                }
            }
            Gui.Draw(Map);
        }

        public void HandleInput()
        {
        }
    }
}