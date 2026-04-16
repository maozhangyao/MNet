using System;

namespace MNet.LTSQL
{
    /// <summary>
    /// SqlWriter扩展
    /// </summary>
    public static class SqlWriterExtensions
    {
        /// <summary>
        /// 调用part.ToString()，当作sql写入
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="part"></param>
        public static void Write(this ISqlWriter writer, object part)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));
            writer.Write(part?.ToString());
        }
        
        /// <summary>
        /// 调用part.ToString()，当作sql写入
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="part"></param>
        public static void WriteLine(this ISqlWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));
            writer.WriteLine(null);
        }

        /// <summary>
        /// 调用part.ToString()，当作sql写入
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="part"></param>
        public static void WriteLine(this ISqlWriter writer, object part)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));
            writer.WriteLine(part?.ToString());
        }
    }
}
