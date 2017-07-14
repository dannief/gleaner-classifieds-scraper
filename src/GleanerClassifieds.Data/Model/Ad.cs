using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GleanerClassifieds.Data.Model
{
    public class Ad
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public int CategoryId { get; set; }

        public Category Category { get; set; }

        [StringLength(64)]
        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime ListedOn { get; set; }

        public DateTime ExpiresOn { get; set; }
    }
}
