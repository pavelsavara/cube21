using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using Zamboch.Cube21;

namespace Viewer
{
    public class VisualCube
    {
        public VisualCube(Cube sourceCube)
        {
            Cube = new Cube(sourceCube);
            FrontModel = new ModelVisual3D();
            BackModel = new ModelVisual3D();
            CreateCube();
        }

        #region Public methods

        public void Flip()
        {
            if (OnBeforeAnimation != null)
                OnBeforeAnimation();
            Cube.Flip();
            MiddleLeft = !MiddleLeft;
            MiddleRight = !MiddleRight;
            AnimateTurnRight();
            AnimateTurnLeft();
        }


        public void Turn()
        {
            if (OnBeforeAnimation != null)
                OnBeforeAnimation();
            Cube.Turn();
            MiddleRight = !MiddleRight;
            AnimateTurnRight();
        }

        public void RotateNextBot()
        {
            if (OnBeforeAnimation != null)
                OnBeforeAnimation();
            int shift = Cube.RotateNextBot();
            AnimateBot(shift);
        }

        public void RotatePrevBot()
        {
            if (OnBeforeAnimation != null)
                OnBeforeAnimation();
            int shift = Cube.RotatePrevBot();
            AnimateBot(shift);
        }

        public void RotateNextTop()
        {
            if (OnBeforeAnimation != null)
                OnBeforeAnimation();
            int shift = Cube.RotateNextTop();
            AnimateTop(shift);
        }

        public void RotatePrevTop()
        {
            if (OnBeforeAnimation != null)
                OnBeforeAnimation();
            int shift = Cube.RotatePrevTop();
            AnimateTop(shift);
        }

        #endregion

        #region Helpers

        private void AnimateTurnRight()
        {
            for (int position = 0; position < 6; position++)
            {
                Piece piece = Cube.TopPieces[position];
                AnimateFlip(position, piece);
                if (PieceHelper.IsBig(piece))
                    position++;
            }
            for (int position = 0; position < 6; position++)
            {
                Piece piece = Cube.BotPieces[position];
                AnimateFlip(position+12, piece);
                if (PieceHelper.IsBig(piece))
                    position++;
            }
            AnimateFlip(MiddleRight ? 13 : 0, Piece.MIDS_G);
        }

        private void AnimateTurnLeft()
        {
            for (int position = 6; position < 12; position++)
            {
                Piece piece = Cube.TopPieces[position];
                AnimateFlip(position, piece);
                if (PieceHelper.IsBig(piece))
                    position++;
            }
            for (int position = 6; position < 12; position++)
            {
                Piece piece = Cube.BotPieces[position];
                AnimateFlip(position + 12, piece);
                if (PieceHelper.IsBig(piece))
                    position++;
            }
            AnimateFlip(MiddleLeft ? 19 : 6, Piece.MIDY_H);
        }

        private void AnimateFlip(int position, Piece piece)
        {
            AxisAngleRotation3D flip = flips[(int)piece];

            int rotAngle;
            int nextAngle;
            GetAngle(piece, position, out rotAngle, out nextAngle);
            double prevAngle = flip.Angle % 360;
            if (prevAngle ==0)
                prevAngle = 360;

            Duration duration = new Duration(new TimeSpan(0, 0, 0, 0, 300));
            DoubleAnimation flipAnimation = new DoubleAnimation(prevAngle, nextAngle, duration);
            flipAnimation.Completed += OnAnimationCompleted;
            movementsCount++;
            flip.BeginAnimation(AxisAngleRotation3D.AngleProperty, flipAnimation);
        }

        private void AnimateTop(int shift)
        {
            if (shift > 6)
                shift = shift-12;
            for (int position = 0; position < 12; position++)
            {
                Piece piece = Cube.TopPieces[position];
                AnimateRotation(shift, position, piece);

                if (PieceHelper.IsBig(piece))
                    position++;
            }
        }

        private void AnimateBot(int shift)
        {
            shift = 12 - shift;
            if (shift > 6)
                shift = shift - 12;
            for (int position = 0; position < 12; position++)
            {
                Piece piece = Cube.BotPieces[position];
                AnimateRotation(shift, position+12, piece);

                if (PieceHelper.IsBig(piece))
                    position++;
            }
        }

        private void AnimateRotation(int shift, int position, Piece piece)
        {
            AxisAngleRotation3D rotation = rotations[(int)piece];

            int flipAngle;
            int nextAngle;
            GetAngle(piece, position, out nextAngle, out flipAngle);
            double prevAngle = rotation.Angle % 360;
            if (prevAngle + shift*30 < 0 )
            {
                prevAngle += 360;
            }
            if (prevAngle + shift*30 >= 360)
            {
                nextAngle += 360;
            }

            Duration duration = new Duration(new TimeSpan(0, 0, 0, 0, Math.Abs(shift) * 90));
            DoubleAnimation rotationAnimation = new DoubleAnimation(prevAngle, nextAngle, duration);
            rotationAnimation.Completed += OnAnimationCompleted;
            movementsCount++;
            rotation.BeginAnimation(AxisAngleRotation3D.AngleProperty, rotationAnimation);
        }

        void OnAnimationCompleted(object sender, EventArgs e)
        {
            bool call;
            lock(this)
            {
                movementsCount--;
                call = (movementsCount == 0);
            }
            if (call && OnAfterAnimation!=null)
                OnAfterAnimation();
        }

        private void CreateCube()
        {
            content = new Model3DGroup();
            for (int position = 0; position < 12; position++)
            {
                Piece piece = Cube.TopPieces[position];
                content.Children.Add(CreatePiece(piece, position));
                if (PieceHelper.IsBig(piece))
                    position++;
            }
            for (int position = 0; position < 12; position++)
            {
                Piece piece = Cube.BotPieces[position];
                content.Children.Add(CreatePiece(piece, position + 12));
                if (PieceHelper.IsBig(piece))
                    position++;
            }
            content.Children.Add(CreatePiece(Piece.MIDS_G, MiddleRight ? 13 : 0));
            content.Children.Add(CreatePiece(Piece.MIDY_H, MiddleLeft ? 19 : 6));

            FrontModel.Content = content;
            BackModel.Content = content;
        }

        private Model3DGroup CreatePiece(Piece piece, int position)
        {
            int rotationAngle;
            int flipAngle;
            GetAngle(piece, position, out rotationAngle, out flipAngle);

            Model3DGroup pieceModel = new Model3DGroup();
            Transform3DGroup transformGroup = new Transform3DGroup();
            pieceModel.Transform = transformGroup;

            char pieceChar = (char)PieceHelper.ToChar(piece);
            if (PieceHelper.IsSmall(piece))
            {
                PiecesFactory.SmallFrame(pieceModel);
                PiecesFactory.SmallTop(pieceModel, PieceHelper.IsTopYellow(piece), pieceChar);
                PiecesFactory.SmallSide(pieceModel, PieceHelper.IsSideYellow(piece));
            }
            else if (PieceHelper.IsBig(piece))
            {
                PiecesFactory.BigFrame(pieceModel);
                PiecesFactory.BigTop(pieceModel, PieceHelper.IsTopYellow(piece), pieceChar);
                PiecesFactory.BigSideOne(pieceModel, PieceHelper.IsSideYellow(piece));
                PiecesFactory.BigSideTwo(pieceModel, PieceHelper.IsSide2Yellow(piece));
            }
            else if (PieceHelper.IsMiddle(piece))
            {
                bool mainYellow = PieceHelper.IsMiddleYellow(piece);
                PiecesFactory.MiddleFrame(pieceModel);
                PiecesFactory.MiddleMainSide(pieceModel, mainYellow);
                PiecesFactory.MiddleShortSide(pieceModel, mainYellow);
                PiecesFactory.MiddleLongSide(pieceModel, mainYellow);
            }
            else
            {
                throw new InvalidProgramException();
            }

            AxisAngleRotation3D rotation = new AxisAngleRotation3D(new Vector3D(0, 0, 1), rotationAngle);
            RotateTransform3D r1 = new RotateTransform3D(rotation);
            transformGroup.Children.Add(r1);
            rotations[(int)piece] = rotation;

            AxisAngleRotation3D flip = new AxisAngleRotation3D(new Vector3D(1, -cos75deg, 0), flipAngle);
            RotateTransform3D r2 = new RotateTransform3D(flip);
            transformGroup.Children.Add(r2);
            flips[(int)piece] = flip;

            return pieceModel;
        }

        //private const double cos75deg = 0.25881904510252074;
        private const double cos75deg = 0.27;//better ?

        private static void GetAngle(Piece piece, int position, out int rotationAngle, out int flipAngle)
        {
            bool top = (position < 12);
            int angle = (position%12) * 30;
            flipAngle = top ? 0 : 180;
            if (PieceHelper.IsSmall(piece))
            {
                if (top)
                    rotationAngle = (angle + 180);
                else
                    rotationAngle = (330 - angle);
            }
            else if (PieceHelper.IsBig(piece))
            {
                if (top)
                    rotationAngle = (angle + 240);
                else
                    rotationAngle = (360 - angle);
            }
            else if (PieceHelper.IsMiddle(piece))
            {
                rotationAngle = angle;
            }
            else
            {
                throw new InvalidProgramException();
            }
            rotationAngle %= 360;
        }

        #endregion

        #region Properties

        private readonly AxisAngleRotation3D[] rotations=new AxisAngleRotation3D[18];
        private readonly AxisAngleRotation3D[] flips = new AxisAngleRotation3D[18];
        private Model3DGroup content;
        private int movementsCount;

        public delegate void RefreshButtons();
        public event RefreshButtons OnAfterAnimation;
        public event RefreshButtons OnBeforeAnimation;

        public ModelVisual3D FrontModel;
        public ModelVisual3D BackModel;
        public Cube Cube;
        public bool MiddleRight;
        public bool MiddleLeft;
        public Storyboard Storyboard;

        #endregion
    }
}
