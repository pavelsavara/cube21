using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using _3DTools;
using Zamboch.Cube21;
using Zamboch.Cube21.Actions;
using Zamboch.Cube21.Engine;
using Zamboch.Cube21.Work;
using MessageBox=System.Windows.MessageBox;
using Path=Zamboch.Cube21.Actions.Path;

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
            DatabaseExt m = new DatabaseExt();
            m.Initialize();
            DatabaseExt.Instance.IsMapped = false;
            InitializeComponent();
            cube = new VisualCube(new Cube());
            mainViewport.Children.Add(cube.FrontModel);
            backViewport.Children.Add(cube.BackModel);
            cube.FrontModel.Transform = TrackBall.Transform;
            cube.BackModel.Transform = TrackBall.Transform;
            TrackBall.EventSource = CaptureBorder;
            cube.OnBeforeAnimation += OnBeforeAnimation;
            cube.OnAfterAnimation += OnAfterAnimation;
            path = new Path(cube.Cube);
            DumpInfo(cube.Cube);
            if (DatabaseManager.DatabaseLocal)
                mnuGenerate.Visibility = Visibility.Collapsed;

        }

        private void DumpInfo(Cube cube)
        {
            StringBuilder sb=new StringBuilder();
            sb.Append(cube.ToString());
            sb.Append("\n");
            sb.Append(cube.NormalShape.ToString());
            sb.Append("\n");
            sb.Append(path);
            textBlockInfo.Text = sb.ToString();
            textBoxTop.Text = cube.TopToString();
            textBoxBot.Text = cube.BotToString();
        }

        private void StepHome()
        {
            if (path.Count == 0)
            {
                solving = false;
                buttonSolve.IsEnabled = true;
            }
            else
            {
                SmartStep step = (SmartStep)path[0];
                if (step.Step != null)
                {
                    if (step.Step.TopShift != 0)
                    {
                        cube.RotateTop(step.Step.TopShift);
                    }
                    else if (step.Step.BotShift != 0)
                    {
                        cube.RotateBot(step.Step.BotShift);
                    }
                    else if (step.Step.BotShift == 0 && step.Step.TopShift == 0)
                    {
                        cube.Turn();
                    }
                }
                else if (step.Correction!=null)
                {
                    if (step.Correction.Flip)
                    {
                        cube.Flip();
                    }
                    else if (step.Correction.TopShift!=0)
                    {
                        cube.RotateTop(step.Correction.TopShift);
                    }
                    else if (step.Correction.BotShift != 0)
                    {
                        cube.RotateBot(step.Correction.BotShift);
                    }
                }
            }
        }

        private Path path;
        private bool solving;

        #endregion

        #region Events
        private void buttonSolve_Click(object sender, RoutedEventArgs e)
        {
            buttonSolve.IsEnabled = false;
            solving = true;
            StepHome();
        }

        private void buttonSolveStep_Click(object sender, RoutedEventArgs e)
        {
            StepHome();
        }

        private void buttonCreate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Cube n = new Cube(textBoxTop.Text + textBoxBot.Text);
                n.CheckFlipable();
                n.CheckPieces();
                path = n.FindWayHome();
                path.FlipMiddle(cube.MiddleLeft, cube.MiddleRight);
                DumpInfo(n);
                cube.Cube = n;
                cube.MiddleRight = false;
                cube.MiddleLeft = false;
                cube.Refresh();
            }
            catch(Exception)
            {
                MessageBox.Show("Invalid cube");
            }
        }

        private void buttonFlipMiddle_Click(object sender, RoutedEventArgs e)
        {
            cube.MiddleRight = !cube.MiddleRight;
            cube.Refresh();
        }

        private void mnuGenerate_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult res =
                MessageBox.Show("This require Minimum: 5GB disk space and more than 512MB RAM\nRecomended: 4GB RAM, Fast disk, Vista OS\nIt could take more than 8 hours on such fast machine. There are 12 levels to explore, 6 810 804 000 uniqe cubes to be found. Your hardware will be heavily loaded. \n\nWould you like to start the process ?",
                    "Warning: Time & Power required!", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
            if (res==MessageBoxResult.Yes)
            {
                FolderBrowserDialog dlg=new FolderBrowserDialog();
                dlg.SelectedPath = DatabaseManager.DatabasePath;
                dlg.Description = "Choose directory on fast unfragmented disk with 5GB of disk space";
                dlg.ShowNewFolderButton = true;
                if (dlg.ShowDialog()==System.Windows.Forms.DialogResult.OK)
                {
                    if (!Directory.Exists(dlg.SelectedPath))
                        Directory.CreateDirectory(dlg.SelectedPath);
                    mnuGenerate.Visibility = System.Windows.Visibility.Collapsed;
                    DatabaseManager.DatabasePath = dlg.SelectedPath;
                    Thread mainThread = new Thread(engine);
                    mainThread.Start();
                }
            }
        }

        private void engine()
        {
            try
            {
                AllocConsole();
                DatabaseExt.Instance.IsMapped = true;
                DatabaseExt.Main();
                DatabaseExt.Instance.IsMapped = false;
            }
            finally
            {
                FreeConsole();
                if (Directory.Exists(DatabaseManager.DatabasePath))
                {
                    Database d = Database.Load(false);
                    if (d.IsExplored)
                    {
                        DatabaseManager.DatabaseLocal = true;
                    }
                    else
                    {
                        mnuGenerate.Visibility = System.Windows.Visibility.Visible;
                    }
                }
                else
                {
                    mnuGenerate.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        private void mnuClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
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
            path = cube.Cube.FindWayHome();
            path.FlipMiddle(cube.MiddleLeft, cube.MiddleRight);
            DumpInfo(cube.Cube);
            if (solving)
                StepHome();
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

        #region Helpers

        [DllImport("kernel32.dll")]
        public static extern Boolean AllocConsole();
        [DllImport("kernel32.dll")]
        public static extern Boolean FreeConsole();

        #endregion

        private void buttonRandomStep_Click(object sender, RoutedEventArgs e)
        {
            int x = 3;
            do
            {
                int next = r.Next(x);
                switch (next)
                {
                    case 0:
                        cube.RotateNextTop();
                        break;
                    case 1:
                        cube.RotateNextBot();
                        break;
                    case 2:
                        cube.Turn();
                        return;
                    default:
                        return;
                }
                x++;
            } while (x > 7);
        }

        Random r = new Random();
    }
}
