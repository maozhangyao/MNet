using System;

namespace MNet.SqlExpression
{
    /// <summary>
    /// 数据集
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    internal class DbSet<T1, T2> : DbPipe<T2>
    {
        public DbSet() : this(null)
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="src">数据集源头</param>
        /// <exception cref="Exception"></exception>
        public DbSet(IDbSet<T1> src)
        {
            DbPipe<T1> from = src as DbPipe<T1>;

            //命名机制流动
            if(from != null)
                this.Namer = from.Namer;

            this.DbSet = new DbSetStrcut(typeof(T2));
            this.DbSet.From = from?.DbSet;

            //为 from 命名
            if (this.DbSet.From != null)
            {
                TypeNamed named = this.CreateNamed(this.DbSet.From.Type);
                this.DbSet.FromNamed = named;
                this.DbSet.TypeNamed.Add(named.Name, named);
            }
        }
    }
}
