using System;
using Zamboch.Cube21.Actions;
using Zamboch.Cube21.Work;

namespace Zamboch.Cube21
{
    public class Test
    {
        public static void Main()
        {
            DatabaseManager manager = new DatabaseManager();
            manager.Initialize();
            Generator.Dump();
        }

        public static void TestData()
        {
            Random r = new Random();
            Cube cube=new Cube();
            int lastLevel = 1;
            for (int i = 0; i < 1000; i++)
            {
                Cube next=new Cube(cube);
                for (int t = 0; t < r.Next(6); t++)
                {
                    next.RotateNextTop();
                }
                for (int b = 0; b < r.Next(6); b++)
                {
                    next.RotateNextBot();
                }
                next.Turn();
                int currentLevel = next.ReadLevel();
                if (currentLevel != lastLevel + 1 && currentLevel != lastLevel - 1)
                {
                    next.WriteLevel(0xf);
                    int level = next.ReadLevel();
                    throw new InvalidProgramException();
                }
                lastLevel = currentLevel;
                cube = next;
            }
        }

        public static void TestPath()
        {
            Random r = new Random();

            for (int i = 0; i < 100; i++)
            {
                TestOnce(r);
            }
        }

        private static void TestOnce(Random r)
        {
            Path path = new Path();

            Cube c = new Cube();
            Cube y = new Cube();
            for (int i = 0; i < 100; i++)
            {
                Step step = new Step();
                int tt = r.Next(10);
                for (int t = 0; t < tt; t++)
                {
                    step.TopShift += c.RotateNextTop();
                }
                int bb = r.Next(10);
                for (int b = 0; b < bb; b++)
                {
                    step.BotShift += c.RotateNextBot();
                }
                c.Turn();
                step.Normalize();
                step.DoAction(y);
                if (!c.Equals(y))
                    throw new InvalidCubeException();
                path.Add(step);
            }

            Cube w = new Cube();

            Cube x = new Cube();
            path.DoActions(x);
            if (!c.Equals(x))
                throw new InvalidCubeException();

            Cube d = new Cube(c);
            path.UndoActions(d);
            if (!w.Equals(d))
                throw new InvalidCubeException();

            Path pb = path.Invert();
            Cube e = new Cube(c);
            pb.DoActions(e);
            if (!w.Equals(e))
                throw new InvalidCubeException();
        }
    }
}