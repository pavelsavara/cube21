// This file is part of project Cube21
// Whole solution including its LGPL license could be found at
// http://cube21.sf.net/
// 2007 Pavel Savara, http://zamboch.blogspot.com/

using System;
using System.IO;

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
            get
            {
                return Path.Combine(DatabaseManager.DatabasePath, "Shape" + ShapeIndex.ToString("00") + ".data");
            }
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