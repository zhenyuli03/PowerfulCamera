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
using System.Windows.Navigation;
using System.Windows.Shapes;
using HalconDotNet; //包含HDevEngine

namespace HDevEnginer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        /**/
        private MainWindowViewModel mainWindowViewModel = new MainWindowViewModel();
        public string ProgramPathString;
        public HDevEngine MyEngine = new HDevEngine();
        public HDevProcedureCall ProgramCall;
        public HWindow Window;
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = mainWindowViewModel;

            var halconExamples = HSystem.GetSystem("example_dir"); //获取例程路径
            var ProcedurePath = halconExamples + @"\hdevengine\procedures";      //指定子程序文件路径 
            ProgramPathString = halconExamples + @"\hdevengine\hdevelop\fin_detection.hdev";      //指定程序文件路径     
            MyEngine.SetProcedurePath(ProcedurePath);   //设置子程序文件路径
        }

        private void Button_Click(object sender, RoutedEventArgs e)//加载
        {
            var Program = new HDevProcedure("detect_fin"); //指定规程文件
            ProgramCall = new HDevProcedureCall(Program);
            Console.WriteLine("加载程序完成");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)//运行
        {
            var Framegrabber = new HFramegrabber();
            Framegrabber.OpenFramegrabber("File", 1, 1, 0, 0, 0, 0, "default", -1, "default", -1, "default", "fin.seq", "default", -1, -1);
            HImage Image = new HImage();
            HRegion FinRegion;
            HTuple FinArea;
            for (int i = 0; i <= 2; i++)
            {
                Image.GrabImage(Framegrabber);
                Image.DispObj(Window);  //Wrong (logical) window number in operator disp_image”
                ProgramCall.SetInputIconicParamObject("Image", Image);
                ProgramCall.Execute();
                FinRegion = ProgramCall.GetOutputIconicParamRegion("FinRegion"); //获取区域
                FinArea = ProgramCall.GetOutputCtrlParamTuple("FinArea");        //获取控制参数
                Image.DispObj(Window);
                //Window.SetColor("red");
                Window.DispObj(FinRegion);
                //Window.SetColor("white");
                //Window.SetTposition(150, 20);
                //Window.WriteString("FinArea: " + FinArea.D);
            }
        }

        private void WpfHWindow_Initialized(object sender, EventArgs e)
        {
            Window = this.WpfHWindow.HalconWindow;
            HOperatorSet.SetDraw(Window, "margin");
        }
    }
}
