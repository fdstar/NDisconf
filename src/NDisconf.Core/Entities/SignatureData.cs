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
        private string _secretKey;
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
                if (string.IsNullOrWhiteSpace(this._secretKey))
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
                tmp.Append(this._secretKey);
                return HashSignatureHelper.SignData(tmp.ToString(), this.HashAlgorithm);
            }
        }
        /// <summary>
        /// 设置请求用的秘钥，默认为空，表示向服务端请求数据时不进行签名
        /// </summary>
        /// <param name="secretKey"></param>
        public void SetSecretKey(string secretKey)
        {
            this._secretKey = secretKey;
        }
        /// <summary>
        /// 校验签名是否一致
        /// </summary>
        /// <param name="compareData"></param>
        /// <returns></returns>
        public bool VerifyData(string compareData)
        {
            return compareData == this.SignData;
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
