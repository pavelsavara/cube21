using System.Text;
using System.Windows;
using _3DTools;
using Zamboch.Cube21;
using Zamboch.Cube21.Work;

namespace Viewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Construction

        public MainWindow()
        {
            DatabaseManager m=new DatabaseManager();
            m.Initialize();
            InitializeComponent();
            cube = new VisualCube(new Cube());
            mainViewport.Children.Add(cube.FrontModel);
            backViewport.Children.Add(cube.BackModel);
            cube.FrontModel.Transform = TrackBall.Transform;
            cube.BackModel.Transform = TrackBall.Transform;
            TrackBall.EventSource = CaptureBorder;
            cube.OnBeforeAnimation += OnBeforeAnimation;
            cube.OnAfterAnimation += OnAfterAnimation;
            DumpInfo();
        }

        #endregion

        #region Helpers

        private void DumpInfo()
        {
            StringBuilder sb=new StringBuilder();
            sb.Append(cube.Cube.ToString());
            sb.Append("\n");
            sb.Append(cube.Cube.NormalShape.ToString());
            textBlockInfo.Text = sb.ToString(); 
        }

        #endregion

        #region Events

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

        private void OnAfterAnimation()
        {
            buttonsEnabled = true;
            DumpInfo();
        }

        private void OnBeforeAnimation()
        {
            buttonsEnabled = false;
        }

        #endregion

        #region Variables

        private bool buttonsEnabled = true;
        private Trackball TrackBall = new Trackball();
        private VisualCube cube;

        #endregion
    }
}
