using System.IO;
using Zamboch.Cube21.Actions;

namespace Zamboch.Cube21.Work
{
    public class Generator
    {
        public static bool testing = false;
        public static bool managed = false;

        public static void Dump()
        {
            StreamWriter sw = new StreamWriter(@"..\..\..\Engine\FastGenPage.cpp");
            Intro(sw);
            if (testing)
            {
                DumpAlternatives(Database.GetShape(0), sw);
                DumpAlternatives(Database.GetShape(1), sw);
            }
            else
            {
                foreach (NormalShape shape in Database.NormalShapes)
                {
                    DumpAlternatives(shape, sw);
                }
            }
            foreach (NormalShape shape in Database.NormalShapes)
            {
                DumpSteps(shape, sw);
                if (testing) break;
            }
            if (managed) OutroIntroM(sw);

            foreach (NormalShape shape in Database.NormalShapes)
            {
                DumpShape(shape, sw);
                if (testing) break;
            }

            if (!managed) OutroIntroM(sw);

            DumpIndex(sw);
            OutroM(sw);
            sw.Close();
        }

        public static void DumpShape(NormalShape sourceShape, TextWriter tw)
        {
            foreach (NormalShape targetShape in sourceShape.AllTargetShapes)
            {
                DumpShapeToShape(sourceShape, targetShape, tw);
                if (testing) break;
            }
        }

        public static void DumpSteps(NormalShape sourceShape, TextWriter tw)
        {
            foreach (NormalShape targetShape in sourceShape.AllTargetShapes)
            {
                foreach (SmartStep step in sourceShape.GetStepsToShape(targetShape))
                {
                    DumpShapeToShapeStep(sourceShape, targetShape, step, tw);
                    if (testing) break;
                }
                if (testing) break;
            }
        }

        private static void Intro(TextWriter tw)
        {
            tw.WriteLine("#include \"StdAfx.h\"");
            tw.WriteLine("#include \"FastGenPage.h\"");
            tw.WriteLine(
                @"

#pragma unmanaged

namespace Zamboch
{
	namespace Cube21
	{
		namespace Engine
		{");
        }

        private static void OutroIntroM(TextWriter tw)
        {
            tw.WriteLine(
                @"
		}
	}
}

#pragma managed

namespace Zamboch
{
	namespace Cube21
	{
		namespace Engine
		{
");
        }

        private static void OutroM(TextWriter tw)
        {
            tw.WriteLine(@"
		}
	}
}
");
        }

        public static void DumpAlternatives(NormalShape sourceShape, TextWriter tw)
        {
            if (sourceShape.Alternatives.Count == 0)
                return;

            tw.Write(
                @"
            void GetMinimalCube{0}(int &bigIndex, int &smallIndex, unsigned int bigSource, unsigned int smallSource)",
                sourceShape.ShapeIndex);
            tw.Write(
                @"
            {
				unsigned int corrBigSource;
				unsigned int corrSmallSource;
				unsigned int corrBigTarget;
				unsigned int corrSmallTarget;
				int corrBigIndex;
				int corrSmallIndex;

");
            foreach (Correction correction in sourceShape.Alternatives)
            {
                tw.Write(@"                //Actions {0}", correction.ToString());
                tw.WriteLine(
                    @"
                corrBigSource = bigSource;
                corrSmallSource = smallSource;
                corrBigTarget = 0;
                corrSmallTarget = 0;");
                correction.DumpActionEx(sourceShape.ExampleCube, "corr", tw);

                tw.WriteLine(
                    @"				corrBigIndex=FastBigCubeRank::Rank(corrBigTarget);
				corrSmallIndex=FastSmallCubeRank::RankEx(corrSmallTarget);");


                tw.WriteLine(@"                min_cube(corrBigIndex, bigIndex, corrSmallIndex, smallIndex);
");
                if (testing) break;
            }
            tw.WriteLine(@"            }");
        }

        private static void DumpShapeToShapeStep(NormalShape sourceShape, NormalShape targetShape, SmartStep step,
                                                 TextWriter tw)
        {
            tw.Write(
                @"
            void DoStep{0}To{1}St{2}(unsigned int targetBigSource, unsigned int targetSmallSource, unsigned int &targetBigTarget, unsigned int &targetSmallTarget, int &targetBigIndex, int &targetSmallIndex)",
                sourceShape.ShapeIndex, targetShape.ShapeIndex, step.ToStringEx());
            tw.Write(@"
            {
");

            tw.Write(@"                //Action {0}", step.ToString());
            tw.Write(@"
                targetBigTarget = 0;
                targetSmallTarget = 0;
");
            step.DumpActionEx(sourceShape.ExampleCube, "target", tw);

            tw.WriteLine(
                @"				targetBigIndex=FastBigCubeRank::Rank(targetBigTarget);
				targetSmallIndex=FastSmallCubeRank::RankEx(targetSmallTarget);");


            tw.WriteLine(@"            }
");
        }

        private static void DumpShapeToShape(NormalShape sourceShape, NormalShape targetShape, TextWriter tw)
        {
            tw.Write(@"            // At level {0}", sourceShape.Level);
            tw.Write(
                @"
            void ExpandPage{0}To{1}(int sourceLevel, int sourceSmallIndex, byte* dataPtr, int* cntpages)",
                sourceShape.ShapeIndex, targetShape.ShapeIndex);
            tw.Write(
                @"
            {
				byte* dtpages[SmallPermCount];
				memset(dtpages, 0,4*SmallPermCount);

				int targetLevel = sourceLevel+1;
				byte hiSourceLevel=sourceLevel<<4;
				byte lowSourceLevel=sourceLevel;
				unsigned int sourceSmall=FastSmallCubeRank::UnRankEx(sourceSmallIndex);

				for(int address=0;address<PageSize;address++)
				{
					byte b=dataPtr[address];
					if ((b&0x0F)==lowSourceLevel) //low
					{
						int sourceBigIndex=address<<1;
					    unsigned int sourceBig=FastBigCubeRank::UnRank(sourceBigIndex);

					    unsigned int targetBig;
					    unsigned int targetSmall;
					    int targetBigIndex;
                        int targetSmallIndex;
");
            DumpTurns(sourceShape, targetShape, tw);
            tw.Write(
                @"
					}
					if ((b&0xF0)==hiSourceLevel)
					{
						int sourceBigIndex=(address<<1)+1;
					    unsigned int sourceBig=FastBigCubeRank::UnRank(sourceBigIndex);

					    unsigned int targetBig;
					    unsigned int targetSmall;
					    int targetBigIndex;
                        int targetSmallIndex;
");
            DumpTurns(sourceShape, targetShape, tw);
            tw.WriteLine(@"
                    }
                }
            }");
        }

        private static void DumpTurns(NormalShape sourceShape, NormalShape targetShape, TextWriter tw)
        {
            foreach (SmartStep step in sourceShape.GetStepsToShape(targetShape))
            {
                tw.Write(@"
                        //Action {0}", step.ToString());
                tw.Write(
                    @"
                        DoStep{0}To{1}St{2}(sourceBig, sourceSmall, targetBig, targetSmall, targetBigIndex, targetSmallIndex);",
                    sourceShape.ShapeIndex, targetShape.ShapeIndex, step.ToStringEx());
                if (targetShape.Alternatives.Count != 0)
                    tw.Write(
                        @"
                        GetMinimalCube{0}(targetBigIndex, targetSmallIndex, targetBig, targetSmall);",
                        targetShape.ShapeIndex);
                tw.WriteLine(
                    @"
    					lSave(targetBigIndex, targetSmallIndex, {0}, targetLevel, dtpages, cntpages);",
                    targetShape.ShapeIndex);
                if (testing) break;
            }
        }

        public static void DumpIndex(TextWriter tw)
        {
            tw.Write(
                @"
            void FastGenPage::ExpandPage(int sourceLevel, int sourceSmallIndex, int targetShapeIndex, byte* dataPtr, int* cntpages)");
            tw.Write(
                @"
            {
				int targetShapeIndex = targetShape->ShapeIndex;

                switch(ShapeIndex)
                {");
            foreach (NormalShape sourceShape in Database.NormalShapes)
            {
                tw.Write(@"
                    case {0}:", sourceShape.ShapeIndex);
                tw.Write(@"
                        switch(targetShapeIndex)
                        {");
                foreach (NormalShape targetShape in sourceShape.AllTargetShapes)
                {
                    tw.Write(@"
                            case {0}:", targetShape.ShapeIndex);
                    tw.Write(
                        @"
                                ExpandPage{0}To{1}(sourceLevel, sourceSmallIndex, dataPtr, cntpages);",
                        sourceShape.ShapeIndex, targetShape.ShapeIndex);
                    tw.Write(@"
                                break;");
                    if (testing) break;
                }
                tw.Write(@"
                        }
                        break;");
                if (testing) break;
            }
            tw.Write(@"
                }
            }");
        }
    }
}