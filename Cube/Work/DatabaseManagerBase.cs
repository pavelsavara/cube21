// This file is part of project Cube21
// Whole solution including its LGPL license could be found at
// http://cube21.sf.net/
// 2007 Pavel Savara, http://zamboch.blogspot.com/

using System;
using System.Collections.Generic;
using System.IO;
using Zamboch.Cube21.Actions;

namespace Zamboch.Cube21.Work
{
    public class DatabaseManagerBase
    {
        #region Data

        public static Database Database;

        #endregion

        public virtual NormalShape CreateShape()
        {
            return new NormalShape();
        }

        public virtual Page CreatePage(int smallIndex, NormalShape shape)
        {
            return new Page(smallIndex, shape);
        }


        public virtual void Initialize()
        {
            if (Database.CanLoad())
            {
                Database = Database.Load(true);
                if (Database.normalShapes.Count != 90)
                    throw new FileLoadException();
            }
            else
            {
                if (Database == null)
                    Database = new Database();
                SearchShapes();
                AssignShapeIds();
                Database.normalShapes.Sort();
                SearchForSteps();
                Database.Save();
            }
            Database.DumpShapes();
        }


        private static void AssignShapeIds()
        {
            int shapeIndex = Database.normalShapes.Count;
            foreach (Shape shape in Database.shapeNormalizer.Values)
            {
                if (!shape.IsNormal)
                {
                    shape.ShapeIndex = shapeIndex;
                    shapeIndex++;
                }
                else
                {
                    NormalShape ns = (NormalShape)shape;
                    if (ns.Parent != null)
                        ns.ParentShapeIndex = ns.Parent.ShapeIndex;
                }
                Database.shapeIndexes.Add(shape.ShapeIndex, shape);
            }
        }

        private static void SearchForSteps()
        {
            foreach (NormalShape shape in Database.normalShapes)
            {
                List<Action> steps;
                Cube sourceCube = shape.ExampleCube;
                List<Cube> expansions = sourceCube.ExpandRotateTurn(out steps);
                Dictionary<string, Cube> unique = new Dictionary<string, Cube>();
                for (int i = 0; i < expansions.Count; i++)
                {
                    Cube expansion = expansions[i];
                    Correction norm = expansion.Normalize();
                    Step steprt = (Step)steps[i];
                    bool found = false;
                    string key;
                    key = expansion.ToString();
                    if (unique.ContainsKey(key))
                    {
                        found = true;
                    }
                    else
                    {
                        foreach (RotatedShape rotation in expansion.NormalShape.Rotations)
                        {
                            Cube rotated = new Cube(expansion);
                            rotation.FromNormalStep.DoAction(rotated);
                            key = rotated.ToString();
#if DEBUG
                            SmartStep testst = steprt + norm + rotation.FromNormalStep;
                            Cube test = new Cube(sourceCube);
                            testst.DoAction(test);
                            if (!test.Equals(rotated))
                                throw new InvalidProgramCubeException();
#endif
                            if (unique.ContainsKey(key))
                            {
                                found = true;
                                break;
                            }
                        }
                    }
                    if (!found)
                    {
                        unique.Add(key, expansion);
                        SmartStep step = new SmartStep(steprt, norm);
                        int targetShapeIndex = expansion.ShapeIndex;
                        step.TargetShapeIndex = targetShapeIndex;
                        shape.NextSteps.Add(step);
                        if (!shape.AllTargetShapeIndexes.Contains(targetShapeIndex))
                        {
                            shape.AllTargetShapeIndexes.Add(targetShapeIndex);
                        }
                    }
                }
                shape.AllTargetShapeIndexes.Sort();
            }
        }

        private static void SearchShapes()
        {
            Database.whiteShape = AddShape(Database.white, null, null);
            Database.whiteShape.Level = 1;
            Database.white.Shape = Database.whiteShape;

            Queue<Cube> queue = new Queue<Cube>();
            queue.Enqueue(Database.white);

            while (queue.Count > 0)
            {
                // do next steps
                SearchShapes(queue);
            }
        }

        private static void SearchShapes(Queue<Cube> queue)
        {
            Cube source = queue.Dequeue();
            List<Action> steps;
            List<Cube> expansions = source.ExpandRotateTurn(out steps);
            for (int i = 0; i < expansions.Count; i++)
            {
                Cube expansion = expansions[i];
                if (!Database.shapeNormalizer.ContainsKey(expansion.ComputeShapeBits()))
                {
                    Step step = (Step)steps[i];
                    expansion.Shape = AddShape(expansion, (NormalShape)source.Shape, step);
                    queue.Enqueue(expansion);
                }
            }
        }

        private static NormalShape AddShape(Cube cube, NormalShape parent, Step parentStep)
        {
            NormalShape normal = new NormalShape();
            normal.Parent = parent;
            if (parent != null)
            {
                normal.Level = parent.Level + 1;
                parent.Childern.Add(normal);
            }
            normal.FromParentStep = parentStep;
            normal.ShapeIndex = Database.normalShapes.Count;
            uint shapeBits = cube.ComputeShapeBits();
            normal.ShapeBits = shapeBits;
            Database.shapeNormalizer.Add(shapeBits, normal);
            Database.normalShapes.Add(normal);
            normal.ExampleCube = new Cube(cube);

            normal.AddHalfShapes();
            AddRotations(cube, normal);
            return normal;
        }

        private static void AddRotations(Cube cube, NormalShape normal)
        {
            //add all rotations
            List<Action> steps;
            //rotate and 
            List<Cube> corrections = cube.ExpandRotateFlip(out steps);
            for (int i = 1; i < corrections.Count; i++)
            {
                Cube correction = corrections[i];
                uint shapeBits = correction.ComputeShapeBits();
                Correction step = (Correction)steps[i];
#if DEBUG
                Correction inversion = new Correction(step);
                inversion.Invert();

                Cube test=new Cube(cube);
                step.DoAction(test);
                inversion.DoAction(test);
#endif

                if (!Database.shapeNormalizer.ContainsKey(shapeBits))
                {
                    RotatedShape rshape = new RotatedShape();
                    rshape.ShapeIndex = -1;
                    rshape.NormalShape = normal;
                    rshape.NormalShape = normal;
                    rshape.FromNormalStep = step;
                    normal.Rotations.Add(rshape);
                    rshape.ShapeBits = shapeBits;
                    Database.shapeNormalizer.Add(rshape.ShapeBits, rshape);
                }
                else
                {
                    Shape sh = Database.shapeNormalizer[shapeBits];
                    if (sh.IsNormal)
                    {
                        if (sh == normal)
                        {
                            normal.Alternatives.Add(step);
                        }
                        else
                        {
                            throw new InvalidProgramCubeException();
                        }
                    }
                }
            }
        }
    }
}