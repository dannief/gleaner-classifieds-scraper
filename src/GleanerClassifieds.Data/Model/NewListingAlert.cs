using System;
using System.ComponentModel.DataAnnotations;

namespace GleanerClassifieds.Data.Model
{
    public class NewListingAlert
    {
        public int Id { get; set; }

        [StringLength(128)]
        public string EmailAddress { get; set; }

        public int? CategoryId { get; set; }

        public Category Category { get; set; }

        public int? SectionId { get; set; }

        public Section Section { get; set; }

        [StringLength(128)]
        public string Keywords { get; set; }

        public DateTime? LastAlerted { get; set; }

        public bool IsActive { get; set; }
    }
}
