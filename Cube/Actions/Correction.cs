// This file is part of project Cube21
// Whole solution including its LGPL license could be found at
// http://cube21.sf.net/
// 2007 Pavel Savara, http://zamboch.blogspot.com/

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Zamboch.Cube21.Actions
{
    [DataContract]
    public class Correction : Action
    {
        public Correction()
        {
            Flip = false;
        }

        public Correction(Action source)
            : base(source)
        {
            Flip = false;
        }

        public Correction(Correction source)
            : base(source)
        {
            Flip = source.Flip;
        }

        public Correction(int topShift, int botShift)
            : base(topShift, botShift)
        {
            Flip = false;
        }

        public Correction(bool flip, int topShift, int botShift)
            : base(topShift, botShift)
        {
            Flip = flip;
        }

        public override IAction Copy()
        {
            return new Correction(this);
        }

        public void Invert()
        {
            if (!Flip)
            {
                TopShift = (144 - TopShift) % 12;
                BotShift = (144 - BotShift) % 12;
            }
            else
            {
                int oldTop = TopShift;
                int oldBot = BotShift;
                TopShift = (oldBot) % 12;
                BotShift = (oldTop) % 12;
            }
        }

        // turn after shifts
        [XmlAttribute()]
        [DataMember]
        public bool Flip;

        public override void DoAction(Cube cube)
        {
            if (Flip)
                cube.Flip();
            base.DoAction(cube);
        }

        public override void DumpAction(Cube exampleCube, string cubeName, TextWriter tw)
        {
            if (Flip)
                tw.WriteLine(@"                {0}Flip();", cubeName);
            base.DumpAction(exampleCube, cubeName, tw);
        }

        public override void UndoAction(Cube cube)
        {
            base.UndoAction(cube);
            if (Flip)
                cube.Flip();
        }

        public override string ToString()
        {
            if (Flip)
                return '!' + base.ToString();
            else
                return base.ToString();
        }

        public override string ToStringEx()
        {
            if (Flip)
                return 'F' + base.ToStringEx();
            else
                return base.ToStringEx();
        }

        public static Correction operator +(Correction c1, Correction c2)
        {
            if (c1 == null) return c2;
            if (c2 == null) return c1;

            Correction correction;
            if (!c1.Flip)
            {
                if (!c2.Flip)
                {
                    correction = new Correction((c1.TopShift + c2.TopShift) % 12, (c1.BotShift + c2.BotShift) % 12);
                }
                else
                {
                    correction = new Correction((12 - c1.BotShift + c2.TopShift) % 12, (12 - c1.TopShift + c2.BotShift) % 12);
                    correction.Flip = true;
                }
            }
            else
            {
                if (!c2.Flip)
                {
                    correction = new Correction((c1.TopShift + c2.TopShift) % 12, (c1.BotShift + c2.BotShift) % 12);
                    correction.Flip = true;
                }
                else
                {
                    correction = new Correction((12 - c1.BotShift + c2.TopShift) % 12, (12 - c1.TopShift + c2.BotShift) % 12);
                }
            }
            return correction;
        }

        public static SmartStep operator +(Step s1, Correction c2)
        {
            return new SmartStep(s1, c2);
        }

        public static SmartStep operator +(Correction c1, Step s2)
        {
            if (c1==null)
                return new SmartStep(s2, null);
            if (s2==null)
                throw new ArgumentException();
            if (!c1.Flip)
            {
                int top = (s2.TopShift + c1.TopShift) % 12;
                int bot = (s2.BotShift + c1.BotShift) % 12;
                Step s=new Step(top, bot);
                return new SmartStep(s, null);
            }
            else
            {
                int top = (12 + c1.TopShift - s2.BotShift) % 12;
                int bot = (12 + c1.BotShift - s2.TopShift) % 12;
                Step s = new Step(top, bot);
                Correction c=new Correction();
                c.Flip = true;
                return new SmartStep(s, c);
            }
        }

        public static SmartStep operator +(Correction c1, SmartStep s2)
        {
            if (c1 == null)
                return new SmartStep(s2);
            if (s2 == null)
                throw new ArgumentException();

            if (!c1.Flip)
            {
                int top = (12 + c1.TopShift + s2.Step.TopShift) % 12;
                int bot = (12 + c1.BotShift + s2.Step.BotShift) % 12;
                Step s = new Step(top, bot);
                return new SmartStep(s, s2.Correction);
            }
            else
            {
                int top = (24 - c1.BotShift - s2.Step.BotShift) % 12;
                int bot = (24 - c1.TopShift - s2.Step.TopShift) % 12;
                Step step = new Step(top, bot);
                Correction c;
                if (s2.Correction != null)
                    if (!s2.Correction.Flip)
                    {
                        c = new Correction((12 + s2.Correction.TopShift) % 12, (12 + s2.Correction.BotShift) % 12);
                        c.Flip = true;
                    }
                    else
                    {
                        c = new Correction((12 + s2.Correction.TopShift) % 12, (12 + s2.Correction.BotShift) % 12);
                        //c = new Correction((12 + s2.Correction.BotShift) % 12, (12 + s2.Correction.TopShift) % 12);
                        c.Flip = false;
                    }
                else
                {
                    c = new Correction();
                    c.Flip = true;
                }
                SmartStep result = new SmartStep(step, c);
                return result;
            }
        }

        public static SmartStep operator +(SmartStep s1, Correction c2)
        {
            if (s1 == null)
                throw new ArgumentException();
            if (c2 == null)
                return new SmartStep(s1);
            Correction c = s1.Correction + c2;
            return new SmartStep(s1.Step, c);
        }
    }
}