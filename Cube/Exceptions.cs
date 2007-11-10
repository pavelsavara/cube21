// This file is part of project Cube21
// Whole solution including its LGPL license could be found at
// http://cube21.sf.net/
// 2007 Pavel Savara, http://zamboch.blogspot.com/

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

    public class NonTunableCubeException : CubeException
    {
    }

    public class InvalidStepMergeCubeException : CubeException
    {
    }

    public class InvalidProgramCubeException : CubeException
    {
    }
}