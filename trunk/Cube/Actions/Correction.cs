using System.IO;
using System.Xml.Serialization;

namespace Zamboch.Cube21.Actions
{
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

        public override IAction Copy()
        {
            return new Correction(this);
        }

        // turn after shifts
        [XmlAttribute()]
        public bool Flip;

        public override void DoAction(Cube cube)
        {
            if (Reverse && Flip)
                cube.Flip();
            base.DoAction(cube);
            if (!Reverse && Flip)
                cube.Flip();
        }

        public override void DumpAction(Cube exampleCube, string cubeName, TextWriter tw)
        {
            if (Reverse && Flip)
                tw.WriteLine(@"                {0}Flip();", cubeName);
            base.DumpAction(exampleCube, cubeName, tw);
            if (!Reverse && Flip)
                tw.WriteLine(@"                {0}Flip();", cubeName);
        }

        public override void UndoAction(Cube cube)
        {
            if (!Reverse && Flip)
                cube.Flip();
            base.UndoAction(cube);
            if (Reverse && Flip)
                cube.Flip();
        }

        public override string ToString()
        {
            if (Flip)
                if (!Reverse)
                    return base.ToString() + '!';
                else
                    return '!' + base.ToString();
            else
                return base.ToString();
        }

        public override string ToStringEx()
        {
            if (Flip)
                if (!Reverse)
                    return base.ToStringEx() + 'F';
                else
                    return 'F' + base.ToStringEx();
            else
                return base.ToStringEx();
        }
    }
}