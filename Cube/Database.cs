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

        public bool IsLocal = false;
        public bool IsExplored = false;
        public bool IsFilled = false;
        public DateTime[] TimeStart = new DateTime[15];
        public DateTime[] TimeEnd = new DateTime[15];
        public long[] LevelCounts = new long[15];
        public long[] LevelFillCounts = new long[15];
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

        private static readonly string databaseFile = @"Cube\Database.xml";
        private static readonly string reportFile = @"Cube\Report.txt";
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
            string workDir = System.IO.Path.GetDirectoryName(databaseFile);
            if (!Directory.Exists(workDir))
                Directory.CreateDirectory(workDir);

            using (StreamWriter sw = new StreamWriter(databaseFile))
            {
                databaseSerializer.Serialize(sw, this);
            }
        }

        public static Database Load()
        {
            using (StreamReader sr = new StreamReader(databaseFile))
            {
                Database ins = (Database)databaseSerializer.Deserialize(sr);
                whiteShape = ins.normalShapes[0];
                white.Shape = whiteShape;
                foreach (NormalShape shape in ins.normalShapes)
                {
                    shape.RegisterLoaded();
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
                //Database.normalShapes.Add(nshape);
                foreach (Page page in nshape.Pages)
                {
                    page.Shape = nshape;
                    nshape.SmallIndexToPages.Add(page.SmallIndex, page);
                }
            }
        }

        #endregion

        #region Dump

        public void DumpShapes()
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
    }
}