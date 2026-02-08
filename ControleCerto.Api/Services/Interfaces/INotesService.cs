using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ControleCerto.DTOs.Note;
using ControleCerto.Errors;

namespace ControleCerto.Services.Interfaces
{
    public interface INotesService
    {
        Task<Result<NoteResponse>> CreateNoteAsync(CreateNoteRequest request, int userId);
        Task<Result<NoteResponse>> UpdateNoteAsync(UpdateNoteRequest request, int userId);
        Task<Result<NoteResponse>> GetNoteAsync(long noteId, int userId);
        Task<Result<IEnumerable<NoteResponse>>> GetNotesByMonthAsync(int? year, int? month, int userId);
        Task<Result<IEnumerable<NoteResponse>>> GetGeneralNotesAsync(int userId);
        Task<Result<IEnumerable<NoteResponse>>> GetAllNotesAsync(int userId);
        Task<Result<bool>> DeleteNoteAsync(long noteId, int userId);
    }
}
