using System;
using System.Collections.Generic;
using System.IO;
using Zamboch.Cube21.Actions;
using Zamboch.Cube21.Ranking;

namespace Zamboch.Cube21.Work
{
    public class PageLoader
    {
        public PageLoader(Page page)
        {
            Page = page;
        }

        public Page Page;

        public NormalShape Shape
        {
            get { return Page.Shape; }
        }

        public ShapeLoader ShapeLoader
        {
            get { return Page.Shape.Loader; }
        }

        public int SmallIndex
        {
            get { return Page.SmallIndex; }
        }

        public int ShapeIndex
        {
            get { return Page.ShapeIndex; }
        }

        #region Expanding to next level

        public virtual byte Touch()
        {
            //void implementation
            return 0;
        }

        public virtual void FillGaps()
        {
            //void implementation
        }

        public virtual void ExploreCubes(ShapeLoader targetShape, int sourceLevel)
        {
            PageLoader[] tgpages = new PageLoader[SmallCubeRank.PermCount];
#if DEBUG
            if (!IsLoaded)
                throw new InvalidProgramException();
#endif

            List<SmartStep> steps = Shape.GetStepsToShape(targetShape.Shape);
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
                    if (targetCube.Shape != targetShape.Shape)
                        throw new InvalidShapeException();
#endif

                    //minimalize alternatives
                    GetMinimalCube(out bigIndex, out smallIndex, targetCube, targetShape.Shape);

                    //save
                    WriteTarget(bigIndex, smallIndex, sourceLevel, targetShape, tgpages);
                }
                address = GetNextAddress(address, sourceLevel);
            }
        }

        public static void WriteTarget(int bigIndex, int smallIndex, int sourceLevel, ShapeLoader targetShape,
                                       PageLoader[] tgpages)
        {
            PageLoader targetPage = tgpages[smallIndex];
            if (targetPage == null)
            {
                targetPage = DatabaseManager.GetPageLoader(targetShape, smallIndex);
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

        public virtual bool IsLoaded
        {
            get { return true; }
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
            Page.LevelCounts[level]++;
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
                fs.Seek(address + (SmallIndex * PageSize), SeekOrigin.Begin);
                d = (byte)fs.ReadByte();
            }
            return d;
        }

        private void WriteByte(int address, byte d)
        {
            using (FileStream fs = OpenFile())
            {
                fs.Seek(address + (SmallIndex * PageSize), SeekOrigin.Begin);
                fs.WriteByte(d);
            }
        }

        public const int PageSize = BigCubeRank.PermCount / 2;

        private FileStream OpenFile()
        {
            bool exists = File.Exists(ShapeLoader.FileName);
            FileStream fs =
                new FileStream(ShapeLoader.FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            if (!exists)
            {
                fs.SetLength(SmallCubeRank.PermCount * PageSize);
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
    }
}