using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml.Serialization;
using Zamboch.Cube21.Actions;
using Zamboch.Cube21.Work;
using Path=System.IO.Path;

namespace Zamboch.Cube21
{
    public delegate void RegisterShapeDelegate(Shape shape);

    public class Database
    {
        #region Static

        public static Shape GetShape(uint shapeBits)
        {
            return instance.shapeNormalizer[shapeBits];
        }

        public static NormalShape GetShape(int shapeIndex)
        {
            return (NormalShape)instance.shapeIndexes[shapeIndex];
        }

        public static NormalShape CreateShape()
        {
            return instance.CreateShapeImpl();
        }

        protected virtual NormalShape CreateShapeImpl()
        {
            return new NormalShape();
        }

        protected virtual Page CreatePageImpl(int smallIndex, NormalShape shape)
        {
            return new Page(smallIndex, shape);
        }

        public static List<ShapePair> PrepareNextLevel(out long levelCount)
        {
            return WorkDatabase.PrepareNextLevel(instance.SourceLevel, out levelCount);
        }

        public static void RegisterShape(Shape shape)
        {
            instance.shapeIndexes.Add(shape.ShapeIndex, shape);
            instance.shapeNormalizer.Add(shape.ShapeBits, shape);
            if (shape.IsNormal)
            {
                NormalShape nshape = (NormalShape)shape;
                //instance.normalShapes.Add(nshape);
                foreach (Page page in nshape.Pages)
                {
                    page.Shape = nshape;
                    nshape.SmallIndexToPages.Add(page.SmallIndex, page);
                }
            }
        }

        public static Page GetPage(int smallIndex, NormalShape shape)
        {
            Page page;
            lock (shape)
            {
                if (!shape.SmallIndexToPages.ContainsKey(smallIndex))
                {
                    page = instance.CreatePageImpl(smallIndex, shape);
                    shape.Pages.Add(page);
                    shape.SmallIndexToPages.Add(smallIndex, page);
                }
                else
                {
                    page = shape.SmallIndexToPages[smallIndex];
                }
            }
            return page;
        }

        public static List<NormalShape> NormalShapes
        {
            get { return instance.normalShapes; }
        }

        #endregion

        #region Initialization

        public static void Initialize()
        {
            if (instance == null)
                instance = new Database();

            if (CanLoad())
            {
                Load();
                if (instance.normalShapes.Count != 90)
                    throw new FileLoadException();
                instance.normalShapes.Sort();
            }
            else
            {
                instance.SearchShapes();
                instance.AssignShapeIds();
                instance.normalShapes.Sort();
                instance.SearchForSteps();
                InitFirstCube();
            }
            instance.DumpShapes();
        }

        private static void InitFirstCube()
        {
            //Init first cube
            instance.SourceLevel = 1;

            Cube minimalWhite = new Cube(instance.white);
            minimalWhite.Minimalize();

            int bigIndex;
            int smallIndex;
            minimalWhite.GetIndexes(out bigIndex, out smallIndex);

            NormalShape whiteShape = (NormalShape)minimalWhite.Shape;
            Page whitePage = GetPage(smallIndex, whiteShape);
            whiteShape.Load();
            whitePage.Write(bigIndex, instance.SourceLevel);
            whiteShape.Release();
            whiteShape.Close();

            instance.ThisLevelWork = PrepareNextLevel(out instance.LevelCounts[instance.SourceLevel]);
            Save();
        }

        protected static bool CanLoad()
        {
            return File.Exists(databaseFile);
        }

        public static void Save()
        {
            string workDir = Path.GetDirectoryName(databaseFile);
            if (!Directory.Exists(workDir))
                Directory.CreateDirectory(workDir);

            StreamWriter sw = new StreamWriter(databaseFile);
            databaseSerializer.Serialize(sw, instance);
            sw.Close();
        }

        public static void Load()
        {
            StreamReader sr = new StreamReader(databaseFile);
            instance = (Database)databaseSerializer.Deserialize(sr);
            sr.Close();
            instance.white.Shape = instance.whiteShape;
            instance.whiteShape = instance.normalShapes[0];
            foreach (NormalShape shape in instance.normalShapes)
            {
                shape.RegisterLoaded(RegisterShape);
            }
        }

        private void AssignShapeIds()
        {
            int shapeIndex = normalShapes.Count;
            foreach (Shape shape in shapeNormalizer.Values)
            {
                if (!shape.IsNormal)
                {
                    shape.ShapeIndex = shapeIndex;
                    shapeIndex++;
                }
                else
                {
                    NormalShape ns = (NormalShape)shape;
                    if (ns.Parent != null)
                        ns.ParentShapeIndex = ns.Parent.ShapeIndex;
                }
                shapeIndexes.Add(shape.ShapeIndex, shape);
            }
        }

        private void SearchForSteps()
        {
            foreach (NormalShape shape in normalShapes)
            {
                List<Action> steps;
                Cube sourceCube = shape.ExampleCube;
                List<Cube> expansions = sourceCube.ExpandRotateTurn(out steps);
                Dictionary<string, Cube> unique = new Dictionary<string, Cube>();
                for (int i = 0; i < expansions.Count; i++)
                {
                    Cube expansion = expansions[i];
                    Correction norm = expansion.Normalize();
                    bool found = false;
                    string key;
                    key = expansion.ToString();
                    if (unique.ContainsKey(key))
                    {
                        found = true;
                    }
                    else
                    {
                        foreach (RotatedShape rotation in expansion.NormalShape.Rotations)
                        {
                            Cube rotated = new Cube(expansion);
                            rotation.FromNormalStep.DoAction(rotated);
                            key = rotated.ToString();
                            if (unique.ContainsKey(key))
                            {
                                found = true;
                                break;
                            }
                        }
                    }
                    if (!found)
                    {
                        unique.Add(key, expansion);
                        SmartStep step = new SmartStep((Step)steps[i], norm);
                        int targetShapeIndex = expansion.ShapeIndex;
                        step.TargetShapeIndex = targetShapeIndex;
                        shape.NextSteps.Add(step);
                        if (!shape.AllTargetShapeIndexes.Contains(targetShapeIndex))
                        {
                            shape.AllTargetShapeIndexes.Add(targetShapeIndex);
                        }
                    }
                }
                shape.AllTargetShapeIndexes.Sort();
            }
        }

        private void SearchShapes()
        {
            whiteShape = AddShape(white, null, null);
            whiteShape.Level = 1;
            white.Shape = whiteShape;

            Queue<Cube> queue = new Queue<Cube>();
            queue.Enqueue(white);

            while (queue.Count > 0)
            {
                // do next steps
                SearchShapes(queue);
            }
        }

        private void SearchShapes(Queue<Cube> queue)
        {
            Cube source = queue.Dequeue();
            List<Action> steps;
            List<Cube> expansions = source.ExpandRotateTurn(out steps);
            for (int i = 0; i < expansions.Count; i++)
            {
                Cube expansion = expansions[i];
                if (!shapeNormalizer.ContainsKey(expansion.ComputeShapeBits()))
                {
                    Step step = (Step)steps[i];
                    expansion.Shape = AddShape(expansion, (NormalShape)source.Shape, step);
                    expansion.Shape.Level = source.Shape.Level + 1;
                    queue.Enqueue(expansion);
                }
            }
        }

        private NormalShape AddShape(Cube cube, NormalShape parent, Step parentStep)
        {
            NormalShape normal = CreateShape();
            normal.Parent = parent;
            if (parent != null)
            {
                parent.Childern.Add(normal);
            }
            normal.FromParentStep = parentStep;
            normal.ShapeIndex = normalShapes.Count;
            uint shapeBits = cube.ComputeShapeBits();
            normal.ShapeBits = shapeBits;
            shapeNormalizer.Add(shapeBits, normal);
            normalShapes.Add(normal);
            normal.ExampleCube = new Cube(cube);

            AddRotations(cube, normal);
            return normal;
        }

        private void AddRotations(Cube cube, NormalShape normal)
        {
            //add all rotations
            List<Action> steps;
            //rotate and 
            List<Cube> corrections = cube.ExpandRotateFlip(out steps);
            for (int i = 1; i < corrections.Count; i++)
            {
                Cube correction = corrections[i];
                uint shapeBits = correction.ComputeShapeBits();
                Correction step = (Correction)steps[i];
                if (!shapeNormalizer.ContainsKey(shapeBits))
                {
                    RotatedShape rshape = new RotatedShape();
                    rshape.ShapeIndex = -1;
                    rshape.NormalShape = normal;
                    rshape.NormalShape = normal;
                    rshape.FromNormalStep = step;
                    normal.Rotations.Add(rshape);
                    rshape.ShapeBits = shapeBits;
                    shapeNormalizer.Add(rshape.ShapeBits, rshape);
                }
                else
                {
                    Shape sh = shapeNormalizer[shapeBits];
                    if (sh == normal)
                    {
                        normal.Alternatives.Add(step);
                    }
                }
            }
        }

        #endregion

        #region Create Database

        public static bool CreateDatabase()
        {
            bool res = false;
            try
            {
                try
                {
                    res = instance.CreateDatabaseImpl();
                }
                finally
                {
                    Save();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return res;
        }

        public virtual bool CreateDatabaseImpl()
        {
            while (true)
            {
                if (ThisLevelWork.Count == 0)
                {
                    break;
                }

                if (DoLevel())
                {
                    return false;
                }
                TimeEnd[SourceLevel] = DateTime.Now;
                Console.WriteLine("Next level!");
                SourceLevel++;

                ThisLevelWork = PrepareNextLevel(out LevelCounts[SourceLevel]);
                Save();
                GC.Collect();

                TimeStart[SourceLevel] = DateTime.Now;
            }
            return true;
        }

        public bool DoLevel()
        {
            int workersCount = Environment.ProcessorCount + 1;
            ManualResetEvent[] results = new ManualResetEvent[workersCount];
            Queue<ShapePair> queue = new Queue<ShapePair>(ThisLevelWork);

            for (int w = 0; w < workersCount; w++)
            {
                ManualResetEvent resSignal = new ManualResetEvent(true);
                results[w] = resSignal;
                if (queue.Count > 0)
                {
                    resSignal.Reset();
                    ShapePair work = queue.Dequeue();
                    work.StartWork(resSignal);
                }
            }

            while (true)
            {
                int w = WaitHandle.WaitAny(results);
                if (Console.KeyAvailable)
                {
                    ThisLevelWork = new List<ShapePair>(queue);
                    WaitHandle.WaitAll(results);
                    return true;
                }
                if (queue.Count > 0)
                {
                    int percent = 100 - ((queue.Count * 100) / ThisLevelWork.Count);
                    Console.WriteLine("({0:00}%) Level {1}", percent, SourceLevel);

                    ManualResetEvent resSignal = results[w];
                    resSignal.Reset();
                    ShapePair work = queue.Dequeue();
                    work.StartWork(resSignal);
                }
                else
                {
                    ThisLevelWork = new List<ShapePair>(queue);
                    WaitHandle.WaitAll(results);
                    return false;
                }
            }
        }

        #endregion

        #region Dump

        private void DumpShapes()
        {
            StreamWriter sw = new StreamWriter(reportFile);
            foreach (NormalShape shape in normalShapes)
            {
                sw.Write(shape);
                sw.Write(" ");
                if (shape.ParentShapeIndex == -1)
                    sw.Write("  ");
                else
                    sw.Write(shape.ParentShapeIndex.ToString("00"));
                sw.Write(" L");
                sw.Write(shape.Level.ToString("0"));
                sw.Write(" *");
                sw.Write(shape.Rotations.Count.ToString("00"));
                sw.Write(" >");
                foreach (RotatedShape rotation in shape.Rotations)
                {
                    sw.Write(" ");
                    sw.Write(rotation);
                }
                sw.WriteLine();
            }

            sw.WriteLine();
            foreach (NormalShape shape in normalShapes)
            {
                sw.Write(shape.ShapeIndex.ToString("00"));
                sw.Write(" -> ");
                foreach (int shapeIndex in shape.AllTargetShapeIndexes)
                {
                    sw.Write(shapeIndex.ToString("00 "));
                }
                sw.WriteLine();
            }

            sw.WriteLine();
            Actions.Path path = new Actions.Path();
            DumpTree((NormalShape)white.Shape, sw, path);
            sw.Close();
        }

        private void DumpTree(NormalShape shape, StreamWriter sw, Actions.Path path)
        {
            string l = new string(' ', shape.Level);
            string s = new string(' ', 10 - shape.Level);
            sw.Write("{0:0} ", shape.Level);
            sw.Write(shape.ShapeIndex.ToString("00"));
            sw.Write(" ");
            if (shape.Parent != null)
            {
                sw.Write(shape.Parent.ShapeIndex.ToString("00"));
            }
            else
            {
                sw.Write("   ");
            }
            sw.Write(" ");
            sw.Write(l);
            sw.Write(shape);
            sw.Write(" ");
            sw.Write(s);
            sw.WriteLine(path);

            if (shape.Childern != null)
            {
                foreach (NormalShape child in shape.Childern)
                {
                    path.Add(child.FromParentStep);
                    DumpTree(child, sw, path);
                    path.RemoveAt(path.Count - 1);
                }
            }
        }

        #endregion

        #region Data

        private static string databaseFile = @"Cube\Database.xml";
        private static string reportFile = @"Cube\Report.txt";
        public const int ShapesCount = 90;

        public DateTime[] TimeStart=new DateTime[15];
        public DateTime[] TimeEnd = new DateTime[15];
        public long[] LevelCounts = new long[15];

        [XmlIgnore]
        public Cube white = new Cube();

        [XmlIgnore]
        public NormalShape whiteShape;

        public int SourceLevel = 0;

        public List<NormalShape> normalShapes = new List<NormalShape>();
        public List<ShapePair> ThisLevelWork = new List<ShapePair>();

        protected Dictionary<uint, Shape> shapeNormalizer = new Dictionary<uint, Shape>();
        protected Dictionary<int, Shape> shapeIndexes = new Dictionary<int, Shape>();

        protected static XmlSerializer databaseSerializer = new XmlSerializer(typeof(Database));
        protected static Database instance;

        #endregion
    }
}