using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Zamboch.Cube21.Actions;
using Zamboch.Cube21.Ranking;
using Zamboch.Cube21.Work;

namespace Zamboch.Cube21
{
    [XmlRoot("NSh")]
    public class NormalShape : Shape, IComparable
    {
        #region Data

        [XmlIgnore]
        public NormalShape Parent;

        [XmlIgnore]
        public ShapeLoader Loader;

        public int ParentShapeIndex = -1;

        public Step FromParentStep;
        public Cube ExampleCube;

        [XmlIgnore]
        public List<NormalShape> Childern = new List<NormalShape>();

        public List<RotatedShape> Rotations = new List<RotatedShape>();
        public List<Correction> Alternatives = new List<Correction>();
        public List<Page> Pages = new List<Page>();

        [XmlIgnore]
        public Dictionary<int, Page> SmallIndexToPages = new Dictionary<int, Page>();

        public List<SmartStep> NextSteps = new List<SmartStep>();
        public List<int> AllTargetShapeIndexes = new List<int>();

        public long[] LevelCounts = new long[15];
        public long[] LevelFillCounts = new long[15];
        [XmlAttribute]
        public long MissingCount = (SmallCubeRank.PermCount * BigCubeRank.PermCount);

        #endregion

        #region Properties

        [XmlIgnore]
        public List<NormalShape> AllTargetShapes
        {
            get
            {
                List<NormalShape> res = new List<NormalShape>();
                foreach (int targetShapeIndex in AllTargetShapeIndexes)
                {
                    res.Add(Database.GetShape(targetShapeIndex));
                }
                return res;
            }
        }

        public Page GetPage(int smallIndex)
        {
            return SmallIndexToPages[smallIndex];
        }

        public override bool IsNormal
        {
            get { return true; }
        }

        public string Name
        {
            get
            {
                string top = Enum.GetName(typeof(HalfShape), Database.halfShapeNormalizer[TopBits]).Substring(3);
                string bot = Enum.GetName(typeof(HalfShape), Database.halfShapeNormalizer[BotBits]).Substring(3);
                return "[" + top + " & " + bot + "]";
            }
        }

        #endregion

        #region Public Helpers

        public List<SmartStep> GetStepsToShape(NormalShape targetShape)
        {
            List<SmartStep> selectedSteps = new List<SmartStep>();
            foreach (SmartStep step in NextSteps)
            {
                if (step.TargetShapeIndex == targetShape.ShapeIndex)
                {
                    selectedSteps.Add(step);
                }
            }

            return selectedSteps;
        }

        public void RegisterLoaded()
        {
            Database.RegisterShape(this);
            if (ParentShapeIndex != -1)
            {
                Parent = Database.GetShape(ParentShapeIndex);
                Parent.Childern.Add(this);
            }
            foreach (RotatedShape rotation in Rotations)
            {
                rotation.NormalShape = this;
                Database.RegisterShape(rotation);
            }
        }

        public int CompareTo(object obj)
        {
            NormalShape n = (NormalShape)obj;
            return ShapeIndex.CompareTo(n.ShapeIndex);
        }

        public override string ToString()
        {
            return string.Format("{0:00} {1}", ShapeIndex, Name);
        }

        #endregion

        #region Half Shapes

        public void AddHalfShapes()
        {
            uint nbits = TopBits;
            uint bits = nbits;
            int pieces = TopPieces;
            for (int t = 0; t < pieces; t++)
            {
                AddHalf(bits, nbits);
                bits = ror(bits, pieces);
            }
            nbits = BotBits;
            bits = nbits;
            pieces = BotPieces;
            for (int t = 0; t < pieces; t++)
            {
                AddHalf(bits, nbits);
                bits = ror(bits, pieces);
            }
        }

        private static uint ror(uint val, int len)
        {
            uint mask = 0xFFFFFFFF >> 32 - len;
            return ((val >> 1) | (val << (len - 1))) & mask;
        }


        private static void AddHalf(uint bits, uint normalBits)
        {
            if (!Database.halfShapeNormalizer.ContainsKey(bits))
            {
                Database.halfShapeNormalizer.Add(bits, (HalfShape)normalBits);
            }
        }

        #endregion
    }
}