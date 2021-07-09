using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using HalconDotNet;
using PowerfulCamera.CameraTools;
using System.IO;
using GalaSoft.MvvmLight.Command;
using PowerfulCamera.CameraSetting;
using System.Collections.ObjectModel;
using System.Threading;
using PowerfulCamera.PythonEdit;

namespace PowerfulCamera
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region ##成员
        private ObservableCollection<Camera> cameras = new ObservableCollection<Camera>();
        private string progressBar;
        #endregion

        #region ##属性区域
        public ObservableCollection<Camera> CamerasList
        {
            set
            {
                cameras = value;
                RaisePropertyChanged();
            }
            get => cameras;
        }
        public string ProgressBar /*"Collapsed" Or "Visible"*/
        {
            set { progressBar = value; RaisePropertyChanged(); }
            get => progressBar;
        }
        public Dictionary<int, Camera> AllCamera = new Dictionary<int, Camera>();   //摄像头表
        /*界面*/
        public RelayCommand CameraCommand { set; get; }
        public RelayCommand SwitchButtonCommand { set; get; }
        public RelayCommand PlayButtonCommand { set; get; }
        public RelayCommand StopButtonCommand { set; get; }
        public RelayCommand RestartButtonCommand { set; get; }
        public RelayCommand ModelButtonCommand { set; get; }
        public RelayCommand LightButtonCommand { set; get; }
        public RelayCommand CommunicationButtonCommand { set; get; }
        public RelayCommand VesionButtonCommand { set; get; }
        public RelayCommand CreatMissionCommand { get; set; }

        /*子界面*/
        public CameraSettingPage cameraSettingPage;
        public PythonEditWindow pythonEditWindow;
        #endregion

        public MainWindowViewModel()
        {
            /*摄像头界面*/
            CameraCommand = new RelayCommand(() =>
            {
                cameraSettingPage = new CameraSettingPage();
                cameraSettingPage.Show();

            });
            /*关闭应用程序*/
            SwitchButtonCommand = new RelayCommand(() =>
            {
                Environment.Exit(0);
            });
            PlayButtonCommand = new RelayCommand(() =>
            {
                for (int i = 0; i < CamerasList.Count(); i++)
                {
                    CamerasList[i].CameraSwitchIsOn = true;
                }
            });
            StopButtonCommand = new RelayCommand(() =>
            {
                for (int i = 0; i < CamerasList.Count(); i++)
                {
                    CamerasList[i].CameraSwitchIsOn = false;
                }
            });
            CreatMissionCommand = new RelayCommand(() =>
            {
                pythonEditWindow = new PythonEditWindow();
                pythonEditWindow.Show();
            });
        }

        /// <summary>
        /// 主任务线程
        /// </summary>
        public void Misssion()
        {
            CameraInit();
            CameraWork();
            ProgressBar = "Collapsed";
            Task.Run(() =>
            {

            });
        }

        /// <summary>
        /// 摄像头初始化
        /// </summary>
        private void CameraInit()
        {
            Camera camera = null;
            try
            {
                Cameras.FindCameras(out AllCamera);       //查找目录下面的Camera
                for (int i = 0; i < AllCamera.Count(); i++)
                {
                    if (AllCamera.ContainsKey(i))
                    {
                        if (AllCamera.TryGetValue(i, out camera))
                        {
                            //连接成功则添加
                            if (camera.Connect())
                            {
                                CamerasList.Add(camera);
                                Cameras.CameraWorkList.Add(camera);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// 摄像头工作
        /// </summary>
        private void CameraWork()
        {
            for (int i = 0; i < CamerasList.Count(); i++)
            {
                CamerasList[i].GrabImageAsync();
            }
        }
    }
}
