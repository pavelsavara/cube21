using System;

namespace Zamboch.Cube21.Work
{
    public class ShapeLoader
    {
        public ShapeLoader(NormalShape shape)
        {
            Shape = shape;
        }

        public NormalShape Shape;
        public long LastTouch;

        public int ShapeIndex
        {
            get { return Shape.ShapeIndex; }
        }

        public virtual String FileName
        {
            get { return "Cube\\Shape" + ShapeIndex.ToString("00") + ".data"; }
        }

        public virtual bool IsLoaded
        {
            get { return false; }
        }

        public virtual bool IsUsed
        {
            get { return false; }
        }

        public virtual bool Load()
        {
            //void implementation
            return false;
        }

        public virtual void Release()
        {
            //void implementation
        }

        public virtual bool Close()
        {
            //void implementation
            return false;
        }
    }
}