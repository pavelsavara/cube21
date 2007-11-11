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
    public class SmartStep : IAction
    {
        #region Data

        [XmlAttribute("Target")]
        [DataMember]
        public int TargetShapeIndex;

        [DataMember]
        public Step Step;
        [DataMember]
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
            if (Step!=null)
                Step.DoAction(cube);
            if (Correction != null)
                Correction.DoAction(cube);
        }

        public virtual void UndoAction(Cube cube)
        {
            if (Correction != null)
                Correction.UndoAction(cube);
            if (Step != null)
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