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
using HalconDotNet;
using System.IO;
using Microsoft.Win32;
using System.ComponentModel;

namespace PowerfulCamera.CameraTools
{
    /// <summary>
    /// HalconWindow.xaml 的交互逻辑
    /// </summary>
    public partial class HalconWindow : UserControl, INotifyPropertyChanged
    {
        //System.InvalidOperationException:“發出呼叫的執行緒必須是 STA，因為許多 UI 元件都這樣要求。”
        public HalconWindow( )
        {
            InitializeComponent();
            //ReadConfig();
        }
        
        static HalconWindow()
        {
            MyHWindowProperty = DependencyProperty.Register("MyHWindow", typeof(HWindow), typeof(HalconWindow));
        }

        #region 变量定义区域
        public int Index;
        private HWindow hWindowHandle;
        private HObject ho_image; //当前窗口的图像
        private double Mouse_Row, Mouse_Col; //用于记录鼠标的坐标信息
        private int zoom_beginRow, zoom_beginCol, zoom_endRow, zoom_endCol;                     // 设定图像的窗口显示部分
        private int current_beginRow, current_beginCol, current_endRow, current_endCol;         // 获取图像的当前显示部分
        private HObject ho_rect1; //鼠标绘制的矩形框1
        private HObject ho_rect2; //鼠标绘制的矩形框2
        private HObject ho_circle;  //鼠标绘制的圆
        private HObject ho_ring;  //鼠标绘制的圆环
        private HObject ho_ellipse; //鼠标绘制的椭圆
        private HObject ho_region; //鼠标绘制的区域
        private double hw_width = 640, hw_height = 480;     //halcon控件尺寸
        private int imageWidth = 0, imageHeight = 0; //当前halcon窗口的图片的尺寸
        private int SystemWidth = 0, SystemHeight = 0;
        public bool EnableDraw = false; //是否可以绘制
        public bool EnableDrag = true; //是否可以拖动图片
        private bool isMouseLeftDown;
        private bool EnableWheel = true;
        public event Action<int, double, double> ShowMouseInfo; //显示鼠标信息
        public event Action<double, double> GetMouseDownEvent; //鼠标点击事件
        public event PropertyChangedEventHandler PropertyChanged;

        private int LineWidth = 1; //显示的线宽
        private int HalconFontSize = 16; //显示的字体尺寸
        private string ConfigPath = Environment.CurrentDirectory + "\\SystemConfig.ini";
        public  List<double> RegionMessage=new List<double> { };
        public static readonly DependencyProperty MyHWindowProperty;
        #endregion

        #region 属性定义区域
        public HWindow MyHWindow
        {
            get
            {
                return (HWindow)GetValue(MyHWindowProperty);
            }
            set
            {
                SetValue(MyHWindowProperty, value);
                Changed("MyHWindow");
            }
        }
        public HWindow Handle { get { return hWindowHandle; } }

        public HObject Image
        {
            get { return ho_image; }
            set { ho_image = value; }
        }

        public HObject Rectangle1
        {
            get { return ho_rect1; }
        }

        public HObject Rectangle2
        {
            get { return ho_rect2; }
        }

        public HObject Circle
        {
            get { return ho_circle; }
        }

        public HObject Ellipse
        {
            get { return ho_ellipse; }
        }

        #endregion

        #region 方法定义区域
        private void hwindow_HInitWindow(object sender, EventArgs e)
        {
            hWindowHandle = hwindow.HalconWindow;
            Console.WriteLine(hwindow.HalconWindow.GetHashCode().ToString()); 
            MyHWindow = hWindowHandle;
            HOperatorSet.SetLineWidth(hWindowHandle, LineWidth);
            string Font = "-default-" + HalconFontSize + "-*-*-*-*-1-";
            HOperatorSet.SetFont(hWindowHandle, Font);
        }

        private void Changed(string p)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
        }
        /// <summary>
        /// 重载,返回窗口句柄的哈希码
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return hwindow.HalconWindow.GetHashCode().ToString();
        }

        private void HalconWindow_KeyDown(object sender, KeyEventArgs e) //Ctrl + S 保存窗口的图片
        {
            if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && Keyboard.IsKeyDown(Key.S))
            {
                SaveFileDialog sf = new SaveFileDialog();
                sf.Filter = "bmp file(*.bmp)|*.bmp|jpg file(*.jpg)|*.jpg|All files(*.*)|*.*";
                if (ho_image != null && sf.ShowDialog() == true)
                {
                    try
                    {
                        string[] extension = sf.FileName.Split('.');
                        HOperatorSet.WriteImage(ho_image, extension[extension.Length - 1], 0, sf.FileName);
                    }
                    catch (HalconException ex)
                    {
                        MessageBox.Show("图片保存失败！" + ex.Message);
                    }
                }
            }
            e.Handled = true;
        }

        private void hwindow_HMouseDown(object sender, HalconDotNet.HMouseEventArgsWPF e)
        {
            if (ho_image != null && e.Button == MouseButton.Left)
            {
                this.GetPosition();
                isMouseLeftDown = true;
            }
            //Global.Tag = Index;
        }

        private void hwindow_HMouseUp(object sender, HalconDotNet.HMouseEventArgsWPF e)
        {
            if (e.Button == MouseButton.Left)
            {
                isMouseLeftDown = false;
                if (GetMouseDownEvent != null) GetMouseDownEvent(Mouse_Row, Mouse_Col);
            }
            else if (e.Button == MouseButton.Middle)
            {
                DispImageFit(ho_image, false);
            }
        }

        private void hwindow_HMouseMove(object sender, HalconDotNet.HMouseEventArgsWPF e)
        {
            if (isMouseLeftDown && EnableDraw == false)
            {
                 this.DragImage();
            }
            else if (isMouseLeftDown == false && EnableDraw == false)
            {
                this.GetPosition();
            }
        }

        private void hwindow_HMouseWheel(object sender, HalconDotNet.HMouseEventArgsWPF e)
        {
            if (EnableWheel && ho_image != null)
            {
                HTuple mode = e.Delta;  //获取滚轮的方向
                //获取当前鼠标的位置
                this.DispImageZoom(mode);
            }
        }

        private void hwindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            hw_height = this.ActualHeight;
            hw_width = this.ActualWidth;
        }

        //获取鼠标位置信息
        private void GetPosition()
        {
            try
            {
                HTuple row, col, button;
                HOperatorSet.GetMposition(hWindowHandle, out row, out col, out button);
                Mouse_Row = row[0].D;
                Mouse_Col = col[0].D;
                if (ShowMouseInfo != null)
                    ShowMouseInfo(Index, Mouse_Row, Mouse_Col);
            }
            catch
            {

            }
        }

     /// <summary>
     /// 适应窗口显示图像
     /// </summary>
     /// <param name="t_image">要显示的图像</param>
     /// <param name="overWrite">是否覆盖当前窗口图像</param>
        public void DispImageFit(HObject t_image, bool overWrite = true)
        {
            hWindowHandle.DispImageFit(t_image);
            //if (t_image != null && hWindowHandle != null)
            //{
            //    try
            //    {
            //        if (t_image.CountObj() == 0)
            //        {
            //            return;
            //        }
            //        if (overWrite)
            //        {
            //            if (ho_image != null) ho_image.Dispose();      
            //            HOperatorSet.CopyImage(t_image, out ho_image);
            //        }
            //        HTuple width, height;
            //        HOperatorSet.GetImageSize(t_image, out width, out height);
            //        imageWidth = width[0].I;
            //        imageHeight = height[0].I;
            //        HOperatorSet.SetPart(hWindowHandle, 0, 0, imageHeight, imageWidth);
            //        current_beginRow = 0;
            //        current_beginCol = 0;
            //        current_endRow = imageHeight;
            //        current_endCol = imageWidth;
            //        HOperatorSet.DispObj(t_image, hWindowHandle);//显示图像
            //    }
            //    catch
            //    { }
            //}
        }

        public void DispImage(HObject t_image)
        {
            if (t_image != null)
            {
                try
                {
                    HOperatorSet.DispObj(t_image, hWindowHandle);
                }
                catch
                { }
            }
        }

        //按照指定的中心缩放当前图像
        public void DispImageZoom(int mode)
        {
            if (ho_image != null)
            {
                try
                {
                    HTuple row1, col1, row2, col2;
                    HOperatorSet.GetPart(hWindowHandle, out row1, out col1, out row2, out col2);
                    current_beginRow = row1[0].I;
                    current_beginCol = col1[0].I;
                    current_endRow = row2[0].I;
                    current_endCol = col2[0].I;
                }
                catch
                {
                    return;
                }
                if (mode > 0)                 // 放大图像
                {
                    zoom_beginRow = (int)(current_beginRow + (Mouse_Row - current_beginRow) * 0.100d);
                    zoom_beginCol = (int)(current_beginCol + (Mouse_Col - current_beginCol) * 0.100d);
                    zoom_endRow = (int)(current_endRow - (current_endRow - Mouse_Row) * 0.100d);
                    zoom_endCol = (int)(current_endCol - (current_endCol - Mouse_Col) * 0.100d);
                }
                else                // 缩小图像
                {
                    zoom_beginRow = (int)(Mouse_Row - (Mouse_Row - current_beginRow) / 0.900d);
                    zoom_beginCol = (int)(Mouse_Col - (Mouse_Col - current_beginCol) / 0.900d);
                    zoom_endRow = (int)(Mouse_Row + (current_endRow - Mouse_Row) / 0.900d);
                    zoom_endCol = (int)(Mouse_Col + (current_endCol - Mouse_Col) / 0.900d);
                }

                try
                {
                    bool _isOutOfArea = true;
                    bool _isOutOfSize = true;
                    bool _isOutOfPixel = true;  //避免像素过大

                    _isOutOfArea = zoom_beginRow >= imageHeight || zoom_endRow <= 0 || zoom_beginCol >= imageWidth || zoom_endCol < 0;
                    _isOutOfSize = (zoom_endRow - zoom_beginRow) > imageHeight * 20 || (zoom_endCol - zoom_beginCol) > imageWidth * 20;
                    _isOutOfPixel = hw_height / (zoom_endRow - zoom_beginRow) > 500 || hw_width / (zoom_endCol - zoom_beginCol) > 500;

                    if (_isOutOfArea || _isOutOfSize)
                    {
                        DispImageFit(ho_image, false);
                    }
                    else if (!_isOutOfPixel)
                    {
                        HOperatorSet.ClearWindow(hWindowHandle);
                        HOperatorSet.SetPaint(hWindowHandle, new HTuple("default"));
                        //保持图像显示比例         
                        HOperatorSet.SetPart(hWindowHandle, zoom_beginRow, zoom_beginCol, zoom_endRow, zoom_endCol);
                        current_beginRow = zoom_beginRow;
                        current_beginCol = zoom_beginCol;
                        current_endRow = zoom_endRow;
                        current_endCol = zoom_endCol;
                        HOperatorSet.DispObj(ho_image, hWindowHandle);
                    }
                }
                catch
                {
                    DispImageFit(ho_image, false);
                }
            }
        }

        //移动图像
        public void DragImage()
        {
            if (ho_image == null || !EnableDrag)
            {
                return;
            }
            double dCurColumn = 0;
            double dCurRow = 0;
            try
            {
                HTuple hv_CurrentRow, hv_CurrentColumn, hv_Button;
                HOperatorSet.GetMpositionSubPix(hWindowHandle, out hv_CurrentRow, out hv_CurrentColumn, out hv_Button);
                dCurColumn = hv_CurrentColumn[0].D;
                dCurRow = hv_CurrentRow[0].D;

                double motionX = dCurColumn - Mouse_Col;
                double motionY = dCurRow - Mouse_Row;

                current_beginRow += -(int)motionY;
                current_endRow += -(int)motionY;
                current_beginCol += -(int)motionX;
                current_endCol += -(int)motionX;
                HOperatorSet.SetPart(hWindowHandle, current_beginRow, current_beginCol, current_endRow, current_endCol);
                RefreshWindow();
            }
            catch (HalconException ex)
            {
                string sError = ex.ToString();
            }
        }

        //刷新窗口
        public void RefreshWindow()
        {
            if (ho_image != null)
            {
                HSystem.SetSystem("flush_graphic", "false");
                HOperatorSet.ClearWindow(hWindowHandle);
                HSystem.SetSystem("flush_graphic", "true");
                HOperatorSet.DispObj(ho_image, hWindowHandle);
            }
        }

        //清空图片
        public void Clear()
        {
            HOperatorSet.ClearWindow(hWindowHandle);
            if (ho_image != null)
            {
                ho_image = null;
            }
        }
        //显示一个object
        public void DispObject(HObject ho_Object, string color = "green", string draw = "margin")//可以选择绘画边缘? 默认fill可能会常规点吧
        {       
            if (ho_Object != null)
            {
                HTuple number;
                HOperatorSet.CountObj(ho_Object, out number);
                if (number[0].I == 0)//传进来可能有很多个对象吗??????/
                    return;
                try
                {
                    HOperatorSet.SetColor(hWindowHandle, color);
                }
                catch { }
                HOperatorSet.SetDraw(hWindowHandle, draw);
                HOperatorSet.DispObj(ho_Object, hWindowHandle);
            }
        }
        //显示信息
        public void DispString(string text, double Row = 10, double Col = 10, string color = "green")
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }
            HOperatorSet.SetTposition(hWindowHandle, Row, Col);
            HOperatorSet.SetColor(hWindowHandle, color);
            HOperatorSet.WriteString(hWindowHandle, text);
        }

        //显示十字
        public void DisplayCross(HTuple Row, HTuple Col, int CrossSize = 50, double Angle = 0, string color = "green")
        {
            HOperatorSet.SetColor(hWindowHandle, color);
            HOperatorSet.DispCross(hWindowHandle, Row, Col, CrossSize, Angle);
        }

        //显示箭头
        public void DisplayArrow(double startRow, double startCol, double endRow, double endCol, int arrowSize = 5, string color = "green")
        {
            HOperatorSet.SetColor(hWindowHandle, color);
            HOperatorSet.DispArrow(hWindowHandle, startRow, startCol, endRow, endCol, arrowSize);
        }
        //画直线
        public void DrawLine(out double startRow, out double startCol, out double endRow, out double endCol, string color = "green")
        {
            EnableDraw = true;
            this.Focus();
            HOperatorSet.GenEmptyObj(out ho_rect1);
            HTuple row1, col1, row2, col2;
            try
            {
                if (ho_image != null)
                {
                    HOperatorSet.DispObj(ho_image, hWindowHandle);
                }
                HOperatorSet.SetColor(hWindowHandle, color);
                HOperatorSet.DrawLine(hWindowHandle, out row1, out col1, out row2, out col2);
                startRow = row1[0].D;
                startCol = col1[0].D;
                endRow = row2[0].D;
                endCol = col2[0].D;
                HOperatorSet.DispLine(hWindowHandle, row1, col1, row2, col2);
            }
            finally
            {
                EnableDraw = false;
            }
        }
        // 画矩形1，并返回矩形
        public HObject DrawRectangle1(bool overWrite = true, string color = "green", string draw = "margin")
        {
            EnableDraw = true;
            HOperatorSet.GenEmptyObj(out ho_rect1);
            try
            {
                this.Focus();
                if (ho_image != null && overWrite)
                {
                    HOperatorSet.DispObj(ho_image, hWindowHandle);
                }
                HOperatorSet.SetDraw(hWindowHandle, draw);
                HOperatorSet.SetColor(hWindowHandle, color);
                HTuple row1, col1, row2, col2;
                HOperatorSet.DrawRectangle1(hWindowHandle, out row1, out col1, out row2, out col2);
                HOperatorSet.GenRectangle1(out ho_rect1, row1, col1, row2, col2);
                HOperatorSet.DispRectangle1(hWindowHandle, row1, col1, row2, col2);
            }
            finally
            {
                EnableDraw = false;
            }
            return ho_rect1;
        }
        //画矩形框1，并返回中心点坐标和长宽
        public void DrawRectangle(out double centerRow, out double centerCol, out double width, out double height, bool overWrite = true, string color = "green", string draw = "margin")
        {
            EnableDraw = true;
            HOperatorSet.GenEmptyObj(out ho_rect1);
            HTuple row1, col1, row2, col2;
            try
            {
                if (ho_image != null && overWrite)
                {
                    HOperatorSet.DispObj(ho_image, hWindowHandle);
                }
                this.Focus();
                HOperatorSet.SetDraw(hWindowHandle, draw);
                HOperatorSet.SetColor(hWindowHandle, color);
                HOperatorSet.DrawRectangle1(hWindowHandle, out row1, out col1, out row2, out col2);
                centerRow = (row1[0].D + row2[0].D) / 2;
                centerCol = (col1[0].D + col2[0].D) / 2;
                width = Math.Abs(col2[0].D - col1[0].D);
                height = Math.Abs(row2[0].D - row1[0].D);
                HOperatorSet.GenRectangle1(out ho_rect1, row1, col1, row2, col2);
                HOperatorSet.DispRectangle1(hWindowHandle, row1, col1, row2, col2);
            }
            finally
            {
                EnableDraw = false;
            }
        }

        //画矩形框2，返回中心点坐标，角度，长度1和长度2
        public void DrawRectangle2(out double centerRow, out double centerCol, out double Phi, out double Length1, out double Length2, bool overWrite = true, string color = "green", string draw = "margin")
        {
            EnableDraw = true;
            this.Focus();
            HTuple row, col, phi, length1, length2;
            try
            {
                if (ho_image != null && overWrite)
                {
                    HOperatorSet.DispObj(ho_image, hWindowHandle);
                }
                HOperatorSet.SetDraw(hWindowHandle, draw);
                HOperatorSet.SetColor(hWindowHandle, color);
                HOperatorSet.DrawRectangle2(hWindowHandle, out row, out col, out phi, out length1, out length2);
                centerRow = row[0].D;
                centerCol = col[0].D;
                Phi = phi[0].D;
                Length1 = length1[0].D;
                Length2 = length2[0].D;
                HOperatorSet.GenRectangle2(out ho_rect2, row, col, phi, length1, length2);
                HOperatorSet.DispRectangle2(hWindowHandle, row, col, phi, length1, length2);
            }
            finally
            {
                EnableDraw = false;
            }
        }
         
        //画矩形2，并返回矩形
        public HObject DrawRectangle2(bool overWrite = true, string color = "green", string draw = "margin")
        {
            EnableDraw = true;
            this.Focus();
            HTuple row, col, phi, length1, length2;
            try
            {
                if (ho_image != null && overWrite)
                {
                    HOperatorSet.DispObj(ho_image, hWindowHandle);
                }
                HOperatorSet.SetDraw(hWindowHandle, draw);
                HOperatorSet.SetColor(hWindowHandle, color);
                HOperatorSet.DrawRectangle2(hWindowHandle, out row, out col, out phi, out length1, out length2);
                HOperatorSet.GenRectangle2(out ho_rect2, row, col, phi, length1, length2);
                HOperatorSet.DispRectangle2(hWindowHandle, row, col, phi, length1, length2);
            }
            finally
            {
                EnableDraw = false;
            }
            return ho_rect2;
        }

        //画圆，返回中心点坐标和半径
        public void DrawCircle(out double centerRow, out double centerCol, out double R, bool overWrite = true, string color = "green", string draw = "margin")
        {
            EnableDraw = true;
            this.Focus();
            HOperatorSet.GenEmptyObj(out ho_circle);
            HTuple row, col, radius;
            try
            {
                if (ho_image != null && overWrite)
                {
                    HOperatorSet.DispObj(ho_image, hWindowHandle);
                }
                HOperatorSet.SetDraw(hWindowHandle, draw);
                HOperatorSet.SetColor(hWindowHandle, color);
                HOperatorSet.DrawCircle(hWindowHandle, out row, out col, out radius);
                centerRow = row[0].D;
                centerCol = col[0].D;
                R = radius[0].D;
                HOperatorSet.GenCircle(out ho_circle, row, col, radius);
                HOperatorSet.DispCircle(hWindowHandle, row, col, radius);
            }
            finally
            {
                EnableDraw = false;
            }
        }

        //画圆，并返回圆
        public HObject DrawCircle(bool overWrite = true, string color = "green", string draw = "margin")
        {
            EnableDraw = true;
            this.Focus();
            HOperatorSet.GenEmptyObj(out ho_circle);
            HTuple row, col, radius;
            try
            {
                if (ho_image != null && overWrite)
                {
                    HOperatorSet.DispObj(ho_image, hWindowHandle);
                }
                HOperatorSet.SetDraw(hWindowHandle, draw);
                HOperatorSet.SetColor(hWindowHandle, color);
                HOperatorSet.DrawCircle(hWindowHandle, out row, out col, out radius);
                HOperatorSet.GenCircle(out ho_circle, row, col, radius);
                HOperatorSet.DispCircle(hWindowHandle, row, col, radius);
            }
            finally
            {
                EnableDraw = false;
            }
            return ho_circle;
        }

        public HObject DrawRing(int band, bool overWrite = true, string color = "green", string draw = "margin")
        {
            EnableDraw = true;
            this.Focus();
            HOperatorSet.GenEmptyObj(out ho_ring);
            HTuple row, col, radius;
            try
            {
                if (ho_image != null && overWrite)
                {
                    HOperatorSet.DispObj(ho_image, hWindowHandle);
                }
                HOperatorSet.SetDraw(hWindowHandle, draw);
                HOperatorSet.SetColor(hWindowHandle, color);
                HOperatorSet.DrawCircle(hWindowHandle, out row, out col, out radius);
                HObject circle1, circle2;
                HOperatorSet.GenCircle(out circle1, row, col, radius);
                HTuple abs;
                HOperatorSet.TupleAbs(radius - band, out abs);
                HOperatorSet.GenCircle(out circle2, row, col, abs);
                HOperatorSet.Difference(circle1, circle2, out ho_ring);
                HOperatorSet.DispObj(ho_ring, hWindowHandle);
            }
            finally
            {
                EnableDraw = false;
            }
            return ho_ring;
        }

        //画圆环，并返回圆环坐标
        public void DrawRing(out double centerRow, out double centerCol, out double R, int band = 50, bool overWrite = true, string color = "green", string draw = "margin")
        {
            EnableDraw = true;
            this.Focus();
            HOperatorSet.GenEmptyObj(out ho_ring);
            HTuple row, col, radius;
            try
            {
                if (ho_image != null && overWrite)
                {
                    HOperatorSet.DispObj(ho_image, hWindowHandle);
                }
                HOperatorSet.SetDraw(hWindowHandle, draw);
                HOperatorSet.SetColor(hWindowHandle, color);
                HOperatorSet.DrawCircle(hWindowHandle, out row, out col, out radius);
                centerRow = row[0].D;
                centerCol = col[0].D;
                R = radius[0].D;
                HObject circle1, circle2;
                HOperatorSet.GenCircle(out circle1, row, col, radius);
                HTuple abs;
                HOperatorSet.TupleAbs(radius - band, out abs);
                HOperatorSet.GenCircle(out circle2, row, col, abs);
                HOperatorSet.Difference(circle1, circle2, out ho_ring);
                HOperatorSet.DispObj(ho_ring, hWindowHandle);
            }
            finally
            {
                EnableDraw = false;              
            }
        }

        //画椭圆，返回中心点坐标，角度，长度1和长度2
        public void DrawEllipse(out double centerRow, out double centerCol, out double Phi, out double Length1, out double Length2, bool overWrite = true, string color = "green", string draw = "margin")
        {
            EnableDraw = true;
            this.Focus();
            HTuple row, col, phi, length1, length2;
            try
            {
                if (ho_image != null && overWrite)
                {
                    HOperatorSet.DispObj(ho_image, hWindowHandle);
                }
                HOperatorSet.SetDraw(hWindowHandle, draw);
                HOperatorSet.SetColor(hWindowHandle, color);
                HOperatorSet.DrawEllipse(hWindowHandle, out row, out col, out phi, out length1, out length2);
                centerRow = row[0].D;
                centerCol = col[0].D;
                Phi = phi[0].D;
                Length1 = length1[0].D;
                Length2 = length2[0].D;
                HOperatorSet.GenEllipse(out ho_ellipse, row, col, phi, length1, length2);
                HOperatorSet.DispEllipse(hWindowHandle, row, col, phi, length1, length2);
            }
            finally
            {
                EnableDraw = false;
            }
        }

        //画椭圆，并返回椭圆
        public HObject DrawEllipse(bool overWrite = true, string color = "green", string draw = "margin")
        {
            EnableDraw = true;
            this.Focus();
            HOperatorSet.GenEmptyObj(out ho_ellipse);
            HTuple row, column, phi, radius1, radius2;
            try
            {
                if (ho_image != null && overWrite)
                {
                    HOperatorSet.DispObj(ho_image, hWindowHandle);
                }
                HOperatorSet.SetDraw(hWindowHandle, draw);
                HOperatorSet.SetColor(hWindowHandle, color);
                HOperatorSet.DrawEllipse(hWindowHandle, out row, out column, out phi, out radius1, out radius2);
                HOperatorSet.GenEllipse(out ho_ellipse, row, column, phi, radius1, radius2);
                HOperatorSet.DispEllipse(hWindowHandle, row, column, phi, radius1, radius2);
            }
            finally
            {
                EnableDraw = false;
            }
            return ho_ellipse;
        }

        //画区域，并返回区域
        public HObject DrawRegion(ROIShape shape = ROIShape.Rectangle1, bool overWrite = false, string color = "green", string draw = "margin")
        {
            EnableDraw = true;
            this.Focus();
            HOperatorSet.GenEmptyObj(out ho_region);
            try
            {
                if (ho_image != null)
                {
                    HTuple width, height;
                    HOperatorSet.GetImageSize(ho_image, out width, out height);
                    SystemWidth = width[0].I;
                    SystemHeight = height[0].I;
                    if (overWrite) HOperatorSet.DispObj(ho_image, hWindowHandle);
                }
                HOperatorSet.SetDraw(hWindowHandle, draw);
                HOperatorSet.SetColor(hWindowHandle, color);
                HTuple row, column, radius, phi, length1, length2;
                switch (shape)
                {
                    case ROIShape.Rectangle1: HOperatorSet.DrawRectangle1(hWindowHandle, out row, out column, out length1, out length2);
                                        HOperatorSet.GenRectangle1(out ho_region, row, column, length1, length2);
                                        this.RegionMessage.Add(row.D);
                                        this.RegionMessage.Add(column.D);
                                        this.RegionMessage.Add(length1.D);
                                        this.RegionMessage.Add(length2.D);break;
                    case ROIShape.Rectangle2: HOperatorSet.DrawRectangle2(hWindowHandle, out row, out column, out phi, out length1, out length2);
                                        HOperatorSet.GenRectangle2(out ho_region, row, column, phi, length1, length2);
                                        this.RegionMessage.Add(row.D);
                                        this.RegionMessage.Add(column.D);
                                        this.RegionMessage.Add(phi.D);
                                        this.RegionMessage.Add(length1.D);
                                        this.RegionMessage.Add(length2.D);break;
                    case ROIShape.Circle:  HOperatorSet.DrawCircle(hWindowHandle, out row,  out column, out radius);
                                        HOperatorSet.GenCircle(out ho_region, row, column, radius);
                                        this.RegionMessage.Add(row.D);
                                        this.RegionMessage.Add(column.D); 
                                        this.RegionMessage.Add(radius.D); break;
                    case ROIShape.Ellipse: HOperatorSet.DrawEllipse(hWindowHandle, out row, out column, out phi, out length1, out length2);
                                        HOperatorSet.GenEllipse(out ho_region, row, column, phi, length1, length2);
                                        this.RegionMessage.Add(row.D);
                                        this.RegionMessage.Add(column.D);
                                        this.RegionMessage.Add(phi.D);
                                        this.RegionMessage.Add(length1.D);
                                        this.RegionMessage.Add(length2.D); break;
                    case ROIShape.Arbitrarily: HOperatorSet.DrawRegion(out ho_region, hWindowHandle); break;
                }                
                HOperatorSet.DispRegion(ho_region, hWindowHandle);
            }
            finally
            {
                EnableDraw = false;
            }
            return ho_region;
        }

        //private void ReadConfig()
        //{
        //    IniFile.Exist(ConfigPath);
        //    SystemWidth = IniFile.ReadIniInt("HalconWindow", "SystemWidth", 0, ConfigPath);
        //    SystemHeight = IniFile.ReadIniInt("HalconWindow", "SystemHeight", 0, ConfigPath);
        //    if (SystemWidth > 0 || SystemHeight > 0)
        //    {
        //        HOperatorSet.SetSystem("width", SystemWidth);
        //        HOperatorSet.SetSystem("height", SystemHeight);
        //    }
        //    LineWidth = IniFile.ReadIniInt("HalconWindow", "HWindowLineWidth", 1, ConfigPath);
        //    HalconFontSize = IniFile.ReadIniInt("HalconWindow", "FontSize", 16, ConfigPath);
        //}

        //private void WriteConfig()
        //{
        //    IniFile.Exist(ConfigPath);
        //    if (SystemWidth > 0 || SystemHeight > 0)
        //    {
        //        IniFile.WriteIniInt("HalconWindow", "SystemWidth", SystemWidth, ConfigPath);
        //        IniFile.WriteIniInt("HalconWindow", "SystemHeight", SystemHeight, ConfigPath);
        //    }
        //    IniFile.WriteIniInt("HalconWindow", "HWindowLineWidth", LineWidth, ConfigPath);
        //    IniFile.WriteIniInt("HalconWindow", "FontSize", HalconFontSize, ConfigPath);
        //}

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            //WriteConfig();
        }

        #endregion
    }

    public enum ROIShape
    {
        Rectangle1 = 1,
        Rectangle2 = 2, 
        Circle = 3,
        Ellipse = 4,
        Arbitrarily = 5
    }
}
