using System;
using System.Collections.Generic;
using System.Text;

namespace NDisconf.Client.Preservations
{
    /// <summary>
    /// 配置临时目录下的本地持久化
    /// </summary>
    public interface IPreservation
    {
        /// <summary>
        /// 用于持久化相关的文件路劲，如果不存在，则需返回null
        /// </summary>
        string FilePath { get; }
        /// <summary>
        /// 批量将所有数据写入文件，如果原文件已存在，则进行内容覆盖
        /// </summary>
        /// <param name="source"></param>
        void WriteAll(IDictionary<string, string> source);
        /// <summary>
        /// 从本地获取映射内容
        /// </summary>
        /// <returns></returns>
        IDictionary<string, string> GetFromLocal();
        /// <summary>
        /// 保存单条数据，如果对应持久化已存在，则进行覆盖替换
        /// </summary>
        /// <param name="key"></param>
        /// <param name="content"></param>
        void Save(string key, string content);
        /// <summary>
        /// 最后更新时间
        /// </summary>
        DateTime LastWriteTime { get; set; }
    }
}
