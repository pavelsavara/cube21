// This file is part of project Cube21
// Whole solution including its LGPL license could be found at
// http://cube21.sf.net/
// 2007 Pavel Savara, http://zamboch.blogspot.com/

using System.IO;

namespace Zamboch.Cube21.Actions
{
    public interface IAction
    {
        void DoAction(Cube cube);
        void DumpAction(Cube exampleCube, string cubeName, TextWriter tw);
        void DumpActionEx(Cube exampleCube, string prefix, TextWriter tw);
        void UndoAction(Cube cube);
        IAction Copy();
    }
}