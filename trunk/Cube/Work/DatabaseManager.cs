using System;
using System.Collections.Generic;
using System.Threading;
using Zamboch.Cube21.Properties;

namespace Zamboch.Cube21.Work
{
    public class DatabaseManager : DatabaseManagerBase
    {
        #region Construction & singleton

        public DatabaseManager()
        {
            if (instance != null)
                throw new InvalidOperationException();
            instance = this;
        }

        #endregion

        #region Data

        public static WorkQueue WorkQueue;
        public static DatabaseManager instance;

        public static List<ShapePair> ActualWork
        {
            get { return WorkQueue.ThisLevelWork; }
            set { WorkQueue.ThisLevelWork = value; }
        }

        public static string DatabasePath
        {
            get
            {
                return Settings.Default.DatabasePath;
            }
        }

        public static int SourceLevel
        {
            get { return WorkQueue.SourceLevel; }
            set { WorkQueue.SourceLevel = value; }
        }

        private static void Save()
        {
            WorkQueue.Save();
            Database.Save();
        }

        #endregion

        #region Loaders Factory

        public virtual PageLoader CreatePageLoder(Page page)
        {
            return new PageLoader(page);
        }

        public virtual ShapeLoader CreateShapeLoder(NormalShape shape)
        {
            return new ShapeLoader(shape);
        }

        public static PageLoader GetPageLoader(ShapeLoader shapeLoader, int smallIndex)
        {
            NormalShape shape = shapeLoader.Shape;
            lock (shape)
            {
                Page page;
                if (!shapeLoader.Shape.SmallIndexToPages.ContainsKey(smallIndex))
                {
                    page = new Page(smallIndex, shapeLoader.Shape);
                    page.Loader = instance.CreatePageLoder(page);
                    shape.Pages.Add(page);
                    shape.SmallIndexToPages.Add(smallIndex, page);
                }
                else
                {
                    page = shape.SmallIndexToPages[smallIndex];
                    if (page.Loader == null)
                        page.Loader = instance.CreatePageLoder(page);
                }
                return page.Loader;
            }
        }

        public static ShapeLoader GetShapeLoader(int shapeIndex)
        {
            NormalShape shape = Database.GetShape(shapeIndex);
            lock (shape)
            {
                if (shape.Loader == null)
                    shape.Loader = instance.CreateShapeLoder(shape);
                return shape.Loader;
            }
        }

        #endregion

        #region Init

        public override void Initialize()
        {
            base.Initialize();
            if (WorkQueue.CanLoad())
            {
                WorkQueue = WorkQueue.Load();
            }
            else
            {
                if (WorkQueue == null)
                    WorkQueue = new WorkQueue();
            }
        }

        #endregion

        #region Exploration

        public virtual bool Explore()
        {
            if (Database.IsExplored)
                return true;

            try
            {
                if (ActualWork.Count == 0)
                {
                    InitFirstCube();
                    Test.TestActions();
                    WorkQueue.ThisLevelWork = PrepareNextLevel(SourceLevel, out Database.LevelCounts[SourceLevel]);
                }

                if (!DoWork())
                    return false;
                Database.IsExplored = true;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            finally
            {
                Save();
            }
        }

        public virtual bool FillGaps()
        {
            if (!Database.IsExplored)
                return false;

            if (Database.IsFilled)
                return true;

            try
            {
                if (ActualWork.Count == 0)
                    InitFill();

                if (!DoWork())
                    return false;

                ComputeCounts();

                Database.IsFilled = true;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            finally
            {
                Save();
            }
        }

        public void ComputeCounts()
        {
            for (int level = 0; level < 15; level++)
            {
                Database.LevelCounts[level] = 0;
                Database.LevelFillCounts[level] = 0;
                foreach (NormalShape shape in Database.NormalShapes)
                {
                    shape.LevelCounts[level] = 0;
                    shape.LevelFillCounts[level] = 0;
                    foreach (Page page in shape.Pages)
                    {
                        shape.LevelCounts[level] += page.LevelCounts[level];
                        shape.LevelFillCounts[level] += page.LevelFillCounts[level];
                    }
                    shape.MissingCount -= (shape.LevelCounts[level] + shape.LevelFillCounts[level]);
                    Database.LevelCounts[level] += shape.LevelCounts[level];
                    Database.LevelFillCounts[level] += shape.LevelFillCounts[level];
                }
            }
        }

        private bool DoWork()
        {
            while (true)
            {
                if (ActualWork.Count == 0)
                {
                    break;
                }
                Test.TestData(SourceLevel, 1000);
                /*
                if (SourceLevel == 4)
                {
                    Database.IsExplored = true;
                    SourceLevel++;
                    break;
                }*/

                if (!Database.IsExplored)
                    Database.TimeStart[SourceLevel] = DateTime.Now;
                if (DoLevel())
                {
                    return false;
                }
                if (!Database.IsExplored)
                    Database.TimeEnd[SourceLevel] = DateTime.Now;

                if (!Database.IsExplored)
                {
                    Console.WriteLine("Next level!");
                    SourceLevel++;
                    WorkQueue.ThisLevelWork = PrepareNextLevel(SourceLevel, out Database.LevelCounts[SourceLevel]);
                }

                Save();
                Test.TestData(SourceLevel, 100);
                GC.Collect();
            }
            return true;
        }

        public bool DoLevel()
        {
#if DEBUG
            int workersCount = 1;
#else
            int workersCount = Environment.ProcessorCount + 1;
#endif
            ManualResetEvent[] results = new ManualResetEvent[workersCount];
            Queue<ShapePair> queue = new Queue<ShapePair>(WorkQueue.ThisLevelWork);

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
                    ActualWork = new List<ShapePair>(queue);
                    WaitHandle.WaitAll(results);
                    return true;
                }
                if (queue.Count > 0)
                {
                    int percent = 100 - ((queue.Count * 100) / ActualWork.Count);
                    Console.WriteLine("({0:00}%) Level {1}", percent, SourceLevel);

                    ManualResetEvent resSignal = results[w];
                    resSignal.Reset();
                    ShapePair work = queue.Dequeue();
                    work.StartWork(resSignal);
                }
                else
                {
                    ActualWork = new List<ShapePair>(queue);
                    WaitHandle.WaitAll(results);
                    return false;
                }
            }
        }

        public void InitFirstCube()
        {
            //Init first cube
            SourceLevel = 1;

            Cube minimalWhite = new Cube(Database.white);
            minimalWhite.Minimalize();

            int bigIndex;
            int smallIndex;
            minimalWhite.GetIndexes(out bigIndex, out smallIndex);

            ShapeLoader whiteShapeLoader = GetShapeLoader(0);
            PageLoader whitePage = GetPageLoader(whiteShapeLoader, smallIndex);
            whiteShapeLoader.Load();
            whitePage.Write(bigIndex, WorkQueue.SourceLevel);
            whiteShapeLoader.Release();
            whiteShapeLoader.Close();
        }

        public void InitFill()
        {
            foreach (NormalShape shape in Database.normalShapes)
            {
                ShapePair pair = new ShapePair(shape.ShapeIndex, shape.ShapeIndex);
                pair.WorkType = WorkType.FillGaps;
                foreach (Page page in shape.Pages)
                {
                    pair.Work.Add(new WorkItem(page, -1, -1));
                }
                ActualWork.Add(pair);
            }
        }

        #endregion

        #region Shapes order

        private static List<ShapePair> PrepareNextLevel(int sourceLevel, out long levelCount)
        {
            List<WorkItem> nextLevel = new List<WorkItem>();
            Dictionary<int, WorkItem> hash = new Dictionary<int, WorkItem>();
            Dictionary<int, ShapePair> shapePairs = new Dictionary<int, ShapePair>();

            levelCount = 0;
            foreach (NormalShape shape in Database.NormalShapes)
            {
                shape.LevelCounts[sourceLevel] = 0;
                foreach (Page page in shape.Pages)
                {
                    shape.LevelCounts[sourceLevel] += page.LevelCounts[sourceLevel];
                }
                levelCount += shape.LevelCounts[sourceLevel];
            }

            foreach (NormalShape tgShape in Database.NormalShapes)
            {
                tgShape.Pages.Sort();
                foreach (Page page in tgShape.Pages)
                {
                    int work = page.LevelCounts[sourceLevel];
                    if (work > 0)
                    {
                        foreach (int targetShapeIndex in page.Shape.AllTargetShapeIndexes)
                        {
                            WorkItem nw = new WorkItem(page, targetShapeIndex, sourceLevel);
                            int ha = nw.GetHashCode();
                            if (!hash.ContainsKey(ha))
                            {
                                hash.Add(ha, nw);
                                nextLevel.Add(nw);
                                ShapePair p = new ShapePair(page.Shape.ShapeIndex, targetShapeIndex);
                                if (shapePairs.ContainsKey(p.ID))
                                {
                                    shapePairs[p.ID].SumOfWork++;
                                    shapePairs[p.ID].Work.Add(nw);
                                }
                                else
                                {
                                    p.SumOfWork = 1;
                                    p.Work = new List<WorkItem>();
                                    p.Work.Add(nw);
                                    shapePairs.Add(p.ID, p);
                                }
                            }
                        }
                    }
                }
            }
            List<ShapePair> pairs = new List<ShapePair>(shapePairs.Values);
            pairs.Sort();
            ReorderPairs(pairs);

            foreach (ShapePair pair in pairs)
            {
                pair.Work.Sort();
            }

            return pairs;
        }

        public const int maxShapesLoaded = 20; //TODO
        public const int preLoaded = 2; //TODO

        private static void ReorderPairs(List<ShapePair> pairs)
        {
            List<ShapePair> pairsDone = new List<ShapePair>();
            List<ShapePair> pairsWaiting = new List<ShapePair>(pairs);
            List<int> shapesIn;

            shapesIn = GetFirstShape(pairsWaiting);
            //move done out
            MovePairs(shapesIn, pairsWaiting, pairsDone);

            while (true)
            {
                int bestScore;
                //kick weakest shape
                if ((shapesIn.Count + 1) > (maxShapesLoaded - preLoaded))
                {
                    do
                    {
                        int kick = KickOne(shapesIn, pairsWaiting, out bestScore);
                        shapesIn.Remove(kick);
                    } while (bestScore == 0);
                }

                if (pairsWaiting.Count == 0)
                {
                    break;
                }

                //load strong shape
                int load = GetOne(shapesIn, pairsWaiting, out bestScore);
                shapesIn.Add(load);

                //move done out
                MovePairs(shapesIn, pairsWaiting, pairsDone);
            }
            pairs.Clear();
            pairs.AddRange(pairsDone);
        }

        private static List<int> GetFirstShape(List<ShapePair> pairsWaiting)
        {
            List<int> shapesIn;
            shapesIn = new List<int>();
            for (int i = 0; i < 90; i++)
            {
                if (ShapePair.GetShapeScore(pairsWaiting, i) > 0)
                {
                    shapesIn.Add(i);
                }
            }
            if (shapesIn.Count >= 80)
            {
                shapesIn.Clear();
                shapesIn.Add(54); //from prev search
                return shapesIn;
            }

            while (shapesIn.Count > 1)
            {
                int bestScore;
                int kick = KickOne(shapesIn, pairsWaiting, out bestScore);
                shapesIn.Remove(kick);
            }
            return shapesIn;
        }

        private static int KickOne(List<int> shapesIn, List<ShapePair> pairsWaiting, out int bestScore)
        {
            List<int> shapesInTemp = new List<int>(shapesIn);
            bestScore = -1;
            int bestKick = -1;
            foreach (int kickShape in shapesIn)
            {
                int score;
                shapesInTemp.Remove(kickShape);
                GetOne(shapesInTemp, pairsWaiting, out score);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestKick = kickShape;
                }
                shapesInTemp.Add(kickShape);
            }
            return bestKick;
        }

        private static int GetOne(List<int> shapesIn, List<ShapePair> pairsWaiting, out int bestScore)
        {
            bestScore = -1;
            int bestShapeIndex = -1;
            for (int shapeOutIndex = 0; shapeOutIndex < 90; shapeOutIndex++)
            {
                if (!shapesIn.Contains(shapeOutIndex))
                {
                    int shapeScore = 0;
                    if (shapesIn.Count == 0)
                    {
                        shapeScore += ShapePair.GetShapeScore(pairsWaiting, shapeOutIndex);
                    }
                    else
                    {
                        foreach (int shapeInIndex in shapesIn)
                        {
                            shapeScore += ShapePair.GetShapeScore(pairsWaiting, shapeInIndex, shapeOutIndex);
                        }
                    }
                    if (shapeScore > bestScore)
                    {
                        bestShapeIndex = shapeOutIndex;
                        bestScore = shapeScore;
                    }
                }
            }
            return bestShapeIndex;
        }


        private static void MovePairs(List<int> shapesIn, List<ShapePair> pairsFrom, List<ShapePair> pairsTo)
        {
            List<ShapePair> toMove = new List<ShapePair>();
            foreach (ShapePair pair in pairsFrom)
            {
                if (shapesIn.Contains(pair.SourceShapeIndex) && shapesIn.Contains(pair.TargetShapeIndex))
                {
                    toMove.Add(pair);
                }
            }
            foreach (ShapePair pair in toMove)
            {
                pairsFrom.Remove(pair);
                pairsTo.Add(pair);
            }
        }

        #endregion
    }
}