using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Zamboch.Cube21.Work
{
    public class ShapePair : IComparable
    {
        #region Data

        public int SourceShapeIndex;
        public int TargetShapeIndex;
        public int SumOfWork = 1;

        [XmlIgnore]
        public List<WorkItem> Work;

        #endregion

        #region Construction

        private ShapePair()
        {
        }

        public ShapePair(int sourceShapeIndex, int targetShapeIndex)
        {
            SourceShapeIndex = sourceShapeIndex;
            TargetShapeIndex = targetShapeIndex;
        }

        #endregion

        #region Public methods

        [XmlIgnore]
        public int ID
        {
            get { return 90 * SourceShapeIndex + TargetShapeIndex; }
        }

        public bool IsShape(int shapeIndex)
        {
            return (SourceShapeIndex == shapeIndex || TargetShapeIndex == shapeIndex);
        }

        public static int GetShapeScore(List<ShapePair> pairs, int shapeIndex)
        {
            int c = 0;
            foreach (ShapePair pair in pairs)
            {
                if (pair.SourceShapeIndex == shapeIndex || pair.TargetShapeIndex == shapeIndex)
                {
                    c += pair.SumOfWork;
                }
            }
            return c;
        }

        public static int GetShapeScore(List<ShapePair> pairs, int shapeIndex1, int shapeIndex2)
        {
            int c = 0;
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
            ShapePair p = (ShapePair)obj;
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