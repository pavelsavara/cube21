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
        void Invert();
    }
}