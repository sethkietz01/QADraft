// Models/GeekQAViewModel.cs
using Microsoft.AspNetCore.Mvc.Rendering;
using QADraft.Models;
using System.Collections.Generic;

public class GeekQAViewModel
{
    public GeekQA GeekQA { get; set; }
    public List<SelectListItem> Users { get; set; }
}
