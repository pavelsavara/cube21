using System.Text;
using System.Xml.Serialization;

namespace Zamboch.Cube21
{
    [XmlInclude(typeof(NormalShape))]
    [XmlInclude(typeof(RotatedShape))]
    [XmlRoot("Sh")]
    public class Shape
    {
        #region Data

        [XmlAttribute("Bits")]
        public uint ShapeBits;

        [XmlAttribute("Index")]
        public int ShapeIndex;

        public int Level;

        #endregion

        #region Properties

        public virtual bool IsNormal
        {
            get { return false; }
        }

        public uint TopBits
        {
            get { return (ShapeBits & 0xFFF000) >> 12; }
        }

        public uint BotBits
        {
            get { return (ShapeBits & 0x000FFF); }
        }

        public int TopPieces
        {
            get { return (int)(ShapeBits & 0xF000000) >> 24; }
        }

        public int BotPieces
        {
            get { return 16 - (int)((ShapeBits & 0xF000000) >> 24); }
        }

        #endregion

        #region Helpers

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(18);
            if (IsNormal)
                sb.Append(ShapeIndex.ToString("00"));
            else
                sb.Append(ShapeIndex.ToString("0000"));
            sb.Append(" ");
            uint top = TopBits;
            for (int t = TopPieces - 1; t >= 0; t--)
            {
                if (((top >> t) & 0x1) == 1)
                {
                    sb.Append('c');
                }
                else
                {
                    sb.Append('e');
                }
            }
            sb.Append('/');
            uint bot = BotBits;
            for (int b = BotPieces - 1; b >= 0; b--)
            {
                if (((bot >> b) & 0x1) == 1)
                {
                    sb.Append('c');
                }
                else
                {
                    sb.Append('e');
                }
            }
            return sb.ToString();
        }

        #endregion
    }
}