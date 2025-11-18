using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Reflection;
using System.Xml.Linq;
using MNet.Utils;

namespace MNet.SqlExpression
{
    /// <summary>
    /// where 表达式
    /// </summary>
    internal class SqlExpressionBuilder : CommonSqlExpressionBuilder
    {
        public SqlExpressionBuilder(SqlBuildContext context)
        {
            this.Context = context;
        }


        private void Clear()
        {
            this.Stack.Clear();
            this.Paramters.Clear();
        }
        private SqlToken Begin(Expression expr)
        {
            this.Visit(expr);
            return this.PopToken();
        }


        //生成where
        public string BuildWhere(Expression expr)
        {
            return this.Begin(expr).SqlPart;
        }
        //生成 order by
        public string BuildOrder(IEnumerable<DbSetOrder> orders)
        {
            List<string> orderbys = new List<string>();
            foreach (DbSetOrder order in orders)
            {
                this.Clear();
                
                SqlToken token = this.Begin(order.OrderByExpress);
                orderbys.Add($"{token.SqlPart}{(order.IsDesc ? " desc" : "")}");
            }
            return string.Join(",", orderbys);
        }
        //生成 select
        public string BuildSelect(Expression select)
        {
            if (select is LambdaExpression lambda && lambda.Body is ParameterExpression pExpr)
            {
                //表示是from 叶节点
                Type parameterType = pExpr.Type;

                PropertyInfo[] props = parameterType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                foreach (PropertyInfo prop in props)
                {
                    this.SqlDecriptor.Fields.Add(DbUtils.Escape(prop.Name, this.Context.Options.Db));
                }

                if (this.SqlDecriptor.Fields.IsEmpty())
                    throw new Exception($"类型未能获取任何字段信息，表达式：{pExpr}");

                return string.Join(',', this.SqlDecriptor.Fields);
            }

            this.Visit(select);
            return "*";
        }
        //生成sql
        public SqlDescriptor Build(DbSetStrcut root)
        {
            this.Clear();
            
            this.SqlDecriptor = new SqlDescriptor();
            this.SqlDecriptor.Define = root;
            this.SqlDecriptor.Name = $"t{this.Context.RefTableCount++}";
            
            this.Context.Descriptors.Add(this.SqlDecriptor);

            if(root.From != null)
            {
                SqlDescriptor from = new SqlExpressionBuilder(this.Context).Build(root.From);
                this.SqlDecriptor.From = from;
            }

            //where
            if(root.WhereExpr != null)
            {
                this.SqlDecriptor.Where = this.BuildWhere(root.WhereExpr);
            }

            //order
            if(root.OrderExprs != null)
            {
                this.SqlDecriptor.OrderBy = this.BuildOrder(root.OrderExprs);
            }

            //select
            if (root.SelectExprs != null)
            {
                this.SqlDecriptor.Select = this.BuildSelect(root.SelectExprs);
            }
            else if (this.SqlDecriptor.From != null)
            {
                this.SqlDecriptor.Fields.AddRange(this.SqlDecriptor.From.Fields); //直接继承 from 的投射
            }

            return this.SqlDecriptor;
        }
    }
}
