// This file is part of project Cube21
// Whole solution including its LGPL license could be found at
// http://cube21.sf.net/
// 2007 Pavel Savara, http://zamboch.blogspot.com/

using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Serialization;
using Zamboch.Cube21.Ranking;

namespace Zamboch.Cube21.Work
{
    public enum WorkType
    {
        ExpandCubes,
        FillGaps,
    }

    public class ShapePair : IComparable
    {
        #region Data

        [XmlAttribute("SS")]
        public int SourceShapeIndex;

        [XmlAttribute("WT")]
        public WorkType WorkType;

        [XmlAttribute("TS")]
        public int TargetShapeIndex;

        [XmlAttribute("SW")]
        public long SumOfWork = 1;

        #endregion

        #region Construction

        protected ShapePair()
        {
        }

        public ShapePair(int sourceShapeIndex, int targetShapeIndex)
        {
            SourceShapeIndex = sourceShapeIndex;
            TargetShapeIndex = targetShapeIndex;
        }

        #endregion

        #region Public methods

        public void StartWork(ManualResetEvent finalSignal)
        {
            ThreadPool.UnsafeQueueUserWorkItem(DoWork, finalSignal);
        }

        public void DoWork(object ofinalSignal)
        {
            ManualResetEvent finalSignal = (ManualResetEvent)ofinalSignal;
            DoWork();
            finalSignal.Set();
        }

        public bool DoWork()
        {
            Console.WriteLine("Started SourceShape {0:00}, TargetShape {1:00}", SourceShapeIndex, TargetShapeIndex);
            for (int sourcePageSmallIndex = 0; sourcePageSmallIndex < SmallCubeRank.PermCount; sourcePageSmallIndex++)
            {
                NormalShape sh = Database.GetShape(SourceShapeIndex);
                if (sh.SmallIndexToPages.ContainsKey(sourcePageSmallIndex))
                {
                    Page srcPage = sh.SmallIndexToPages[sourcePageSmallIndex];
                    if (srcPage != null && srcPage.LevelCounts[DatabaseManager.SourceLevel] > 0)
                    {
                        switch (WorkType)
                        {
                            case WorkType.ExpandCubes:
                                ExploreCubes(sourcePageSmallIndex);
                                break;
                            case WorkType.FillGaps:
                                FillGaps(sourcePageSmallIndex);
                                break;
                        }
                    }
                }
            }
            Console.WriteLine("Finished SourceShape {0:00}, TargetShape {1:00}", SourceShapeIndex, TargetShapeIndex);
            if (WorkType==WorkType.FillGaps)
                DatabaseManager.GetShapeLoader(SourceShapeIndex).Close();
            return true;
        }

        public bool ExploreCubes(int sourcePageSmallIndex)
        {
            ShapeLoader sourceShape = DatabaseManager.GetShapeLoader(SourceShapeIndex);
            ShapeLoader targetShape = DatabaseManager.GetShapeLoader(TargetShapeIndex);
            PageLoader sourcePage = DatabaseManager.GetPageLoader(sourceShape, sourcePageSmallIndex);

            try
            {
                sourceShape.Load();
                try
                {
                    targetShape.Load();

                    sourcePage.ExploreCubes(targetShape, DatabaseManager.SourceLevel);
                }
                finally
                {
                    targetShape.Release();
                }
            }
            finally
            {
                sourceShape.Release();
            }

            return true;
        }

        public bool FillGaps(int sourcePageSmallIndex)
        {
            ShapeLoader shape = DatabaseManager.GetShapeLoader(SourceShapeIndex);
            PageLoader page = DatabaseManager.GetPageLoader(shape, sourcePageSmallIndex);

            try
            {
                shape.Load();
                page.FillGaps();
            }
            finally
            {
                shape.Release();
            }

            return true;
        }


        #endregion

        #region Helpers

        [XmlIgnore]
        public int ID
        {
            get { return 90 * SourceShapeIndex + TargetShapeIndex; }
        }

        public bool IsShape(int shapeIndex)
        {
            return (SourceShapeIndex == shapeIndex || TargetShapeIndex == shapeIndex);
        }

        public static long GetShapeScore(List<ShapePair> pairs, int shapeIndex)
        {
            long c = 0;
            foreach (ShapePair pair in pairs)
            {
                if (pair.SourceShapeIndex == shapeIndex || pair.TargetShapeIndex == shapeIndex)
                {
                    c += pair.SumOfWork;
                }
            }
            return c;
        }

        public static long GetShapeScore(List<ShapePair> pairs, int shapeIndex1, int shapeIndex2)
        {
            long c = 0;
            foreach (ShapePair pair in pairs)
            {
                if (pair.IsShape(shapeIndex1) && pair.IsShape(shapeIndex2))
                {
                    c += pair.SumOfWork;
                }
            }
            return c;
        }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            ShapePair p = obj as ShapePair;
            if (p==null)
                return false;
            return (p.SourceShapeIndex == SourceShapeIndex && p.TargetShapeIndex == TargetShapeIndex);
        }

        public override string ToString()
        {
            return SourceShapeIndex + " " + TargetShapeIndex;
        }

        public int CompareTo(object obj)
        {
            ShapePair p = (ShapePair)obj;
            int res;
            res = SourceShapeIndex.CompareTo(p.SourceShapeIndex);
            if (res != 0)
                return res;
            return TargetShapeIndex.CompareTo(p.TargetShapeIndex);
        }

        public override int GetHashCode()
        {
            return SourceShapeIndex + (TargetShapeIndex * 90);
        }

        #endregion
    }
}