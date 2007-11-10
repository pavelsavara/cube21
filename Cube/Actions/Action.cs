// This file is part of project Cube21
// Whole solution including its LGPL license could be found at
// http://cube21.sf.net/
// 2007 Pavel Savara, http://zamboch.blogspot.com/

using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Zamboch.Cube21.Actions
{
    /// <summary>
    /// Abstract base for actions
    /// </summary>
    [XmlInclude(typeof(Correction))]
    [XmlInclude(typeof(Step))]
    public abstract class Action : IAction
    {
        #region Data

        [XmlAttribute("Top")]
        public int TopShift;

        [XmlAttribute("Bot")]
        public int BotShift;

        #endregion

        #region Construction

        public Action()
        {
            TopShift = 0;
            BotShift = 0;
        }

        public Action(Action source)
        {
            TopShift = source.TopShift;
            BotShift = source.BotShift;
        }

        public Action(int topShift, int botShift)
        {
            TopShift = topShift;
            BotShift = botShift;
        }

        public abstract IAction Copy();

        #endregion

        #region Public methods

        public void Normalize()
        {
            TopShift = (TopShift + 144) % 12;
            BotShift = (BotShift + 144) % 12;
        }

        public virtual void DoAction(Cube cube)
        {
            cube.RotateTop(TopShift);
            cube.RotateBot(BotShift);
        }

        public virtual void UndoAction(Cube cube)
        {
            cube.RotateBot(12 - BotShift);
            cube.RotateTop(12 - TopShift);
        }

        #endregion

        #region Code Generator

        public virtual void DumpAction(Cube exampleCube, string cubeName, TextWriter tw)
        {
            tw.WriteLine(@"
                {2}RotateTop({0});
                {2}RotateBot({1});", TopShift, BotShift,
                         cubeName);
        }

        public virtual void DumpActionEx(Cube exampleCube, string prefix, TextWriter tw)
        {
            Cube target = new Cube(exampleCube);
            DoAction(target);
            DumpActionEx(exampleCube, target, prefix, tw);
        }

        public static void DumpActionEx(Cube source, Cube target, string prefix, TextWriter tw)
        {
            byte[] bigSource;
            byte[] smallSource;
            byte[] bigTarget;
            byte[] smallTarget;
            source.GetBytes(out bigSource, out smallSource);
            target.GetBytes(out bigTarget, out smallTarget);

            DumpPart(bigSource, bigTarget, "Big", prefix, tw);
            DumpPart(smallSource, smallTarget, "Small", prefix, tw);
        }

        private static void DumpPart(byte[] source, byte[] target, string bs, string prefix, TextWriter tw)
        {
            //tw.WriteLine();
            //tw.WriteLine(@"                //{2} {0} -> {1}", Dump(source), Dump(target), bs);
            byte lastshift = 0;
            for (byte shift = 0; shift < 8; shift++)
            {
                uint mask = 0;
                for (byte p = 0; p < 8; p++)
                {
                    mask <<= 4;
                    if (source[p] == target[p])
                    {
                        mask |= 0xF;
                    }
                }
                if (mask != 0)
                {
                    if (shift != lastshift)
                    {
                        tw.WriteLine(@"                asm_ror({0}{2}Source, {1});", prefix, (shift - lastshift) * 4, bs);
                    }
                    //tw.WriteLine(@"                //{0}", Dump(source));
                    tw.WriteLine(@"                {0}{2}Target|=({0}{2}Source&0x{1});", prefix, mask.ToString("X8"), bs);
                    lastshift = shift;
                }
                source = Rotate(1, source);
            }
        }

        public static byte[] Rotate(int shift, byte[] source)
        {
            shift = (shift + 8) % 8;
            if (shift == 0)
                return source;

            byte[] temp = new byte[8];
            int rest = 8 - shift;
            Array.Copy(source, 0, temp, shift, rest);
            Array.Copy(source, rest, temp, 0, shift);
            return temp;
        }

        public static string Dump(byte[] source)
        {
            StringBuilder sb = new StringBuilder();
            for (byte s = 0; s < 8; s++)
            {
                sb.Append(source[s]);
            }
            return sb.ToString();
        }

        #endregion

        #region Helpers

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(5);
            if (TopShift % 12 != 0 || BotShift % 12 != 0)
            {
                sb.Append('(');
                if (TopShift > 6)
                    sb.Append(TopShift - 12);
                else
                    sb.Append(TopShift);
                sb.Append(',');
                if (BotShift > 6)
                    sb.Append(BotShift - 12);
                else
                    sb.Append(BotShift);
                sb.Append(')');
            }
            return sb.ToString();
        }

        public virtual string ToStringEx()
        {
            StringBuilder sb = new StringBuilder(5);
            if (TopShift % 12 != 0 || BotShift % 12 != 0)
            {
                sb.Append(TopShift);
                sb.Append('x');
                sb.Append(BotShift);
            }
            return sb.ToString();
        }

        #endregion
    }
}