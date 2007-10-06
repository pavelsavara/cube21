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

        public List<WorkItem> Work=new List<WorkItem>();

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
            lock(this)
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
                    Work=new List<WorkItem>(queue);
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
        
        public void PrefetchNext()
        {
            NormalShape sourceShape = Database.GetShape(SourceShapeIndex);
            NormalShape targetShape = Database.GetShape(TargetShapeIndex);

            //source
            try
            {
                if (sourceShape.Load())
                    Console.WriteLine("Loaded SourceShape {0:00}", TargetShapeIndex);

                List<WorkItem> copy;
                lock (this)
                {
                    copy = new List<WorkItem>(Work);
                }
                int d = 0;
                foreach (WorkItem item in copy)
                {
                    Page sourcePage = sourceShape.GetPage(item.SourcePageSmallIndex);
                    d += sourcePage.Touch();
                }
                Console.WriteLine("Touched SourceShape {0:00}, {1:000}", SourceShapeIndex, d);
            }
            finally
            {
                sourceShape.Release();
            }

            //target
            try
            {
                if (targetShape.Load())
                    Console.WriteLine("Loaded TargetShape {0:00}", TargetShapeIndex);

                List<Page> copyPages;
                lock (targetShape)
                {
                    copyPages = new List<Page>(targetShape.Pages);
                }
                int d = 0;
                foreach (Page targetPage in copyPages)
                {
                    d += targetPage.Touch();
                }
                Console.WriteLine("Touched TargetShape {0:00}, {1:000}", TargetShapeIndex, d);
            }
            finally
            {
                targetShape.Release();
            }
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