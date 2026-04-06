using System;
using System.Collections.Generic;
using System.Linq;

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
        public SyntaxToken(string text, bool escape)
        {
            this.Text = text;
            this.EscapeKey = escape;
        }


        public readonly string Text;
        //是否需要关键字转义
        public readonly bool EscapeKey;


        public override IEnumerable<LTSQLToken> GetChildren()
        {
            return Array.Empty<LTSQLToken>();
        }
        public static SyntaxToken Create(string text)
        {
            return new SyntaxToken(text);
        }
        public static SyntaxToken Create(string text, bool escape)
        {
            return new SyntaxToken(text, escape);
        }
        public static SyntaxToken[] CreateBatch(params string[] texts)
        {
            SyntaxToken[] tokens = texts.Select(txt => Create(txt)).ToArray();
            return tokens;
        }
    }
}
