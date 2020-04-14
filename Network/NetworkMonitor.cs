/********************************************************
 * Chinese Name: 赵可(Zhao Ke)
 * English Name: Kathy Zhao
 * CreateTime: April 1, 2018 
 * Email:kathyatc@outlook.com
 * 
 * Copyright © 2018 Kathy Zhao. All Rights Reserved.
 * *******************************************************/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;

namespace Monitor
{
    public delegate void NetworkMonitorEvent(double speed, SpeedUnitType unitType);
    public class NetworkMonitor : IDisposable
    {
        #region 构造函数
        public NetworkMonitor()
        {
            this._allAdapters = new List<NetworkAdapter>();
            this._currentAdapters = new List<NetworkAdapter>();

            this._timer = new Timer(1000);
            this._timer.Elapsed += new ElapsedEventHandler(TimerElapsedEvent);

            this._lockObj = new object();
        }
        #endregion

        #region 字段属性
        private object _lockObj;
        /// <summary>
        /// 所有设备列表
        /// </summary>
        private List<NetworkAdapter> _allAdapters;

        /// <summary>
        /// 当前监控的设备
        /// </summary>
        private List<NetworkAdapter> _currentAdapters;

        /// <summary>
        /// 计时器
        /// </summary>
        private Timer _timer;

        private NetworkMonitorEvent _uploadEvent;
        /// <summary>
        /// 上传速度事件
        /// </summary>
        public event NetworkMonitorEvent UploadEvent
        {
            add
            {
                this._uploadEvent += value;
            }
            remove
            {
                this._uploadEvent -= value;
            }
        }

        private NetworkMonitorEvent _downloadEvent;
        /// <summary>
        /// 下载速度事件
        /// </summary>
        public event NetworkMonitorEvent DownloadEvent
        {
            add
            {
                this._downloadEvent += value;
            }
            remove
            {
                this._downloadEvent -= value;
            }
        }
        #endregion

        #region 对外函数
        /// <summary>  
        /// 开始监控  
        /// </summary>  
        public void StartMonitoring()
        {
            this.Initialization();
            if (this._allAdapters.Count > 0)
            {
                foreach (NetworkAdapter adapter in this._allAdapters)
                {
                    if (!this._currentAdapters.Contains(adapter))
                    {
                        this._currentAdapters.Add(adapter);
                        adapter.Initialization();
                    }
                }
            }

            this._timer.Start();
        }

        /// <summary>  
        /// 结束监控，清空列表  
        /// </summary>  
        public void StopMonitoring()
        {
            this._allAdapters.Clear();
            this._currentAdapters.Clear();

            this._timer.Stop();
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Dispose()
        {
            try
            {
                this.StopMonitoring();
                if (this._timer != null)
                {
                    this._timer.Close();
                }
            }
            catch { }
        }
        #endregion

        #region 内部方法
        /// <summary>
        /// 初始化监视器
        /// </summary>
        private void Initialization()
        {
            PerformanceCounterCategory category = new PerformanceCounterCategory("Network Interface");

            foreach (string name in category.GetInstanceNames())
            {
                if (name == "MS TCP Loopback interface")
                    continue;

                NetworkAdapter tempAdapter = new NetworkAdapter(name);
                tempAdapter.DownloadCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", name);
                tempAdapter.UploadCounter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", name);
                this._allAdapters.Add(tempAdapter);
            }
        }

        /// <summary>
        /// 计时器事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerElapsedEvent(object sender, ElapsedEventArgs e)
        {
            try
            {
                lock (this._lockObj)
                {
                    double downloadSpeed = 0;
                    double uploadSpeed = 0;

                    foreach (NetworkAdapter adapter in this._currentAdapters)
                    {
                        adapter.Refresh();
                        downloadSpeed += adapter.DownloadSpeed;
                        uploadSpeed += adapter.UploadSpeed;
                    }

                    this.DoSpeedEvent(downloadSpeed, this._downloadEvent);
                    this.DoSpeedEvent(uploadSpeed, this._uploadEvent);
                }
            }
            catch { }
        }

        /// <summary>
        /// 执行速度事件
        /// </summary>
        /// <param name="speed">速度</param>
        /// <param name="speedEvent">速度事件</param>
        private void DoSpeedEvent(double speed, NetworkMonitorEvent speedEvent)
        {
            if (speedEvent != null)
            {
                SpeedUnitType unitType = SpeedUnitType.BPS;

                switch (GetSpeedUnit(ref speed, 0))
                {
                    case 0:
                        unitType = SpeedUnitType.BPS;
                        break;
                    case 1:
                        unitType = SpeedUnitType.KBPS;
                        break;
                    case 2:
                        unitType = SpeedUnitType.MBPS;
                        break;
                    case 3:
                        unitType = SpeedUnitType.GBPS;
                        break;
                    case 4:
                        unitType = SpeedUnitType.TBPS;
                        break;
                }

                speedEvent?.Invoke(Math.Round(speed, 2), unitType);
            }
        }

        /// <summary>
        /// 得到速度单位
        /// </summary>
        /// <param name="speed">速度</param>
        /// <param name="nowUnit">当前单位</param>
        /// <returns></returns>
        private int GetSpeedUnit(ref double speed, int nowUnit)
        {
            if (speed < 1000 || nowUnit > 4)
            {
                if (speed <= 0)
                {
                    return -1;
                }

                return nowUnit;
            }
            else
            {
                if (speed > 0)
                {
                    speed /= 1024.0;
                }

                return this.GetSpeedUnit(ref speed, ++nowUnit);
            }
        }
        #endregion
    }
}
