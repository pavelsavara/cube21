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
            if (Database.instance.IsExplored)
            {
                TestActions();
                TestData(12, 10000);
            }
            else
            {
                Generator.Dump();
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
                if (DatabaseManager.SourceLevel >= 2)
                {
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
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static void TestActions()
        {
            Random r = new Random();

            Cube w = new Cube();
            TestJoin(w, new Step(6, 2), new Correction(2, 0));
            TestJoin(w, new Step(6, 2), new Correction(true, 2, 0));
            TestJoin(w, new Step(6, 11), new Correction(2, 1));
            TestJoin(w, new Step(6, 11), new Correction(true, 2, 1));

            foreach (NormalShape normalShape in Database.NormalShapes)
            {
                for (int i=0;i<1000;i++)
                {
                    Cube x = new Cube(normalShape.ExampleCube);
                    Cube t = new Cube(normalShape.ExampleCube);
                    Cube u = new Cube(normalShape.ExampleCube);

                    bool flip = r.Next(10) > 5;
                    int top = 0;
                    int bot = 0;
                    if (flip)
                        x.Flip();
                    while (r.Next(10) > 5)
                        top += x.RotateNextTop();
                    while (r.Next(10) > 5)
                        bot += x.RotateNextBot();
                    Correction re = new Correction(flip, top % 12 , bot % 12);
                    re.DoAction(t);
                    if (!t.Equals(x))
                        throw new InvalidProgramException();
                    
                    SmartStep step = x.GetRandomStep(r);
                    step.DoAction(x);
                    step.DoAction(t);
                    
                    SmartStep ss = re + step;
                    ss.DoAction(u);
                    if (!u.Equals(x))
                        throw new InvalidProgramException();
                }
            }
        }

        private static void TestJoin(Cube a, Step s, Correction r)
        {
            SmartStep ss = s + r;

            Cube c = new Cube(a);
            Cube t = new Cube(a);
            
            s.DoAction(c);
            r.DoAction(c);

            ss.DoAction(t);
            if (!t.Equals(c))
                throw new InvalidProgramException();
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