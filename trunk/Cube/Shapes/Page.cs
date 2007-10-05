using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using Zamboch.Cube21.Actions;
using Zamboch.Cube21.Ranking;

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

        public int[] LevelCounts = new int[14];

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

        public virtual bool IsLoaded
        {
            get { return true; }
        }

        #endregion

        #region Expanding to next level

        public virtual byte Touch()
        {
            //void implementation
            return 0;
        }
        
        public virtual void ExpandCubes(NormalShape targetShape, int sourceLevel)
        {
            Page[] tgpages = new Page[SmallCubeRank.PermCount];
#if DEBUG
            if (!IsLoaded)
                throw new InvalidProgramException();
#endif

            List<SmartStep> steps = Shape.GetStepsToShape(targetShape);
            int address = -1;
            address = GetNextAddress(address, sourceLevel);
            while (address != -1)
            {
                Cube sourceCube = new Cube(Shape, SmallIndex, address);
#if DEBUG
                sourceCube.CheckFlipable();
#endif
                foreach (SmartStep step in steps)
                {
                    Cube targetCube;
                    int bigIndex;
                    int smallIndex;

                    targetCube = new Cube(sourceCube);
                    step.DoAction(targetCube);
#if DEBUG
                    targetCube.CheckFlipable();

                    if (!targetCube.IsNormalShape)
                        throw new NonNormalizedCubeException();
                    if (targetCube.Shape != targetShape)
                        throw new InvalidShapeException();
#endif

                    //minimalize alternatives
                    GetMinimalCube(out bigIndex, out smallIndex, targetCube, targetShape);

                    //save
                    WriteTarget(bigIndex, smallIndex, sourceLevel, targetShape, tgpages);
                }
                address = GetNextAddress(address, sourceLevel);
            }
        }

        public static void WriteTarget(int bigIndex, int smallIndex, int sourceLevel, NormalShape targetShape,
                                       Page[] tgpages)
        {
            Page targetPage = tgpages[smallIndex];
            if (targetPage == null)
            {
                targetPage = targetShape.GetPage(smallIndex);
                tgpages[smallIndex] = targetPage;
            }
            targetPage.Write(bigIndex, sourceLevel + 1);
        }

        public static void GetMinimalCube(out int bigIndex, out int smallIndex, Cube targetCube, NormalShape targetShape)
        {
            targetCube.GetIndexes(out bigIndex, out smallIndex);
            foreach (Correction correction in targetShape.Alternatives)
            {
                Cube corr;
                int bigIndexCorr;
                int smallIndexCorr;
                corr = new Cube(targetCube);
                correction.DoAction(corr);

                corr.GetIndexes(out bigIndexCorr, out smallIndexCorr);

                if (smallIndexCorr < smallIndex || bigIndexCorr < bigIndex)
                {
                    bigIndex = bigIndexCorr;
                    smallIndex = smallIndexCorr;
                }
            }
        }

        #endregion

        #region Virtual file functions

        public virtual void UpdatePointer()
        {
            //void implementation
        }

        public virtual bool Write(int address, int level)
        {
            bool hi = ((address & 0x1) == 0x1);
            address >>= 1;
            byte d = ReadByte(address);
            if (hi)
            {
                if ((d & 0xF0) != 0x00)
                {
                    return false;
                }
                d |= (byte)(level << 4);
            }
            else
            {
                if ((d & 0x0F) != 0x00)
                {
                    return false;
                }
                d |= (byte)level;
            }
            WriteByte(address, d);
            return true;
        }

        public virtual byte Read(int address)
        {
            bool hi = ((address & 0x1) == 0x1);
            address >>= 1;
            byte d = ReadByte(address);

            if (hi)
            {
                return (byte)((d & 0xF0) >> 4);
            }
            else
            {
                return (byte)((d & 0x0F));
            }
        }

        private byte ReadByte(int address)
        {
            byte d;
            using (FileStream fs = OpenFile())
            {
                fs.Seek(address + (ShapeIndex * pageSize), SeekOrigin.Begin);
                d = (byte)fs.ReadByte();
            }
            return d;
        }

        private void WriteByte(int address, byte d)
        {
            using (FileStream fs = OpenFile())
            {
                fs.Seek(address + (ShapeIndex * pageSize), SeekOrigin.Begin);
                fs.WriteByte(d);
            }
        }

        int pageSize = BigCubeRank.PermCount / 2;

        private FileStream OpenFile()
        {
            bool exists = File.Exists(Shape.FileName);
            FileStream fs = new FileStream(Shape.FileName, FileMode.Create, FileAccess.Read, FileShare.Read);
            if (!exists)
            {
                fs.SetLength(SmallCubeRank.PermCount * pageSize);
            }
            return fs;
        }

        public virtual int GetNextAddress(int address, int level)
        {
            do
            {
                address++;
                if (address >= BigCubeRank.PermCount)
                    return -1;
                bool hi = ((address & 0x1) == 0x1);
                int ad = address >> 1;
                byte d = Read(ad);
                if (hi)
                {
                    d &= 0xF0;
                    d >>= 4;
                }
                else
                {
                    d &= 0x0F;
                }
                if (d == level)
                    return address;
            } while (true);
        }

        #endregion

        #region IComparable Members

        public int CompareTo(object obj)
        {
            Page p = (Page)obj;
            return PageFullID.CompareTo(p.PageFullID);
        }

        #endregion
    }
}