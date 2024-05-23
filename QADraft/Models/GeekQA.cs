using QADraft.Data;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QADraft.Models
{
    public class GeekQA
    {
        public int Id { get; set; }

        public int CommittedById { get; set; }
        [ForeignKey("CommittedById")]
        public User CommittedBy { get; set; }

        public int FoundById { get; set; }
        [ForeignKey("FoundById")]
        public User FoundBy { get; set; }

        [StringLength(100)]
        public string CategoryOfError { get; set; }

        [StringLength(255)]
        public string NatureOfError { get; set; }

        [Range(1, 10)]
        public int Severity { get; set; } // e.g., from 1-10

        [StringLength(100)]
        public string CustomerName { get; set; }

        [StringLength(50)]
        public string UnitId { get; set; }

        public DateTime ErrorDate { get; set; }

        public DateTime FoundOn { get; set; }

        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
