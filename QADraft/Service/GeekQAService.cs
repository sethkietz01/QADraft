using QADraft.Data;
using QADraft.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QADraft.Services
{
    public class GeekQAService
    {
        private readonly ApplicationDbContext _context;

        public GeekQAService(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<GeekQA> GetFilteredQAs(DateTime? startDate, DateTime? endDate, string categoryOfError)
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

            if (!string.IsNullOrWhiteSpace(categoryOfError))
            {
                query = query.Where(qa => qa.CategoryOfError == categoryOfError);
            }

            return query.ToList();
        }
    }
}
