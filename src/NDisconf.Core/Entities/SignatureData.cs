using NDisconf.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NDisconf.Core.Entities
{
    /// <summary>
    /// 签名请求
    /// </summary>
    public class SignatureData
    {
        /// <summary>
        /// 请求用的秘钥，默认为空，表示向服务端请求数据时不进行签名
        /// </summary>
        public string SecretKey { get; set; } = string.Empty;
        /// <summary>
        /// 签名用的Hash算法
        /// </summary>
        public string HashAlgorithm { get; set; } = "MD5";
        /// <summary>
        /// 签名结果
        /// </summary>
        public string SignData
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.SecretKey))
                {//如果秘钥为空，则不进行签名
                    return "";
                }
                var dic = this.GetSortedDictionary();
                StringBuilder tmp = new StringBuilder();
                foreach (var kv in dic)
                {
                    tmp.Append('&');
                    tmp.Append(kv.Key);
                    tmp.Append('=');
                    tmp.Append(Uri.EscapeDataString(kv.Value));
                }
                if (tmp.Length > 0)
                {
                    tmp = tmp.Remove(0, 1);
                }
                tmp.Append(this.SecretKey);
                return HashSignatureHelper.SignData(tmp.ToString(), this.HashAlgorithm);
            }
        }
        /// <summary>
        /// 获取签名用的字典
        /// </summary>
        /// <returns></returns>
        public virtual SortedDictionary<string, string> GetSortedDictionary()
        {
            var dic = new SortedDictionary<string, string>();
            return dic;
        }
    }
}
