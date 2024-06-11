using System;
using System.Collections.Generic;
using System.Linq;
using QADraft.Data;
using QADraft.Models;

namespace QADraft.Services
{
    public class GeekQAService
    {
        private readonly ApplicationDbContext _context;

        public GeekQAService(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<GeekQA> GetFilteredQAs(DateTime? startDate, DateTime? endDate, string category)
        {
            var query = _context.GeekQAs.AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(qa => qa.ErrorDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(qa => qa.ErrorDate <= endDate.Value);
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(qa => qa.CategoryOfError == category);
            }

            return query.ToList();
        }
    }
}
