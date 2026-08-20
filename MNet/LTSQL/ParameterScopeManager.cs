using System;
using MNet.LTSQL.SqlTokens;
using System.Collections.Generic;

namespace MNet.LTSQL
{
    /// <summary>
    /// 参数管理器
    /// </summary>
    public class ParameterScopeManager
    {
        public ParameterScopeManager()
        {
            this._scopes = new Stack<Dictionary<string, LTSQLToken>>();
        }

        private Dictionary<string, LTSQLToken> _cur => this._scopes.Count > 0 ? this._scopes.Peek() : null;
        private readonly Stack<Dictionary<string, LTSQLToken>> _scopes = new Stack<Dictionary<string, LTSQLToken>>();
        

        public void PopScope()
        {
            if (this._scopes.Count > 0)
                this._scopes.Pop();
        }
        public void PushScope()
        {
            this._scopes.Push(new Dictionary<string, LTSQLToken>());
        }
        public void RemoveParameter(string parameter)
        {
            if (this._cur == null)
                return;

            this._cur.Remove(parameter);
        }
        public LTSQLToken RetrieveParameter(string parameter)
        {
            foreach (Dictionary<string, LTSQLToken> scope in this._scopes)
            {
                if (scope.TryGetValue(parameter, out LTSQLToken val))
                    return val;
            }
            return null;
        }
        public void InjectParameter(string parameter, LTSQLToken parameterValue)
        {
            if (this._cur == null)
                throw new Exception("空参数栈，无法注入参数值.");

            this._cur[parameter] = parameterValue;
        }       
    }
}
