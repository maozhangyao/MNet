using System;

namespace MNet.LTSQL.Attributes
{
    /// <summary>
    /// 忽略识别为表字段
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class NonFiledAttribute : Attribute
    {

    }
}

