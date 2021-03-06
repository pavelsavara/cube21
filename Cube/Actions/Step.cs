// This file is part of project Cube21
// Whole solution including its LGPL license could be found at
// http://cube21.sf.net/
// 2007 Pavel Savara, http://zamboch.blogspot.com/

using System;
using System.IO;
using System.Runtime.Serialization;

namespace Zamboch.Cube21.Actions
{
    [Serializable]
    [DataContract]
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
            base.DoAction(cube);
            cube.Turn();
        }

        public override void DumpAction(Cube exampleCube, string cubeName, TextWriter tw)
        {
            base.DumpAction(exampleCube, cubeName, tw);
            tw.WriteLine(@"                {0}Turn();", cubeName);
        }

        public override void UndoAction(Cube cube)
        {
            base.UndoAction(cube);
            cube.Turn();
        }



        #endregion

        #region Helpers

        public override string ToString()
        {
            return base.ToString() + '/';
        }

        public override string ToStringEx()
        {
            return base.ToStringEx() + 'T';
        }

        #endregion
    }
}