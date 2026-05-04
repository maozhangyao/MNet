using System.Reflection;

namespace MNet.LTSQL
{
    public class LTSQLMemberContext
    {
        /// <summary>
        /// 所属者类型
        /// </summary>
        public Type Owner { get; set; }
        /// <summary>
        /// 所属者使用的参数名称
        /// </summary>
        public string OwnerName { get; set; }
        /// <summary>
        /// 成员
        /// </summary>
        public MemberInfo Member { get; set; }
    }
}
