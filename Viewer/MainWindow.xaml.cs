using System.Windows;
using System.Windows.Media.Animation;
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
            cube = new VisualCube(new Cube());
            mainViewport.Children.Add(cube.FrontModel);
            backViewport.Children.Add(cube.BackModel);
            cube.FrontModel.Transform = TrackBall.Transform;
            cube.BackModel.Transform = TrackBall.Transform;
            TrackBall.EventSource = CaptureBorder;
        }

        private Trackball TrackBall = new Trackball();
        private VisualCube cube;

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
        }

        private void buttonTurn_Click(object sender, RoutedEventArgs e)
        {
            cube.Turn();
        }

        private void buttonBottomRight_Click(object sender, RoutedEventArgs e)
        {
            cube.RotateNextBot();
        }

        private void topTopRight_Click(object sender, RoutedEventArgs e)
        {
            cube.RotateNextTop();
        }

        private void buttonBottomLeft_Click(object sender, RoutedEventArgs e)
        {
            cube.RotatePrevBot();
        }

        private void buttonTopLeft_Click(object sender, RoutedEventArgs e)
        {
            cube.RotatePrevTop();
        }
    }
}
