using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace NDisconf.Client.Preservations
{
    /// <summary>
    /// 临时目录下的键值对本地持久化
    /// </summary>
    public class ItemPreservation : BasePreservation
    {
        /// <summary>
        /// 键值对配置的本地持久化
        /// </summary>
        /// <param name="setting"></param>
        public ItemPreservation(PreservationSetting setting)
            : base(setting)
        {
        }
        /// <summary>
        ///  用于持久化相关的文件路劲，如果不存在，则需返回null
        /// </summary>
        public override string FilePath => this.GetFullPath(this._setting.TmpItemsLocalName, this._tmpRootPath);
        /// <summary>
        /// 从本地获取映射内容
        /// </summary>
        /// <returns></returns>
        public override IDictionary<string, string> GetFromLocal()
        {
            IDictionary<string, string> dic = null;
            if (this.FilePath != null && File.Exists(this.FilePath))
            {
                XElement root = XElement.Load(this.FilePath);
                var eles = root.Elements("item");
                if (eles.Any())
                {
                    dic = eles.ToDictionary(e => e.Attribute("key").Value, e => e.Attribute("value").Value);
                }
            }
            return dic;
        }
        /// <summary>
        /// 保存单条数据，如果对应持久化已存在，则进行覆盖替换
        /// </summary>
        /// <param name="key"></param>
        /// <param name="content"></param>
        public override void Save(string key, string content)
        {
            if (this.FilePath != null && File.Exists(this.FilePath))
            {
                XElement root = XElement.Load(this.FilePath);
                var ele = root.Elements("item").FirstOrDefault(e => e.Attribute("key").Value == key);
                if (ele == null)
                {
                    this.AddElementItem(root, key, content);
                }
                else
                {
                    ele.Attribute("value").SetValue(content);
                }
                root.Save(this.FilePath);
            }
        }
        /// <summary>
        /// 批量将所有数据写入文件，如果原文件已存在，则进行内容覆盖
        /// </summary>
        /// <param name="source"></param>
        public override void WriteAll(IDictionary<string, string> source)
        {
            if (FilePath != null && source != null && source.Count > 0)
            {
                XElement root = new XElement("items");
                foreach (var kv in source)
                {
                    this.AddElementItem(root, kv.Key, kv.Value);
                }
                root.Save(this.FilePath);
            }
        }
        private void AddElementItem(XElement root, string key, string value)
        {
            root.Add(new XElement("item", new XAttribute("key", key), new XAttribute("value", value)));
        }
    }
}
