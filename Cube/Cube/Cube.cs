// This file is part of project Cube21
// Whole solution including its LGPL license could be found at
// http://cube21.sf.net/
// 2007 Pavel Savara, http://zamboch.blogspot.com/

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;
using Zamboch.Cube21.Actions;
using Zamboch.Cube21.Ranking;
using Zamboch.Cube21.Work;
using System.Runtime.Serialization;
using System.ServiceModel;


namespace Zamboch.Cube21
{
    [DataContract]
    public class Cube 
    {
        #region Data

        private Piece[] top;
        private Piece[] bot;
        private Shape shape;

        #endregion

        #region Construction

        public Cube()
            : this("0123456789ABCDEF")
        {
        }

        public Cube(string form)
        {
            PieceHelper.ToForm(form, out top, out bot);
        }

        public Cube(Cube source)
        {
            top = source.top;
            bot = source.bot;
            shape = source.shape;
        }

        public Cube(NormalShape shape, int smallIndex, int bigIndex)
        {
            uint smallBits = SmallCubeRank.UnrankEx(smallIndex);
            uint bigBits = BigCubeRank.Unrank(bigIndex);
            top = new Piece[12];
            bot = new Piece[12];
            PieceHelper.Decompress(smallBits, bigBits, top, bot, shape);
#if DEBUG
            CheckPieces();
#endif
        }

        #endregion

        #region Properties

        [XmlIgnore]
        public Piece[] TopPieces
        {
            get
            {
                return top;
            }
        }

        [XmlIgnore]
        public Piece[] BotPieces
        {
            get
            {
                return bot;
            }
        }

        [XmlIgnore]
        public int SmallIndex
        {
            get
            {
                int bigIndex;
                int smallIndex;
                GetIndexes(out bigIndex, out smallIndex);
                return smallIndex;
            }
        }

        [XmlIgnore]
        public int BigIndex
        {
            get
            {
                int bigIndex;
                int smallIndex;
                GetIndexes(out bigIndex, out smallIndex);
                return bigIndex;
            }
        }

        [XmlIgnore]
        public uint BigBits
        {
            get
            {
                uint bigb;
                uint smallb;
                GetBits(out bigb, out smallb);
                return bigb;
            }
        }

        [XmlIgnore]
        public uint SmallBits
        {
            get
            {
                uint bigb;
                uint smallb;
                GetBits(out bigb, out smallb);
                return smallb;
            }
        }

        [XmlIgnore]
        public int ShapeIndex
        {
            get { return Shape.ShapeIndex; }
        }

        [XmlIgnore]
        public bool IsNormalShape
        {
            get { return Shape.IsNormal; }
        }

        [XmlIgnore]
        public uint ShapeBits
        {
            get
            {
                if (shape == null)
                    InitShape();
                return shape.ShapeBits;
            }
        }

        [XmlIgnore]
        public Shape Shape
        {
            get
            {
                if (shape == null)
                    InitShape();
                return shape;
            }
            set { shape = value; }
        }

        [XmlIgnore]
        public NormalShape NormalShape
        {
            get
            {
                if (shape == null)
                    InitShape();
                if (shape.IsNormal)
                    return (NormalShape)shape;
                return ((RotatedShape)shape).NormalShape;
            }
        }

        [XmlIgnore]
        public int NextTop
        {
            get { return GetNext(top); }
        }

        [XmlIgnore]
        public int NextBot
        {
            get { return GetNext(bot); }
        }

        [XmlIgnore]
        public int PrevTop
        {
            get { return GetPrev(top); }
        }

        [XmlIgnore]
        public int PrevBot
        {
            get { return GetPrev(bot); }
        }

        public int PositionOf(Piece piece)
        {
            for(int t=0;t<12;t++)
            {
                if (top[t]==piece)
                    return t;
            }
            for (int b = 0; b < 12; b++)
            {
                if (bot[b] == piece)
                    return b + 12;
            }
            throw new InvalidProgramCubeException();
        }

        #endregion

        #region Active Operations

        public Correction Normalize()
        {
            if (Shape.IsNormal)
                return null;
            RotatedShape rs = (RotatedShape)Shape;
            rs.ToNormalStep.DoAction(this);
#if DEBUG
            CheckFlipable();
#endif
            return rs.ToNormalStep;
        }

        public Correction Minimalize()
        {
            if (!IsNormalShape)
                throw new NonNormalizedCubeException();

            int bigIndex;
            int smallIndex;
            Cube best = this;
            Correction bestCorrection = null;
            GetIndexes(out bigIndex, out smallIndex);
            foreach (Correction correction in NormalShape.Alternatives)
            {
                Cube candidate;
                int bigIndexCorr;
                int smallIndexCorr;
                candidate = new Cube(this);
                correction.DoAction(candidate);
#if DEBUG
                if (candidate.Shape != Shape)
                    throw new InvalidProgramCubeException();
#endif
                candidate.GetIndexes(out bigIndexCorr, out smallIndexCorr);

                if (smallIndexCorr < smallIndex || (
                    smallIndexCorr <= smallIndex && bigIndexCorr < bigIndex))
                {
                    bigIndex = bigIndexCorr;
                    smallIndex = smallIndexCorr;
                    best = candidate;
                    bestCorrection = correction;
                }
            }
            top = best.top;
            bot = best.bot;
            shape = null;
            return bestCorrection;
        }

        public void RotateTop(int shift)
        {
            top = Rotate(shift, top);
            shape = null;
        }

        public void RotateBot(int shift)
        {
            bot = Rotate(shift, bot);
            shape = null;
        }

        public int RotateNextTop()
        {
            int shift = GetNext(top);
            RotateTop(shift);
            return shift;
        }

        public int RotateNextBot()
        {
            int shift = GetNext(bot);
            RotateBot(shift);
            return shift;
        }

        public int RotatePrevTop()
        {
            int shift = GetPrev(top);
            RotateTop(shift);
            return shift;
        }

        public int RotatePrevBot()
        {
            int shift = GetPrev(bot);
            RotateBot(shift);
            return shift;
        }

        /// <summary>
        /// Flip right-hand side of the cube
        /// </summary>
        public void Turn()
        {
            if (!CheckTurn(0, top) || !CheckTurn(0, bot))
                throw new NonTunableCubeException();
            TurnImlementation();
            shape = null;
        }

        /// <summary>
        /// Flip whole cube, both sides
        /// </summary>
        public void Flip()
        {
            if (!CheckTurn(0, top) || !CheckTurn(0, bot))
                throw new NonTunableCubeException();
            FlipImlementation();
            shape = null;
        }

        #endregion

        #region Expansinon

        public List<Cube> ExpandRotateTurn(out List<Action> actions)
        {
            //original rotations
            List<Cube> rotations = ExpandRotate(out actions);

            //turn existing
            for (int i = 0; i < rotations.Count; i++)
            {
                Cube rotation = rotations[i];
                actions[i] = new Step(actions[i]);
                rotation.Turn();
            }
            return rotations;
        }

        public List<Cube> ExpandRotateFlip(out List<Action> actions)
        {
            //original rotations
            List<Cube> rotations = ExpandRotate(out actions);

            Cube flip=new Cube(this);
            flip.Flip();
            List<Action> actionsf;
            List<Cube> rotationsf = flip.ExpandRotate(out actionsf);

            for (int i = 0; i < actionsf.Count; i++)
            {
                //upgrade action to correction
                Correction correction = new Correction(actionsf[i]);
                correction.Flip = true;
                actionsf[i] = correction;
            }
            for (int i = 0; i < actions.Count; i++)
            {
                //upgrade action to correction
                Correction correction = new Correction(actions[i]);
                actions[i] = correction;
            }
            rotations.AddRange(rotationsf);
            actions.AddRange(actionsf);

            return rotations;
        }

        public List<Cube> ExpandRotate(out List<Action> actions)
        {
            List<Cube> res = new List<Cube>();
            actions = new List<Action>();
            for (int t = 0; t < 12; t++)
            {
                if (CheckTurn(t, top))
                {
                    Cube ct = new Cube(this);
                    ct.RotateTop(t);
                    for (int b = 0; b < 12; b++)
                    {
                        if (CheckTurn(b, ct.bot))
                        {
                            Cube cb = new Cube(ct);
                            cb.RotateBot(b);
                            actions.Add(new Step(t, b));
                            res.Add(cb);
                        }
                    }
                }
            }
            return res;
        }

        #endregion

        #region Validation

        private static bool CheckTurn(int shift, Piece[] source)
        {
            return
                source[(12 - shift + 11) % 12] != source[(12 - shift + 0) % 12] &&
                source[(12 - shift + 5) % 12] != source[(12 - shift + 6) % 12];
        }

        public void CheckFlipable()
        {
            if (!CheckTurn(0, top) || !CheckTurn(0, bot))
                throw new NonTunableCubeException();
        }

        public void CheckPieces()
        {
            bool[] c = new bool[16];
            for (int t = 0; t < 12; t++)
            {
                Piece p = top[t];
                if (PieceHelper.IsBig(p))
                {
                    t++;
                }
                if (c[(int)p])
                    throw new InvalidCubeException();
                c[(int)p] = true;
            }
            for (int b = 0; b < 12; b++)
            {
                Piece p = bot[b];
                if (PieceHelper.IsBig(p))
                {
                    b++;
                }
                if (c[(int)p])
                    throw new InvalidCubeException();
                c[(int)p] = true;
            }
        }

        #endregion

        #region Conversions

        public int GetPermutation()
        {
            int bigRank;
            int smallRank;
            GetIndexes(out bigRank, out smallRank);
            return smallRank * FullRank.PermCount + bigRank;
        }

        public void GetIndexes(out int bigIndex, out int smallIndex)
        {
            if (!Shape.IsNormal)
                throw new NonNormalizedCubeException();
            byte[] small;
            byte[] big;
            uint smallb;
            uint bigb;
            PieceHelper.Compress(out small, out big, out smallb, out bigb, top, bot, (NormalShape)Shape);
            smallIndex = SmallCubeRank.RankEx(smallb);
            bigIndex = BigCubeRank.Rank(bigb);
        }

        public void GetBits(out uint bigb, out uint smallb)
        {
            if (!Shape.IsNormal)
                throw new NonNormalizedCubeException();
            byte[] small;
            byte[] big;
            PieceHelper.Compress(out small, out big, out smallb, out bigb, top, bot, (NormalShape)Shape);
        }

        public void GetBytes(out byte[] big, out byte[] small)
        {
            if (!Shape.IsNormal)
                throw new NonNormalizedCubeException();
            uint smallb;
            uint bigb;
            PieceHelper.Compress(out small, out big, out smallb, out bigb, top, bot, (NormalShape)Shape);
        }

        #endregion

        #region Helpers

        public uint ComputeShapeBits()
        {
            uint tp = 0;
            uint topBits = 0;
            for (int t = 0; t < 12; t++)
            {
                topBits <<= 1;
                tp++;
                uint bit = (uint)top[t] & 0x1;
                topBits |= bit;
                if (bit != 0)
                    t++;
            }
            uint botBits = 0;
            for (int b = 0; b < 12; b++)
            {
                botBits <<= 1;
                uint bit = (uint)bot[b] & 0x1;
                botBits |= bit;
                if (bit != 0)
                    b++;
            }
            return (tp << 24) | (topBits << 12) | botBits;
        }

        private void InitShape()
        {
            uint shapeBits = ComputeShapeBits();
            shape = Database.GetShape(shapeBits);
        }

        private void TurnImlementation()
        {
            Piece[] newtop = new Piece[12];
            Piece[] newbot = new Piece[12];

            //right half
            newtop[0] = bot[5];
            newtop[1] = bot[4];
            newtop[2] = bot[3];
            newtop[3] = bot[2];
            newtop[4] = bot[1];
            newtop[5] = bot[0];

            newbot[0] = top[5];
            newbot[1] = top[4];
            newbot[2] = top[3];
            newbot[3] = top[2];
            newbot[4] = top[1];
            newbot[5] = top[0];

            //left half copy only
            Array.Copy(top, 6, newtop, 6, 6);
            Array.Copy(bot, 6, newbot, 6, 6);

            top = newtop;
            bot = newbot;
        }

        private void FlipImlementation()
        {
            Piece[] newtop = new Piece[12];
            Piece[] newbot = new Piece[12];

            //right half
            newtop[0] = bot[5];
            newtop[1] = bot[4];
            newtop[2] = bot[3];
            newtop[3] = bot[2];
            newtop[4] = bot[1];
            newtop[5] = bot[0];

            newbot[0] = top[5];
            newbot[1] = top[4];
            newbot[2] = top[3];
            newbot[3] = top[2];
            newbot[4] = top[1];
            newbot[5] = top[0];

            //left half
            newtop[6] = bot[11];
            newtop[7] = bot[10];
            newtop[8] = bot[9];
            newtop[9] = bot[8];
            newtop[10] = bot[7];
            newtop[11] = bot[6];

            newbot[6] = top[11];
            newbot[7] = top[10];
            newbot[8] = top[9];
            newbot[9] = top[8];
            newbot[10] = top[7];
            newbot[11] = top[6];

            top = newtop;
            bot = newbot;
        }

        private static int GetNext(Piece[] source)
        {
            for (int i = 1; i < 12; i++)
            {
                if (CheckTurn(i, source))
                {
                    return i;
                }
            }
            throw new InvalidCubeException();
        }

        private static int GetPrev(Piece[] source)
        {
            for (int i = 11; i >= 1; i--)
            {
                if (CheckTurn(i, source))
                {
                    return i;
                }
            }
            throw new InvalidCubeException();
        }

        private static Piece[] Rotate(int shift, Piece[] source)
        {
            shift = (shift + 144) % 12;
            if (shift == 0)
                return source;
#if DEBUG
            if (!CheckTurn(shift, source))
            {
                CheckTurn(shift, source);
                throw new NonTunableCubeException();
            }
#endif
            Piece[] temp = new Piece[12];
            int rest = 12 - shift;
            Array.Copy(source, 0, temp, shift, rest);
            Array.Copy(source, rest, temp, 0, shift);
#if DEBUG
            if (!CheckTurn(0, temp))
                throw new NonTunableCubeException();
#endif
            return temp;
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return PieceHelper.ToString(top) + "-" + PieceHelper.ToString(bot);
        }

        public string ToStringShort()
        {
            return PieceHelper.ToString(top) + PieceHelper.ToString(bot);
        }

        public string TopToString()
        {
            return PieceHelper.ToString(top);
        }

        public string BotToString()
        {
            return PieceHelper.ToString(bot);
        }

        public bool Equals(Cube o)
        {
            for (int i = 0; i < 12; i++)
            {
                if (top[i] != o.top[i] || bot[i] != o.bot[i])
                    return false;
            }
            return true;
        }

        public bool EqualsSmallWise(Cube o)
        {
            for (int i = 0; i < 12; i++)
            {
                if (PieceHelper.IsSmall(top[i]))
                {
                    if (((int)top[i] & 0xD) != ((int)o.top[i] & 0xD))
                        return false;
                }
                else
                {
                    if (top[i] != o.top[i])
                        return false;
                }


                if (PieceHelper.IsSmall(bot[i]))
                {
                    if (((int)bot[i] & 0xD) != ((int)o.bot[i] & 0xD))
                        return false;
                }
                else
                {
                    if (bot[i] != o.bot[i])
                        return false;
                }
            }
            return true;
        }

        [XmlAttribute("Perm")]
        [DataMember]
        public string CubePermutation
        {
            get
            {
                return PieceHelper.ToString(top) + PieceHelper.ToString(bot);
            }
            set
            {
                PieceHelper.ToForm(value, out top, out bot);
            }
        }

        #endregion

        #region Database usage

        public void ExpandToShape(int targetShapeIndex)
        {
            Cube copy = new Cube(this);
            copy.Normalize();
            copy.Minimalize();
            int bigIndex;
            int smallIndex;
            copy.GetIndexes(out bigIndex, out smallIndex);
            ShapeLoader sourceShapeLoader = DatabaseManager.GetShapeLoader(copy.ShapeIndex);
            ShapeLoader targetShapeLoader = DatabaseManager.GetShapeLoader(targetShapeIndex);
            try
            {
                sourceShapeLoader.Load();
                targetShapeLoader.Load();
                PageLoader pageLoader = DatabaseManager.GetPageLoader(sourceShapeLoader, smallIndex);
                int sourceLevel = pageLoader.Read(bigIndex);
                pageLoader.ExploreCubes(targetShapeLoader, sourceLevel);
            }
            finally
            {
                targetShapeLoader.Release();
                sourceShapeLoader.Release();
            }
            
        }

        public bool WriteLevel(int level)
        {
            Cube copy = new Cube(this);
            copy.Normalize();
            copy.Minimalize();
            int bigIndex;
            int smallIndex;
            copy.GetIndexes(out bigIndex, out smallIndex);
            ShapeLoader shapeLoader = DatabaseManager.GetShapeLoader(copy.ShapeIndex);
            try
            {
                shapeLoader.Load();
                PageLoader pageLoader = DatabaseManager.GetPageLoader(shapeLoader, smallIndex);
                return pageLoader.Write(bigIndex, level);
            }
            finally
            {
                shapeLoader.Release();
            }
        }

        public int ReadLevel()
        {
            return ReadLevel(true);
        }

        public int ReadLevel(bool minimalize)
        {
            Cube copy=new Cube(this);
            copy.Normalize();
            if (minimalize)
                copy.Minimalize();
            int bigIndex;
            int smallIndex;
            copy.GetIndexes(out bigIndex, out smallIndex);
            ShapeLoader shapeLoader = DatabaseManager.GetShapeLoader(copy.ShapeIndex);
            try
            {
                shapeLoader.Load();
                PageLoader pageLoader = DatabaseManager.GetPageLoader(shapeLoader, smallIndex);
                return pageLoader.Read(bigIndex);
            }
            finally
            {
                shapeLoader.Release();
            }
        }

        public Path FindWayHome()
        {
            Cube temp=new Cube(this);
            Path path=new Path(temp);
            while (!temp.EqualsSmallWise(Database.white))
            {
                SmartStep step = temp.FindStepHome();
                if (step == null)
                    break;
                path.Add(step);
                step.DoAction(temp);
            }
            int t = temp.PositionOf(Piece.BTSS_1) - 1;
            int b = temp.PositionOf(Piece.BBSS_9) - 13;
            Correction correction;
            if (t >= 12)
            {
                Cube flip = new Cube(temp);
                flip.Flip();
                t = flip.PositionOf(Piece.BTSS_1) - 1;
                b = flip.PositionOf(Piece.BBSS_9) - 13;
                correction = new Correction((24 - t) % 12, (24 - b) % 12);
                correction.Flip = true;
                correction.DoAction(temp);
            }
            else
            {
                correction = new Correction((24 - t) % 12, (24 - b) % 12);
                correction.DoAction(temp);
            }

            if (path.Count > 0)
            {
                SmartStep step = path[path.Count - 1];
                if (step.Correction == null)
                    step.Correction = correction;
                else
                {
                    step.Correction = step.Correction + correction;
                }
            }
            else if (correction.TopShift!=0 || correction.BotShift!=0 || correction.Flip)
            {
                path.Add(new SmartStep(null, correction));
            }
            path.Compress();
            return path;
        }

        public SmartStep FindStepHome()
        {
            List<SmartStep> steps = FindSteps(false, true);
            if (steps == null || steps.Count == 0)
                return null;
            return steps[0];
        }

        public SmartStep FindStepAway()
        {
            List<SmartStep> steps = FindSteps(true, true);
            if (steps == null || steps.Count == 0)
                return null;
            return steps[0];
        }

        public SmartStep FindRandomStep(Random random, bool away)
        {
            List<SmartStep> steps = FindSteps(away, false);
            if (steps == null || steps.Count == 0)
                return null;
            return steps[random.Next(steps.Count)];
        }

        public SmartStep GetRandomStep(Random random)
        {
            Cube normal = new Cube(this);
            Correction normalize = normal.Normalize();
            Correction minimalize = normal.Minimalize();
            SmartStep nextStep = NormalShape.NextSteps[random.Next(NormalShape.NextSteps.Count - 1)];
            SmartStep res = normalize + minimalize + nextStep;
#if DEBUG
            Cube test = new Cube(this);
            nextStep.DoAction(normal);
            res.DoAction(test);
            if (!test.Equals(normal))
            {
                SmartStep res2 = normalize + minimalize + nextStep;
                throw new InvalidProgramCubeException();
            }
#endif 
            return res;
        }

        public List<SmartStep> FindSteps(bool away, bool first)
        {
            Cube normal=new Cube(this);
            Correction normalize = normal.Normalize();
            Correction minimalize = normal.Minimalize();

            int currentLevel = normal.ReadLevel();
            if (!away && currentLevel == 1)
            {
                return null;
            }
            if (away && currentLevel == 12)
            {
                return null;
            }
            List<SmartStep> solutions=new List<SmartStep>();
            foreach (SmartStep nextStep in NormalShape.NextSteps)
            {
                Cube candidate = new Cube(normal);
                nextStep.DoAction(candidate);
                int newLevel = candidate.ReadLevel();
                if (newLevel==0)
                {
                    if (Database.instance.IsExplored)
                        throw new InvalidProgramCubeException();
                }
                else
                {
                    if ((away && newLevel > currentLevel) ||
                        (!away && newLevel < currentLevel))
                    {
                        SmartStep step = normalize + minimalize + nextStep;
#if DEBUG
                        Cube test=new Cube(this);
                        step.DoAction(test);
                        if (!test.Equals(candidate))
                        {
                            SmartStep step2 = normalize + minimalize + nextStep;
                            Cube test2 = new Cube(this);
                            step2.DoAction(test2);
                            throw new InvalidProgramCubeException();
                        }
#endif

                        solutions.Add(step);
                        if (first)
                            return solutions;
                    }
                }
            }
            return solutions;
        }

        #endregion

    }
}