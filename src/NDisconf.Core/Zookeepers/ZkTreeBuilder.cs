using NDisconf.Core.Entities;
using NDisconf.Core.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NDisconf.Core.Zookeepers
{
    /// <summary>
    /// zookeeper树构建类
    /// </summary>
    public class ZkTreeBuilder : IZkTreeBuilder
    {
        #region fields
        private AppInfo _appInfo;
        private string _hashAlgorithm;
        private string _zkRootPath;
        /// <summary>
        /// 用于存储znodeName与configName对应关系的字典,Key为znodeName,Value为configName
        /// </summary>
        protected ConcurrentDictionary<string, string> _dic = new ConcurrentDictionary<string, string>();
        #endregion

        #region props
        /// <summary>
        /// 是否忽略大小写
        /// </summary>
        protected bool IgnoreCase { get; private set; }
        /// <summary>
        /// 指示所有生成的zk节点应该基于哪个起始路径
        /// </summary>
        protected string ZkBasePath { get; private set; }
        /// <summary>
        /// 配置类型
        /// </summary>
        public ConfigType ConfigType { get; private set; }
        #endregion

        #region octr
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="appInfo">应用信息</param>
        /// <param name="zkBasePath">zk树的根路径，即当前树应当生成在哪个根节点下</param>
        /// <param name="zkPathIgnoreCase">zk树路径是否忽略大小写</param>
        /// <param name="hashAlgorithm">生成节点用的hash算法，可能的值为MD5,SHA1,SHA256,SHA384,SHA512，默认SHA1</param>
        public ZkTreeBuilder(AppInfo appInfo, ConfigType configType, string zkBasePath = "", bool zkPathIgnoreCase = false, string hashAlgorithm = "SHA1")
        {
            this._appInfo = appInfo ?? throw new ArgumentNullException("appInfo can not be null");
            this.ZkBasePath = zkBasePath ?? string.Empty;
            this.IgnoreCase = zkPathIgnoreCase;
            this._hashAlgorithm = hashAlgorithm?.ToUpper();
            this.ConfigType = configType;
            this.SetZkRootPath();
        }
        #endregion

        #region methods
        #region IZkTreeBuilder
        /// <summary>
        /// 根据配置名称获取对应的Zookeeper下的节点名称，如果无法找到对应节点则添加并返回对应节点名称，该方法用于解决路径问题
        /// </summary>
        /// <param name="configName"></param>
        /// <returns></returns>
        public string GetOrAddZnodeName(string configName)
        {
            string znodeName = this.GetZnodeName(configName);
            //之所以要进行Hash，是因为configName支持文件层级，即支持 rootX/rootY/xxx.config格式
            this._dic[znodeName] = configName;
            return znodeName;
        }
        /// <summary>
        /// 根据Zookeeper的节点名称获取对应的配置名称，该方法用于watch到变更时，如何反向处理获取配置，如果无法找到对应的znode，则返回null
        /// </summary>
        /// <param name="znodeName"></param>
        /// <returns></returns>
        public string GetConfigNameByZnodeName(string znodeName)
        {
            this._dic.TryGetValue(znodeName, out string configName);
            return configName;
        }
        /// <summary>
        /// 获取指定应用在zk中的根路径
        /// </summary>
        /// <returns></returns>
        public string GetZkRootPath()
        {
            return this._zkRootPath;
        }
        /// <summary>
        /// 获取指定节点名称在zookeeper中的完整路径，注意是znodeName，而不是configName
        /// </summary>
        /// <param name="znodeName"></param>
        /// <returns></returns>
        public string GetZkPathByZnodeName(string znodeName)
        {
            return string.Format("{0}/{1}", this.GetZkRootPath(), znodeName);
        }
        /// <summary>
        /// 获取所有已配置的znodeName
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAllZnodes()
        {
            return this._dic.Keys;
        }
        #endregion
        private string GetIgnoreCaseString(string inputStr)
        {
            if (this.IgnoreCase)
            {
                return inputStr.ToLower();
            }
            return inputStr;
        }
        private string GetValidZkPath(string path)
        {
            path = path.Replace("\\", "/");
            if (path[0] != '/')
            {
                return "/" + path;
            }
            return path;
        }
        private void SetZkRootPath()
        {
            string path = Path.Combine(this._appInfo.AppName, this._appInfo.Version, this._appInfo.Environment, this.ConfigType.ToString());
            if (!string.IsNullOrWhiteSpace(this.ZkBasePath))
            {
                path = Path.Combine(this.ZkBasePath, path);
            }
            this._zkRootPath = this.GetIgnoreCaseString(this.GetValidZkPath(path));
        }
        private string GetZnodeName(string configName)
        {
            var tmp = this.GetIgnoreCaseString(configName);
            switch (this._hashAlgorithm)
            {
                case "MD5":
                    return MD5Helper.HashOf(tmp);
                case "SHA256":
                    return SHA256Helper.HashOf(tmp);
                case "SHA384":
                    return SHA384Helper.HashOf(tmp);
                case "SHA512":
                    return SHA512Helper.HashOf(tmp);
                default:
                    return SHA1Helper.HashOf(tmp);
            }
        }
        #endregion
    }
}
