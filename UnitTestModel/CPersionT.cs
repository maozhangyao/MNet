using System.ComponentModel.DataAnnotations.Schema;

namespace UnitTestModel
{
    /// <summary>
    /// a human info
    /// </summary>
    [Table("c_persion_t")]
    public class CPersionT
    {
        [Column("id")]
        public int Id { get; set; }

        //[NonFiled]
        public int Age { get; set; }

        public string SelfName { get; set; }
        //[Column("FId")]
        public int FatherId { get; set; }

        //[Column("MId")]
        public int MotherId { get; set; }
    }

    /// <summary>
    /// 课程
    /// </summary>
    [Table("c_course_t")]
    public class CCourseT
    {
        [Column("id")]
        public int Id { get; set; }
        //课程名称
        public string Course { get; set; }
    }

    /// <summary>
    /// 老师
    /// </summary>
    [Table("c_teacher_t")]
    public class CTeacherT
    {
        //一个human id
        public int PersionId { get; set; }
        //课程id
        public int CourseId { get; set; }
    }
}
