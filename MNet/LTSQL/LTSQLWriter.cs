using System;
using System.Text;
using System.Collections.Generic;
using System.Data.Common;

namespace MNet.LTSQL
{
    /// <summary>
    /// 默认的SQL构建器
    /// </summary>
    public class LTSQLWriter : ISqlWriter
    {
        public LTSQLWriter() : this (false)
        { }
        public LTSQLWriter(bool beautifulSql) : this(beautifulSql, null)
        { }
        public LTSQLWriter(bool beautifulSql, StringBuilder sqlBuilder)
        {
            this._sqlBuilder = sqlBuilder;
            this._beautifulSql = beautifulSql;
        }


        //当前字符数
        private int _charCount = 0;
        //当前缩进层级
        private int _indentLevel;
        //是否美化SQL
        private readonly bool _beautifulSql;
        private readonly StringBuilder _sqlBuilder;
        private Queue<string> _sqlParts = new Queue<string>();


        /// <summary>
        /// 写一个sql部分
        /// </summary>
        /// <param name="part"></param>
        public void Write(string part)
        {
            if (string.IsNullOrEmpty(part))
                return;

            this._charCount += part.Length;
            this._sqlParts.Enqueue(part);
        }

        /// <summary>
        /// 写一个sql部分并换行
        /// </summary>
        /// <param name="part"></param>
        public void WriteLine(string? part)
        {
            this.Write(part);
            
            this._charCount++;
            this._charCount++;
            this._sqlParts.Enqueue("\r\n");
            
            this.WriteIndent();
        }

        /// <summary>
        /// 写入当前级别的缩进
        /// </summary>
        public void WriteIndent()
        {
            string indent = "    ";
            for (int i = 0; i < _indentLevel; i++)
            {
                this._charCount += indent.Length;
                this._sqlParts.Enqueue(indent);
            }
        }

        /// <summary>
        /// 开始一个缩进级别
        /// </summary>
        public void BeginIndent()
        {
            _indentLevel++;
            this.WriteLine(null);
        }
        
        /// <summary>
        /// 结束一个子作用域,减少缩进级别
        /// </summary>
        public void EndIndent()
        {
            if (_indentLevel > 0)
                _indentLevel--;
             this.WriteLine(null);
        }

        /// <summary>
        /// 获取当前的SQL构建器
        /// </summary>
        /// <returns></returns>
        public StringBuilder GetSqlBuilder()
        {    
            StringBuilder sqlBuilder = this._sqlBuilder ?? new StringBuilder(this._charCount + 1024);
            foreach (string part in _sqlParts)
            {
                sqlBuilder.Append(part);
            }
            return sqlBuilder;
        }
    }
}
