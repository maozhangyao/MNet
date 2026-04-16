using System;
using System.Text;

namespace MNet.LTSQL
{
    /// <summary>
    /// sql 写入器
    /// </summary>
    public interface ISqlWriter
    {
        /// <summary>
        /// 写一个sql部分
        /// </summary>
        /// <param name="part"></param>
        void Write(string part);
        /// <summary>
        /// 写一个sql部分并换行
        /// </summary>
        /// <param name="part"></param>
        void WriteLine(string? part);
        /// <summary>
        /// 缩进
        /// </summary>
        void WriteIndent();
        /// <summary>
        /// 开始一个
        /// </summary>
        void BeginIndent();
        /// <summary>
        /// 结束一个子作用域
        /// </summary>
        void EndIndent();
        
        /// <summary>
        /// 当前的SQL构建器
        /// </summary>
        /// <returns></returns>
        StringBuilder GetSqlBuilder();
    }
}