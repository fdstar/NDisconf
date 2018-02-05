using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NDisconf.Core.Entities;

namespace NDisconf.Client.Preservations
{
    /// <summary>
    /// 文件持久化方案
    /// </summary>
    public class FilePreservation : BasePreservation
    {
        /// <summary>
        /// 配置文件持久化构造函数
        /// </summary>
        /// <param name="setting"></param>
        public FilePreservation(PreservationSetting setting)
            : base(setting)
        {
        }
        /// <summary>
        /// 用于持久化相关的文件路劲，如果不存在，则需返回null
        /// </summary>
        public override string FilePath => this.GetFullPath(this._setting.TmpFilesLocalName, this._tmpRootPath);
        /// <summary>
        /// 当前持久化对应的配置类型
        /// </summary>
        public override ConfigType ConfigType => ConfigType.File;
        /// <summary>
        /// 从本地获取映射内容
        /// </summary>
        /// <returns></returns>
        public override IDictionary<string, string> GetFromLocal()
        {
            IDictionary<string, string> dic = null;
            if (this.FilePath != null && File.Exists(this.FilePath))
            {
                dic = File.ReadAllText(this.FilePath, Encoding.UTF8).Split(',')
                    .Select(f => { this.SaveAndCopy(f, null, true); return f; })
                    .ToDictionary(k => k, v => string.Empty);
            }
            return dic;
        }
        /// <summary>
        /// 保存单条数据，如果对应持久化已存在，则进行覆盖替换
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="content"></param>
        public override void Save(string fileName, string content)
        {
            this.SaveAndCopy(fileName, content, false);
        }
        private void SaveAndCopy(string fileName, string content, bool onlyCopy)
        {
            var tmpPath = this.GetFullPath(fileName, this._tmpRootPath, true);
            var factPath = this.GetFullPath(fileName, this._factRootPath, true);
            if (!onlyCopy)
            {
                File.WriteAllText(tmpPath, content, Encoding.UTF8);
            }
            File.Copy(tmpPath, factPath, true);
        }
        /// <summary>
        /// 批量将所有数据写入文件，如果原文件已存在，则进行内容覆盖
        /// </summary>
        /// <param name="source"></param>
        public override void WriteAll(IDictionary<string, string> source)
        {
            if (source != null && source.Count > 0)
            {
                foreach (var kv in source)
                {
                    this.SaveAndCopy(kv.Key, kv.Value, false);
                }
                var filePath = this.GetFullPath(this._setting.TmpFilesLocalName, this._tmpRootPath, true);
                if (filePath != null)
                {
                    File.WriteAllText(filePath, string.Join(",", source.Keys), Encoding.UTF8);
                }
            }
        }
    }
}
