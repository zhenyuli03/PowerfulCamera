using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using PowerfulCamera.CameraTools;
using PowerfulCamera.ItemBase;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using HalconDotNet;
using System.Windows.Threading;

namespace PowerfulCamera.PythonEdit
{
    public class PythonEditWindowViewModel : ViewModelBase
    {
        #region DLL引用
        [DllImport("User32.dll ", SetLastError = true, EntryPoint = "SetParent")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        [DllImport("user32.dll", EntryPoint = "SendMessage", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hwnd, uint wMsg, int wParam, int lParam); //对外部软件窗口发送一些消息(如 窗口最大化、最小化等)
        [DllImport("user32.dll ", EntryPoint = "ShowWindow")]
        public static extern int ShowWindow(IntPtr hwnd, int nCmdShow);
        #endregion
        /// <summary>
        /// 保存窗口和句柄的哈希表
        /// </summary>
        public Dictionary<string, HalconWindow> HalconWindowKeyValuePairs = new Dictionary<string, HalconWindow>(); 

        private ObservableCollection<HalconWindow> photoWindowList = new ObservableCollection<HalconWindow>();
        public ObservableCollection<HalconWindow> PhotoWindowList
        {
            set
            {
                photoWindowList = value;
                RaisePropertyChanged();
            }
            get => photoWindowList;
        }
        public Action<System.Windows.Forms.PictureBox> SetChild;
        public Action<string> AppendText;
        public Func<int, string> AddWindow;
        public RelayCommand OpenVSCodeCommand { set; get; }
        public RelayCommand OpenPythonCommand { set; get; }
        public RelayCommand ClearPhotoCommand { set; get; }
        public RelayCommand ClearRichTextBoxCommand { set; get; }

        public Process ExeThread;        

        public string progressBar;
        public string ProgressBar
        {
            set
            {
                progressBar = value;
                RaisePropertyChanged();
            }
            get => progressBar;
        }        
        public GrpcServer grpcServer;
        /// <summary>
        /// 构造函数，会打开Grpc服务，等待Python-Client
        /// </summary>
        public PythonEditWindowViewModel()
        {      
            ProgressBar = "Collapsed";
            grpcServer = new GrpcServer(PrintString, AddHalconWindows);
            grpcServer.Open();
            OpenVSCodeCommand = new RelayCommand(() =>
            {
                OpenExe(GlobalBase.VSCodeDir + "//Code.exe");
            });
            OpenPythonCommand = new RelayCommand(() =>
            {
                OpenExe(GlobalBase.CmdDir, 3000, 1200, 800);
                //HObject image = null;
                //HOperatorSet.GenEmptyObj(out image);
                //HOperatorSet.ReadImage(out image, GlobalBase.ItemDir + "//dog1.jpg");
            });
            ClearPhotoCommand = new RelayCommand(() =>
            {
            });
            ClearRichTextBoxCommand = new RelayCommand(() =>
            {
            });
            //HalconWindow halconWindow = new HalconWindow { Height = 200 };
            //PhotoWindowList.Add(halconWindow);
        }

        /// <summary>
        /// 打印信息，供Grpc调用
        /// </summary>
        /// <param name="s">字符串</param>
        public void PrintString(string s)
        {
            AppendText(s);
        }

        /// <summary>
        /// 添加窗口
        /// </summary>
        /// <param name="height"></param>
        /// <returns></returns>
        public string AddHalconWindows(int height)
        {
            return AddWindow(height);
        }

        ///// <summary>
        ///// 添加halcon窗口
        ///// </summary>
        ///// <param name="halconWindow">窗口</param>
        ///// <param name="height">高度，默认200</param>
        //public string AddHalconWindow(int height)
        //{
        //    //string value = "";
        //    ////HalconWindow halconWindow;
        //    ////halconWindow = new HalconWindow();
        //    //HalconWindow halconWindow = new HalconWindow();
        //    //halconWindow.Height = height;
        //    //photoWindowList.Add(halconWindow);
        //    //value = halconWindow.ToString();
        //    ///*存表*/
        //    //if (!HalconWindowKeyValuePairs.TryGetValue(value, out halconWindow))
        //    //{
        //    //    HalconWindowKeyValuePairs.Add(value, halconWindow);
        //    //}
        //    //else
        //    //    return "error"; //出错，已经存在此表了
        //    //return value;
        //}

        /// <summary>
        /// 打开exe程序，并且嵌入到WPF中
        /// </summary>
        /// <param name="path">文件的绝对路径</param>
        /// <param name="delay_ms">等待打开时间</param>
        /// <param name="width">嵌入窗口显示:宽</param>
        /// <param name="height">嵌入窗口显示:高</param>
        public void OpenExe(string path, int delay_ms=3000, int width=500, int height=400)
        {
            if (ExeThread != null)
            {
                try 
                { 
                    ExeThread.Kill();
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            string PythonFileDir = GlobalBase.OpenFileDir("Files (*.py)|*.py");
            if (PythonFileDir != null)
            {
                ExeThread = new Process();
                ExeThread.StartInfo.FileName = path;
                ExeThread.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized;//加上这句效果更好
                ExeThread.StartInfo.Arguments = PythonFileDir;
                ExeThread.Start();
                System.Threading.Thread.Sleep(delay_ms);//加上，100如果效果没有就继续加大
                System.Windows.Forms.PictureBox pp = new System.Windows.Forms.PictureBox
                {
                    Width = width,
                    Height = height,
                    Bounds = new System.Drawing.Rectangle(0, 0, width, height),
                    Name = "PictureBox"
                };
                SetChild(pp);
                IntPtr hpanel1 = pp.Handle;
                SetParent(ExeThread.MainWindowHandle, hpanel1); //panel1.Handle为要显示外部程序的容器
                ShowWindow(ExeThread.MainWindowHandle, 3);
            }
        }
    }
}
