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
            Cube c=new Cube();
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
            try
            {
                Test2();
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static void Test2()
        {
            Random r = new Random();
            Cube cube=new Cube();
            int lastLevel = 1;
            bool up = true;
            for (int i = 0; i < 10000; i++)
            {
                int currentLevel;
                Cube next;
                /*
                Step s;
                next = RandomMove(cube, r, out s);
                currentLevel = next.ReadLevel();
                TestLevel(currentLevel, lastLevel, next);
                lastLevel = currentLevel;
                cube = next;
                 */

                next = FindMove(cube, ref up);
                currentLevel = next.ReadLevel();
                TestLevel(currentLevel, lastLevel, next);
                lastLevel = currentLevel;
                cube = next;

                Console.WriteLine("{0:00} {1}", lastLevel, cube);
            }
            Console.WriteLine("Test done");
        }

        private static Cube FindMove(Cube cube, ref bool up)
        {
            Cube next;
            if (up)
            {
                next = cube.FindRandomStepAway();
            }
            else
            {
                next = cube.FindStepHome();
            }
            if (next == null || next == cube)
            {
                up = !up;
                return cube;
            }
            else
            {
                return next;
            }
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