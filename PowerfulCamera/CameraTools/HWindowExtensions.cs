using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerfulCamera.CameraTools
{
    public static class HWindowExtensions
    {
        /// <summary>
        /// 显示图像
        /// </summary>
        /// <param name="CurrentHWindow">当前窗口的索引</param>
        /// <param name="t_image">显示的图像</param>
        /// <returns></returns>
        public static bool DispImageFit(this HWindow CurrentHWindow, HObject t_image)
        {
            int current_beginRow, current_beginCol, current_endRow, current_endCol;         // 获取图像的当前显示部分
            if (t_image != null && CurrentHWindow != null)
            {
                try
                {
                    if (t_image.CountObj() == 0)
                    {
                        return false;
                    }
                    HTuple width, height;
                    HOperatorSet.GetImageSize(t_image, out width, out height);
                    current_beginRow = 0;
                    current_beginCol = 0;
                    HOperatorSet.SetPart(CurrentHWindow, current_beginRow, current_beginCol, height, width);
                    current_endRow = height;
                    current_endCol = width;
                    HOperatorSet.DispObj(t_image, CurrentHWindow);//显示图像
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }
        /// <summary>
        /// 显示文本信息
        /// </summary>
        /// <param name="hWindowHandle">句柄</param>
        /// <param name="text">文本</param>
        /// <param name="Row">行:默认10</param>
        /// <param name="Col">列:默认10</param>
        /// <param name="color">颜色:默认绿</param>
        public static void DispString(this HWindow hWindowHandle, string text, double Row = 10, double Col = 10, string color = "green")
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }
            HOperatorSet.SetTposition(hWindowHandle, Row, Col);
            HOperatorSet.SetColor(hWindowHandle, color);
            HOperatorSet.WriteString(hWindowHandle, text);
        }

        /// <summary>
        /// 显示十字
        /// </summary>
        /// <param name="hWindowHandle">句柄</param>
        /// <param name="Row">行</param>
        /// <param name="Col">列</param>
        /// <param name="CrossSize">大小:默认50</param>
        /// <param name="Angle">角度:默认0</param>
        /// <param name="color">颜色:默认绿</param>
        public static void DisplayCross(this HWindow hWindowHandle, HTuple Row, HTuple Col, int CrossSize = 50, int Angle = 0, string color = "green")
        {
            if (Angle < 0 || Angle > 360)               
                return;
            HOperatorSet.SetColor(hWindowHandle, color);
            HOperatorSet.DispCross(hWindowHandle, Row, Col, CrossSize, (new HTuple(Angle)).TupleRad());
        }

        /// <summary>
        /// 显示箭头
        /// </summary>
        /// <param name="hWindowHandle">句柄</param>
        /// <param name="startRow"></param>
        /// <param name="startCol"></param>
        /// <param name="endRow"></param>
        /// <param name="endCol"></param>
        /// <param name="arrowSize"></param>
        /// <param name="color"></param>
        public static void DisplayArrow(this HWindow hWindowHandle, double startRow, double startCol, double endRow, double endCol, int arrowSize = 5, string color = "green")
        {
            HOperatorSet.SetColor(hWindowHandle, color);
            HOperatorSet.DispArrow(hWindowHandle, startRow, startCol, endRow, endCol, arrowSize);
        }

        /// <summary>
        /// 画直线
        /// </summary>
        /// <param name="startRow">开始行</param>
        /// <param name="startCol">开始列</param>
        /// <param name="endRow">结束行</param>
        /// <param name="endCol">结束列</param>
        /// <param name="color">颜色</param>
        public static void DrawLine(this HWindow hWindowHandle, HObject ho_image, out double startRow, out double startCol, out double endRow, out double endCol, string color = "green")
        {
            //EnableDraw = true;
            //this.Focus();
            HObject ho_rect1 = null;
            startRow = 0;
            startCol = 0;
            endRow = 0;
            endCol = 0;
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
            catch(Exception ex)
            {
                Console.WriteLine($"绘制直线错误:{ex}");
            }
            finally
            {
            }
        }
    }
}
