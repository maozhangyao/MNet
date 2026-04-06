using System;
using System.Collections.Generic;
using MNet.LTSQL.v1.SqlTokens;

namespace MNet.LTSQL.v1
{
    public class FunctionTokenBuilder
    {
        private string _funcName;
        private Type _typeOfValue;
        private LTSQLToken[] _funcArgs;
        private Func<LTSQLToken[], Stack<LTSQLToken>> _rangingArgs;
        private Action<Stack<LTSQLToken>, Queue<LTSQLToken>> _takingArgs;


        public FunctionTokenBuilder WithFunctionName(string functionName, Type typeOfValue)
        {
            this._funcName = functionName;
            this._typeOfValue = typeOfValue;
            return this;
        }

        public FunctionTokenBuilder WithFunctionArgs(params LTSQLToken[] args)
        {
            this._funcArgs = args;
            if (args == null)
                return this.UseRecursionCall(null, false);

            return this.UseRecursionCall((all, use) => {
                while (all.Count > 0)
                    use.Enqueue(all.Pop());
            }, reverse: false);
        }

        public FunctionTokenBuilder UseRecursionCall(
            Action<Stack<LTSQLToken>, Queue<LTSQLToken>> takingArgs
            , bool reverse = false
            )
        {
            if (takingArgs == null)
            {
                this._takingArgs = null;
                this._rangingArgs = null;
                return this;
            }

            this._rangingArgs = (args) =>
            {
                Stack<LTSQLToken> stack = new Stack<LTSQLToken>();
                if (reverse)
                {
                    for (int i = 0; i < args.Length; i++)
                        stack.Push(args[i]);
                }
                else
                {
                    for (int i = args.Length - 1; i >= 0; i--)
                        stack.Push(args[i]);
                }
                return stack;
            };

            this._takingArgs = takingArgs;
            return this;
        }

        public LTSQLToken Builder()
        {
            LTSQLToken func = LTSQLTokenFactory.CreateFunctionCallToken(this._funcName, null, _typeOfValue); 
            Queue<LTSQLToken> use = new Queue<LTSQLToken>();
            Stack<LTSQLToken> all = this._rangingArgs != null ? this._rangingArgs(this._funcArgs) : new Stack<LTSQLToken>();

            while (all.Count > 0)
            {
                int cnt = all.Count;
                this._takingArgs(all, use);
                if (all.Count != 0 && !(cnt - all.Count >= 1))
                    throw new Exception("递归构造函数调用时，每次参数个数的消耗必须大于等2个。");

                func = LTSQLTokenFactory.CreateFunctionCallToken(this._funcName, use.ToArray(), this._typeOfValue);

                if (all.Count <= 0)
                    break;

                all.Push(func);
                use.Clear();
            }

            return func;
        }
    }
}
