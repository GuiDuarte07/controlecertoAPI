using System;

namespace ControleCerto.DTOs.Note
{
    public class CreateNoteRequest
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
    }
}
