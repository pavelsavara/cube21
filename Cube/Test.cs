// This file is part of project Cube21
// Whole solution including its LGPL license could be found at
// http://cube21.sf.net/
// 2007 Pavel Savara, http://zamboch.blogspot.com/

using System;
using System.Collections.Generic;
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
                TestData(13, 100000);
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
                Cube c;
                if (DatabaseManager.SourceLevel >= 9)
                {
                    Cube b = new Cube("C479F23056A18BED");
                    for (int i = 0; i < 1000; i++)
                    {
                        TestCube(b, new Random());
                    }
                    b.Normalize();
                    b.Minimalize();
                    int lm = b.ReadLevel();
                    if (lm != 7)
                        throw new InvalidProgramCubeException();
                    SmartStep ss = new SmartStep(new Step(4, 2), new Correction(10, 6));
                    b = new Cube("C479F23056A18BED");
                    c = new Cube("C479F23056A18BED");
                    Cube d = new Cube("C479F23056A18BED");
                    ss.DoAction(c);
                    CheckNext(b, c);

                    Correction normalize = d.Normalize();
                    normalize.Invert();
                    SmartStep ss2 = normalize + ss;
                    ss2.DoAction(d);
                    if (!d.Equals(c))
                        throw new InvalidProgramCubeException();

                    b.ExpandToShape(18);

                    List<Action> a;
                    List<Cube> rotate = c.ExpandRotate(out a);
                    bool found = false;
                    foreach (Cube cube in rotate)
                    {
                        lm = cube.ReadLevel(false);
                        if (lm == 8)
                            found = true;
                    }
                    if (!found)
                        throw new InvalidProgramCubeException();

                    b.Normalize();
                    b.Minimalize();
                    lm = c.ReadLevel();
                    
                    if (lm != 8)
                        throw new InvalidProgramCubeException();
                }
                c = new Cube();
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
                        throw new InvalidProgramCubeException();
                    c.Minimalize();
                    c.Minimalize();
                    int lm = c.ReadLevel();
                    if (lm != 2)
                        throw new InvalidProgramCubeException();
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
                    Cube source = normalShape.ExampleCube;
                    TestCube(source, r);
                }
            }
        }

        private static void TestCube(Cube source, Random r)
        {
            Cube x = new Cube(source);
            Cube t = new Cube(source);
            Cube u = new Cube(source);

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
                throw new InvalidProgramCubeException();
                    
            SmartStep step = x.GetRandomStep(r);
            step.DoAction(x);
            step.DoAction(t);
                    
            SmartStep ss = re + step;
            ss.DoAction(u);
            if (!u.Equals(x))
                throw new InvalidProgramCubeException();


            Cube y = new Cube(source);
            Step s;
            y = RandomMove(y, r, out s);
            y.Normalize();
            y.Minimalize();
            CheckNext(source, y);
        }

        private static void CheckNext(Cube source, Cube target)
        {
            bool found = false;
            foreach (SmartStep nextStep in source.NormalShape.NextSteps)
            {
                Cube z = new Cube(source);
                z.Normalize();
                z.Minimalize();
                nextStep.DoAction(z);
                z.Normalize();
                z.Minimalize();
                if (z.Equals(target))
                {
                    found = true;
                    break;
                }
            }
            if (!found)
                throw new InvalidProgramCubeException();
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
                throw new InvalidProgramCubeException();
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

                //Console.WriteLine("{0:00} {1}", lastLevel, cube);

                if (lastLevel == 1)
                {
                    up = true;
                }
                else if (lastLevel >= maxLevel)
                {
                    up = false;
                }
                else if (step == null)
                {
                    Step s;
                    Cube n2 = next;
                    next = RandomMove(n2, r, out s);
                    currentLevel = next.ReadLevel();
                    TestLevel(currentLevel, lastLevel, next);

                    Console.WriteLine("Reached {0:00} {1} {2} {3}", currentLevel, next, next.Shape, next.NormalShape);
                    
                    //restart
                    lastLevel = 1;
                    next = new Cube();
                }
                cube = next;
            }
            Console.WriteLine("Test done");
        }

        private static Cube RandomMove(Cube cube, Random r, out Step s)
        {
            s = new Step();
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
                throw new InvalidProgramCubeException();
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
            Cube c = new Cube();
            Cube y = new Cube();
            Path path = new Path(c);
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
                path.Add(new SmartStep(step, null));
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