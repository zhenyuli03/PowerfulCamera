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
using MahApps.Metro.Controls;
using PowerfulCamera.CameraSetting;
using PowerfulCamera.CameraTools;
using System.Collections.ObjectModel;


namespace PowerfulCamera
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private MainWindowViewModel mainWindowViewModel = new MainWindowViewModel();
        //public Dictionary<int, GroupBox> groupBoxDictionary = new Dictionary<int, GroupBox>();          
        //public Dictionary<int, StackPanel> stackPanelDictionary = new Dictionary<int, StackPanel>();
        //public Dictionary<int, HalconWindow> halconDictionary = new Dictionary<int, HalconWindow>();
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = mainWindowViewModel;
            mainWindowViewModel.Misssion();
        }

        public HalconWindow AddHalconWindow(int index)
        {
            throw new NotImplementedException();
            ///*CameraScrollViewer
            // *        |-->groupBox
            // *              |-->StackPanel
            // *                      |-->HalconWindow
            // *                      |
            // *                      |-->StackPanel
            // *                              |-->DockPanel
            // *                                      |-->TextBlock:Device:
            // *                                      |-->TextBlock(Binding CameraDevice)
            // *                              |-->DockPanel
            // *                                      |-->TextBlock:FPS:
            // *                                      |-->TextBlock(Binding CameraFPS)
            // */
            ///*先添加GroupBox*/
            //if(groupBoxDictionary.ContainsKey(index) || stackPanelDictionary.ContainsKey(index) || halconDictionary.ContainsKey(index))
            //{
            //    return null;
            //}
            //GroupBox groupBox = new GroupBox();
            //groupBox.Name = "CameraGroupBox" + index;
            //groupBox.Header = "摄像头" + index;
            //groupBox.FontWeight = FontWeights.Bold;
            //groupBox.Margin = new Thickness(0, 10, 0, 0);
            //groupBox.FontSize = 12;
            //groupBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1C93E2"));
            ////groupBox.BorderThickness = new Thickness(3, 0, 0, 0);
            //groupBox.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1C93E2"));
            ///*进字典*/
            //groupBoxDictionary.Add(index, groupBox);

            ///*再设置StackPanel*/
            //StackPanel stackPanel = new StackPanel();
            //stackPanel.Name = "CameraStackpanel" + index;
            //stackPanel.VerticalAlignment = VerticalAlignment.Top;
            ///*进字典*/
            //stackPanelDictionary.Add(index, stackPanel);

            ///*再设置HalconWindow*/
            //HalconWindow halconWindow = new HalconWindow();
            //halconWindow.Name = "HalconWindow" + index;
            //halconWindow.Index = index;
            //halconWindow.Height = 300;
            //halconDictionary.Add(index, halconWindow);

            ///*再设置子StackPanel*/
            //StackPanel stackPanel1 = new StackPanel();
            //stackPanel1.Name = "CamerInfo" + index;

            ///*信息框*/
            //Grid grid = new Grid();
            //RowDefinition row1 = new RowDefinition();
            //RowDefinition row2 = new RowDefinition();
            //row1.Height = GridLength.Auto;
            //row2.Height = GridLength.Auto;
            //grid.RowDefinitions.Add(row1);
            //grid.RowDefinitions.Add(row2);

            //DockPanel dockPanel = new DockPanel();
            //dockPanel.HorizontalAlignment = HorizontalAlignment.Left;
            //TextBlock textBlock = new TextBlock();
            //textBlock.Text = "Device:";
            //TextBlock textBlock1 = new TextBlock();
            //textBlock1.SetBinding(TextBlock.TextProperty, new Binding("Device") { Source = mainWindowViewModel.AllCameraList[index] }); //绑定设备号
            //dockPanel.Children.Add(textBlock);
            //dockPanel.Children.Add(textBlock1);

            //DockPanel dockPanel1 = new DockPanel();
            //dockPanel1.HorizontalAlignment = HorizontalAlignment.Left;
            //TextBlock textBlock2 = new TextBlock();
            //textBlock2.Text = "FPS:";
            //TextBlock textBlock3 = new TextBlock();
            //textBlock3.SetBinding(TextBlock.TextProperty, new Binding("CameraFPS") { Source = mainWindowViewModel.AllCameraList[index] }); //绑定FPS
            //dockPanel1.Children.Add(textBlock2);
            //dockPanel1.Children.Add(textBlock3);

            //grid.Children.Add(dockPanel); grid.Children.Add(dockPanel1);
            //Grid.SetRow(dockPanel, 0);  Grid.SetRow(dockPanel1, 1);

            ///*开始添加进入*/
            //stackPanel1.Children.Add(grid);

            //stackPanel.Children.Add(halconWindow);
            //stackPanel.Children.Add(stackPanel1);

            //groupBox.Content                                                                                = stackPanel;
            ////CameraStackPanelConterl.Content = groupBox;
            //CameraStackPanelConterl.Children.Add(groupBox);
            //return halconWindow;            
        }

        public void RemoveHalconWindow(int index)
        {

        }
    }
}
