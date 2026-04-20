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
        public LTSQLWriter() : this(false)
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
        //是否美化SQL
        private bool _beautifulSql = false;
        private bool _newLineFlag = false;
        private StringBuilder _sqlBuilder;
        private Queue<string> _sqlParts = new Queue<string>();
        private Stack<string> _indentStk = new Stack<string>();

        private void NewLine()
        {
            this._charCount++;
            this._charCount++;
            this._sqlParts.Enqueue("\r\n");
        }
        /// <summary>
        /// 写一个sql部分
        /// </summary>
        /// <param name="part"></param>
        public void Write(string part)
        {
            bool newLine = this._newLineFlag;
            this._newLineFlag = false;

            if (string.IsNullOrEmpty(part))
                return;

            if (newLine)
                this.WriteLine(null, true);

            this._charCount += part.Length;
            this._sqlParts.Enqueue(part);
        }

        public void WriteLine(string? part, bool indent)
        {
            this.Write(part);

            if (this._beautifulSql)
            {
                this.NewLine();
                if (indent)
                    this.WriteIndent();
            }
        }
        /// <summary>
        /// 写一个sql部分并换行
        /// </summary>
        /// <param name="part"></param>
        public void WriteLine(string? part)
        {
            this.WriteLine(part, true);
        }

        public void WriteWhite(string? part)
        {
            this.Write(part);

            this._charCount++;
            this.Write(" ");
        }

        private void WriteIndent()
        {
            if (this._beautifulSql)
            {
                if (this._indentStk.Count <= 0)
                    return;

                string prefix = this._indentStk.Pop();

                this.WriteIndent();
                if (!string.IsNullOrEmpty(prefix))
                    this.Write(prefix);

                this._indentStk.Push(prefix);
            }
        }


        /// <summary>
        /// 开始一个缩进级别
        /// </summary>
        public void BeginScope(string prefix)
        {
            this._indentStk.Push(prefix);
            this._newLineFlag = false;
            this.WriteLine(null, true);
        }

        /// <summary>
        /// 结束一个子作用域,减少缩进级别
        /// </summary>
        public void EndScope()
        {
            this._indentStk.Pop();
            this._newLineFlag = true;
            //this.WriteLine(null, false);
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
