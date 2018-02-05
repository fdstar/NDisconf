using System;
using System.Collections.Generic;
using System.Text;

namespace NDisconf.Core.Entities
{
    /// <summary>
    /// 特定配置查询类
    /// </summary>
    public class ConfigFetchFilter : FetchFilter
    {
        /// <summary>
        /// 配置名
        /// </summary>
        public string ConfigName { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public ConfigType ConfigType { get; set; }

        public override SortedDictionary<string, string> GetSortedDictionary()
        {
            var dic = base.GetSortedDictionary();
            dic.Add("ConfigName", this.ConfigName);
            dic.Add("ConfigType", ((int)this.ConfigType).ToString());
            return dic;
        }
    }
}
