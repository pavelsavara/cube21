using System;
using System.Collections.Generic;
using System.Threading;
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

        public List<WorkItem> Work = new List<WorkItem>();

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
            Queue<WorkItem> queue;
            lock (this)
            {
                queue = new Queue<WorkItem>(Work);
            }
            Console.WriteLine("Started SourceShape {0:00}, TargetShape {1:00}", SourceShapeIndex, TargetShapeIndex);
            while (queue.Count > 0)
            {
                WorkItem workItem = queue.Dequeue();
                workItem.DoWork();
                if (Console.KeyAvailable)
                {
                    Work = new List<WorkItem>(queue);
                    return false;
                }
            }
            Console.WriteLine("Finished SourceShape {0:00}, TargetShape {1:00}", SourceShapeIndex, TargetShapeIndex);
            lock (this)
            {
                Work.Clear();
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