using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PowerfulCamera.ItemBase
{
    /// <summary>
    /// ROI
    /// </summary>
    public enum ROIShape
    {
        Rectangle1 = 1, 
        Rectangle2 = 2,
        Circle = 3,
        Ellipse = 4,
        Arbitrarily = 5
    }
    /// <summary>
    /// 工程的全局变量
    /// </summary>
    public static class GlobalBase
    {
        public static string ItemDir = Environment.CurrentDirectory;    //工作目录
        public static string CameraDir = ItemDir + "\\CameraConfig";    //摄像机列表目录
        public static string RoiDir = ItemDir + "\\Roi";                //Roi路径
        public static string VSCodeDir = "C://Users//ZYu.Li//AppData//Local//Programs//Microsoft VS Code";
        public static string CmdDir = "cmd.exe";

        /// <summary>
        /// 打开文件
        /// </summary>
        /// <param name="str">"Files (*.txt)|*.txt"</param>
        /// <returns></returns>
        public static string OpenFileDir(string str) //选择文件
        {
            string path = null;
            var openFileDialog = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = str//如果需要筛选txt文件"Files (*.txt)|*.txt"
            };
            var result = openFileDialog.ShowDialog();
            if (result == true)
            {
                path = openFileDialog.FileName;
            }
            return path;
        }

        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="str"></param>
        public static void SaveFileDir(string path, string str)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = str;
            sfd.FilterIndex = 1;
            sfd.RestoreDirectory = true;
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show(sfd.FileName.ToString());
            }
            else
            {
                MessageBox.Show("取消保存");
                return;
            }
        }
    }
}
