using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;
using Zamboch.Cube21.Actions;
using Zamboch.Cube21.Ranking;
using Zamboch.Cube21.Work;

namespace Zamboch.Cube21
{
    public class Cube : IXmlSerializable
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

        public Piece[] TopPieces
        {
            get
            {
                return top;
            }
        }

        public Piece[] BotPieces
        {
            get
            {
                return bot;
            }
        }

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

        public int ShapeIndex
        {
            get { return Shape.ShapeIndex; }
        }

        public bool IsNormalShape
        {
            get { return Shape.IsNormal; }
        }

        public uint ShapeBits
        {
            get
            {
                if (shape == null)
                    InitShape();
                return shape.ShapeBits;
            }
        }

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

        public int NextTop
        {
            get { return GetNext(top); }
        }

        public int NextBot
        {
            get { return GetNext(bot); }
        }

        public int PrevTop
        {
            get { return GetPrev(top); }
        }

        public int PrevBot
        {
            get { return GetPrev(bot); }
        }

        #endregion

        #region Active Operations

        public Correction Normalize()
        {
            if (Shape.IsNormal)
                return null;
            RotatedShape rs = (RotatedShape)Shape;
            Correction correction = rs.FromNormalStep;
            Correction invert = (Correction)correction.Copy();
            invert.Invert();
            invert.DoAction(this);
#if DEBUG
            CheckFlipable();
#endif
            return invert;
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
                    throw new InvalidProgramException();
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
                throw new NonFlipableCubeException();
            TurnImlementation();
            shape = null;
        }

        /// <summary>
        /// Flip whole cube, both sides
        /// </summary>
        public void Flip()
        {
            if (!CheckTurn(0, top) || !CheckTurn(0, bot))
                throw new NonFlipableCubeException();
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


            List<Cube> outlist = new List<Cube>(rotations);

            //add flips
            for (int i = 0; i < rotations.Count; i++)
            {
                //upgrade action to correction
                actions[i] = new Correction(actions[i]);

                //copy cube and correction for flip
                Cube tu = new Cube(rotations[i]);
                Correction correction = new Correction(actions[i]);
                tu.Flip();
                correction.Flip = true;

                //addd to lists
                outlist.Add(tu);
                actions.Add(correction);
            }

            return outlist;
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
                throw new NonFlipableCubeException();
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
            if (!CheckTurn(shift, source))
                throw new NonFlipableCubeException();
            Piece[] temp = new Piece[12];
            int rest = 12 - shift;
            Array.Copy(source, 0, temp, shift, rest);
            Array.Copy(source, rest, temp, 0, shift);
            if (!CheckTurn(0, temp))
                throw new NonFlipableCubeException();
            return temp;
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return PieceHelper.ToString(top) + "-" + PieceHelper.ToString(bot);
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

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            reader.MoveToElement();
            string form = reader.ReadElementContentAsString();
            PieceHelper.ToForm(form, out top, out bot);
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            string form = PieceHelper.ToString(top) + PieceHelper.ToString(bot);
            writer.WriteString(form);
        }

        #endregion

        #region Database usage

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
            Cube copy=new Cube(this);
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
                return pageLoader.Read(bigIndex);
            }
            finally
            {
                shapeLoader.Release();
            }
        }

        public Cube FindStepHome()
        {
            //out SmartStep step
            Cube normal = new Cube(this);
            normal.Normalize();
            normal.Minimalize();
            int currentLevel = normal.ReadLevel();
            if (currentLevel==1)
            {
                //step = null;
                return this;
            }
            foreach (SmartStep nextStep in NormalShape.NextSteps)
            {
                Cube candidate = new Cube(normal);
                nextStep.DoAction(candidate);
                int newLevel = candidate.ReadLevel();
                if (newLevel<currentLevel)
                {
                    //step = nextStep;
                    return candidate;
                }
            }
            throw new InvalidProgramException();
        }

        public Cube FindStepAway()
        {
            Cube normal = new Cube(this);
            normal.Normalize();
            normal.Minimalize();
            //out SmartStep step
            int currentLevel = normal.ReadLevel();
            if (currentLevel == 12)
            {
                //step = null;
                return this;
            }
            foreach (SmartStep nextStep in NormalShape.NextSteps)
            {
                Cube candidate = new Cube(normal);
                nextStep.DoAction(candidate);
                int newLevel = candidate.ReadLevel();
                if (newLevel > currentLevel)
                {
                    //step = nextStep;
                    return candidate;
                }
            }
            // no way out
            //step = null;
            return null;
        }

        public Cube FindRandomStepAway()
        {
            Cube normal = new Cube(this);
            normal.Normalize();
            normal.Minimalize();
            //out SmartStep step
            int currentLevel = normal.ReadLevel();
            if (currentLevel == 12)
            {
                //step = null;
                return this;
            }
            List<Cube> candidates=new List<Cube>();
            //List<SmartStep> steps = new List<SmartStep>();
            foreach (SmartStep nextStep in NormalShape.NextSteps)
            {
                Cube candidate = new Cube(normal);
                nextStep.DoAction(candidate);
                int newLevel = candidate.ReadLevel();
                if (newLevel > currentLevel)
                {
                    //step = nextStep;
                    candidates.Add(candidate);
                }
            }
            if(candidates.Count>0)
            {
                Random r=new Random();
                return candidates[r.Next(candidates.Count)];
            }
            // no way out
            //step = null;
            return null;
        }

        #endregion
    }
}