using System;
using System.Collections.Generic;
using System.Text;

namespace MNet.LTSQL
{
    /// <summary>
    /// sql builder 上下文
    /// </summary>
    public class SqlBuilderContext
    {
        public DbType DbType { get; set; }
        public bool UseParameter { get; set; }
        //sql写入
        public StringBuilder Sql { get; set; }


        //参数列表，如果需要参数化，则在生成sql时，参数部分用占位符(参数名)表示，并将参数值放入这个列表中
        public Dictionary<string, object> SqlParameters { get; set; }
        //一个实例对象如何转成sql的一部分，主要用于不需要参数化的场景，日期格式化、GUID格式化、数字格式化等等
        public Func<object, SqlBuilderContext, string> Obj2SqlPart { get; set; }
        //选项
        //public LTSQLOptions Options { get; set; }
        // sql 标识符&关键字转义，常用于对表名字段名的转义，如mysql： `t1`.`Name`, sql server：[t1].[Name]
        public Func<string, SqlBuilderContext, string> SqlKeyWordEscap { get; set; }
    }
}
