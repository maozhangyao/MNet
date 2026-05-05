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

    public class CPersionSelect1
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
