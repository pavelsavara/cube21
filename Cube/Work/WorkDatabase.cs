using System.Collections.Generic;

namespace Zamboch.Cube21.Work
{
    public class WorkDatabase
    {
        #region Shapes order

        public static List<ShapePair> PrepareNextLevel(int sourceLevel)
        {
            List<WorkItem> nextLevel = new List<WorkItem>();
            Dictionary<int, WorkItem> hash = new Dictionary<int, WorkItem>();
            Dictionary<int, ShapePair> shapePairs = new Dictionary<int, ShapePair>();


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
                if ((shapesIn.Count + 1) > (maxShapesLoaded-preLoaded))
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