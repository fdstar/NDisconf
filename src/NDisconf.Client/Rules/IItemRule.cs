using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NDisconf.Client.Rules
{
    /// <summary>
    /// 键值对变更委托
    /// </summary>
    /// <param name="configName"></param>
    /// <param name="value"></param>
    public delegate void ItemChanged(string configName, string value);
    /// <summary>
    /// 键值对更新规则
    /// </summary>
    public interface IItemRule : IRule
    {
        /// <summary>
        /// 当值发生变更时如何进行回调，注意此处将采用委托链的方式，即多次调用均会被执行
        /// </summary>
        /// <param name="action">键值对变更委托</param>
        /// <returns></returns>
        IItemRule Changed(ItemChanged action);
        /// <summary>
        /// 注册Rule规则，设置默认的属性映射
        /// </summary>
        /// <param name="propName">要赋值的属性名，默认采用远程的configName</param>
        /// <returns></returns>
        new IItemRule MapTo(string propName);
        /// <summary>
        /// 更新指定实体的属性值，按默认方式获取实例属性，注意此处多次调用均会被执行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="propName">要赋值的属性名，如果该值为空，则使用默认配置名</param>
        /// <param name="typeConvert"></param>
        /// <returns></returns>
        IItemRule SetProperty<T>(T entity, string propName = null, Func<string, object> typeConvert = null);
        /// <summary>
        /// 更新指定实体的属性值，注意此处多次调用均会被执行
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="prop"></param>
        /// <param name="typeConvert"></param>
        /// <returns></returns>
        IItemRule SetProperty(object entity, PropertyInfo prop, Func<string, object> typeConvert = null);
        /// <summary>
        /// 更新静态属性的值，按默认方式获取静态属性，注意此处多次调用均会被执行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propName">要赋值的属性名，如果该值为空，则使用默认配置名</param>
        /// <param name="typeConvert"></param>
        /// <returns></returns>
        IItemRule SetStaticProperty<T>(string propName = null, Func<string, object> typeConvert = null);
        /// <summary>
        /// 更新静态属性的值，注意此处多次调用均会被执行
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="typeConvert"></param>
        /// <returns></returns>
        IItemRule SetStaticProperty(PropertyInfo prop, Func<string, object> typeConvert = null);
    }
}
