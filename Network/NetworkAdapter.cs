/********************************************************
 * Chinese Name: 赵可(Zhao Ke)
 * English Name: Kathy Zhao
 * CreateTime: April 1, 2018 
 * Email:kathyatc@outlook.com
 * 
 * Copyright © 2018 Kathy Zhao. All Rights Reserved.
 * *******************************************************/
using System;
using System.Diagnostics;

namespace Monitor
{
    public class NetworkAdapter
    {
        #region 构造函数
        internal NetworkAdapter(string name)
        {
            this.Name = name;
        }
        #endregion

        #region 字段属性
        /// <summary>
        /// （下载&上传）值
        /// </summary>
        private long _downloadValue, _uploadValue;


        /// <summary>
        /// 时间记录
        /// </summary>
        private long _timeTicks;

        /// <summary>  
        /// 每秒下载速度 
        /// </summary>  
        public long DownloadSpeed { get; private set; }
        /// <summary>  
        /// 每秒上传速度  
        /// </summary>  
        public long UploadSpeed { get; private set; }

        public string Name { get; private set; }

        internal PerformanceCounter DownloadCounter { get; set; }

        internal PerformanceCounter UploadCounter { get; set; }
        #endregion

        #region 内外方法
        /// <summary>
        /// 初始化数据
        /// </summary>
        internal void Initialization()
        {
            this._downloadValue = this.DownloadCounter.NextSample().RawValue;
            this._uploadValue = this.UploadCounter.NextSample().RawValue;
        }

        /// <summary>
        /// 刷新数据
        /// </summary>
        internal void Refresh()
        {
            var ticks = DateTime.Now.Ticks;

            var download = this.DownloadCounter.NextSample().RawValue;
            var upload = this.UploadCounter.NextSample().RawValue;

            var scend = (ticks - this._timeTicks) / 10000000;

            this.DownloadSpeed = (download - this._downloadValue) / scend;
            this.UploadSpeed = (upload - this._uploadValue) / scend;

            this._downloadValue = download;
            this._uploadValue = upload;
            this._timeTicks = ticks;
        }
        #endregion
    }
}
