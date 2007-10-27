using System.Windows;
using System.Windows.Media.Media3D;
using _3DTools;

namespace Viewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitCube();
        }

        private Trackball TrackBall = new Trackball();

        private void InitCube()
        {
            Model3DGroup group = new Model3DGroup();

            group.Children.Add(PiecesFactory.MiddlePiece(true, 0));
            group.Children.Add(PiecesFactory.MiddlePiece(false, 180));

            group.Children.Add(PiecesFactory.SmallPiece(false, true, 0, 0, '0'));
            group.Children.Add(PiecesFactory.BigPiece(false, true, true, 30, 0, '1'));
            group.Children.Add(PiecesFactory.SmallPiece(false, true, 90, 0, '2'));
            group.Children.Add(PiecesFactory.BigPiece(false, true, false, 120, 0, '3'));
            group.Children.Add(PiecesFactory.SmallPiece(false, false, 180, 0, '4'));
            group.Children.Add(PiecesFactory.BigPiece(false, false, false, 210, 0, '5'));
            group.Children.Add(PiecesFactory.SmallPiece(false, false, 270, 0, '6'));
            group.Children.Add(PiecesFactory.BigPiece(false, false, true, 300, 0, '7'));

            group.Children.Add(PiecesFactory.SmallPiece(true, false, 0, 2, '8'));
            group.Children.Add(PiecesFactory.BigPiece(true, false, true, 30, 2, '9'));
            group.Children.Add(PiecesFactory.SmallPiece(true, true, 90, 2, 'A'));
            group.Children.Add(PiecesFactory.BigPiece(true, true, true, 120, 2, 'B'));
            group.Children.Add(PiecesFactory.SmallPiece(true, true, 180, 2, 'C'));
            group.Children.Add(PiecesFactory.BigPiece(true, true, false, 210, 2, 'D'));
            group.Children.Add(PiecesFactory.SmallPiece(true, false, 270, 2, 'E'));
            group.Children.Add(PiecesFactory.BigPiece(true, false, false, 300, 2, 'F'));

            ModelVisual3D model = new ModelVisual3D();
            model.Content = group;
            model.Transform = TrackBall.Transform;
            TrackBall.EventSource = CaptureBorder;
            mainViewport.Children.Add(model);
        }
    }
}
