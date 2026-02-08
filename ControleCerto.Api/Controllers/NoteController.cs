using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ControleCerto.DTOs.Note;
using ControleCerto.Services.Interfaces;
using ControleCerto.Decorators;
using ControleCerto.Extensions;

namespace ControleCerto.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ExtractTokenInfo]
    public class NoteController : ControllerBase
    {
        private readonly INotesService _notesService;

        public NoteController(INotesService notesService)
        {
            _notesService = notesService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateNote([FromBody] CreateNoteRequest request)
        {
            var userId = (int)HttpContext.Items["UserId"]!;
            var result = await _notesService.CreateNoteAsync(request, userId);
            
            if (result.IsSuccess)
                return Created("GetAllNotes", result.Value);
            
            return result.HandleReturnResult();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNote(long id, [FromBody] UpdateNoteRequest request)
        {
            var userId = (int)HttpContext.Items["UserId"]!;
            request.Id = id;
            var result = await _notesService.UpdateNoteAsync(request, userId);
            return result.HandleReturnResult();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetNote(long id)
        {
            var userId = (int)HttpContext.Items["UserId"]!;
            var result = await _notesService.GetNoteAsync(id, userId);
            return result.HandleReturnResult();
        }

        [HttpGet("month")]
        public async Task<IActionResult> GetNotesByMonth([FromQuery] int? year, [FromQuery] int? month)
        {
            var userId = (int)HttpContext.Items["UserId"]!;
            var result = await _notesService.GetNotesByMonthAsync(year, month, userId);
            return result.HandleReturnResult();
        }

        [HttpGet("general")]
        public async Task<IActionResult> GetGeneralNotes()
        {
            var userId = (int)HttpContext.Items["UserId"]!;
            var result = await _notesService.GetGeneralNotesAsync(userId);
            return result.HandleReturnResult();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllNotes()
        {
            var userId = (int)HttpContext.Items["UserId"]!;
            var result = await _notesService.GetAllNotesAsync(userId);
            return result.HandleReturnResult();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNote(long id)
        {
            var userId = (int)HttpContext.Items["UserId"]!;
            var result = await _notesService.DeleteNoteAsync(id, userId);
            return result.HandleReturnResult();
        }
    }
}
