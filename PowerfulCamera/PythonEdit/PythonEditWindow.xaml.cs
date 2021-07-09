//using PowerfulCamera.CameraTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using HalconWindow = PowerfulCamera.CameraTools.HalconWindow; //指定此次的HalconWindow为CameraTools.HalconWindow

namespace PowerfulCamera.PythonEdit
{
    /// <summary>
    /// PythonEditWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PythonEditWindow
    {
        public PythonEditWindowViewModel VM { get; set; }
        /// <summary>
        /// ??????曲线救国
        /// </summary>
        private PythonEditWindowViewModel pythonEditWindowViewModel = new PythonEditWindowViewModel();
        public PythonEditWindow()
        {
            InitializeComponent();
            this.DataContext = pythonEditWindowViewModel;
            pythonEditWindowViewModel.AppendText += AppendText;
            pythonEditWindowViewModel.AddWindow = AddHalconWindow;
            pythonEditWindowViewModel.SetChild = SetChild;
        }

        public void SetChild(System.Windows.Forms.PictureBox pp)
        {
            form1.Child = pp;
        }

        public void ClearTextBox()
        {
        }
        public void AppendText(string text)
        {
            DateTime timenow = DateTime.Now;
            string Timenow = timenow.ToString("T");
            /*实现跨线程范围，不然会报错*/
            Dispatcher.Invoke(() =>
            {
                this.richTextBox.AppendText("\n");
                this.richTextBox.AppendText(Timenow + "      " + text);
            });
        }

        /// <summary>
        /// 添加HalCon窗口
        /// </summary>
        /// <param name="height">高度</param>
        /// <returns></returns>
        public string AddHalconWindow(int height)
        {
            string value = "";
            Dispatcher.Invoke(() =>
            {
                HalconWindow halconWindow = new HalconWindow { Height = height };
                value = halconWindow.ToString();                
                if (!pythonEditWindowViewModel.HalconWindowKeyValuePairs.ContainsKey(value)) //Map
                {
                    pythonEditWindowViewModel.HalconWindowKeyValuePairs.Add(value, halconWindow);
                    pythonEditWindowViewModel.PhotoWindowList.Add(halconWindow);
                }
                else
                {
                    value = "error";
                    halconWindow = null;
                }
            });
            return value;
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(pythonEditWindowViewModel.ExeThread != null)
            {
                pythonEditWindowViewModel.ExeThread.Kill();
            }
            if(pythonEditWindowViewModel.grpcServer.IsOpen)
            {
                pythonEditWindowViewModel.grpcServer.Close();
            }
        }

    }
}
