namespace Zamboch.Cube21
{

    /// <summary>
    /// e is small cube, C is big one
    /// 0 is small cube, 1 is big one
    /// </summary>
    public enum HalfShape
    {
        HS6AllBig     = 0x03F, // 111111
        
        HS7BigOne     = 0x02F, // 0101111
        HS7BigZero    = 0x07C, // 1111100
        HS7BigTwo     = 0x037, // 0110111

        HS8Square     = 0x055, // 01010101
        HS8Mushroom   = 0x06C, // 01101100
        HS8Shield     = 0x039, // 11100100
        HS8RightFist  = 0x065, // 11001010
        HS8LeftPaw    = 0x047, // 11101000
        HS8Barrel     = 0x099, // 11001100
        HS8Scallop    = 0x03C, // 11110000
        HS8Kite       = 0x0A5, // 11010010
        HS8LeftFist   = 0x0A6, // 11010100
        HS8RightPaw   = 0x05C, // 11100010

        HS9BrokenSqare= 0x142, // 101000010
        HS9Triangle   = 0x049, // 001001001
        HS9Halves     = 0x01C, // 000011100
        HS9GreyLeft   = 0x04A, // 001001010 
        HS9GreyRight  = 0x045, // 001000101 
        HS9Symetry    = 0x08C, // 010001100
        HS9BlockLeft  = 0x106, // 100000110
        HS9BlockRight = 0x105, // 100000101
        HS9EvenLeft   = 0x019, // 000011001
        HS9EvenRight  = 0x109, // 100001001

        HSASmallZero  = 0x201, // 1000000001
        HSASmallOne   = 0x101, // 0100000001
        HSASmallTwo   = 0x102, // 0100000010
        HSASmallThree = 0x110, // 0100010000
        HSASmallFour  = 0x084, // 0010000100
    }
}
