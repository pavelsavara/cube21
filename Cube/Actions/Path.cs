using System;
using System.Collections.Generic;
using System.Text;

namespace Zamboch.Cube21.Actions
{
    public class Path : List<IAction>
    {
        public Path()
        {
        }

        public Path Invert()
        {
            throw new NotImplementedException();
        }

        public void Compress()
        {
            if (Count<2) return;

            Path n = new Path();
            SmartStep prev = (SmartStep)this[0];
            for (int i = 1; i < Count; i++)
            {
                SmartStep curr = (SmartStep)this[i];
                SmartStep prod = prev.Correction + curr;
                prev.Correction = null;
                n.Add(prev);
                prev = prod;
            }
            n.Add(prev);
            Clear();
            AddRange(n);
        }

        public Path(Action action)
        {
            Add(action);
        }

        public Path(Action action1, Action action2)
        {
            Add(action1);
            Add(action2);
        }

        public void DoActions(Cube cube)
        {
            for (int i = 0; i < Count; i++)
            {
                this[i].DoAction(cube);
            }
        }

        public void UndoActions(Cube cube)
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                this[i].UndoAction(cube);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (IAction step in this)
            {
                sb.Append(step.ToString());
            }
            return sb.ToString();
        }
    }
}