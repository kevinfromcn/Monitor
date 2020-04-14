using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Monitor
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region 构造函数
        public MainWindow()
        {
            InitializeComponent();

            this.WindowStyle = WindowStyle.ToolWindow;
            this.AllowsTransparency = false;
            this.Loaded += new RoutedEventHandler(WindowLoad);
            this.Closed += new EventHandler(WindowClose);
        }
        #endregion

        #region 字段属性
        /// <summary>
        /// 计时器
        /// </summary>
        private Timer _timer;

        /// <summary>
        /// 网速监视器
        /// </summary>
        private NetworkMonitor _monitor;

        /// <summary>
        /// 任务栏中内容
        /// </summary>
        private IntPtr reBarInPtr;

        /// <summary>
        /// 任务栏中空闲内容
        /// </summary>
        private IntPtr msTaskSwInPtr;

        /// <summary>
        /// 当前窗口句柄
        /// </summary>
        private IntPtr handle;

        /// <summary>
        /// 任务栏中内容位置大小信息
        /// </summary>
        private Rectangle reBarRC;

        /// <summary>
        /// 任务栏中空闲内容位置大小信息
        /// </summary>
        private Rectangle msTaskSwRC;

        /// <summary>
        /// CPU占用率监控器
        /// </summary>
        private PerformanceCounter provessor;

        /// <summary>
        /// 内存监控器
        /// </summary>
        private PerformanceCounter memery;

        /// <summary>
        /// 总内存
        /// </summary>
        private int totleMemory;

        /// <summary>
        /// cpu使用率
        /// </summary>
        private double CpuUsageRate
        {
            get
            {
                return Math.Round(this.provessor.NextValue() / 100, 4);
            }
        }

        /// <summary>
        /// cpu使用率
        /// </summary>
        private double MemoryUsageRate
        {
            get
            {
                return Math.Round((this.totleMemory - memery.NextValue()) / this.totleMemory, 4);
            }
        }
        #endregion

        #region 内外方法
        /// <summary>
        /// 窗体加载事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">信息</param>
        private void WindowLoad(object sender, RoutedEventArgs e)
        {
            this.handle = new WindowInteropHelper(this).Handle;

            //获取任务栏句柄
            IntPtr taskBarInPtr = WindowApi.FindWindow("Shell_TrayWnd", null);
            this.reBarInPtr = WindowApi.FindWindowEx(taskBarInPtr, (IntPtr)0, "ReBarWindow32", null);
            WindowApi.SetParent(this.handle, this.reBarInPtr);

            this.msTaskSwInPtr = WindowApi.FindWindowEx(this.reBarInPtr, (IntPtr)0, "MSTaskSwWClass", null);

            this.provessor = new PerformanceCounter("Processor Information", "% Processor Time", "_Total");
            this.memery = new PerformanceCounter("Memory", "Available MBytes");
            this.totleMemory = this.GetPhisicalMemory();

            this._timer = new Timer(1000);
            this._timer.Elapsed += new ElapsedEventHandler(TimerElapsedEvent);
            this._timer.Elapsed += new ElapsedEventHandler(LocationEvent);

            this._monitor = new NetworkMonitor();
            this._monitor.DownloadEvent += new NetworkMonitorEvent(DownloadSpeedEvent);
            this._monitor.UploadEvent += new NetworkMonitorEvent(UpdateSpeedEvent);

            this._monitor.StartMonitoring();
            this._timer.Start();

            this.RefreshRect();
        }

        private void WindowClose(object sender, EventArgs e)
        {
            this._monitor.Dispose();

            if (this._timer != null)
            {
                this._timer.Stop();
                this._timer.Close();
            }
        }

        /// <summary>
        /// 刷新边框
        /// </summary>
        private void RefreshRect()
        {
            Rectangle reBarRC = new Rectangle();
            Rectangle msTaskSwRC = new Rectangle();

            WindowApi.GetWindowRect(msTaskSwInPtr, ref msTaskSwRC);
            WindowApi.GetWindowRect(reBarInPtr, ref reBarRC);

            if (this.reBarRC != reBarRC || this.msTaskSwRC != msTaskSwRC)
            {
                this.reBarRC = reBarRC;
                this.msTaskSwRC = msTaskSwRC;

                if (this.msTaskSwRC.Right - this.msTaskSwRC.Left > this.reBarRC.Botton - this.reBarRC.Top)
                {
                    WindowApi.MoveWindow(this.handle, (IntPtr)(this.msTaskSwRC.Right - this.msTaskSwRC.Left - this.Width + 2),
                             (IntPtr)((this.reBarRC.Botton - this.reBarRC.Top - this.Height) / 2), (int)this.Width, (int)this.Height, 1);
                }
                else
                {
                    WindowApi.MoveWindow(this.handle, (IntPtr)((this.msTaskSwRC.Right - this.msTaskSwRC.Left - this.Width) / 2),
                                 (IntPtr)(this.reBarRC.Botton - this.reBarRC.Top - this.Height + 2), (int)this.Width, (int)this.Height, 1);
                }
            }
        }

        /// <summary>
        /// 更新网络信息
        /// </summary>
        /// <param name="download">下载速度</param>
        /// <param name="upload">上传速度</param>
        /// <param name="unitType">单位</param>
        private void DownloadSpeedEvent(double download, SpeedUnitType unitType)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    this.download.Content = this.ConvertString(download);
                    this.dunit.Content = unitType.GetStringValue();

                    this.downloadSpeed.Content = this.download.Content + " " + this.dunit.Content;
                }
                catch { }
            }));
        }

        /// <summary>
        /// 更新网络信息
        /// </summary>
        /// <param name="download">下载速度</param>
        /// <param name="upload">上传速度</param>
        /// <param name="unitType">单位</param>
        private void UpdateSpeedEvent(double upload, SpeedUnitType unitType)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    this.update.Content = this.ConvertString(upload);
                    this.uunit.Content = unitType.GetStringValue();

                    this.updateSpeed.Content = this.update.Content + " " + this.uunit.Content;
                }
                catch { }
            }));
        }

        /// <summary>
        /// 转换工具
        /// </summary>
        /// <param name="value">原始值</param>
        /// <returns>转换后值</returns>
        private string ConvertString(double value)
        {
            if (value <= 10)
            {
                return value.ToString("0.000");
            }
            else if (value < 100)
            {
                return value.ToString("00.00");
            }
            else
            {
                return value.ToString("000.0");
            }
        }


        /// <summary>
        /// 计时器事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LocationEvent(object sender, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    IntPtr taskBarInPtr = WindowApi.FindWindow("Shell_TrayWnd", null);
                    var reBarInPtr = WindowApi.FindWindowEx(taskBarInPtr, (IntPtr)0, "ReBarWindow32", null);

                    if (this.reBarInPtr != reBarInPtr)
                    {
                        this.reBarInPtr = WindowApi.SetParent(this.handle, reBarInPtr);
                    }
                }
                catch { }
            }));
        }

        /// <summary>
        /// 计时器事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerElapsedEvent(object sender, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    this.RefreshCpuAndMemory();
                    this.RefreshRect();
                }
                catch { }
            }));
        }

        /// <summary>
        /// 刷新Cpu和内存
        /// </summary>
        private void RefreshCpuAndMemory()
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                double cpurate = this.CpuUsageRate;
                double memrate = this.MemoryUsageRate;
                this.cpucontent.Height = cpurate * (this.cpuborder.ActualHeight - 2);
                this.memcontent.Height = this.MemoryUsageRate * (this.cpuborder.ActualHeight - 2);

                this.cpuRate.Content = (cpurate * 100).ToString("00.00") + " %";
                this.memRate.Content = (memrate * 100).ToString("00.00") + " %";
            }));
        }

        /// <summary>
        /// 获取系统内存大小
        /// </summary>
        /// <returns>内存大小（单位M）</returns>
        private int GetPhisicalMemory()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher();   //用于查询一些如系统信息的管理对象 
            searcher.Query = new SelectQuery("Win32_PhysicalMemory ", "", new string[] { "Capacity" });//设置查询条件 
            ManagementObjectCollection collection = searcher.Get();   //获取内存容量 
            ManagementObjectCollection.ManagementObjectEnumerator em = collection.GetEnumerator();

            long capacity = 0;
            while (em.MoveNext())
            {
                ManagementBaseObject baseObj = em.Current;
                if (baseObj.Properties["Capacity"].Value != null)
                {
                    try
                    {
                        capacity += long.Parse(baseObj.Properties["Capacity"].Value.ToString());
                    }
                    catch
                    {
                        return 0;
                    }
                }
            }

            return (int)(capacity / 1024 / 1024);
        }

        /// <summary>
        /// 清理内存信息
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">信息</param>
        private void CleanMemory(object sender, MouseButtonEventArgs e)
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Process[] processes = Process.GetProcesses();
                foreach (Process process in processes)
                {
                    //对于系统进程会拒绝访问，导致出错，此处对异常不进行处理。
                    try
                    {
                        WindowApi.EmptyWorkingSet(process.Handle);
                    }
                    catch { }
                }
            });
        }
        #endregion
    }
}
