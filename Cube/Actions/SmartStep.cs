using System;
using System.IO;
using System.Xml.Serialization;

namespace Zamboch.Cube21.Actions
{
    public class SmartStep : IAction
    {
        #region Data

        [XmlAttribute("Target")]
        public int TargetShapeIndex;

        public Step Step;
        public Correction Correction;

        #endregion

        #region Construction

        public SmartStep()
        {
        }

        public SmartStep(SmartStep source)
        {
            TargetShapeIndex = source.TargetShapeIndex;
            Step = (Step)source.Step.Copy();
            if (source.Correction != null)
                Correction = (Correction)source.Correction.Copy();
        }

        public SmartStep(Step step, Correction correction)
        {
            Step = step;
            Correction = correction;
        }

        public virtual IAction Copy()
        {
            return new SmartStep(this);
        }

        #endregion

        #region Public methods

        public virtual void DoAction(Cube cube)
        {
            Step.DoAction(cube);
            if (Correction != null)
                Correction.DoAction(cube);
        }

        public virtual void UndoAction(Cube cube)
        {
            if (Correction != null)
                Correction.UndoAction(cube);
            Step.UndoAction(cube);
        }

        public virtual void Invert()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Helpers

        public override string ToString()
        {
            return Step + " " + Correction;
        }

        public virtual string ToStringEx()
        {
            if (Correction != null)
                return Step.ToStringEx() + Correction.ToStringEx();
            else
                return Step.ToStringEx();
        }

        #endregion

        #region Code generators

        public virtual void DumpAction(Cube exampleCube, string cubeName, TextWriter tw)
        {
            Step.DumpAction(exampleCube, cubeName, tw);
            if (Correction != null)
            {
                Correction.DumpAction(exampleCube, cubeName, tw);
            }
        }

        public virtual void DumpActionEx(Cube exampleCube, string prefix, TextWriter tw)
        {
            Cube target = new Cube(exampleCube);
            DoAction(target);
            Action.DumpActionEx(exampleCube, target, prefix, tw);
        }

        #endregion
    }
}