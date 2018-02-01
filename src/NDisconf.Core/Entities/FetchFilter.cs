using System;
using System.Collections.Generic;
using System.Text;

namespace NDisconf.Core.Entities
{
    /// <summary>
    /// API服务调用查询类
    /// </summary>
    public class FetchFilter : AppInfo
    {
        /// <summary>
        /// 客户端唯一标志
        /// </summary>
        public string ClientIdentity { get; set; }
    }
}
