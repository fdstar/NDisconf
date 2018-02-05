using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NDisconf.Client.Fetchers
{
    internal class FetcherFactory
    {
        /// <summary>
        /// 通过反射获取IFetcher实例，注意所有IFetcher实现均存在一个NDisconfSetting参数的构造函数
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public static IFetcher GetFetcher(NDisconfSetting setting)
        {
            return (IFetcher)Activator.CreateInstance(Type.GetType(setting.FetcherType), setting);
        }
    }
}
