using System.Xml.Serialization;
using Zamboch.Cube21.Actions;

namespace Zamboch.Cube21
{
    [XmlRoot("RSh")]
    public class RotatedShape : Shape
    {
        public override bool IsNormal
        {
            get { return false; }
        }

        [XmlIgnore]
        public NormalShape NormalShape;

        public Correction FromNormalStep;
        
        [XmlIgnore]
        public Correction ToNormalStep
        {
            get
            {
                Correction inversion = new Correction(FromNormalStep);
                inversion.Invert();
                return inversion;
            }
        }
    }
}