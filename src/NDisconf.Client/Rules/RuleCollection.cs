using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace NDisconf.Client.Rules
{
    /// <summary>
    /// Rule集合抽象类
    /// </summary>
    public abstract class RuleCollection
    {
        /// <summary>
        /// 尝试按规则进行变更通知
        /// </summary>
        /// <param name="configName"></param>
        /// <param name="changedValue"></param>
        public abstract void TryNoticeChanged(string configName, string changedValue);
    }
    /// <summary>
    /// Rule集合，简化Rule入口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RuleCollection<T> : RuleCollection
        where T : Rule
    {
        private ConcurrentDictionary<string, T> _rules = new ConcurrentDictionary<string, T>();
        private Func<string, T> _createFunc;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="createFunc">创建T委托</param>
        public RuleCollection(Func<string, T> createFunc)
        {
            _createFunc = createFunc ?? throw new ArgumentNullException("createFunc can not be null");
        }
        /// <summary>
        /// 根据configName获取对应的Rule对象
        /// </summary>
        /// <param name="configName">注意configName区分大小写</param>
        /// <returns></returns>
        public T For(string configName)
        {
            return this._rules.GetOrAdd(configName, this._createFunc);
        }
        /// <summary>
        /// 尝试按规则进行变更通知
        /// </summary>
        /// <param name="configName"></param>
        /// <param name="changedValue"></param>
        public override void TryNoticeChanged(string configName, string changedValue)
        {
            if (this._rules.TryGetValue(configName, out T rule))
            {
                rule.ConfigChanged(changedValue);
            }
        }
    }
}
