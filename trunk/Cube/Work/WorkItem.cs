using System;
using System.Xml.Serialization;

namespace Zamboch.Cube21.Work
{
    public class WorkItem : IComparable
    {
        #region Data

        [XmlAttribute]
        public int SourceShapeIndex;

        [XmlAttribute]
        public int SourcePageSmallIndex;

        [XmlAttribute]
        public int TargetShapeIndex;

        [XmlAttribute]
        public int SourceLevel;

        #endregion

        #region Construction

        private WorkItem()
        {
        }

        public WorkItem(Page sourcePage, int targetShapeIndex, int sourceLevel)
        {
            SourceShapeIndex = sourcePage.ShapeIndex;
            SourcePageSmallIndex = sourcePage.SmallIndex;
            TargetShapeIndex = targetShapeIndex;
            SourceLevel = sourceLevel;
        }

        #endregion

        #region Public methods

        public bool DoWork()
        {
            ShapeLoader sourceShape = DatabaseManager.GetShapeLoader(SourceShapeIndex);
            ShapeLoader targetShape = DatabaseManager.GetShapeLoader(TargetShapeIndex);
            PageLoader sourcePage = DatabaseManager.GetPageLoader(sourceShape, SourcePageSmallIndex);

            /*
            string time = DateTime.Now.ToLongTimeString();
            int count = sourcePage.LevelCounts[SourceLevel];
            Console.WriteLine("{5} Level {0}, TargetShape {2:00}, SourceShape {1:00}, SourcePage {3:0000}({4:00000})",
                              SourceLevel, SourceShapeIndex, TargetShapeIndex, SourcePageSmallIndex,
                              count, time);
             */

            try
            {
                sourceShape.Load();
                try
                {
                    targetShape.Load();

                    sourcePage.ExpandCubes(targetShape, SourceLevel);
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

        #endregion

        #region Extensions

        public override bool Equals(object obj)
        {
            WorkItem wi = (WorkItem)obj;
            return
                SourceShapeIndex == wi.SourceShapeIndex && SourcePageSmallIndex == wi.SourcePageSmallIndex &&
                TargetShapeIndex == wi.TargetShapeIndex && SourceLevel == wi.SourceLevel;
        }

        public override int GetHashCode()
        {
            return
                SourceShapeIndex + (TargetShapeIndex * 90) + (SourcePageSmallIndex * 90 * 90) +
                (SourceLevel * 90 * 90 * 2600);
        }

        public int CompareTo(object obj)
        {
            WorkItem wi = (WorkItem)obj;
            int res;
            res = SourceLevel.CompareTo(wi.SourceLevel);
            if (res != 0) return res;
            res = TargetShapeIndex.CompareTo(wi.TargetShapeIndex);
            if (res != 0) return res;
            res = SourceShapeIndex.CompareTo(wi.SourceShapeIndex);
            if (res != 0) return res;
            res = SourcePageSmallIndex.CompareTo(wi.SourcePageSmallIndex);
            if (res != 0) return res;
            return 0;
        }

        public override string ToString()
        {
            NormalShape sourceShape = Database.GetShape(SourceShapeIndex);
            Page sourcePage = sourceShape.GetPage(SourcePageSmallIndex);

            return
                string.Format("Level {0}, TargetShape {2}, SourceShape {1}, SourcePage {3}({4:00000})", SourceLevel,
                              SourceShapeIndex, TargetShapeIndex, SourcePageSmallIndex,
                              sourcePage.LevelCounts[SourceLevel]);
        }

        #endregion
    }
}