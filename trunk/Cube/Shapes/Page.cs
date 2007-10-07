using System;
using System.Xml.Serialization;
using Zamboch.Cube21.Work;

namespace Zamboch.Cube21
{
    public class Page : IComparable
    {
        #region  Construction

        protected Page()
        {
        }

        public Page(int smallIndex, NormalShape shape)
        {
            SmallIndex = smallIndex;
            Shape = shape;
        }

        public Page(Page source)
        {
            SmallIndex = source.SmallIndex;
            Shape = source.Shape;
        }

        #endregion

        #region Data

        [XmlAttribute("Id")]
        public int SmallIndex;

        [XmlIgnore]
        public NormalShape Shape;

        [XmlIgnore]
        public PageLoader Loader;

        public int[] LevelCounts = new int[15];
        public int[] LevelFillCounts = new int[15];

        #endregion

        #region Properties

        public int GetLevelCount(int level, int count)
        {
            return LevelCounts[level - 1];
        }

        public int ShapeIndex
        {
            get { return Shape.ShapeIndex; }
        }

        public int PageFullID
        {
            get { return (Database.ShapesCount * SmallIndex) + Shape.ShapeIndex; }
        }

        #endregion

        #region IComparable Members

        public int CompareTo(object obj)
        {
            Page p = (Page)obj;
            int res = ShapeIndex.CompareTo(p.ShapeIndex);
            if (res != 0) return res;
            return SmallIndex.CompareTo(p.SmallIndex);
        }

        #endregion
    }
}