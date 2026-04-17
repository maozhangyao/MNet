using System;
using System.Collections.Generic;
using System.Linq;

namespace MNet.LTSQL.SqlTokens
{
    //直接表示SQL语法编码的一部分
    public class SyntaxToken : LTSQLToken
    {
        public SyntaxToken(string text)
        {
            this.Text = text;
        }
        

        public readonly string Text;
        //是否需要关键字转义
        public readonly bool EscapeKey;


        public static SyntaxToken Create(string text)
        {
            return new SyntaxToken(text);
        }
        public static SyntaxToken[] CreateBatch(params string[] texts)
        {
            SyntaxToken[] tokens = texts.Select(txt => Create(txt)).ToArray();
            return tokens;
        }
    }
}
