using System;
using System.Collections.Generic;

namespace MNet.LTSQL.v1.SqlTokens
{
    //直接表示SQL语法编码的一部分
    public class SyntaxToken : LTSQLToken
    {
        public SyntaxToken()
        { }
        public SyntaxToken(string text)
        {
            this.Text = text;
        }


        public readonly string Text;

        public override IEnumerable<LTSQLToken> GetChildren()
        {
            return Array.Empty<LTSQLToken>();
        }
        public static SyntaxToken Create(string text) 
        {
            return new SyntaxToken(text);
        }
    }
}
