using System;

namespace MIRA
{
    public class JournalEntry
    {
        public DateTimeOffset EnteredAt {get; set;}
        public string Entry {get; set;}

        public JournalEntry(string entry)
        {
            Entry = entry;
            EnteredAt = DateTimeOffset.Now;
        }
    }
}