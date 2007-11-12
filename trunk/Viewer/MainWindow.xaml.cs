using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using _3DTools;
using Viewer.Properties;
using System.Windows.Forms;
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
            DatabaseExt m = new DatabaseExt(Settings.Default.DatabasePath);
            m.Initialize();
            DatabaseExt.Instance.IsMapped = false;
            InitializeComponent();
            visualCube = new VisualCube(new Cube());
            mainViewport.Children.Add(visualCube.FrontModel);
            backViewport.Children.Add(visualCube.BackModel);
            visualCube.FrontModel.Transform = TrackBall.Transform;
            visualCube.BackModel.Transform = TrackBall.Transform;
            TrackBall.EventSource = CaptureBorder;
            visualCube.OnBeforeAnimation += OnBeforeAnimation;
            visualCube.OnAfterAnimation += OnAfterAnimation;
            path = new Path(visualCube.Cube);
            DumpInfo(visualCube.Cube);
            UpdateMenu();
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
                buttonSolve.IsEnabled = false;
                buttonSolveStep.IsEnabled = false;
            }
            else
            {
                SmartStep step = path[0];
                if (step.Step != null)
                {
                    if (step.Step.TopShift != 0)
                    {
                        visualCube.RotateTop(step.Step.TopShift);
                        step.Step.TopShift = 0;
                    }
                    else if (step.Step.BotShift != 0)
                    {
                        visualCube.RotateBot(step.Step.BotShift);
                        step.Step.BotShift = 0;
                    }
                    else if (step.Step.BotShift == 0 && step.Step.TopShift == 0)
                    {
                        visualCube.Turn();
                        step.Step = null;
                    }
                }
                else if (step.Correction!=null)
                {
                    if (step.Correction.Flip)
                    {
                        visualCube.Flip();
                        step.Correction.Flip = false;
                    }
                    else if (step.Correction.TopShift!=0)
                    {
                        visualCube.RotateTop(step.Correction.TopShift);
                        step.Correction.TopShift = 0;
                    }
                    else if (step.Correction.BotShift != 0)
                    {
                        visualCube.RotateBot(step.Correction.BotShift);
                        step.Correction.BotShift = 0;
                    }
                    if (step.Correction.TopShift == 0 && step.Correction.BotShift==0 && !step.Correction.Flip)
                    {
                        step.Correction = null;
                    }
                }
                if (step.Step==null && step.Correction==null)
                {
                    path.RemoveAt(0);
                }
            }
        }

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

        private void mnuConnect_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.UseLocal = !Settings.Default.UseLocal;
            Settings.Default.Save();
            UpdateMenu();
        }

        private void UpdateMenu()
        {
            if (Settings.Default.UseLocal)
            {
                mnuConnect.Header = "Conect";
            }
            else
            {
                mnuConnect.Header = "Use Local";
            }
            if (Settings.Default.LocalReady)
            {
                mnuGenerate.Visibility = Visibility.Collapsed;
                mnuConnect.Visibility = Visibility.Visible;
            }
            else
            {
                mnuGenerate.Visibility = Visibility.Visible;
                mnuConnect.Visibility = Visibility.Collapsed;
            }
        }

        private void buttonCreate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Cube n = new Cube(textBoxTop.Text + textBoxBot.Text);
                n.CheckFlipable();
                n.CheckPieces();
                visualCube.Cube = n;
                visualCube.MiddleRight = false;
                visualCube.MiddleLeft = false;
                UpdatePath();
                DumpInfo(n);
                visualCube.Refresh();
            }
            catch(Exception)
            {
                MessageBox.Show("Invalid cube");
            }
        }

        private void buttonFlipMiddle_Click(object sender, RoutedEventArgs e)
        {
            visualCube.MiddleRight = !visualCube.MiddleRight;
            visualCube.Refresh();
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
                    Settings.Default.DatabasePath = dlg.SelectedPath;
                    Settings.Default.Save();
                    DatabaseManager.instance.Initialize();
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
                Cube c=new Cube();
                c.ReadLevel();
                if (DatabaseManager.instance.Explore())
                {
                    DatabaseManager.CloseAll();
                    DatabaseExt.Instance.IsMapped = false;
                    Test.TestData(12, 1000);
                }
                else
                {
                    DatabaseExt.Instance.IsMapped = false;
                }
            }
            finally
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new AfterEngine(afterEngine));
                FreeConsole();
            }
        }

        private delegate void AfterEngine();

        private void afterEngine()
        {
            if (Directory.Exists(DatabaseManager.DatabasePath))
            {
                Database d = Database.Load(false);
                if (d.IsExplored)
                {
                    Settings.Default.LocalReady = true;
                    Settings.Default.UseLocal = true;
                    Settings.Default.Save();
                }
            }
            UpdateMenu();
        }

        private void buttonRandomStep_Click(object sender, RoutedEventArgs e)
        {
            if (!buttonsEnabled) return;
            randomizing = 1;
            RandomStep();
        }

        private void buttonRandom_Click(object sender, RoutedEventArgs e)
        {
            if (!buttonsEnabled) return;
            randomizing = 15;
            RandomStep();
        }

        private void RandomStep()
        {
            if (randomizing == 0)
                return;
            randomizing--;
            int x = 3;
            do
            {
                int next = r.Next(x);
                switch (next)
                {
                    case 0:
                        visualCube.RotateNextTop();
                        break;
                    case 1:
                        visualCube.RotateNextBot();
                        break;
                    case 2:
                        visualCube.Turn();
                        UpdatePath();
                        return;
                    default:
                        UpdatePath();
                        return;
                }
                x++;
            } while (x > 7);
            UpdatePath();
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
            visualCube.Flip();
            UpdatePath();
        }

        private void buttonTurn_Click(object sender, RoutedEventArgs e)
        {
            if (!buttonsEnabled) return;
            visualCube.Turn();
            UpdatePath();
        }

        private void buttonBottomRight_Click(object sender, RoutedEventArgs e)
        {
            if (!buttonsEnabled) return;
            visualCube.RotateNextBot();
            UpdatePath();
        }

        private void topTopRight_Click(object sender, RoutedEventArgs e)
        {
            if (!buttonsEnabled) return;
            visualCube.RotateNextTop();
            UpdatePath();
        }

        private void buttonBottomLeft_Click(object sender, RoutedEventArgs e)
        {
            if (!buttonsEnabled) return;
            visualCube.RotatePrevBot();
            UpdatePath();
        }

        private void buttonTopLeft_Click(object sender, RoutedEventArgs e)
        {
            if (!buttonsEnabled) return;
            visualCube.RotatePrevTop();
            UpdatePath();
        }

        private void OnAfterAnimation()
        {
            buttonsEnabled = true;
            DumpInfo(visualCube.Cube);
            if (solving)
                StepHome();
            if (randomizing>0)
                RandomStep();
        }

        private void UpdatePath()
        {
            visualCube.Cube.CheckPieces();
            visualCube.Cube.CheckFlipable();
            if (randomizing==0)
                path = DatabaseProxy.FindWayHome(visualCube.Cube);
            if (path.Count > 0)
            {
                buttonSolve.IsEnabled = true;
                buttonSolveStep.IsEnabled = true;
            }
        }

        private void OnBeforeAnimation()
        {
            buttonsEnabled = false;
        }

        #endregion

        #region Variables

        private Path path;
        private bool solving;
        private int randomizing;
        private bool buttonsEnabled = true;
        private Trackball TrackBall = new Trackball();
        private VisualCube visualCube;
        private Random r = new Random();

        #endregion

        #region Helpers

        [DllImport("kernel32.dll")]
        public static extern Boolean AllocConsole();
        [DllImport("kernel32.dll")]
        public static extern Boolean FreeConsole();

        #endregion

    }
}
