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
			// Hämtar alla ord från databasen
			List<string> allWords = _context.Words
				.Select(w => w.WordString)
				.ToList();

			// Slumpar 3 ord 
			return allWords
				.OrderBy(w => Guid.NewGuid())
				.Take(3)
				.ToList();
		}
    }
}