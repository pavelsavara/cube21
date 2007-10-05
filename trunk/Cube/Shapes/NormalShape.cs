using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Zamboch.Cube21.Actions;

namespace Zamboch.Cube21
{
    [XmlRoot("NSh")]
    public class NormalShape : Shape, IComparable
    {
        #region Data

        [XmlIgnore]
        public NormalShape Parent;

        public int ParentShapeIndex = -1;

        public Step FromParentStep;
        public Cube ExampleCube;

        [XmlIgnore]
        public long LastTouch;

        [XmlIgnore]
        public List<NormalShape> Childern = new List<NormalShape>();

        public List<RotatedShape> Rotations = new List<RotatedShape>();
        public List<Correction> Alternatives = new List<Correction>();
        public List<Page> Pages = new List<Page>();

        [XmlIgnore]
        public Dictionary<int, Page> SmallIndexToPages = new Dictionary<int, Page>();

        public List<SmartStep> NextSteps = new List<SmartStep>();
        public List<int> AllTargetShapeIndexes = new List<int>();

        #endregion

        #region Properties

        [XmlIgnore]
        public List<NormalShape> AllTargetShapes
        {
            get
            {
                List<NormalShape> res = new List<NormalShape>();
                foreach (int targetShapeIndex in AllTargetShapeIndexes)
                {
                    res.Add(Database.GetShape(targetShapeIndex));
                }
                return res;
            }
        }

        public Page GetPage(int smallIndex)
        {
            return Database.GetPage(smallIndex, this);
        }

        public override bool IsNormal
        {
            get { return true; }
        }

        public virtual bool IsLoaded
        {
            get { return false; }
        }

        #endregion

        #region Abstract

        public virtual void Load()
        {
            //void implementation
        }

        public virtual void Close()
        {
            //void implementation
        }

        #endregion

        #region Public Helpers

        public List<SmartStep> GetStepsToShape(NormalShape targetShape)
        {
            List<SmartStep> selectedSteps = new List<SmartStep>();
            foreach (SmartStep step in NextSteps)
            {
                if (step.TargetShapeIndex == targetShape.ShapeIndex)
                {
                    selectedSteps.Add(step);
                }
            }

            return selectedSteps;
        }

        #endregion

        #region Xml Deserialization

        public void RegisterLoaded(RegisterShapeDelegate reg)
        {
            reg(this);
            if (ParentShapeIndex != -1)
            {
                Parent = Database.GetShape(ParentShapeIndex);
                Parent.Childern.Add(this);
            }
            foreach (RotatedShape rotation in Rotations)
            {
                rotation.NormalShape = this;
                reg(rotation);
            }
        }

        #endregion

        #region IComparable Members

        public int CompareTo(object obj)
        {
            NormalShape n = (NormalShape)obj;
            return ShapeIndex.CompareTo(n.ShapeIndex);
        }

        #endregion
    }
}