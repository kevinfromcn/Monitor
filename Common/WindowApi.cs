/********************************************************
 * Chinese Name: 赵可(Zhao Ke)
 * English Name: Kathy Zhao
 * CreateTime: April 1, 2018 
 * Email:kathyatc@outlook.com
 * 
 * Copyright © 2018 Kathy Zhao. All Rights Reserved.
 * *******************************************************/
using System;
using System.Runtime.InteropServices;

namespace Monitor
{
    public class WindowApi
    {
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string className, string windowName);

        [DllImport("user32.dll")]//在窗口列表中寻找与指定条件相符的第一个子窗口  
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string className, string windowName);

        /// <summary>      
        /// 该函数改变指定子窗口的父窗口。      
        /// </summary>      
        [DllImport("user32.dll")]
        public extern static IntPtr SetParent(IntPtr hChild, IntPtr hParent);

        /// <summary>      
        /// 获取子窗口的父窗口。      
        /// </summary>      
        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GetParent(IntPtr hWnd);

        /// <summary>      
        /// 该函数改变指定窗口的位置和尺寸。对于顶层窗口，位置和尺寸是相对于屏幕的左上角的：对于子窗口，位置和尺寸是相对于父窗口客户区的左上角坐标的。      
        /// </summary>      
        [DllImport("user32.dll")]
        public static extern bool MoveWindow(IntPtr hWnd, IntPtr x, IntPtr y, int width, int height, int repaint);

        /// <summary>      
        /// 该函数返回指定窗口的边框矩形的尺寸。该尺寸以相对于屏幕坐标左上角的屏幕坐标给出。      
        /// </summary>      
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, ref Rectangle rect);

        /// <summary>
        /// 清理资源
        /// </summary>
        [DllImport("psapi.dll")]
        public static extern int EmptyWorkingSet(IntPtr hwProc);
    }


    public struct Rectangle
    {
        public int Left;
        public int Top;
        public int Right;
        public int Botton;


        public static bool operator ==(Rectangle rect1, Rectangle rect2)
        {
            if (rect1.Left == rect2.Left && rect1.Top == rect2.Top && rect1.Right == rect2.Right && rect1.Botton == rect2.Botton)
            {
                return true;
            }
            return false;
        }

        public static bool operator !=(Rectangle rect1, Rectangle rect2)
        {
            if (rect1.Left == rect2.Left && rect1.Top == rect2.Top && rect1.Right == rect2.Right && rect1.Botton == rect2.Botton)
            {
                return false;
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
