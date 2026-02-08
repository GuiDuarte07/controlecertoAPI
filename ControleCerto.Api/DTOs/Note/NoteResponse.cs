using System;

namespace ControleCerto.DTOs.Note
{
    public class NoteResponse
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
