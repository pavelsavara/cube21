using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Zamboch.Cube21.Work
{
    public class ShapePair : IComparable
    {
        #region Data

        [XmlAttribute]
        public int SourceShapeIndex;
        [XmlAttribute]
        public int TargetShapeIndex;
        [XmlAttribute]
        public int SumOfWork = 1;

        public List<WorkItem> Work=new List<WorkItem>();

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

        public bool DoWork()
        {
            Queue<WorkItem> queue;
            lock(this)
            {
                queue = new Queue<WorkItem>(Work);
            }
            while (queue.Count > 0)
            {
                WorkItem workItem = queue.Dequeue();
                workItem.DoWork();
                if (Console.KeyAvailable)
                {
                    Work=new List<WorkItem>(queue);
                    return false;
                }
            }
            lock (this)
            {
                Work.Clear();
            }
            return true;
        }
        
        public void PrefetchNext()
        {
            NormalShape sourceShape = Database.GetShape(SourceShapeIndex);
            NormalShape targetShape = Database.GetShape(TargetShapeIndex);

            sourceShape.Load();
            targetShape.Load();

            List<WorkItem> copy;
            lock(this)
            {
                copy=new List<WorkItem>(Work);
            }
            foreach (WorkItem item in copy)
            {
                Page sourcePage = sourceShape.GetPage(item.SourcePageSmallIndex);
                byte d = sourcePage.Touch();
                Console.WriteLine("Touch SourceShape {0:00}, SourcePage {1:0000} {2:XX}",
                                  SourceShapeIndex, item.SourcePageSmallIndex, d);
            }
            List<Page> copyPages;
            lock(targetShape)
            {
                copyPages = new List<Page>(targetShape.Pages);
            }
            foreach (Page targetPage in copyPages)
            {
                Console.WriteLine("Touch TargetShape {0:00}, TargetPage {1:0000}",
                                  TargetShapeIndex, targetPage.SmallIndex);
                targetPage.Touch();
            }
        }

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