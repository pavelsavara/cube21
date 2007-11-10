// This file is part of project Cube21
// Whole solution including its LGPL license could be found at
// http://cube21.sf.net/
// 2007 Pavel Savara, http://zamboch.blogspot.com/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Zamboch.Cube21;

namespace Viewer
{
    public class PiecesFactory
    {
        #region Constants

        private const double o1 = 0.05;
        private const double o2 = 0.3;
        private const double os = 2.6;
        private const double ob = 1.2;

        private const double z0 = 3.33;//0.0;
        private const double z1 = 6.66;
        private const double z3 = z0 + z1;

        private const double y0 = 0.0;
        private const double y1 = 2.68;
        private const double y2 = 10;
        private static readonly Color white = Colors.White;
        private static readonly Color yellow = Colors.Yellow;
        private static readonly Color inside = Colors.Black;
        private static readonly Color names = Colors.DarkBlue;

        #endregion

        #region Small

        public static void SmallSide(Model3DGroup piece, bool sideYellow)
        {
            GeometryModel3D side = new GeometryModel3D();
            MeshGeometry3D geometry = new MeshGeometry3D();
            side.Geometry = geometry;
            if (sideYellow)
                side.Material = new DiffuseMaterial(new SolidColorBrush(yellow));
            else
                side.Material = new DiffuseMaterial(new SolidColorBrush(white));

            Point3D p1 = new Point3D(y1 - o2, y2, z0 + o2);
            Point3D p2 = new Point3D(-y1 + o2, y2, z0 + o2);
            Point3D q1 = new Point3D(y1 - o2, y2, z3 - o2);
            Point3D q2 = new Point3D(-y1 + o2, y2, z3 - o2);

            AddTriangle(geometry, p1, p2, q1);
            AddTriangle(geometry, q1, p2, q2);

            piece.Children.Add(side);
        }

        public static void SmallTop(Model3DGroup piece, bool topYellow, char name)
        {
            GeometryModel3D top = new GeometryModel3D();
            MeshGeometry3D geometry = new MeshGeometry3D();
            top.Geometry = geometry;

            Point3D p3 = new Point3D(y0, y0 + o2 * os, z3);
            Point3D p4 = new Point3D(y1 - o2, y2 - o2, z3);
            Point3D p5 = new Point3D(-y1 + o2, y2 - o2, z3);

            AddTriangle(geometry, p5, p3, p4);

            TextBlock textBlock = new TextBlock();
            textBlock.Foreground = new SolidColorBrush(names);
            if (topYellow)
                textBlock.Background = new SolidColorBrush(yellow);
            else
                textBlock.Background = new SolidColorBrush(white);

            textBlock.Text = "\n\n " + name + " ";
            textBlock.Height = 20;
            textBlock.FontSize = 5;
            VisualBrush brush = new VisualBrush();
            brush.Visual = textBlock;
            top.Material = new DiffuseMaterial(brush);

            geometry.TextureCoordinates.Add(new Point(1, 1));
            geometry.TextureCoordinates.Add(new Point(0.5, 0));
            geometry.TextureCoordinates.Add(new Point(0, 1));
            piece.Children.Add(top);
        }

        public static void SmallFrame(Model3DGroup piece)
        {
            GeometryModel3D box = new GeometryModel3D();
            MeshGeometry3D geometry = new MeshGeometry3D();
            box.Geometry = geometry;
            box.Material = new DiffuseMaterial(new SolidColorBrush(inside));

            Point3D p0 = new Point3D(+y0, y0, z0);
            Point3D p1 = new Point3D(+y1, y2 - o1, z0);
            Point3D p2 = new Point3D(-y1, y2 - o1, z0);
            Point3D q1 = new Point3D(+y0, y0, z3 - o1);
            Point3D q2 = new Point3D(+y1, y2 - o1, z3 - o1);
            Point3D q3 = new Point3D(-y1, y2 - o1, z3 - o1);

            AddTriangle(geometry, p0, p2, p1);
            AddTriangle(geometry, q3, q1, q2);
            AddTriangle(geometry, p2, p0, q3);
            AddTriangle(geometry, p0, q1, q3);
            AddTriangle(geometry, q1, p0, q2);
            AddTriangle(geometry, p0, p1, q2);
            AddTriangle(geometry, p1, p2, q2);
            AddTriangle(geometry, q2, p2, q3);
            piece.Children.Add(box);

        }

        #endregion

        #region Big

        public static void BigSideTwo(Model3DGroup piece, bool side2Yellow)
        {
            GeometryModel3D side2 = new GeometryModel3D();
            MeshGeometry3D geometry = new MeshGeometry3D();
            side2.Geometry = geometry;
            if (side2Yellow)
                side2.Material = new DiffuseMaterial(new SolidColorBrush(yellow));
            else
            {
                side2.Material = new DiffuseMaterial(new SolidColorBrush(white));
            }

            Point3D p2 = new Point3D(y1 + o2, y2, z0 + o2);
            Point3D p3 = new Point3D(y2 - o2, y2, z0 + o2);

            Point3D q2 = new Point3D(y1 + o2, y2, z3 - o2);
            Point3D q3 = new Point3D(y2 - o2, y2, z3 - o2);

            AddTriangle(geometry, q3, p3, p2);
            AddTriangle(geometry, q3, p2, q2);

            piece.Children.Add(side2);
        }

        public static void BigSideOne(Model3DGroup piece, bool side1Yellow)
        {
            GeometryModel3D side1 = new GeometryModel3D();
            MeshGeometry3D geometry = new MeshGeometry3D();
            side1.Geometry = geometry;
            if (side1Yellow)
                side1.Material = new DiffuseMaterial(new SolidColorBrush(yellow));
            else
                side1.Material = new DiffuseMaterial(new SolidColorBrush(white));

            Point3D p1 = new Point3D(y2, y1 + o2, z0 + o2);
            Point3D p3 = new Point3D(y2, y2 - o2, z0 + o2);

            Point3D q1 = new Point3D(y2, y1 + o2, z3 - o2);
            Point3D q3 = new Point3D(y2, y2 - o2, z3 - o2);

            AddTriangle(geometry, p3, q3, p1);
            AddTriangle(geometry, p1, q3, q1);

            piece.Children.Add(side1);
        }

        public static void BigTop(Model3DGroup piece, bool topYellow, char name)
        {
            GeometryModel3D top = new GeometryModel3D();
            MeshGeometry3D geometry = new MeshGeometry3D();
            top.Geometry = geometry;

            Point3D p0 = new Point3D(y0 + o2 * ob, y0 + o2 * ob, z3);
            Point3D p1 = new Point3D(y2 - o2, y1 + o2, z3);
            Point3D p2 = new Point3D(y1 + o2, y2 - o2, z3);
            Point3D p3 = new Point3D(y2 - o2, y2 - o2, z3);

            AddTriangle(geometry, p0, p1, p2);
            AddTriangle(geometry, p3, p2, p1);


            TextBlock textBlock = new TextBlock();
            textBlock.Foreground = new SolidColorBrush(names);
            if (topYellow)
                textBlock.Background = new SolidColorBrush(yellow);
            else
                textBlock.Background = new SolidColorBrush(white);

            textBlock.Text = "\n\n   " + name + "   ";
            //textBlock.Text = "A\nB\nC";
            textBlock.Height = 20;
            textBlock.FontSize = 4;
            VisualBrush brush = new VisualBrush();
            brush.Visual = textBlock;
            top.Material = new DiffuseMaterial(brush);

            geometry.TextureCoordinates.Add(new Point(0, 0));
            geometry.TextureCoordinates.Add(new Point(0, 0));
            geometry.TextureCoordinates.Add(new Point(0, 0));

            geometry.TextureCoordinates.Add(new Point(0.5, 0.7));
            geometry.TextureCoordinates.Add(new Point(1, 0.4));
            geometry.TextureCoordinates.Add(new Point(0, 0.4));


            piece.Children.Add(top);
        }

        public static void BigFrame(Model3DGroup piece)
        {
            GeometryModel3D box = new GeometryModel3D();
            MeshGeometry3D geometry = new MeshGeometry3D();
            box.Geometry = geometry;
            box.Material = new DiffuseMaterial(new SolidColorBrush(inside));

            Point3D p0 = new Point3D(0, 0, z0);
            Point3D p1 = new Point3D(y2 - o1, y1, z0);
            Point3D p2 = new Point3D(y1, y2 - o1, z0);
            Point3D p3 = new Point3D(y2 - o1, y2 - o1, z0);

            Point3D q0 = new Point3D(0, 0, z3 - o1);
            Point3D q1 = new Point3D(y2 - o1, y1, z3 - o1);
            Point3D q2 = new Point3D(y1, y2 - o1, z3 - o1);
            Point3D q3 = new Point3D(y2 - o1, y2 - o1, z3 - o1);

            AddTriangle(geometry, p1, p0, p3);
            AddTriangle(geometry, p3, p0, p2);
            AddTriangle(geometry, q0, q1, q3);
            AddTriangle(geometry, q0, q3, q2);
            AddTriangle(geometry, q0, p0, p1);
            AddTriangle(geometry, q0, p1, q1);
            AddTriangle(geometry, p0, q0, p2);
            AddTriangle(geometry, p2, q0, q2);
            AddTriangle(geometry, q3, p3, p2);
            AddTriangle(geometry, q3, p2, q2);
            AddTriangle(geometry, p3, q3, p1);
            AddTriangle(geometry, p1, q3, q1);

            piece.Children.Add(box);
        }

        #endregion

        #region MiddleRight

        public static void MiddleLongSide(Model3DGroup piece, bool mainYellow)
        {
            GeometryModel3D longSide = new GeometryModel3D();
            MeshGeometry3D geometry = new MeshGeometry3D();
            longSide.Geometry = geometry;
            if (mainYellow)
                longSide.Material = new DiffuseMaterial(new SolidColorBrush(yellow));
            else
            {
                longSide.Material = new DiffuseMaterial(new SolidColorBrush(white));
            }


            Point3D p1 = new Point3D(y2 - o2, -y2, z0 - o2);
            Point3D p4 = new Point3D(-y1 + o2, -y2, z0 - o2);

            Point3D q1 = new Point3D(y2 - o2, -y2, -z0 + o2);
            Point3D q4 = new Point3D(-y1 + o2, -y2, -z0 + o2);

            AddTriangle(geometry, p4, q4, p1);
            AddTriangle(geometry, p1, q4, q1);

            piece.Children.Add(longSide);
        }

        public static void MiddleShortSide(Model3DGroup piece, bool mainYellow)
        {
            GeometryModel3D shortSide = new GeometryModel3D();
            MeshGeometry3D geometry = new MeshGeometry3D();
            shortSide.Geometry = geometry;
            if (!mainYellow)
                shortSide.Material = new DiffuseMaterial(new SolidColorBrush(yellow));
            else
            {
                shortSide.Material = new DiffuseMaterial(new SolidColorBrush(white));
            }

            Point3D p2 = new Point3D(y1 + o2, y2, z0 - o2);
            Point3D p3 = new Point3D(y2 - o2, y2, z0 - o2);

            Point3D p6 = new Point3D(y1 + o2, y2, -z0 + o2);
            Point3D p7 = new Point3D(y2 - o2, y2, -z0 + o2);

            AddTriangle(geometry, p3, p7, p2);
            AddTriangle(geometry, p2, p7, p6);

            piece.Children.Add(shortSide);
        }

        public static void MiddleMainSide(Model3DGroup piece, bool mainYellow)
        {
            GeometryModel3D mainSide = new GeometryModel3D();
            MeshGeometry3D geometry = new MeshGeometry3D();
            mainSide.Geometry = geometry;
            if (mainYellow)
                mainSide.Material = new DiffuseMaterial(new SolidColorBrush(yellow));
            else
                mainSide.Material = new DiffuseMaterial(new SolidColorBrush(white));

            Point3D p1 = new Point3D(y2, -y2 + o2, z0 - o2);
            Point3D p3 = new Point3D(y2, y2 - o2, z0 - o2);

            Point3D q1 = new Point3D(y2, -y2 + o2, -z0 + o2);
            Point3D q3 = new Point3D(y2, y2 - o2, -z0 + o2);

            AddTriangle(geometry, q3, p3, p1);
            AddTriangle(geometry, q3, p1, q1);

            piece.Children.Add(mainSide);
        }

        public static void MiddleFrame(Model3DGroup piece)
        {
            GeometryModel3D box = new GeometryModel3D();
            MeshGeometry3D geometry = new MeshGeometry3D();
            box.Geometry = geometry;
            box.Material = new DiffuseMaterial(new SolidColorBrush(inside));

            Point3D p0 = new Point3D(0, 0, z0);
            Point3D p1 = new Point3D(y2 - o1, -y2 + o1, z0);
            Point3D p2 = new Point3D(y1, y2 - o1, z0);
            Point3D p3 = new Point3D(y2 - o1, y2 - o1, z0);
            Point3D p4 = new Point3D(-y1 + o1, -y2 + o1, z0);

            Point3D q0 = new Point3D(0, 0, -z0);
            Point3D q1 = new Point3D(y2 - o1, -y2 + o1, -z0);
            Point3D q2 = new Point3D(y1, y2 - o1, -z0);
            Point3D q3 = new Point3D(y2 - o1, y2 - o1, -z0);
            Point3D q4 = new Point3D(-y1 + o1, -y2 + o1, -z0);

            AddTriangle(geometry, p0, p1, p3);
            AddTriangle(geometry, p0, p3, p2);
            AddTriangle(geometry, q1, q0, q3);
            AddTriangle(geometry, q3, q0, q2);
            AddTriangle(geometry, p3, q3, p2);
            AddTriangle(geometry, p2, q3, q2);
            AddTriangle(geometry, q3, p3, p1);
            AddTriangle(geometry, q3, p1, q1);
            AddTriangle(geometry, q4, p4, p2);
            AddTriangle(geometry, q4, p2, q2);
            AddTriangle(geometry, p4, q4, p1);
            AddTriangle(geometry, p1, q4, q1);
            AddTriangle(geometry, p0, p4, p1);
            AddTriangle(geometry, q4, q0, q1);

            piece.Children.Add(box);
        }

        #endregion

        #region Helpers

        private static Vector3D CalculateNormal(Point3D p0, Point3D p1, Point3D p2)
        {
            Vector3D v0 = new Vector3D(
                p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
            Vector3D v1 = new Vector3D(
                p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
            return Vector3D.CrossProduct(v0, v1);
        }

        private static void AddTriangle(MeshGeometry3D geometry, Point3D p0, Point3D p1, Point3D p2)
        {
            int i0 = geometry.Positions.Count + 0;
            int i1 = geometry.Positions.Count + 1;
            int i2 = geometry.Positions.Count + 2;

            geometry.Positions.Add(p0);
            geometry.Positions.Add(p1);
            geometry.Positions.Add(p2);

            geometry.TriangleIndices.Add(i0);
            geometry.TriangleIndices.Add(i1);
            geometry.TriangleIndices.Add(i2);

            Vector3D normal = CalculateNormal(p0, p1, p2);
            geometry.Normals.Add(normal);
            geometry.Normals.Add(normal);
            geometry.Normals.Add(normal);
        }

        #endregion
    }
}
