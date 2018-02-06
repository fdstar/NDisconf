using NLog;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NDisconf.Client.Rules
{
    /// <summary>
    /// 键值对更新规则
    /// </summary>
    public class ItemRule : Rule, IItemRule
    {
        private static readonly Logger _logger = LogManager.GetLogger(NDisconfManager.LOGPREFIX + "ItemRule");
        private List<PropertyMap> _list = new List<PropertyMap>();
        /// <summary>
        /// 默认要映射的属性名
        /// </summary>
        public string DefaultPropName { get; private set; }
        /// <summary>
        /// 键值对变更事件
        /// </summary>
        internal event ItemChanged Changed;
        /// <summary>
        /// 键值对更新规则
        /// </summary>
        /// <param name="configName"></param>
        public ItemRule(string configName) :
            base(configName)
        {
            this.MapTo(configName);
        }
        /// <summary>
        /// 当值发生变更时如何进行回调，注意此处将采用委托链的方式，即多次调用均会被执行
        /// </summary>
        /// <param name="changedValue"></param>
        public override void ConfigChanged(string changedValue)
        {
            if (this._list != null && this._list.Count > 0)
            {
                for (var i = this._list.Count - 1; i >= 0; i--)
                {
                    var map = this._list[i];
                    try
                    {
                        var pi = map.GetPropertyInfo(this.DefaultPropName);
                        if (pi != null)
                        {
                            object value;
                            if (map.TypeConvert != null)
                            {
                                value = map.TypeConvert(changedValue);
                            }
                            else
                            {
                                Type pType = null;
                                var p = map.PropertyInfo.PropertyType;
                                if (!p.IsGenericType)
                                {
                                    pType = p;
                                }
                                else if (p.GetGenericTypeDefinition() == typeof(Nullable<>))
                                {
                                    pType = Nullable.GetUnderlyingType(p);
                                }
                                if (pType == null)
                                {
                                    //默认规则无法转换，则该项配置无效，直接移除
                                    this._list.RemoveAt(i);
                                    _logger.Info(string.Format("Key '{0}' has removed '{1}:{2}' because there has no effective convert method", this.ConfigName, map.EntityType.FullName, map.PropertyName));
                                    continue;
                                }
                                value = ConvertTo(changedValue, pType);
                            }
                            map.PropertyInfo.SetValue(map.Entity, value);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, string.Format("Item '{0}:{1}' set value '{2}' error", map.EntityType.FullName, map.PropertyName, changedValue));
                    }
                }
            }
            this.Changed?.Invoke(this.DefaultPropName, changedValue);
        }
        private static object ConvertTo(string value, Type type)
        {
            if (type.IsEnum)
            {
                return Enum.Parse(type, value);
            }
            else
            {
                return Convert.ChangeType(value, type);
            }
        }
        /// <summary>
        /// 注册Rule规则，设置默认的属性映射
        /// </summary>
        /// <param name="propName"></param>
        /// <returns></returns>
        public IItemRule MapTo(string propName)
        {
            if (!string.IsNullOrWhiteSpace(propName))
            {
                this.DefaultPropName = this.GetPropName(propName);
            }
            return this;
        }
        private string GetPropName(string propName)
        {
            int idx = propName.LastIndexOf('.');
            if (idx >= 0)
            {
                return propName.Substring(idx + 1);
            }
            return propName;
        }
        public IItemRule OnChanged(ItemChanged action)
        {
            this.Changed += action;
            return this;
        }
        /// <summary>
        /// 更新指定实体的属性值，按默认方式获取实例属性，注意此处多次调用均会被执行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="propName"></param>
        /// <param name="typeConvert"></param>
        /// <returns></returns>
        public IItemRule SetProperty<T>(T entity, string propName = null, Func<string, object> typeConvert = null)
        {
            return this.SetProperty(entity, typeof(T), propName, null, typeConvert);
        }
        /// <summary>
        /// 更新指定实体的属性值，注意此处多次调用均会被执行
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="prop"></param>
        /// <param name="typeConvert"></param>
        /// <returns></returns>
        public IItemRule SetProperty(object entity, PropertyInfo prop, Func<string, object> typeConvert = null)
        {
            return this.SetProperty(entity, null, null, prop, typeConvert);
        }
        /// <summary>
        /// 更新静态属性的值，按默认方式获取静态属性，注意此处多次调用均会被执行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propName"></param>
        /// <param name="typeConvert"></param>
        /// <returns></returns>
        public IItemRule SetStaticProperty<T>(string propName = null, Func<string, object> typeConvert = null)
        {
            return this.SetProperty<T>(default(T), propName, typeConvert);
        }
        /// <summary>
        /// 更新静态属性的值，注意此处多次调用均会被执行
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="typeConvert"></param>
        /// <returns></returns>
        public IItemRule SetStaticProperty(PropertyInfo prop, Func<string, object> typeConvert = null)
        {
            return this.SetProperty(null, prop, typeConvert);
        }
        private IItemRule SetProperty(object entity, Type entityType, string propName, PropertyInfo prop, Func<string, object> typeConvert = null)
        {
            this._list.Add(new PropertyMap
            {
                Entity = entity,
                EntityType = entityType,
                PropertyName = propName,
                PropertyInfo = prop,
                TypeConvert = typeConvert
            });
            return this;
        }

        private class PropertyMap
        {
            public object Entity { get; set; }
            public Type EntityType { get; set; }
            public string PropertyName { get; set; }
            public PropertyInfo PropertyInfo { get; set; }
            public Func<string, object> TypeConvert { get; set; }
            public PropertyInfo GetPropertyInfo(string defaultPropertyName)
            {
                /*
                因为无法确认SetProperty和MapTo方法被调用的先后顺序
                所以通过PropertyName来得到对应的PropertyInfo这个过程只能在最后调用
                */
                PropertyInfo pi = this.PropertyInfo;
                if (pi == null)
                {
                    string propName = this.PropertyName;
                    if (string.IsNullOrWhiteSpace(propName))
                    {
                        propName = defaultPropertyName;
                    }
                    if (!string.IsNullOrWhiteSpace(propName))
                    {
                        pi = this.EntityType.GetProperty(propName);
                    }
                    this.PropertyInfo = pi;
                }
                return pi;
            }
        }
    }
}
