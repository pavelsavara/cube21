using System.Windows;
using System.Windows.Media.Media3D;
using _3DTools;
using Zamboch.Cube21;

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
        private Cube cube=new Cube();
        private ModelVisual3D mainModel;
        private ModelVisual3D backModel;

        private void InitCube()
        {
            mainViewport.Children.Remove(mainModel);
            mainModel = new ModelVisual3D();
            mainModel.Content = PiecesFactory.CreateCube(cube);
            mainModel.Transform = TrackBall.Transform;
            mainViewport.Children.Add(mainModel);

            backViewport.Children.Remove(backModel);
            backModel = new ModelVisual3D();
            backModel.Content = PiecesFactory.CreateCube(cube);
            backModel.Transform = TrackBall.Transform;
            backViewport.Children.Add(backModel);

            TrackBall.EventSource = CaptureBorder;
        }


        private void buttonTopReset_Click(object sender, RoutedEventArgs e)
        {
            TrackBall.Reset();
        }


        private void buttonBottomReset_Click(object sender, RoutedEventArgs e)
        {
            TrackBall.Reset();
        }

        private void buttonFlip_Click(object sender, RoutedEventArgs e)
        {
            cube.Flip();
            InitCube();
        }

        private void buttonTurn_Click(object sender, RoutedEventArgs e)
        {
            cube.Turn();
            InitCube();
        }

        private void buttonBottomRight_Click(object sender, RoutedEventArgs e)
        {
            cube.RotateNextBot();
            InitCube();
        }

        private void topTopRight_Click(object sender, RoutedEventArgs e)
        {
            cube.RotateNextTop();
            InitCube();
        }

        private void buttonBottomLeft_Click(object sender, RoutedEventArgs e)
        {
            cube.RotatePrevBot();
            InitCube();
        }

        private void buttonTopLeft_Click(object sender, RoutedEventArgs e)
        {
            cube.RotatePrevTop();
            InitCube();
        }
    }
}
