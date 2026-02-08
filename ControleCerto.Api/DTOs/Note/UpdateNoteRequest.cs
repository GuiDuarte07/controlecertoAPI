using System;

namespace ControleCerto.DTOs.Note
{
    public class UpdateNoteRequest
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
    }
}
