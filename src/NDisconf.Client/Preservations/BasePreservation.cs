using NDisconf.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NDisconf.Client.Preservations
{
    /// <summary>
    /// 持久化抽象基础
    /// </summary>
    public abstract class BasePreservation : IPreservation
    {
        protected readonly string _tmpRootPath;
        protected readonly string _factRootPath;
        public BasePreservation()
        {
            //TODO: 持久化相关配置读取
            DirectoryHelper.CreateDirectories(this._tmpRootPath, this._factRootPath);
        }
        /// <summary>
        /// 用于持久化相关的文件路劲，如果不存在，则需返回null
        /// </summary>
        public abstract string FilePath { get; }
        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime LastWriteTime
        {
            get
            {
                if (this.FilePath != null && File.Exists(this.FilePath))
                {
                    return File.GetLastWriteTime(this.FilePath);
                }
                return DateTime.MinValue;
            }
            set
            {
                if (this.FilePath != null && File.Exists(this.FilePath))
                {
                    File.SetLastWriteTime(this.FilePath, value);
                }
            }
        }
        /// <summary>
        /// 从本地获取映射内容
        /// </summary>
        /// <returns></returns>
        public abstract IDictionary<string, string> GetFromLocal();
        /// <summary>
        /// 保存单条数据，如果对应持久化已存在，则进行覆盖替换
        /// </summary>
        /// <param name="key"></param>
        /// <param name="content"></param>
        public abstract void Save(string key, string content);
        /// <summary>
        /// 批量将所有数据写入文件，如果原文件已存在，则进行内容覆盖
        /// </summary>
        /// <param name="source"></param>
        public abstract void WriteAll(IDictionary<string, string> source);
        /// <summary>
        /// 获取物理路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        protected string GetPhysicalPath(string path)
        {
            var physicalPath = path;
            //if (!this._preservation.AbsolutePath)
            //{
            //    physicalPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
            //}
            return physicalPath;
        }
        /// <summary>
        /// 获取文件的完整路径
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="rootPath"></param>
        /// <param name="createDirectory"></param>
        /// <returns></returns>
        protected string GetFullPath(string fileName, string rootPath, bool createDirectory = false)
        {
            var fullPath = Path.Combine(rootPath, fileName);
            if (createDirectory && fileName.IndexOfAny(new char[] { '/', '\\' }) > 0)
            {
                DirectoryHelper.CreateDirectories(Path.GetDirectoryName(fullPath));
            }
            return fullPath;
        }
    }
}
