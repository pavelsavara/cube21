using System;
using System.Text;

namespace Zamboch.Cube21
{
    public enum Piece : byte
    {
        /*
        Size = 0x1,
        SecondColour = 0x2,
        SideColour = 0x4,
        Top = 0x8,

        1842
        */
        STS0_0 = 0x0,
        BTSS_1 = 0x1,
        STS1_2 = 0x2,
        BTSY_3 = 0x3,
        STY0_4 = 0x4,
        BTYY_5 = 0x5,
        STY1_6 = 0x6,
        BTYS_7 = 0x7,

        SBS0_8 = 0x8,
        BBSS_9 = 0x9,
        SBS1_A = 0xA,
        BBSY_B = 0xB,
        SBY0_C = 0xC,
        BBYY_D = 0xD,
        SBY1_E = 0xE,
        BBYS_F = 0xF,

        MIDS_G = 0x10,
        MIDY_H = 0x11,
    }

    // selection of small pieces without first bit
    public enum SmallPiece : byte
    {
        STS0_0 = 0x0,
        STS1_2 = 0x1,
        STY0_4 = 0x2,
        STY1_6 = 0x3,
        SBS0_8 = 0x4,
        SBS1_A = 0x5,
        SBY0_C = 0x6,
        SBY1_E = 0x7,
    }

    // selection of big pieces without first bit
    public enum BigPiece : byte
    {
        BTSS_1 = 0x0,
        BTSY_3 = 0x1,
        BTYY_5 = 0x2,
        BTYS_7 = 0x3,
        BBSS_9 = 0x4,
        BBSY_B = 0x5,
        BBYY_D = 0x6,
        BBYS_F = 0x7,
    }

    public enum PieceChar
    {
        STS0 = '0',
        BTSS = '1',
        STS1 = '2',
        BTSY = '3',
        STY0 = '4',
        BTYY = '5',
        STY1 = '6',
        BTYS = '7',

        SBS0 = '8',
        BBSS = '9',
        SBS1 = 'A',
        BBSY = 'B',
        SBY0 = 'C',
        BBYY = 'D',
        SBY1 = 'E',
        BBYS = 'F',

        MIDS = 'G',
        MIDY = 'H',
    }

    public static class PieceHelper
    {
        private static readonly Piece[] toPiece;
        private static readonly PieceChar[] toChar;
        private static readonly byte[] toColor;

        static PieceHelper()
        {
            toPiece = new Piece['I'];
            toPiece['0'] = 0x0;
            toPiece['1'] = (Piece)0x1;
            toPiece['2'] = (Piece)0x2;
            toPiece['3'] = (Piece)0x3;
            toPiece['4'] = (Piece)0x4;
            toPiece['5'] = (Piece)0x5;
            toPiece['6'] = (Piece)0x6;
            toPiece['7'] = (Piece)0x7;
            toPiece['8'] = (Piece)0x8;
            toPiece['9'] = (Piece)0x9;
            toPiece['A'] = (Piece)0xA;
            toPiece['B'] = (Piece)0xB;
            toPiece['C'] = (Piece)0xC;
            toPiece['D'] = (Piece)0xD;
            toPiece['E'] = (Piece)0xE;
            toPiece['F'] = (Piece)0xF;
            toPiece['G'] = (Piece)0x10;
            toPiece['H'] = (Piece)0x11;

            toChar = new PieceChar[18];
            toChar[0x0] = (PieceChar)'0';
            toChar[0x1] = (PieceChar)'1';
            toChar[0x2] = (PieceChar)'2';
            toChar[0x3] = (PieceChar)'3';
            toChar[0x4] = (PieceChar)'4';
            toChar[0x5] = (PieceChar)'5';
            toChar[0x6] = (PieceChar)'6';
            toChar[0x7] = (PieceChar)'7';
            toChar[0x8] = (PieceChar)'8';
            toChar[0x9] = (PieceChar)'9';
            toChar[0xA] = (PieceChar)'A';
            toChar[0xB] = (PieceChar)'B';
            toChar[0xC] = (PieceChar)'C';
            toChar[0xD] = (PieceChar)'D';
            toChar[0xE] = (PieceChar)'E';
            toChar[0xF] = (PieceChar)'F';
            toChar[0x10] = (PieceChar)'G';
            toChar[0x11] = (PieceChar)'H';

            toColor = new byte[18];
            toColor[0x0] = 0x0;
            toColor[0x1] = 0x1;
            toColor[0x2] = 0x2;
            toColor[0x3] = 0x3;
            toColor[0x4] = 0x4;
            toColor[0x5] = 0x7;//
            toColor[0x6] = 0x6;
            toColor[0x7] = 0x5;//
            
            toColor[0x8] = 0x8;
            toColor[0x9] = 0x9;
            toColor[0xA] = 0xA;
            toColor[0xB] = 0xD;//
            toColor[0xC] = 0xC;
            toColor[0xD] = 0xF;//
            toColor[0xE] = 0xE;
            toColor[0xF] = 0xB;//

            toColor[0x10] = 0x10;
            toColor[0x11] = 0x11;
        }

        public static Piece ToPiece(PieceChar piece)
        {
            return toPiece[(int)piece];
        }

        public static PieceChar ToChar(Piece pieceChar)
        {
            return toChar[(int)pieceChar];
        }

        public static bool IsBig(Piece piece)
        {
            return (piece < Piece.MIDS_G) && ((int)piece & 0x1) == 0x1;
        }

        public static bool IsSmall(Piece piece)
        {
            return (piece < Piece.MIDS_G) && ((int)piece & 0x1) != 0x1;
        }

        public static bool IsMiddle(Piece piece)
        {
            return (piece >= Piece.MIDS_G);
        }

        public static bool IsMiddleYellow(Piece piece)
        {
            return ((int)piece & 0x1) == 0x1;
        }

        public static bool IsTopYellow(Piece piece)
        {
            return ((int)piece & 0x8) == 0x8;
        }

        public static bool IsSideYellow(Piece piece)
        {
            return (toColor[(int)piece] & 0x4) == 0x4;
        }

        public static bool IsSide2Yellow(Piece piece)
        {
            return (toColor[(int)piece] & 0x2) == 0x2;
        }

        public static void ToForm(string form, out Piece[] top, out Piece[] bot)
        {
            if (form.Length != 16)
                throw new ArgumentException();
            top = new Piece[12];
            bot = new Piece[12];
            int i = 0;
            bool[] check = new bool[16];
            for (int s = 0; s < 16; s++)
            {
                Piece p = ToPiece((PieceChar)form[s]);
                SetForm(ref i, p, top, bot);
                if (IsBig(p))
                {
                    SetForm(ref i, p, top, bot);
                }
                check[(int)p] = true;
            }
            for (int s = 0; s < 16; s++)
            {
                if (check[s] == false)
                    throw new ArgumentException();
            }
        }

        private static void SetForm(ref int i, Piece p, Piece[] top, Piece[] bot)
        {
            if (i < 12)
            {
                top[i] = p;
            }
            else
            {
                bot[i - 12] = p;
            }
            i++;
        }

        public static string ToString(Piece[] part)
        {
            if (part.Length != 12)
                throw new ArgumentException();
            StringBuilder sb = new StringBuilder(16);
            for (int i = 0; i < 12; i++)
            {
                Piece p = part[i];
                PieceChar c = ToChar(p);
                sb.Append((char)c);
                if (IsBig(p))
                    i++;
            }
            return sb.ToString();
        }

        public static void Decompress(uint smallb, uint bigb, Piece[] top, Piece[] bot, NormalShape shape)
        {
#if DEBUG
            int small = 8;
            int big = 8;
#endif
            uint topb = shape.TopBits;
            uint botb = shape.BotBits;

            for (int b = 11; b >= 0; b--)
            {
                if ((botb & 0x1) == 1)
                {
                    Piece p = (Piece)(((bigb & 0x7) << 1) | 0x01);
                    bigb >>= 4;
#if DEBUG
                    big--;
                    if (big < 0)
                        throw new InvalidCubeException();
#endif
                    bot[b] = p;
                    b--;
                    bot[b] = p;
                }
                else
                {
                    Piece p = (Piece)(((smallb & 0x7) << 1));
                    smallb >>= 4;
#if DEBUG
                    small--;
                    if (small < 0)
                        throw new InvalidCubeException();
#endif
                    bot[b] = p;
                }
                botb >>= 1;
            }
            for (int t = 11; t >= 0; t--)
            {
                if ((topb & 0x1) == 1)
                {
                    Piece p = (Piece)(((bigb & 0x7) << 1) | 0x01);
                    bigb >>= 4;
#if DEBUG
                    big--;
                    if (big < 0)
                        throw new InvalidCubeException();
#endif
                    top[t] = p;
                    t--;
                    top[t] = p;
                }
                else
                {
                    Piece p = (Piece)(((smallb & 0x7) << 1));
                    smallb >>= 4;
#if DEBUG
                    small--;
                    if (small < 0)
                        throw new InvalidCubeException();
#endif
                    top[t] = p;
                }
                topb >>= 1;
            }
        }

        public static void Compress(out byte[] small, out byte[] big, out uint smallb, out uint bigb, Piece[] top,
                                    Piece[] bot, NormalShape shape)
        {
            small = new byte[8];
            big = new byte[8];
            smallb = 0;
            bigb = 0;
            int s = 0;
            int b = 0;

            for (int i = 0; i < 12; i++)
            {
                Piece p = top[i];
                byte pi = (byte)((byte)p >> 1);
                if (IsBig(p))
                {
                    bigb <<= 4;
                    bigb |= ((uint)p >> 1);
                    big[b] = pi;
                    b++;
                    i++;
                }
                else
                {
                    smallb <<= 4;
                    smallb |= ((uint)p >> 1);
                    small[s] = pi;
                    s++;
                }
            }
            for (int i = 0; i < 12; i++)
            {
                Piece p = bot[i];
                byte pi = (byte)((byte)p >> 1);
                if (IsBig(p))
                {
                    bigb <<= 4;
                    bigb |= ((uint)p >> 1);
                    big[b] = pi;
                    b++;
                    i++;
                }
                else
                {
                    smallb <<= 4;
                    smallb |= ((uint)p >> 1);
                    small[s] = pi;
                    s++;
                }
            }
        }
    }
}