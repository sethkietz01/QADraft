using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QADraft.Models
{   //What creates the QA Tables and what keeps the references for them
    public class GeekQA
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select the user who committed the QA")]
        public int? CommittedById { get; set; }

        [ForeignKey("CommittedById")]
        public User? CommittedBy { get; set; }

        [Required(ErrorMessage = "Please select the user who found the QA")]
        public int? FoundById { get; set; }

        [ForeignKey("FoundById")]
        public User? FoundBy { get; set; }

        [Required(ErrorMessage = "Please select the category of error")]
        [StringLength(100)]
        public string CategoryOfError { get; set; }

        [Required(ErrorMessage = "Please select the nature of error")]
        [StringLength(255)]
        public string NatureOfError { get; set; }

        public int Severity { get; set; }

        [Required(ErrorMessage = " ")]
        [StringLength(100)]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = " ")]
        [StringLength(50)]
        public string UnitId { get; set; }

        // Validation attribute for ErrorDate being before FoundOn
        [DateBefore(nameof(FoundOn), ErrorMessage = "Error Date must be before or the same as  Found On Date.")]
        public DateTime ErrorDate { get; set; }

        // Validation attribute for FoundOn being before the current date
        [DateBeforeNow()]
        public DateTime FoundOn { get; set; }

        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? SubmittedBy { get; set; } 

        [NotMapped]
        public List<SelectListItem>? Users { get; set; }
        [NotMapped]
        public List<SelectListItem>? Coordinators { get; set; }
    }

    // Validation class to check if Error Date is before Found On date
    public class DateBeforeAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;
        
        public DateBeforeAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var propertyInfo = validationContext.ObjectType.GetProperty(_comparisonProperty);
            if (propertyInfo == null)
            {
                return new ValidationResult($"Unknown property {_comparisonProperty}");
            }

            var comparisonValue = propertyInfo.GetValue(validationContext.ObjectInstance) as DateTime?;

            if (value != null && comparisonValue != null && (DateTime)value > comparisonValue)
            {
                return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} must be before {_comparisonProperty}");
            }

            return ValidationResult.Success;
        }
    }

    public class DateBeforeNow : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                DateTime currentValue = (DateTime)value;

                if (currentValue > DateTime.Today.Date)
                {
                    return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} must be before today's date.");
                }
            }

            return ValidationResult.Success;
        }
    }
}
