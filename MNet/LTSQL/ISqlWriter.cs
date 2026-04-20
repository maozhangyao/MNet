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
        /// 写一个sql部分并添加一个空格
        /// </summary>
        /// <param name="part"></param>
        void WriteWhite(string? part);
        
        /// <summary>
        /// 开始一个子作用域
        /// </summary>
        void BeginScope(string prefix);
        /// <summary>
        /// 结束一个子作用域
        /// </summary>
        void EndScope();
        
        /// <summary>
        /// 当前的SQL构建器
        /// </summary>
        /// <returns></returns>
        StringBuilder GetSqlBuilder();
    }
}