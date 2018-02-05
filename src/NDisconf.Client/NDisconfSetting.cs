using System;
using System.Collections.Generic;
using System.Text;
#if !NETSTANDARD2_0
using System.Configuration;
#endif

namespace NDisconf.Client
{
    /// <summary>
    /// NDisconf客户端配置
    /// </summary>
    public class NDisconfSetting
    {
        /// <summary>
        /// Rest服务器域名地址
        /// </summary>
        public string WebApiHost { get; set; }
        /// <summary>
        /// zookeeper的根节点路径
        /// </summary>
        public string ZookeeperBasePath { get; set; } = string.Empty;
        /// <summary>
        /// zookeeper的会话超时时间，单位毫秒，默认30000
        /// </summary>
        public int ZookeeperSessionTimeout { get; set; } = 30000;
        /// <summary>
        /// 是否启用远程配置，默认true，设为false的话表示不从远程服务器下载配置
        /// </summary>
        public bool EnableRemote { get; set; } = true;
        /// <summary>
        /// 客户端配置
        /// </summary>
        public ClientInfoSetting ClientInfo { get; set; }
        /// <summary>
        /// 更新策略
        /// </summary>
        public UpdateStrategySetting UpdateStrategy { get; set; }
        /// <summary>
        /// 本地持久化配置
        /// </summary>
        public PreservationSetting Preservation { get; set; }
        /// <summary>
        /// 请求安全配置
        /// </summary>
        public SecuritySetting Security { get; set; }
        /// <summary>
        /// 与服务端进行通讯的具体实现类
        /// </summary>
        public string FetcherType { get; set; } = "NDisconf.Client.Fetchers.NDisconfFetcher, NDisconf.Client";
    }
    /// <summary>
    /// 客户端信息
    /// </summary>
    public class ClientInfoSetting
    {
        /// <summary>
        /// 客户端程序名称，如果IgnoreCase为false注意大小写要与服务端一致
        /// </summary>
        public string AppName { get; set; }
        /// <summary>
        /// 当前客户端程序所处环境，如果IgnoreCase为false注意大小写要与服务端一致
        /// </summary>
        public string Environment { get; set; }
        /// <summary>
        /// 当前客户端程序版本，如果IgnoreCase为false注意大小写要与服务端一致
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// 客户端标识，用于服务端查看已更新客户端，如果不设置则默认获取客户端电脑名称
        /// </summary>
        public string ClientIdentity { get; set; } = System.Environment.MachineName;
        /// <summary>
        /// 该配置节下所有的配置是否忽略大小写，默认不忽略
        /// </summary>
        public bool IgnoreCase { get; set; } = false;
    }
    /// <summary>
    /// 请求安全配置
    /// </summary>
    public class SecuritySetting
    {
        /// <summary>
        /// 请求用的秘钥，默认为空，表示向服务端请求数据时不进行签名
        /// </summary>
        public string SecretKey { get; set; }
        /// <summary>
        /// 签名用的Hash算法
        /// </summary>
        public string HashAlgorithm { get; set; } = "MD5";
    }
    /// <summary>
    /// 更新策略配置
    /// </summary>
    public class UpdateStrategySetting
    {
        /// <summary>
        /// 要忽略更新的文件配置，以,分割，注意大小写要与服务端一致
        /// </summary>
        public string FileIgnores { get; set; }
        /// <summary>
        /// 要忽略更新的键值对配置，以,分割，注意大小写要与服务端一致
        /// </summary>
        public string ItemIgnores { get; set; }
        ///// <summary>
        ///// 启动时是否同步加载，默认同步
        ///// </summary>
        //public bool StartedSync { get; set; } = true;
        /// <summary>
        /// 当获取失败时的重试次数，默认为3
        /// </summary>
        public int RetryTimes { get; set; } = 3;
        /// <summary>
        /// 每次重试时间间隔，单位秒，默认每10秒重试一次
        /// </summary>
        public int RetryIntervalSeconds { get; set; } = 10;
        /// <summary>
        /// 是否强制下载更新，如果设为true，表示强制下载，否则将根据本地记录的最后更新时间来判断是否需要下载
        /// </summary>
        public bool ForcedDownload { get; set; } = false;
        /// <summary>
        /// 根据FileIgnores来获取拆分后的忽略文件
        /// </summary>
        public string[] FileIgnoreList
        {
            get
            {
                return this.FileIgnores?.Split(',');
            }
        }
        /// <summary>
        /// 根据ItemIgnoreList来获取拆分后的忽略键值对
        /// </summary>
        public string[] ItemIgnoreList
        {
            get
            {
                return this.ItemIgnores?.Split(',');
            }
        }
    }
    /// <summary>
    /// 配置本地持久化相关配置
    /// </summary>
    public class PreservationSetting
    {
        /// <summary>
        /// 是否绝对路径，默认false
        /// 当false时，表示默认以AppDomain.CurrentDomain.BaseDirectory为比较点
        /// 注意：该配置同时适用于TmpRootDirectory、FactRootDirectory，即要么都只能绝对路径，要么都只能相对路径
        /// </summary>
        public bool AbsolutePath { get; set; } = false;
        /// <summary>
        /// 下载下来的配置临时保存文件夹根目录，默认为Tmp/Download/Configs
        /// </summary>
        public string TmpRootDirectory { get; set; } = "Tmp/Download/Configs";
        /// <summary>
        /// 配置文件实际所在的根目录，默认值为Configs
        /// </summary>
        public string FactRootDirectory { get; set; } = "Configs";
        /// <summary>
        /// 在临时目录下用于保存所有键值对的文件名，设置为空表示不保存
        /// 为方便服务器配置发生变更时进行对应值的修改，这里存储格式为xml
        /// 文件保存在TmpRootDirectory目录下，所以注意不要与实际配置文件名字冲突
        /// </summary>
        public string TmpItemsLocalName { get; set; } = "~items";
        /// <summary>
        /// 在临时目录下用于保存所有文件配置名的文件名，设置为空表示不保存
        /// 因为运行中不存在修改的可能性，所以此部分直接简单的存储为文本格式，多个文件名之间以,分隔
        /// 文件保存在TmpRootDirectory目录下，所以注意不要与实际配置文件名字冲突
        /// </summary>
        public string TmpFilesLocalName { get; set; } = "~files";
    }
}
