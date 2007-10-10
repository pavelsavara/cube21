using System.IO;
using Zamboch.Cube21.Actions;

namespace Zamboch.Cube21.Work
{
    public class Generator
    {
        public static bool testing = false;

        public static void Dump()
        {
            StreamWriter sw = new StreamWriter(@"..\..\..\Engine\FastGenPage.cpp");
            Intro(sw);
            if (testing)
            {
                MinimizeAlternatives(Database.GetShape(0), sw);
                MinimizeAlternatives(Database.GetShape(1), sw);
            }
            else
            {
                foreach (NormalShape shape in Database.NormalShapes)
                {
                    MinimizeAlternatives(shape, sw);
                }
            }
            foreach (NormalShape sourceShape in Database.NormalShapes)
            {
                foreach (NormalShape targetShape in sourceShape.AllTargetShapes)
                {
                    foreach (SmartStep step in sourceShape.GetStepsToShape(targetShape))
                    {
                        ExploreStep(sourceShape, targetShape, step, sw);
                        if (testing) break;
                    }
                    ExploreShapeToShape(sourceShape, targetShape, sw);
                    if (testing) break;
                }
                if (testing) break;
            }

            foreach (NormalShape shape in Database.NormalShapes)
            {
                if (shape.Alternatives.Count > 0)
                {
                    FillAlternatives(shape, sw);
                    FillShape(shape, sw);
                }
                if (testing) break;
            }

            OutroIntroM(sw);

            DumpExploreIndex(sw);
            DumpFillIndex(sw);
            OutroM(sw);
            sw.Close();
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

        public static void MinimizeAlternatives(NormalShape sourceShape, TextWriter tw)
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
                tw.Write(@"                //Action {0}", correction);
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

        public static void FillAlternatives(NormalShape sourceShape, TextWriter tw)
        {
            tw.Write(
                @"
            void WriteAlternatedCubes{0}(unsigned int bigSource, unsigned int smallSource, int level, byte** dtpages, int* cntpages)",
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
                tw.Write(@"                //Action {0}", correction);
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

                tw.WriteLine(
                    @"
                lSaveFill(corrBigIndex, corrSmallIndex, {0}, level, dtpages, cntpages);", sourceShape.ShapeIndex);
                if (testing) break;
            }
            tw.WriteLine(@"            }");
        }

        private static void ExploreStep(NormalShape sourceShape, NormalShape targetShape, SmartStep step,
                                                 TextWriter tw)
        {
            tw.Write(
                @"
            void DoStep{0}To{1}St{2}(unsigned int targetBigSource, unsigned int targetSmallSource, unsigned int &targetBigTarget, unsigned int &targetSmallTarget, int &targetBigIndex, int &targetSmallIndex)",
                sourceShape.ShapeIndex, targetShape.ShapeIndex, step.ToStringEx());
            tw.Write(@"
            {
");

            tw.Write(@"                //Action {0}", step);
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

        private static void ExploreShapeToShape(NormalShape sourceShape, NormalShape targetShape, TextWriter tw)
        {
            tw.Write(@"            // Starting at level {0}", sourceShape.Level);
            tw.Write(
                @"
            void ExplorePage{0}To{1}(int sourceLevel, int sourceSmallIndex, byte* dataPtr, int* cntpages)",
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


        private static void FillShape(NormalShape sourceShape, TextWriter tw)
        {
            tw.Write(
                @"
            void FillPage{0}(int sourceSmallIndex, byte* dataPtr, int* cntpages)",
                sourceShape.ShapeIndex);
            tw.Write(
                @"
            {
				byte* dtpages[SmallPermCount];
				memset(dtpages, 0,4*SmallPermCount);

				unsigned int smallSource=FastSmallCubeRank::UnRankEx(sourceSmallIndex);

				for(int address=0;address<PageSize;address++)
				{
					byte b=dataPtr[address];
                    byte hiLevel=b>>4;
                    byte lowLevel=b&0xF;
                    if (lowLevel!=0)
                    {
					    int sourceBigIndex=address<<1;
				        unsigned int bigSource=FastBigCubeRank::UnRank(sourceBigIndex);
");
            tw.Write(@"
                        WriteAlternatedCubes{0}(bigSource, smallSource, hiLevel, dtpages, cntpages);", sourceShape.ShapeIndex);

            tw.Write(
                @"
                    }
                    if (hiLevel!=0)
                    {
					    int sourceBigIndex=(address<<1)+1;
				        unsigned int bigSource=FastBigCubeRank::UnRank(sourceBigIndex);
");
            tw.Write(@"
                        WriteAlternatedCubes{0}(bigSource, smallSource, lowLevel, dtpages, cntpages);", sourceShape.ShapeIndex);
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
                        //Action {0}", step);
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
    					lSaveExplore(targetBigIndex, targetSmallIndex, {0}, targetLevel, dtpages, cntpages);",
                    targetShape.ShapeIndex);
                    
                if (testing) break;
            }
        }

        public static void DumpExploreIndex(TextWriter tw)
        {
            tw.Write(
                @"
            void FastGenPage::ExplorePage(int sourceLevel, int sourceSmallIndex, int targetShapeIndex, byte* dataPtr, int* cntpages)");
            tw.Write(
                @"
            {
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
                                ExplorePage{0}To{1}(sourceLevel, sourceSmallIndex, dataPtr, cntpages);",
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

        public static void DumpFillIndex(TextWriter tw)
        {
            tw.Write(
                @"
            void FastGenPage::FillPage(int sourceSmallIndex, byte* dataPtr, int* cntpages)");
            tw.Write(
                @"
            {
                switch(ShapeIndex)
                {");
            foreach (NormalShape sourceShape in Database.NormalShapes)
            {
                if (sourceShape.Alternatives.Count > 0)
                {
                    tw.Write(@"
                    case {0}:", sourceShape.ShapeIndex);
                    tw.Write(@"
                        FillPage{0}(sourceSmallIndex, dataPtr, cntpages);",
                             sourceShape.ShapeIndex);
                    tw.Write(@"
                        break;");
                }
                if (testing) break;
            }
            tw.Write(@"
                }
            }");
        }
    }
}