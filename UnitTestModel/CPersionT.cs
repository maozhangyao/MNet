using System.ComponentModel.DataAnnotations.Schema;

namespace UnitTestModel
{
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

    [Table("c_course_t")]
    public class CCourseT
    {
        [Column("id")]
        public int Id { get; set; }
        public string Course { get; set; }
    }

    [Table("c_teacher_t")]
    public class CTeacherT
    {
        public int PersionId { get; set; }
        public int CourseId { get; set; }
    }
}
