using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QADraft.Models
{   //What creates the Event Table
    public class Events
    {
        public int Id { get; set; }

        [Required]
        public DateTime EventTime { get; set; }

        [Required]
        public string EventInformation { get; set; }
        
        public string? Color { get; set; }



    }
}
