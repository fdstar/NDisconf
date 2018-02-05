using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace NDisconf.Core.Entities
{
    /// <summary>
    /// 应用信息
    /// </summary>
    public class AppInfo : SignatureData
    {
        private string _appName;
        private string _version;
        private string _environment;
        /// <summary>
        /// 应用名称,只允许输入数字、字母及下划线
        /// </summary>
        public string AppName
        {
            get { return this._appName; }
            set
            {
                this._appName = this.GetValidInput(value, @"[^a-zA-Z0-9\_]");
            }
        }
        ///
        /// 应用当前环境,只允许输入数字、字母及下划线
        /// </summary>
        public string Environment
        {
            get { return this._environment; }
            set
            {
                this._environment = this.GetValidInput(value, @"[^a-zA-Z0-9\_]");
            }
        }
        /// <summary>
        /// 应用版本号,只允许输入数字、字母、小数点以及下划线
        /// </summary>
        public string Version
        {
            get { return this._version; }
            set
            {
                this._version = this.GetValidInput(value, @"[^a-zA-Z0-9\.\_]");
            }
        }
        private string GetValidInput(string inputStr, string pattern)
        {
            string value = null;
            if (inputStr != null)
            {
                value = Regex.Replace(inputStr, pattern, string.Empty);
            }
            return value;
        }

        public override SortedDictionary<string, string> GetSortedDictionary()
        {
            var dic =  base.GetSortedDictionary();
            dic.Add("AppName", this.AppName);
            dic.Add("Environment", this.Environment);
            dic.Add("Version", this.Version);
            return dic;
        }
    }
}
