// This file is part of project Cube21
// Whole solution including its LGPL license could be found at
// http://cube21.sf.net/
// 2007 Pavel Savara, http://zamboch.blogspot.com/

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Zamboch.Cube21.Actions
{
    public class Path : List<SmartStep>
    {
        public Cube From;

        public Path()
        {
        }

        public Path(IEnumerable<SmartStep> source)
        {
            AddRange(source);
        }

        public Path(Cube from)
        {
            From = new Cube(from);
        }

        //doesn't work well
        public void FlipMiddle(bool left, bool right)
        {
            if (Count == 0)
                return;
            int turns = Count;
            SmartStep last = this[Count - 1];
            if (last.Step == null)
                turns--;
            if (turns < 3)
                //not supported
                return;

            Step last3 = last.Step;
            Step last2 = this[Count - 2].Step;
            Step last1 = this[Count - 3].Step;

            if (last.Correction != null && last.Correction.Flip)
            {
                left = !left;
                right = !right;
            }
            if (!left && !right)
                // no problem
                return;
            if (!left && right)
            {
                /*
                last1.TopShift = (this[0].Step.TopShift + 6) % 12;
                last2.TopShift = (this[1].Step.TopShift + 6) % 12;
                last3.TopShift = (this[2].Step.TopShift + 6) % 12;
                 */
            }
            else if (left && right)
            {
                /*
                last1.TopShift = (this[0].Step.TopShift + 1) % 12;
                last2.TopShift = (this[1].Step.TopShift + 6) % 12;
                last2.BotShift = (this[1].Step.BotShift + 6) % 12;
                last3.TopShift = (this[2].Step.TopShift + 11) % 12;
                 */
            }
            else if (left && !right)
            {
                /*
                last1.TopShift = (this[0].Step.TopShift + 1) % 12;
                last2.BotShift = (this[1].Step.BotShift + 6) % 12;
                last3.TopShift = (this[1].Step.TopShift + 11) % 12;
                last3.BotShift = (this[1].Step.BotShift + 7) % 12;
                 */
            }
#if DEBUG
            if (From != null)
            {
                Cube test = new Cube(From);
                DoActions(test);
            }
#endif
        }

        public void Compress()
        {
            if (Count<2) return;

            Path n = new Path(From);
            SmartStep prev = this[0];
            for (int i = 1; i < Count; i++)
            {
                SmartStep curr = this[i];
                SmartStep prod = prev.Correction + curr;
                prev.Correction = null;
                n.Add(prev);
                prev = prod;
            }
            n.Add(prev);
            Clear();
            AddRange(n);
        }

        public Path(SmartStep action)
        {
            Add(action);
        }

        public Path(SmartStep action1, SmartStep action2)
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