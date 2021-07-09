using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using HalconDotNet;
using PowerfulCamera.CameraTools;
using PowerfulCamera.ItemBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
using Camera = PowerfulCamera.CameraTools.Camera; //指定来源

namespace PowerfulCamera.CameraSetting
{
    public class CameraSettingPageViewModel : ViewModelBase
    {
        #region ##成员
        private int WorkNum = 0;
        private string currentItemName;
        private HWindow currentHWindow;
        public List<HWindow> MainWindowHWindowList = new List<HWindow>();
        public List<Camera> CameraWorkList = Cameras.CameraWorkList;
        private bool mergeIsChecked;                //合并
        private bool intersectionIsChecked;         //交集
        private bool subtractionIsChecked;          //差集
        private string leftButtonVisibility;
        private string rightButtonVisibility;
        public int CurrentDisplayCameraID = 0;
        private bool stopGrabIsChecked;
        private string progressBar;
        private double currentExposureValue;        //当前曝光值
        private double exposureSliderMinNum;        //曝光最小值
        private double exposureSliderMaxNum;        //曝光最大值
        private double currentGainValue;            //当前增益值
        private double gainSliderMinNum;            //增益最小值
        private double gainSliderMaxNum;            //增益最大值
        #endregion


        #region ##属性
        public double CurrentGainValue
        {
            set
            {
                currentGainValue = value;
                RaisePropertyChanged();
            }
            get => currentGainValue;
        }
        public double GainSliderMinNum
        {
            set
            {
                gainSliderMinNum = value;
                RaisePropertyChanged();
            }
            get => gainSliderMinNum;
        }
        public double GainSliderMaxNum
        {
            set
            {
                gainSliderMaxNum = value;
                RaisePropertyChanged();
            }
            get => gainSliderMaxNum;
        }
        public double CurrentExposureValue
        {
            set 
            {
                currentExposureValue = value;
                RaisePropertyChanged();
            }
            get => currentExposureValue;
        }
        public double ExposureSliderMinNum
        {
            set
            {
                exposureSliderMinNum = value;
                RaisePropertyChanged();
            }
            get => exposureSliderMinNum;
        }
        public double ExposureSliderMaxNum
        {
            set
            {
                exposureSliderMaxNum = value;
                RaisePropertyChanged();
            }
            get => exposureSliderMaxNum;
        }
        public bool MergeIsChecked
        {
            set
            {
                mergeIsChecked = value;
                RaisePropertyChanged();
            }
            get => mergeIsChecked;
        }
        public bool StopGrabIsChecked
        {
            set
            {
                stopGrabIsChecked = value;
                RaisePropertyChanged();
            }
            get => stopGrabIsChecked;
        }
        public string LeftButtonVisibility
        {
            set { leftButtonVisibility = value;RaisePropertyChanged(); }
            get => leftButtonVisibility;
        }
        public string RightButtonVisibility
        {
            set { rightButtonVisibility = value;RaisePropertyChanged(); }
            get => rightButtonVisibility;
        }
        public bool IntersectionIsChecked
        {
            set
            {
                intersectionIsChecked = value;
                RaisePropertyChanged();
            }
            get => intersectionIsChecked;
        }
        public bool SubtractionIsChecked
        {
            set
            {
                subtractionIsChecked = value;
                RaisePropertyChanged();
            }
            get => subtractionIsChecked;
        }
        public string CurrentItemName
        {
            set
            {
                currentItemName = value; 
                RaisePropertyChanged();
            }
            get => currentItemName;
        }
        public HWindow CurrentHWindow
        {
            set => currentHWindow = value;
            get => currentHWindow;
        }
        public string ProgressBar
        {
            set
            {
                progressBar = value;
                RaisePropertyChanged();
            }
            get => progressBar;
        }
        public RelayCommand LeftButtonCommand { set; get; } //左
        public RelayCommand RightButtonCommand { set; get; }//右
        public RelayCommand CleanROICommand { set; get; }
        public RelayCommand DrawLineCommand { set; get; }
        public RelayCommand DrawCircleCommand { set; get; }
        public RelayCommand DrawEllipseCommand { set; get; }
        public RelayCommand DrawRectangleCommand { set; get; }
        public RelayCommand DrawVectorRectangleCommand { set; get; }
        public RelayCommand DrawAnythingCommand { set; get; }
        public RelayCommand StopGrabClick { set; get; }
        public RelayCommand OpenImageFileCommand { set; get; }
        #endregion

        /// <summary>
        /// CameraSettingPageViewModel构造函数
        /// </summary>
        public CameraSettingPageViewModel()
        {
            ProgressBar = "Collapsed";
            MergeIsChecked = true;
            CurrentDisplayCameraID = 0;
            LeftButtonVisibility = "Collapsed";
            RightButtonVisibility = "Visible";
            if (CameraWorkList.Count() > 0)
            {
                CurrentItemName = CameraWorkList[0].Name;
                CurrentGainValue = CameraWorkList[0].GainRaw;
                if(CameraWorkList[0].Name == "ChinaVision17_X64")
                {
                    GainSliderMaxNum = 7.28;
                    GainSliderMinNum = 1.28;
                }
            }
            if (CameraWorkList.Count() == 1 || CameraWorkList.Count() == 0)
            {
                LeftButtonVisibility = "Collapsed";
                RightButtonVisibility = "Collapsed";
            }
            LeftButtonCommand = new RelayCommand(() =>
            {
                LeftButtonEvent();
            });
            RightButtonCommand = new RelayCommand(() =>
            {
                RightButtonEvent();
            });
            StopGrabClick = new RelayCommand(() =>
            {
                StopGrabEvent();
            });
            DrawLineCommand = new RelayCommand(() =>
            {
                DrawLine("green");
            });
            DrawCircleCommand = new RelayCommand(() =>
            {
                DrawCircle("green");
            });
            DrawEllipseCommand = new RelayCommand(() =>
            {
                DrawEllipse("green");
            });
            DrawRectangleCommand = new RelayCommand(() =>
            {
                DrawRectangle("green");
            });
            DrawVectorRectangleCommand = new RelayCommand(() =>
            {
                DrawVectorRectangle("green");
            });
            DrawAnythingCommand = new RelayCommand(() =>
            {
                DrawAnything("green");
            });
            OpenImageFileCommand = new RelayCommand(() =>
            {
                OpenImageFile(GlobalBase.OpenFileDir("所有图像文件|*.bmp;*.pcx;*.png;*.jpg"));
            });
            CleanROICommand = new RelayCommand(() =>
            {
                HObject image = null;
                string fileFullPath = GlobalBase.RoiDir + "\\" + CameraWorkList[CurrentDisplayCameraID].Name + " & " + CameraWorkList[CurrentDisplayCameraID].Index + ".reg";
                CameraWorkList[CurrentDisplayCameraID].ImageROI = null; //清空
                CameraWorkList[CurrentDisplayCameraID].GrabImage(out image);
                currentHWindow.DispImageFit(image);
                try
                {
                    if (File.Exists(fileFullPath))
                    {
                        File.Delete(fileFullPath);
                    }
                    Console.WriteLine($"删除成功");
                }
                catch
                {

                }
            });
        }

        /// <summary>
        /// 主任务
        /// </summary>
        public void Mission()
        {
            /*切换窗口线程,切换完成，自动退出*/
            Task.Run(() =>
            {
                string roipath;
                HObject region = null;
                HTuple area, row, column;
                while (true)
                {
                    if (this.CurrentHWindow != null)
                    {
                        MainWindowHWindowList.Clear();
                        for (int i = 0; i < CameraWorkList.Count(); i++)
                        {
                            WorkNum++;
                            CameraWorkList[i].Stop();
                            MainWindowHWindowList.Add(CameraWorkList[i].CurrentHWindow);    //把之前的窗口保存起来
                            CameraWorkList[i].CurrentHWindow = this.CurrentHWindow;
                            CameraWorkList[i].Play();
                            roipath = GlobalBase.RoiDir + "\\" + CameraWorkList[i].Name + " & " + CameraWorkList[i].Index + ".hobj";
                            if (File.Exists(roipath))
                            {
                                HOperatorSet.ReadRegion(out region, roipath);
                                HOperatorSet.AreaCenter(region, out area, out row, out column);
                                Console.WriteLine($"面积:{(double)area},行{(double)row},列{(double)column}");
                                CameraWorkList[i].ImageROI = region;
                            }
                        }
                        break;
                    }
                }
                Console.WriteLine($"切换窗口线程退出");
            });
        }
        /// <summary>
        /// 左按钮触发函数
        /// </summary>
        public void LeftButtonEvent()
        {
            CurrentDisplayCameraID--;
            if (CurrentDisplayCameraID == 0)
            {
                LeftButtonVisibility = "Collapsed";
                RightButtonVisibility = "Visible";
                CurrentItemName = CameraWorkList[CurrentDisplayCameraID].Name;
            }
        }
        /// <summary>
        /// 右按钮触发函数
        /// </summary>
        public void RightButtonEvent()
        {
            CurrentDisplayCameraID++;
            if (CurrentDisplayCameraID == CameraWorkList.Count()-1)
            {
                RightButtonVisibility = "Collapsed";
                LeftButtonVisibility = "Visible";
                CurrentItemName = CameraWorkList[CurrentDisplayCameraID].Name;
            }                
        }
        /// <summary>
        /// 停止采集按钮触发函数
        /// </summary>
        public void StopGrabEvent()
        {
            HObject Image = null;
            try
            {
                if (CameraWorkList[CurrentDisplayCameraID].IsOpen)
                {
                    CameraWorkList[CurrentDisplayCameraID].Stop();
                    CameraWorkList[CurrentDisplayCameraID].GrabImage(out Image);
                }
                else if (!CameraWorkList[CurrentDisplayCameraID].IsOpen)
                    CameraWorkList[CurrentDisplayCameraID].Play();
            }
            catch
            {
                Console.WriteLine($"停止失败");
            }
        }

        /// <summary>
        /// 画直线
        /// </summary>
        /// <param name="color">颜色:蓝</param>
        /// <param name="width">线宽:3</param>
        /// <returns></returns>
        public HObject DrawLine(string color = "blue",int width = 3)
        {
            HObject image = null;
            HObject LineRoi = null;
            HTuple row, column, row2, column2;
            HObject LastRoi = null;
            ProgressBar = "Visible";
            if (CameraWorkList.Count() > 0)
            {
                StopGrabIsChecked = true;
                CameraWorkList[CurrentDisplayCameraID].Stop();
                LastRoi = CameraWorkList[CurrentDisplayCameraID].ImageROI;
            }
            HOperatorSet.GenEmptyObj(out LineRoi);
            try
            {
                HOperatorSet.SetLineWidth(CurrentHWindow, width);
                /*采图*/
                CameraWorkList[CurrentDisplayCameraID].GrabImage(out image);
                currentHWindow.DispImageFit(image);
                if (LastRoi != null)
                {
                    HOperatorSet.DispRegion(LastRoi, currentHWindow);
                }
                /*生成ROI*/
                HOperatorSet.SetColor(this.CurrentHWindow, color);
                HOperatorSet.DrawLine(currentHWindow, out row, out column, out row2, out column2);
                HOperatorSet.GenRegionLine(out LineRoi, row, column, row2, column2);
                /*显示ROI*/
                if (LastRoi != null)
                {
                    if (MergeIsChecked) //合并
                    {
                        HOperatorSet.Union2(LineRoi, LastRoi, out LineRoi);
                    }
                    else if (IntersectionIsChecked) //交集
                    {
                        HOperatorSet.Intersection(LineRoi, LastRoi, out LineRoi);
                    }
                    else if (SubtractionIsChecked) //差集
                    {
                        HOperatorSet.Difference(LineRoi, LastRoi, out LineRoi);
                    }
                }
                currentHWindow.DispImageFit(image);
                HOperatorSet.DispRegion(LineRoi, currentHWindow);
                if (CameraWorkList.Count() > 0)
                {
                    CameraWorkList[CurrentDisplayCameraID].ImageROI = LineRoi;
                    if (SaveRoi(LineRoi, GlobalBase.RoiDir + "\\" + CameraWorkList[CurrentDisplayCameraID].Name + " & " + CameraWorkList[CurrentDisplayCameraID].Index + ".hobj"))
                    {
                        MessageBox.Show($"保存成功");
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"绘制直线失败:{ex}");
            }
            ProgressBar = "Collapsed";
            return LineRoi;
        }

        /// <summary>
        /// 画矩形
        /// </summary>
        /// <param name="color">黑色</param>
        /// <param name="draw">边缘模式</param>
        /// <param name="width">线宽3</param>
        /// <returns></returns>
        public HObject DrawRectangle(string color = "blue", string draw = "margin",int width = 3)
        {
            HObject image = null;
            HObject RectangleRoi = null;
            HObject LastRoi = null;
            HTuple row, column, row2, column2;
            ProgressBar = "Visible";
            HOperatorSet.GenEmptyObj(out LastRoi);
            if (CameraWorkList.Count() > 0)
            {
                StopGrabIsChecked = true;
                CameraWorkList[CurrentDisplayCameraID].Stop();
                LastRoi = CameraWorkList[CurrentDisplayCameraID].ImageROI;
            }
            HOperatorSet.GenEmptyObj(out RectangleRoi);
            try
            {
                HOperatorSet.SetLineWidth(CurrentHWindow, width);
                HOperatorSet.SetDraw(this.CurrentHWindow, draw);
                /*采图*/
                CameraWorkList[CurrentDisplayCameraID].GrabImage(out image);
                currentHWindow.DispImageFit(image);
                if(LastRoi != null)
                {
                    HOperatorSet.DispRegion(LastRoi, currentHWindow);
                }
                /*生成ROI*/
                HOperatorSet.SetColor(this.CurrentHWindow, color);
                HOperatorSet.DrawRectangle1(this.CurrentHWindow, out row, out column, out row2, out column2);
                HOperatorSet.GenRectangle1(out RectangleRoi, row, column, row2, column2);
                /*显示ROI*/
                if(LastRoi != null)
                {
                    if(MergeIsChecked) //合并
                    {
                        HOperatorSet.Union2(RectangleRoi, LastRoi, out RectangleRoi);
                    }
                    else if(IntersectionIsChecked) //交集
                    {
                        HOperatorSet.Intersection(RectangleRoi, LastRoi, out RectangleRoi);
                    }
                    else if(SubtractionIsChecked) //差集
                    {
                        HOperatorSet.Difference(RectangleRoi, LastRoi, out RectangleRoi);
                    }
                }
                currentHWindow.DispImageFit(image);
                HOperatorSet.DispRegion(RectangleRoi, currentHWindow);
                if (CameraWorkList.Count() > 0)
                {
                    CameraWorkList[CurrentDisplayCameraID].ImageROI = RectangleRoi;
                    if (SaveRoi(RectangleRoi, GlobalBase.RoiDir + "\\" + CameraWorkList[CurrentDisplayCameraID].Name + " & " + CameraWorkList[CurrentDisplayCameraID].Index + ".hobj"))
                    {
                        MessageBox.Show($"保存成功");
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"绘制矩形失败:{ex}");
            }
            ProgressBar = "Collapsed";
            return RectangleRoi;
        }


        /// <summary>
        /// 画圆
        /// </summary>
        /// <param name="color">颜色</param>
        /// <param name="draw">绘画方式</param>
        /// <param name="width">线宽</param>
        /// <returns></returns>
        public HObject DrawCircle(string color = "blue", string draw = "margin",int width = 3)
        {
            HObject image = null;
            HObject CircleRoi = null;
            HTuple row, column, radius;
            HObject LastRoi = null;
            ProgressBar = "Visible";
            if (CameraWorkList.Count() > 0)
            {
                StopGrabIsChecked = true;
                CameraWorkList[CurrentDisplayCameraID].Stop();
                LastRoi = CameraWorkList[CurrentDisplayCameraID].ImageROI;
            }
            HOperatorSet.GenEmptyObj(out CircleRoi);
            try
            {
                HOperatorSet.SetLineWidth(CurrentHWindow, width);
                HOperatorSet.SetDraw(this.CurrentHWindow, draw);
                /*采图*/
                CameraWorkList[CurrentDisplayCameraID].GrabImage(out image);
                currentHWindow.DispImageFit(image);
                if (LastRoi != null)
                {
                    HOperatorSet.DispRegion(LastRoi, currentHWindow);
                }
                /*生成ROI*/
                HOperatorSet.SetColor(this.CurrentHWindow, color);
                HOperatorSet.DrawCircle(this.CurrentHWindow, out row, out column, out radius);
                HOperatorSet.GenCircle(out CircleRoi, row, column, radius);
                /*显示ROI*/
                if (LastRoi != null)
                {
                    if (MergeIsChecked) //合并
                    {
                        HOperatorSet.Union2(CircleRoi, LastRoi, out CircleRoi);
                    }
                    else if (IntersectionIsChecked) //交集
                    {
                        HOperatorSet.Intersection(CircleRoi, LastRoi, out CircleRoi);
                    }
                    else if (SubtractionIsChecked) //差集
                    {
                        HOperatorSet.Difference(CircleRoi, LastRoi, out CircleRoi);
                    }
                }
                currentHWindow.DispImageFit(image);
                HOperatorSet.DispRegion(CircleRoi, currentHWindow);
                if (CameraWorkList.Count() > 0)
                {
                    CameraWorkList[CurrentDisplayCameraID].ImageROI = CircleRoi;
                    if (SaveRoi(CircleRoi, GlobalBase.RoiDir + "\\" + CameraWorkList[CurrentDisplayCameraID].Name + " & " + CameraWorkList[CurrentDisplayCameraID].Index + ".hobj"))
                    {
                        MessageBox.Show($"保存成功");
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"绘制圆形失败:{ex}");
            }
            ProgressBar = "Collapsed";
            return CircleRoi;
        }

        /// <summary>
        /// 画椭圆
        /// </summary>
        /// <param name="color">颜色</param>
        /// <param name="draw">绘画方式</param>
        /// <param name="width">线宽</param>
        /// <returns></returns>
        public HObject DrawEllipse(string color = "blue", string draw = "margin", int width = 3)
        {
            HObject image = null;
            HObject EllipseRoi = null;
            HTuple row, column, phi, radius1, radius2;
            HObject LastRoi = null;
            ProgressBar = "Visible";
            if (CameraWorkList.Count() > 0)
            {
                StopGrabIsChecked = true;
                CameraWorkList[CurrentDisplayCameraID].Stop();
                LastRoi = CameraWorkList[CurrentDisplayCameraID].ImageROI;
            }
            HOperatorSet.GenEmptyObj(out EllipseRoi);
            try
            {
                HOperatorSet.SetLineWidth(CurrentHWindow, width);
                HOperatorSet.SetDraw(this.CurrentHWindow, draw);
                /*采图*/
                CameraWorkList[CurrentDisplayCameraID].GrabImage(out image);
                currentHWindow.DispImageFit(image);
                if (LastRoi != null)
                {
                    HOperatorSet.DispRegion(LastRoi, currentHWindow);
                }
                /*生成ROI*/
                HOperatorSet.SetColor(this.CurrentHWindow, color);
                HOperatorSet.DrawEllipse(this.CurrentHWindow, out row, out column, out phi, out radius1, out radius2);
                HOperatorSet.GenEllipse(out EllipseRoi, row, column, phi, radius1, radius2);
                /*显示ROI*/
                if (LastRoi != null)
                {
                    if (MergeIsChecked) //合并
                    {
                        HOperatorSet.Union2(EllipseRoi, LastRoi, out EllipseRoi);
                    }
                    else if (IntersectionIsChecked) //交集
                    {
                        HOperatorSet.Intersection(EllipseRoi, LastRoi, out EllipseRoi);
                    }
                    else if (SubtractionIsChecked) //差集
                    {
                        HOperatorSet.Difference(EllipseRoi, LastRoi, out EllipseRoi);
                    }
                }
                currentHWindow.DispImageFit(image);
                HOperatorSet.DispRegion(EllipseRoi, currentHWindow);
                if (CameraWorkList.Count() > 0)
                {
                    CameraWorkList[CurrentDisplayCameraID].ImageROI = EllipseRoi;
                    if (SaveRoi(EllipseRoi, GlobalBase.RoiDir + "\\" + CameraWorkList[CurrentDisplayCameraID].Name + " & " + CameraWorkList[CurrentDisplayCameraID].Index + ".hobj"))
                    {
                        MessageBox.Show($"保存成功");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"绘制椭圆形失败:{ex}");
            }
            ProgressBar = "Collapsed";
            return EllipseRoi;
        }

        /// <summary>
        /// 画带方向的矩形
        /// </summary>
        /// <param name="color">颜色</param>
        /// <param name="draw">绘画方式</param>
        /// <param name="width">线宽</param>
        /// <returns></returns>
        public HObject DrawVectorRectangle(string color = "blue", string draw = "margin", int width = 3)
        {
            HObject image = null;
            HObject VectorRectangle = null;
            HTuple row, column, phi, length1, length2;
            HObject LastRoi = null;
            ProgressBar = "Visible";
            if (CameraWorkList.Count() > 0)
            {
                StopGrabIsChecked = true;
                CameraWorkList[CurrentDisplayCameraID].Stop();
                LastRoi = CameraWorkList[CurrentDisplayCameraID].ImageROI;
            }
            HOperatorSet.GenEmptyObj(out VectorRectangle);
            try
            {
                HOperatorSet.SetLineWidth(CurrentHWindow, width);
                HOperatorSet.SetDraw(this.CurrentHWindow, draw);
                /*采图*/
                CameraWorkList[CurrentDisplayCameraID].GrabImage(out image);
                currentHWindow.DispImageFit(image);
                if (LastRoi != null)
                {
                    HOperatorSet.DispRegion(LastRoi, currentHWindow);
                }
                /*生成ROI*/
                HOperatorSet.SetColor(this.CurrentHWindow, color);
                HOperatorSet.DrawRectangle2(CurrentHWindow, out row, out column, out phi, out length1, out length2);
                HOperatorSet.GenRectangle2(out VectorRectangle, row, column, phi, length1, length2);
                /*显示ROI*/
                if (LastRoi != null)
                {
                    if (MergeIsChecked) //合并
                    {
                        HOperatorSet.Union2(VectorRectangle, LastRoi, out VectorRectangle);
                    }
                    else if (IntersectionIsChecked) //交集
                    {
                        HOperatorSet.Intersection(VectorRectangle, LastRoi, out VectorRectangle);
                    }
                    else if (SubtractionIsChecked) //差集
                    {
                        HOperatorSet.Difference(VectorRectangle, LastRoi, out VectorRectangle);
                    }
                }
                currentHWindow.DispImageFit(image);
                HOperatorSet.DispRegion(VectorRectangle, currentHWindow);
                if (CameraWorkList.Count() > 0)
                {
                    CameraWorkList[CurrentDisplayCameraID].ImageROI = VectorRectangle;
                    if (SaveRoi(VectorRectangle, GlobalBase.RoiDir + "\\" + CameraWorkList[CurrentDisplayCameraID].Name + " & " + CameraWorkList[CurrentDisplayCameraID].Index + ".hobj"))
                    {
                        MessageBox.Show($"保存成功");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"绘制方向矩形失败:{ex}");
            }
            ProgressBar = "Collapsed";
            return VectorRectangle;
        }

        /// <summary>
        /// 绘制任意区域
        /// </summary>
        /// <param name="color">颜色</param>
        /// <param name="draw">绘画</param>
        /// <param name="width">线宽</param>
        /// <returns></returns>
        public HObject DrawAnything(string color = "blue", string draw = "margin", int width = 3)
        {
            HObject image = null;
            HObject Rectangle = null;
            HObject LastRoi = null;
            ProgressBar = "Visible";
            if (CameraWorkList.Count() > 0)
            {
                StopGrabIsChecked = true;
                CameraWorkList[CurrentDisplayCameraID].Stop();
                LastRoi = CameraWorkList[CurrentDisplayCameraID].ImageROI;
            }
            HOperatorSet.GenEmptyObj(out Rectangle);
            try
            {
                HOperatorSet.SetLineWidth(CurrentHWindow, width);
                HOperatorSet.SetDraw(this.CurrentHWindow, draw);
                /*采图*/
                CameraWorkList[CurrentDisplayCameraID].GrabImage(out image);
                currentHWindow.DispImageFit(image);
                if (LastRoi != null)
                {
                    HOperatorSet.DispRegion(LastRoi, currentHWindow);
                }
                /*生成ROI*/
                HOperatorSet.SetColor(this.CurrentHWindow, color);
                HOperatorSet.DrawRegion(out Rectangle, currentHWindow);
                /*显示ROI*/
                if (LastRoi != null)
                {
                    if (MergeIsChecked) //合并
                    {
                        HOperatorSet.Union2(Rectangle, LastRoi, out Rectangle);
                    }
                    else if (IntersectionIsChecked) //交集
                    {
                        HOperatorSet.Intersection(Rectangle, LastRoi, out Rectangle);
                    }
                    else if (SubtractionIsChecked) //差集
                    {
                        HOperatorSet.Difference(Rectangle, LastRoi, out Rectangle);
                    }
                }
                if (CameraWorkList.Count() > 0)
                {
                    CameraWorkList[CurrentDisplayCameraID].ImageROI = Rectangle;
                }
                currentHWindow.DispImageFit(image);
                HOperatorSet.DispRegion(Rectangle, currentHWindow);
                MessageBox.Show($"绘制成功", "提示", MessageBoxButtons.OK);
            }
            catch(Exception ex)
            {
                MessageBox.Show($"绘制区域失败:{ex}");
            }
            ProgressBar = "Collapsed";
            return Rectangle;
        }

        /// <summary>
        /// 打开图像文件
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns></returns>
        public bool OpenImageFile(string path)
        {
            HObject image = null;
            HOperatorSet.GenEmptyObj(out image);
            try
            {
                Console.WriteLine(path);
                if(path != null)
                {
                    HOperatorSet.ReadImage(out image, path);
                    CurrentHWindow.DispImageFit(image);
                }
            }
            catch
            {
                Console.WriteLine($"打开图像失败");
                MessageBox.Show($"打开图像失败");
            }
            return true;
        }

        /// <summary>
        /// 保存ROI
        /// </summary>
        /// <param name="region">ROI</param>
        /// <param name="path">保存路径</param>
        /// <returns></returns>
        public bool SaveRoi(HObject region, string path)
        {
            if(region.CountObj() > 0)
            {
                try
                {
                    HOperatorSet.WriteRegion(region, path);
                    return true;
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"保存失败{ex}");
                    MessageBox.Show($"保存ROI失败,请检查路径是否正确","错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            return false;
        }
    }
}
