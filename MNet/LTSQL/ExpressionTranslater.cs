using MNet.LTSQL.SqlTokenExtends;
using MNet.LTSQL.SqlTokens;
using MNet.Utils;
using System;
using System.Collections.Generic;
#if NET6_0_OR_GREATER
using System.ComponentModel.DataAnnotations.Schema;
#endif
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;


namespace MNet.LTSQL
{
    /// <summary>
    /// expression 翻译器，负责将表达式树翻译为SQL令牌
    /// </summary>
    public class ExpressionTranslater : ExpressionVisitor
    {
        public ExpressionTranslater()
        {
            this.StkTokens = new Stack<LTSQLToken>();
        }


        public LTSQLContext Context => this.Scope?.Context;
        public LTSQLTranslateScope Scope { private set; get; }
        protected Stack<LTSQLToken> StkTokens { private set;  get; }



        protected LTSQLToken PopToken()
        {
            return this.StkTokens.Count > 0 ? this.StkTokens.Pop() : null;
        }
        protected LTSQLToken PeekToken()
        {
            return this.StkTokens.Peek();
        }
        protected void PushToken(LTSQLToken token)
        {
            this.StkTokens.Push(token);
        }
        protected LTSQLToken[] PopAsParamters(int cnt)
        {
            if (cnt <= 0)
                return new LTSQLToken[0];

            Stack<LTSQLToken> args = new Stack<LTSQLToken>();
            for (int i = 0; i < cnt; i++)
                args.Push(this.PopToken());

            return args.ToArray();
        }
        // 向上查找参数，直到找到对应的上下文作用域
        protected virtual LTSQLToken GetScopeParameter(string parameterName)
        {
            LTSQLContext context = this.Context;
            LTSQLTranslateScope scope = this.Scope;

            do
            {
                LTSQLToken param = context.GetScopeParameter(parameterName);
                if (param != null)
                    return param;

                scope = scope.Parent;
                context = scope?.Context;
            } while (context != null);

            throw new Exception($"参数名({parameterName})无法找到对应的上下文作用域, 无法解析表命名");
        }
        protected static object GetPropOrFieldValue(MemberInfo member, object? inst)
        {
            if (member is PropertyInfo prop)
                return prop.GetValue(inst);
            else if (member is FieldInfo field)
                return field.GetValue(inst);
            else
                throw new Exception($"非字段或者属性无法求值：{member.Name}");
        }
        // 调用外部翻译扩展
        protected virtual bool OnTranslateExpression(TranslateContext ctx)
        {
            this.Context.LTSQLTranslater.TranslateExpression(ctx);
            if (ctx.ResultToken != null)
                this.PushToken(ctx.ResultToken);

            return ctx.ResultToken != null;
        }
        protected virtual bool OnTranslateExpression(Expression expr, Type exprType = null)
        {
            TranslateContext ctx = new TranslateContext();
            ctx.TranslateExpr = expr;
            ctx.ExpressionValueType = exprType;
            ctx.Options = this.Context.Options;
            ctx.ParameterNameGenerator = this.Context.ParameterNameGenerator;

            return this.OnTranslateExpression(ctx);
        }
        // 调用外部翻译扩展
        protected virtual bool OnTranslateMember(TranslateContext ctx)
        {
            this.Context.LTSQLTranslater.TranslateMember(ctx);
            if (ctx.ResultToken != null)
                this.PushToken(ctx.ResultToken);

            return ctx.ResultToken != null;
        }
        protected virtual bool OnTranslateMember(MemberInfo member, object owner, Type ownerType, Expression expr, Type exprType = null, LTSQLToken ownerToken = null, LTSQLToken[] memberCallParameters = null)
        {
            TranslateContext ctx = new TranslateContext();
            ctx.TranslateExpr = expr;
            ctx.ExpressionValueType = exprType;
            ctx.Options = this.Context.Options;
            ctx.ParameterNameGenerator = this.Context.ParameterNameGenerator;

            ctx.Member = member;
            ctx.Owner = owner;
            ctx.OwnerType = ownerType;
            ctx.OwnerToken = ownerToken;
            ctx.MethodParameterTokenList = memberCallParameters;

            return this.OnTranslateMember(ctx);
        }
        protected void ApplyScope(LTSQLTranslateScope scope)
        {
            this.Scope = scope;
        }



        //翻译参数
        protected override Expression VisitParameter(ParameterExpression node)
        {
            //外部转换优先
            if (!this.OnTranslateExpression(node, node.Type))
            {
                //确定参数范围
                LTSQLToken ptoken = this.GetScopeParameter(node.Name);
                if (ptoken == null)
                    throw new Exception($"无法解析参数节点：{node}");

                this.PushToken(ptoken);
            }

            return base.VisitParameter(node);
        }
        //常量
        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (this.OnTranslateExpression(node, node.Type))
            {
                return node;
            }

            this.PushToken(LTSQLTokenFactory.CreateSqlParameterToken(this.Context.ParameterNameGenerator.Next(), node.Value, node.Type));
            return base.VisitConstant(node);
        }
        //字段或者属性
        protected override Expression VisitMember(MemberExpression node)
        {
            Expression expr = base.VisitMember(node);

            //静态成员
            if (node.Expression == null)
            {
                //外部对表达式树翻译
                if (this.OnTranslateExpression(node, node.Type))
                    return expr;

                //外部对成员调用翻译
                if (this.OnTranslateMember(node.Member, null, node.Member.ReflectedType, node, node.Type, null, null))
                    return expr;

                object val = GetPropOrFieldValue(node.Member, null);
                this.PushToken(LTSQLTokenFactory.CreateSqlParameterToken(this.Context.ParameterNameGenerator.Next(), val, node.Type));
                return expr;
            }


            /*非静态成员*/
            string memberName = node.Member.Name;
            LTSQLToken objToken = this.PopToken();
            if (objToken == null)
                throw new Exception($"表达式解析结果为null:{node}");

            {
                //外部对表达式树翻译
                if (this.OnTranslateExpression(node, node.Type))
                    return expr;

                //对常量求值
                if (objToken is SqlParameterToken p)
                {
                    object obj = p.Value;
                    if (obj == null)
                        throw new Exception($"表达式不能依赖null对象求值：{obj}");

                    if (!this.OnTranslateMember(node.Member, obj, node.Expression.Type, node, node.Type, objToken, null))
                    {
                        //对象访问
                        object val = GetPropOrFieldValue(node.Member, obj);
                        this.PushToken(LTSQLTokenFactory.CreateSqlParameterToken(p.ParameterName, val, node.Type));
                    }
                }
                //非常量(表)
                else
                {
                    if (this.OnTranslateMember(node.Member, null, node.Expression.Type, node, node.Type, objToken, null))
                        return expr;

                    if (objToken is ITupleable tuple)
                    {
                        LTSQLToken prop = tuple[memberName];
                        if (prop == null)
                            throw new Exception($"没有找到对应属性的解析结果, 表达式解析失败: {node}");

                        //对于元组的访问，转换为对应属性的token
                        this.PushToken(prop);
                    }
                    else
                    {
                        //对象访问
                        //或者透明对象访问到头了
                        //string fieldName = this.OnGetColumnName((objToken as ObjectToken)?.ValueType, (objToken as ObjectToken)?.Alias, node.Member);
                        //this.PushToken(LTSQLTokenFactory.CreateAccessToken(objToken, fieldName, node.Type));
                        throw new Exception($"无法解析属性访问: {node}");
                    }
                }
            }

            return expr;
        }
        //函数调用
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            Expression expr = base.VisitMethodCall(node);
            //外部表达式树翻译
            if (this.OnTranslateExpression(node, node.Type))
                return expr;

            object val = null;
            LTSQLToken token = null;
            LTSQLToken objToken = null;
            LTSQLToken[] parameters = null;

            //参数列表
            parameters = this.PopAsParamters(node.Arguments.Count);

            //静态方法的调用
            if (node.Object == null)
            {
                //外部成员翻译
                if (this.OnTranslateMember(node.Method, null, node.Method.ReflectedType, node, node.Type, null, parameters))
                    return expr;

                //参数个数为0的静态方法，直接调用求值
                if (node.Arguments.Count == 0)
                {
                    val = node.Method.Invoke(null, null);
                    token = LTSQLTokenFactory.CreateSqlParameterToken(this.Context.ParameterNameGenerator.Next(), val, node.Method.ReturnType);
                    this.PushToken(token);
                    return expr;
                }

                if (!parameters.All(p => p is SqlParameterToken))
                    throw new Exception($"静态方法引用动态参数值，无法继续转换：{node}");

                val = node.Method.Invoke(null, parameters.Select(p => ((SqlParameterToken)p).Value).ToArray());
                token = LTSQLTokenFactory.CreateSqlParameterToken(this.Context.ParameterNameGenerator.Next(), val, node.Method.ReturnType);
                this.PushToken(token);
                return expr;
            }


            /* 实力方法调用*/
            MethodInfo method = node.Method;
            //实例对象
            objToken = this.PopToken();
            if (this.OnTranslateMember(node.Method, objToken is SqlParameterToken p ? p.Value : null, node.Object.Type, node, node.Type, objToken, parameters))
                return expr;

            //实例对象求值
            if (objToken is SqlParameterToken inst)
            {
                if (parameters.IsNotEmpty() && !parameters.All(p => p is SqlParameterToken))
                    throw new Exception($"实例方法无法求值：{node}");
                if (inst.Value == null)
                    throw new Exception($"实例对象为null，无法求值：{node}");

                val = node.Method.Invoke(inst.Value, parameters.Select(p => ((SqlParameterToken)p).Value).ToArray());
                token = LTSQLTokenFactory.CreateSqlParameterToken(this.Context.ParameterNameGenerator.Next(), val, node.Method.ReturnType);
                this.PushToken(token);
                return expr;
            }

            //sql 函数调用
            token = LTSQLTokenFactory.CreateFunctionCallToken(node.Method.Name, parameters, node.Method.ReturnType);
            this.PushToken(token);

            return expr;
        }
        //lambda 表达式
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            //if (this.OnTranslateExpression(node, node.Type))
            //    return node;

            LTSQLToken token = this.PeekToken();
            if (token is GroupObjToken groupToken)
            {
                //表示开始对分组对象的聚合函数作翻译，需要解析lambda表达式作为聚合函数的参数
                LTSQLToken ret = this.TranslateLambda(node.AsLambda(), groupToken.Element);
                this.PushToken(ret);
                return node;
            }

            //访问到lambda表达式，表示某些函数求值，其入参为lambda函数
            this.PushToken(LTSQLTokenFactory.CreateSqlParameterToken(this.Context.ParameterNameGenerator.Next(), node, node.Type));
            return node;
        }
        //new 表达式
        protected override Expression VisitNew(NewExpression node)
        {
            Expression expr = base.VisitNew(node);
            if (this.OnTranslateExpression(node, node.Type))
                return expr;

            TupleToken tuple = new TupleToken(node.Type);
            LTSQLToken[] paras = this.PopAsParamters(node.Arguments.Count);
            if (node.Members.IsNotEmpty())
            {
                for (int i = 0; i < node.Members.Count; i++)
                {
                    PropertyInfo prop = node.Members[i] as PropertyInfo;
                    FieldInfo field = node.Members[i] as FieldInfo;
                    tuple.Add(node.Members[i].Name, paras[i], prop?.PropertyType ?? field.FieldType);
                }
            }

            this.PushToken(tuple);
            return expr;
        }
        //初始化实例
        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            Expression expr = base.VisitMemberInit(node);
            if (this.OnTranslateExpression(node, node.Type))
                return expr;

            if (node.Bindings.Count > 0)
            {
                LTSQLToken[] bindProps = this.PopAsParamters(node.Bindings.Count);
                TupleToken tuple = this.PopToken() as TupleToken;
                tuple = LTSQLTokenFactory.CreateTupleToken(tuple);

                for (int i = 0; i < node.Bindings.Count; i++)
                {
                    PropertyInfo prop = node.Bindings[i].Member as PropertyInfo;
                    FieldInfo field = node.Bindings[i].Member as FieldInfo;
                    tuple.Add(node.Bindings[i].Member.Name, bindProps[i], prop?.PropertyType ?? field.FieldType);
                }
                this.PushToken(tuple);
            }
            return expr;
        }
        //二元表达式
        protected override Expression VisitBinary(BinaryExpression node)
        {
            Expression expr = base.VisitBinary(node);
            if (this.OnTranslateExpression(node, node.Type))
                return expr;

            LTSQLToken right = this.PopToken();
            LTSQLToken left = this.PopToken();
            ValueToken vall = left as ValueToken;
            ValueToken valr = right as ValueToken;

            if (vall == null || valr == null)
                throw new Exception($"二元表达式左右两边的子节点无法正常表示一个值:{node}");

            if (node.NodeType == ExpressionType.Add && node.Type == typeof(string))
            {
                // TODO
                LTSQLToken concat = SqlFunctionHelper.StringConcatFunction(this.Context.Options.DbType, vall, valr).Build();
                this.PushToken(concat);
                return expr;
            }

            vall = vall.TryPriority(true) as ValueToken;
            valr = valr.TryPriority(true) as ValueToken;

            //理论上也不需要验证类型是否相等，因为编译编译通过了就证明类型能够相互转换了
            if (vall.ValueType != valr.ValueType)
            {
                //对可空类型的检验支持，如：int? 与 int 是相等的
                Type nullable = typeof(Nullable<>);
                bool flag1 = vall.ValueType.IsGenericType && (vall.ValueType.GetGenericTypeDefinition() == nullable);
                bool flag2 = valr.ValueType.IsGenericType && (valr.ValueType.GetGenericTypeDefinition() == nullable);
                bool flag3 = !flag1 && !flag2; //是否需要异常
                if (!flag3)
                {
                    Type selfType = null;
                    Type argsType = null;
                    if (flag1)
                    {
                        selfType = valr.ValueType;
                        argsType = vall.ValueType.GetGenericArguments()[0];
                    }
                    else
                    {
                        selfType = vall.ValueType;
                        argsType = valr.ValueType.GetGenericArguments()[0];
                    }
                    flag3 = selfType != argsType;
                }
                if (flag3)
                    throw new Exception($"二元表达式左右两边的子节点求值后的类型不一致:{node}");
            }

            if (node.NodeType == ExpressionType.Equal)
            {
                // join 的联表条件，可能会导致产元组条件
                if (vall is TupleToken tupl && valr is TupleToken tupr)
                {
                    if (tupl.Props.Length != tupr.Props.Length)
                        throw new Exception($"二元表达式左右两边的子节点求值后的类型不一致:{node}");

                    //元组中的各个属性做相等操作，用AND操作连接（join 操作会出现元组对比）
                    BinaryToken cur = null;
                    for (int i = 0; i < tupl.Props.Length; i++)
                    {
                        BinaryToken equals = LTSQLTokenFactory.CreateEqToken(tupl.Props[i], tupr.Props[i]);
                        cur = cur == null ? equals : LTSQLTokenFactory.CreateAndToken(cur, equals);
                    }

                    this.PushToken(cur.IsPriority ? cur : (cur.SetPriority(true) as LTSQLToken));
                    return expr;
                }
            }

            SqlValueToken sqll = vall as SqlValueToken;
            SqlValueToken sqlr = valr as SqlValueToken;
            if (sqll == null || sqlr == null)
                throw new Exception($"二元表达式左右两边的子节点求值后的类型不一致:{node}");

            LTSQLToken binary = null;
            switch (node.NodeType)
            {
                case ExpressionType.Add:
                    binary = LTSQLTokenFactory.CreateAdd(sqll, sqlr, node.Type);
                    break;
                case ExpressionType.Subtract:
                    binary = LTSQLTokenFactory.CreateSubtract(sqll, sqlr, node.Type);
                    break;
                case ExpressionType.Divide:
                    binary = LTSQLTokenFactory.CreateDivide(sqll, sqlr, node.Type);
                    break;
                case ExpressionType.Multiply:
                    binary = LTSQLTokenFactory.CreateMultiply(sqll, sqlr, node.Type);
                    break;
                case ExpressionType.Equal:
                    binary = LTSQLTokenFactory.CreateEqToken(sqll, sqlr);
                    break;
                case ExpressionType.NotEqual:
                    binary = LTSQLTokenFactory.CreateNeqToken(sqll, sqlr);
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    binary = LTSQLTokenFactory.CreateGeToken(sqll, sqlr);
                    break;
                case ExpressionType.LessThanOrEqual:
                    binary = LTSQLTokenFactory.CreateLeToken(sqll, sqlr);
                    break;
                case ExpressionType.LessThan:
                    binary = LTSQLTokenFactory.CreateLtToken(sqll, sqlr);
                    break;
                case ExpressionType.GreaterThan:
                    binary = LTSQLTokenFactory.CreateGtToken(sqll, sqlr);
                    break;
                case ExpressionType.AndAlso:
                    binary = LTSQLTokenFactory.CreateAndToken(sqll, sqlr);
                    break;
                case ExpressionType.OrElse:
                    binary = LTSQLTokenFactory.CreateOrToken(sqll, sqlr);
                    break;
                case ExpressionType.Coalesce:
                    {
                        //空值合并符处理： a ?? b
                        binary = SqlFunctionHelper.CoalesceFunction(this.Context.Options.DbType, node.Type, sqll, sqlr)
                                .Build();
                        break;
                    }
                default:
                    throw new NotImplementedException($"暂不支持此二元表达式翻译：{node.NodeType}");
            }

            if (binary is IPriorable prior)
                binary = prior.IsPriority ? binary : (prior.SetPriority(true) as LTSQLToken);
            if (binary != null)
                this.PushToken(binary);

            return expr;
        }
        //一元表达式：主要是取反操作，not exists 以及 not in 等
        protected override Expression VisitUnary(UnaryExpression node)
        {
            // not int 支持
            // not exists 支持
            Expression expr = base.VisitUnary(node);

            // (int?)val; 类型转换也是一元表达式，需要过滤下
            if (node.NodeType == ExpressionType.Convert)
            {
                //类型转换
                LTSQLToken value = this.PopToken();
                if (value is SqlParameterToken p)
                {
                    p = LTSQLTokenFactory.CreateSqlParameterToken(p.ParameterName, Convert.ChangeType(p.Value, node.Type), node.Type);
                    this.PushToken(p);
                }
                else if (value is ValueToken v)
                {
                    this.PushToken(v.ChangeType(node.Type));
                }
                else
                {
                    this.PushToken(value);
                }
            }

            if (node.NodeType != ExpressionType.Not)
                return expr;
            if (this.OnTranslateExpression(node, node.Type))
                return expr;

            LTSQLToken token = this.PopToken();
            if (token is INotable notable)
                token = notable.Not();
            else
                throw new Exception($"表达式不支持取反操作：{node}");

            this.PushToken(token);
            return expr;
        }
        //条件表达式：三元运算符
        protected override Expression VisitConditional(ConditionalExpression node)
        {
            Expression expr = base.VisitConditional(node);
            if (this.OnTranslateExpression(node, node.Type))
                return expr;

            LTSQLToken thenElse = this.PopToken(); // else 的值
            LTSQLToken thenValue = this.PopToken(); // then 的值
            LTSQLToken then = this.PopToken(); // then 的判断

            this.PushToken(
                LTSQLTokenFactory.CreateSwitchCase(then, thenValue, thenElse, node.Type).SetPriority(true) as LTSQLToken
                );
            return expr;
        }
        //lambda 表达式翻译
        protected virtual LTSQLToken TranslateLambda(LambdaExpression lambda, params LTSQLToken[] rets)
        {
            if (rets.IsNotEmpty() && rets.Length > lambda.Parameters.Count)
                throw new Exception("替换参数个数大于实际的参数个数");

            var paras = lambda.Parameters.ToArray();
            int len = Math.Min(rets?.Length ?? 0, paras?.Length ?? 0);
            for (int i = 0; i < len; ++i)
                this.Context.SetScopeParameter(paras[i].Name, rets[i]);

            this.Visit(lambda.Body);
            return this.PopToken();
        }
    }
}
