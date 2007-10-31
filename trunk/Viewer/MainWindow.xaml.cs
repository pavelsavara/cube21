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
            cube.OnBeforeAnimation += BeforeAnimation;
            cube.OnAfterAnimation += AfterAnimation;
            textBlockInfo.Text = cube.Cube.ToString();
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
            if (!buttonsEnabled) return;
            cube.Flip();
        }

        private void buttonTurn_Click(object sender, RoutedEventArgs e)
        {
            if (!buttonsEnabled) return;
            cube.Turn();
        }

        private void buttonBottomRight_Click(object sender, RoutedEventArgs e)
        {
            if (!buttonsEnabled) return;
            cube.RotateNextBot();
        }

        private void topTopRight_Click(object sender, RoutedEventArgs e)
        {
            if (!buttonsEnabled) return;
            cube.RotateNextTop();
        }

        private void buttonBottomLeft_Click(object sender, RoutedEventArgs e)
        {
            if (!buttonsEnabled) return;
            cube.RotatePrevBot();
        }

        private void buttonTopLeft_Click(object sender, RoutedEventArgs e)
        {
            if (!buttonsEnabled) return;
            cube.RotatePrevTop();
        }

        private void AfterAnimation()
        {
            //RefreshButtons(true);
            buttonsEnabled = true;
            textBlockInfo.Text = cube.Cube.ToString();
        }

        private void BeforeAnimation()
        {
            //RefreshButtons(false);
            buttonsEnabled = false;
        }

        private bool buttonsEnabled = true;

        private void RefreshButtons(bool enabled)
        {
            buttonTopLeft.IsEnabled = enabled;
            buttonTopReset.IsEnabled = enabled;
            buttonTopRight.IsEnabled = enabled;
            buttonBottomLeft.IsEnabled = enabled;
            buttonBottomReset.IsEnabled = enabled;
            buttonBottomRight.IsEnabled = enabled;
            buttonFlip.IsEnabled = enabled;
            buttonTurn.IsEnabled = enabled;
        }
    }
}
