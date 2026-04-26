using System;

namespace AIA
{
    public class JournalEntry
    {
        public DateTimeOffset EnteredAt {get; set;}
        public string Entry {get; set;}

        public JournalEntry(string entry)
        {
            Entry = entry;
            EnteredAt = DateTimeOffset.UtcNow;
        }
    }
}