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

        public DateTime EventTime { get; set; }

        
        public string EventInformation { get; set; }

    }
}
