using System;
using System.Collections.Generic;
using MNet.LTSQL.SqlTokens;

namespace MNet.LTSQL
{
    /// <summary>
    /// sql写入器上下文
    /// </summary>
    public class SqlWriterContext
    {
        public LTSQLToken ParentToken{ get; set; }
        public Stack<LTSQLToken> TokenStack{ get; } = new Stack<LTSQLToken>();
        //目标数据库
        public DbType DbType { get; set; }
        //是否使用参数化
        public bool UseParameter { get; set; }
        //sql写入器
        public ISqlWriter Writer{ get; set; }
        //参数列表，如果需要参数化，则在生成sql时，参数部分用占位符(参数名)表示，并将参数值放入这个列表中
        public List<(string key, object value)> SqlParameters { get; set; }
        //一个实例对象如何转成sql的一部分，主要用于不需要参数化的场景，日期格式化、GUID格式化、数字格式化等等
        public Func<object, SqlWriterContext, string> Obj2SqlPart { get; set; }
        // sql 标识符&关键字转义，常用于对表名字段名的转义，如mysql： `t1`.`Name`, sql server：[t1].[Name]
        public Func<string, SqlWriterContext, string> SqlKeyWordEscape { get; set; }
        public SqlBuilderContext BuildContext { get; set; }

        public void AddParameter(string key, object value)
        {
            SqlParameters??= new List<(string key, object value)>(8);
            SqlParameters.Add((key, value));
        }
    }
}
