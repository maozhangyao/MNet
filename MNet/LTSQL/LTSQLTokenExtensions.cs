using System;
using MNet.LTSQL.SqlTokenExtends;
using MNet.LTSQL.SqlTokens;

namespace MNet.LTSQL
{
    public static class LTSQLTokenExtensions
    {
        public static LTSQLToken TryPriority(this LTSQLToken token, bool prior)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            if (token is IPriorable priorable && priorable.IsPriority != prior)
                return priorable.SetPriority(prior) as LTSQLToken;
            return token;
        }

        //元组展开
        public static ITupleable ExpendTuple(this ITupleable tuple, Type newMappingType)
        {
            if (tuple == null)
                throw new ArgumentNullException(nameof(tuple));

            // 需要注意，key的唯一性问题
            TupleToken _new = new TupleToken(newMappingType ?? tuple.MappingType);
            foreach ((string key, LTSQLToken value) in tuple)
            {
                if (value is ITupleable innerTuple)
                {
                    ITupleable subTuple = ExpendTuple(innerTuple, innerTuple.MappingType);
                    foreach ((string subKey, LTSQLToken subValue) in subTuple)
                    {
                        _new.Add(subKey, subValue, subTuple.GetValueType(subKey));
                    }
                }
                else
                {
                    _new.Add(key, value, tuple.GetValueType(key));
                }
            }
            return _new;
        }

        public static bool TryGetSqlQueryable(this LTSQLToken token, out ILTSQLObjectQueryable queryable)
        {
            if (token is SqlParameterToken p && p.Value is ILTSQLObjectQueryable q)
            {
                queryable = q;
                return true;
            }

            queryable = null;
            return false;
        }
    }
}

