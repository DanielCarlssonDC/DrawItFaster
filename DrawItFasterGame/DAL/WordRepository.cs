using System;
using System.Collections.Generic;
using System.Linq;
using DrawItFaster.Models;

namespace DrawItFaster.DAL
{
    public class WordRepository
    {
        private readonly DrawItDbContext _context;

        // Database context injection
        public WordRepository(DrawItDbContext context)
        {
            _context = context;
        }

        public List<string> GetThreeRandomWords()
        {
            // Mix, extract text, take 3, convert to list
            return _context.Words
                .OrderBy(w => Guid.NewGuid())
                .Select(w => w.WordString)
                .Take(3)
                .ToList();
        }
    }
}