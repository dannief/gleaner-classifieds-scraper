using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GleanerClassifieds.Data.Model
{
    public class Category
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [StringLength(32)]
        public string Name { get; set; }

        public int SectionId { get; set; }

        public Section Section { get; set; }
    }
}
