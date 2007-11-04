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
            //Generator.Dump();
            if (Database.instance.IsExplored)
            {
                TestData(12, 10000);
            }
        }

        public static void TestData(int maxLevel, int count)
        {
            try
            {
                Cube c = new Cube();
                c.Minimalize();
                c.Minimalize();
                c.Minimalize();
                c.Minimalize();

                c.RotateTop(6);
                c.RotateBot(2);
                c.Turn();
                c.Normalize();
                int ln = c.ReadLevel();
                if (ln != 2)
                    throw new InvalidProgramException();
                c.Minimalize();
                c.Minimalize();
                int lm = c.ReadLevel();
                if (lm != 2)
                    throw new InvalidProgramException();
                TestLevel(maxLevel, count);
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static void TestLevel(int maxLevel, int count)
        {
            Random r = new Random();
            Cube cube=new Cube();
            int lastLevel = 1;
            bool up = true;
            for (int i = 0; i < count; i++)
            {
                Cube next = new Cube(cube);
                SmartStep step = cube.FindRandomStep(r, up);
                if (step==null)
                {
                    up = false;
                }
                else
                {
                    step.DoAction(next);
                }

                int currentLevel;
                currentLevel = next.ReadLevel();
                TestLevel(currentLevel, lastLevel, next);
                lastLevel = currentLevel;
                cube = next;

                //Console.WriteLine("{0:00} {1}", lastLevel, cube);

                if (lastLevel == 1)
                    up = true;
                if (lastLevel == maxLevel)
                    up = false;

            }
            Console.WriteLine("Test done");
        }

        private static Cube RandomMove(Cube cube, Random r, out Step s)
        {
            s=new Step();
            Cube next=new Cube(cube);
            for (int t = 0; t < r.Next(6); t++)
            {
                s.TopShift+=next.RotateNextTop();
            }
            for (int b = 0; b < r.Next(6); b++)
            {
                s.BotShift+=next.RotateNextBot();
            }
            next.Turn();
            return next;
        }

        private static void TestLevel(int currentLevel, int lastLevel, Cube tested)
        {
            if (currentLevel != (lastLevel + 1) && 
                currentLevel != (lastLevel - 1) && 
                currentLevel != lastLevel)
            {
                int level = tested.ReadLevel();
                throw new InvalidProgramException();
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