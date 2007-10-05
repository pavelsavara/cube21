using System;

namespace Zamboch.Cube21
{
    public class CubeException : Exception
    {
    }

    public class InvalidCubeException : CubeException
    {
    }

    public class InvalidShapeException : CubeException
    {
    }

    public class NonNormalizedCubeException : CubeException
    {
    }

    public class NonFlipableCubeException : CubeException
    {
    }

    public class InvalidStepMergeCubeException : CubeException
    {
    }
}