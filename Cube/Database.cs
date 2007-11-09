using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Zamboch.Cube21.Work;
using Path=Zamboch.Cube21.Actions.Path;

namespace Zamboch.Cube21
{
    public delegate void RegisterShapeDelegate(Shape shape);

    public class Database
    {
        #region Data

        public bool IsExplored = false;
        public DateTime[] TimeStart = new DateTime[15];
        public DateTime[] TimeEnd = new DateTime[15];
        public long[] LevelCounts = new long[15];
        public List<NormalShape> normalShapes = new List<NormalShape>();

        #endregion

        #region Helper Data

        public static Database instance
        {
            get { return DatabaseManagerBase.Database; }
        }

        private static readonly XmlSerializer databaseSerializer = new XmlSerializer(typeof(Database));

        public static Cube white = new Cube();
        public static NormalShape whiteShape;
        public static Dictionary<uint, Shape> shapeNormalizer = new Dictionary<uint, Shape>();
        public static Dictionary<int, Shape> shapeIndexes = new Dictionary<int, Shape>();

        public static Dictionary<uint, HalfShape> halfShapeNormalizer = new Dictionary<uint, HalfShape>();


        private static String databaseFile
        {
            get
            {
                return System.IO.Path.Combine(DatabaseManager.DatabasePath, "Database.xml");
            }
        }

        private static String reportFile
        {
            get
            {
                return System.IO.Path.Combine(DatabaseManager.DatabasePath, "Report.txt");
            }
        }
        
        public const int ShapesCount = 90;

        #endregion

        #region Public methods

        public static NormalShape GetShape(int shapeIndex)
        {
            return (NormalShape)shapeIndexes[shapeIndex];
        }

        public static Shape GetShape(uint shapeBits)
        {
            return shapeNormalizer[shapeBits];
        }

        public static List<NormalShape> NormalShapes
        {
            get { return instance.normalShapes; }
        }

        #endregion

        #region Persistence

        public static bool CanLoad()
        {
            return File.Exists(databaseFile);
        }

        public void Save()
        {
            if (IsExplored)
            {
                foreach (NormalShape normalShape in normalShapes)
                {
                    normalShape.Pages = new List<Page>();
                }
            }
            string workDir = System.IO.Path.GetDirectoryName(databaseFile);
            if (!Directory.Exists(workDir))
                Directory.CreateDirectory(workDir);

            using (StreamWriter sw = new StreamWriter(databaseFile))
            {
                databaseSerializer.Serialize(sw, this);
            }
        }

        public static Database Load(bool register)
        {
            using (StreamReader sr = new StreamReader(databaseFile))
            {
                Database ins = (Database)databaseSerializer.Deserialize(sr);
                if (register)
                {
                    whiteShape = ins.normalShapes[0];
                    white.Shape = whiteShape;
                    foreach (NormalShape shape in ins.normalShapes)
                    {
                        shape.RegisterLoaded();
                    }
                }
                return ins;
            }
        }

        public static void RegisterShape(Shape shape)
        {
            shapeIndexes.Add(shape.ShapeIndex, shape);
            shapeNormalizer.Add(shape.ShapeBits, shape);
            if (shape.IsNormal)
            {
                NormalShape nshape = (NormalShape)shape;
                foreach (Page page in nshape.Pages)
                {
                    page.Shape = nshape;
                    nshape.SmallIndexToPages.Add(page.SmallIndex, page);
                }
                nshape.AddHalfShapes();
            }
        }

        #endregion

        #region Dump

        public void DumpShapes()
        {
            StreamWriter sw = new StreamWriter(reportFile);
            foreach (NormalShape shape in normalShapes)
            {
                sw.Write("{0,-26}", shape);
                sw.Write(" -> ");
                if (shape.ParentShapeIndex == -1)
                    sw.Write("   ");
                else
                    sw.Write("P{0:00}", shape.ParentShapeIndex);
                sw.Write(" L");
                sw.Write(shape.Level.ToString("0"));
                if (shape.Alternatives.Count>0)
                    sw.Write(" A{0:00}", shape.Alternatives.Count);
                else
                    sw.Write("    ");
                sw.Write(" {1,2}+{2,-2} {0}", shape.Code, shape.TopPieces, shape.BotPieces);
                sw.Write(" > ");
                foreach (int shapeIndex in shape.AllTargetShapeIndexes)
                {
                    if (shapeIndex > shape.ShapeIndex)
                    {
                        sw.Write("{0,-26}", GetShape(shapeIndex));
                    }
                }
                sw.Write(" <= ");
                foreach (int shapeIndex in shape.AllTargetShapeIndexes)
                {
                    if (shapeIndex <= shape.ShapeIndex)
                    {
                        sw.Write("{0,-26}", GetShape(shapeIndex));
                    }
                }
                sw.WriteLine();
            }

            sw.WriteLine();
            foreach (NormalShape shape in normalShapes)
            {
                sw.Write(shape.ShapeIndex.ToString("00"));
                sw.Write("R{0,2} R>", shape.Rotations.Count);
                foreach (RotatedShape rotation in shape.Rotations)
                {
                    sw.Write(" ");
                    sw.Write(rotation);
                }
                sw.WriteLine();
            }

            if (IsExplored)
            {
                sw.WriteLine();
                sw.Write("             ");
                for (int l = 0; l < 13; l++)
                {
                    sw.Write(" Level {0,2}   |", l);
                }
                sw.WriteLine();
                foreach (NormalShape shape in normalShapes)
                {
                    sw.Write(shape.ShapeIndex.ToString("00"));
                    sw.Write(" -> ");
                    sw.Write(" P");
                    sw.Write(shape.Pages.Count.ToString("0000"));
                    for (int l = 0; l < 13; l++)
                    {
                        sw.Write("|L={0,10}", shape.LevelCounts[l]);
                    }
                    sw.WriteLine();
                }
            }

            sw.WriteLine();
            Path path = new Path();
            DumpTree((NormalShape)white.Shape, sw, path);
            sw.Close();
        }

        private static void DumpTree(NormalShape shape, StreamWriter sw, Path path)
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
            sw.Write(" {0} {1,-26} {2}", l, shape, s);
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
    }
}