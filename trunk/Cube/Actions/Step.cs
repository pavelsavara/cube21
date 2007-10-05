using System;
using System.IO;

namespace Zamboch.Cube21.Actions
{
    [Serializable]
    public class Step : Action
    {
        #region Construction

        public Step()
        {
        }

        public Step(Action source)
            : base(source)
        {
        }

        public Step(Step source)
            : base(source)
        {
        }

        public Step(int topShift, int botShift)
            : base(topShift, botShift)
        {
        }

        public override IAction Copy()
        {
            return new Step(this);
        }

        #endregion

        #region Public methods

        public override void DoAction(Cube cube)
        {
            if (Reverse)
                cube.Turn();
            base.DoAction(cube);
            if (!Reverse)
                cube.Turn();
        }

        public override void DumpAction(Cube exampleCube, string cubeName, TextWriter tw)
        {
            if (Reverse)
                tw.WriteLine(@"                {0}Turn();", cubeName);
            base.DumpAction(exampleCube, cubeName, tw);
            if (!Reverse)
                tw.WriteLine(@"                {0}Turn();", cubeName);
        }

        public override void UndoAction(Cube cube)
        {
            if (!Reverse)
                cube.Turn();
            base.UndoAction(cube);
            if (Reverse)
                cube.Turn();
        }

        #endregion

        #region Helpers

        public override string ToString()
        {
            if (Reverse)
                return '/' + base.ToString();
            else
                return base.ToString() + '/';
        }

        public override string ToStringEx()
        {
            if (Reverse)
                return 'T' + base.ToStringEx();
            else
                return base.ToStringEx() + 'T';
        }

        #endregion
    }
}