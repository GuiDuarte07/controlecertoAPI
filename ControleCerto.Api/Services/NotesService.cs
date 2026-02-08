using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ControleCerto.DTOs.Note;
using ControleCerto.Errors;
using ControleCerto.Models.AppDbContext;
using ControleCerto.Models.Entities;
using ControleCerto.Services.Interfaces;
using ControleCerto.Enums;
using Microsoft.EntityFrameworkCore;

namespace ControleCerto.Services
{
    public class NotesService : INotesService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;

        public NotesService(AppDbContext appDbContext, IMapper mapper)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
        }

        public async Task<Result<NoteResponse>> CreateNoteAsync(CreateNoteRequest request, int userId)
        {
            var note = new Note
            {
                UserId = userId,
                Title = request.Title,
                Content = request.Content,
                Year = request.Year,
                Month = request.Month
            };

            await _appDbContext.Notes.AddAsync(note);
            await _appDbContext.SaveChangesAsync();

            return _mapper.Map<NoteResponse>(note);
        }

        public async Task<Result<NoteResponse>> UpdateNoteAsync(UpdateNoteRequest request, int userId)
        {
            var note = await _appDbContext.Notes.FirstOrDefaultAsync(n => n.Id == request.Id && n.UserId == userId);
            if (note is null)
                return new AppError("Anotação não encontrada.", ErrorTypeEnum.NotFound);

            note.Title = request.Title;
            note.Content = request.Content;
            note.UpdatedAt = DateTime.UtcNow;

            _appDbContext.Notes.Update(note);
            await _appDbContext.SaveChangesAsync();

            return _mapper.Map<NoteResponse>(note);
        }

        public async Task<Result<NoteResponse>> GetNoteAsync(long noteId, int userId)
        {
            var note = await _appDbContext.Notes.FirstOrDefaultAsync(n => n.Id == noteId && n.UserId == userId);
            if (note is null)
                return new AppError("Anotação não encontrada.", ErrorTypeEnum.NotFound);

            return _mapper.Map<NoteResponse>(note);
        }

        public async Task<Result<IEnumerable<NoteResponse>>> GetNotesByMonthAsync(int? year, int? month, int userId)
        {
            var query = _appDbContext.Notes.Where(n => n.UserId == userId);

            if (year.HasValue && month.HasValue)
            {
                query = query.Where(n => n.Year == year && n.Month == month);
            }
            else if (year.HasValue)
            {
                query = query.Where(n => n.Year == year && n.Month == null);
            }

            var notes = await query
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            // Se não encontrou notas para o mês/ano específico e ambos foram fornecidos, criar um vazio
            if (!notes.Any() && year.HasValue && month.HasValue)
            {
                var emptyNote = new Note
                {
                    UserId = userId,
                    Title = $"Anotações - {month}/{year}",
                    Content = "",
                    Year = year,
                    Month = month
                };

                await _appDbContext.Notes.AddAsync(emptyNote);
                await _appDbContext.SaveChangesAsync();

                notes.Add(emptyNote);
            }

            var dto = _mapper.Map<IEnumerable<NoteResponse>>(notes);
            return new Result<IEnumerable<NoteResponse>>(dto);
        }

        public async Task<Result<IEnumerable<NoteResponse>>> GetGeneralNotesAsync(int userId)
        {
            var notes = await _appDbContext.Notes
                .Where(n => n.UserId == userId && n.Year == null && n.Month == null)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            var dto = _mapper.Map<IEnumerable<NoteResponse>>(notes);
            return new Result<IEnumerable<NoteResponse>>(dto);
        }

        public async Task<Result<IEnumerable<NoteResponse>>> GetAllNotesAsync(int userId)
        {
            var notes = await _appDbContext.Notes
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.Year)
                .ThenByDescending(n => n.Month)
                .ThenByDescending(n => n.CreatedAt)
                .ToListAsync();

            var dto = _mapper.Map<IEnumerable<NoteResponse>>(notes);
            return new Result<IEnumerable<NoteResponse>>(dto);
        }

        public async Task<Result<bool>> DeleteNoteAsync(long noteId, int userId)
        {
            var note = await _appDbContext.Notes.FirstOrDefaultAsync(n => n.Id == noteId && n.UserId == userId);
            if (note is null)
                return new AppError("Anotação não encontrada.", ErrorTypeEnum.NotFound);

            _appDbContext.Notes.Remove(note);
            await _appDbContext.SaveChangesAsync();

            return true;
        }
    }
}
