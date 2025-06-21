using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotesApp.Api.Models;
using NotesApp.Api.Services;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NotesApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotesController : ControllerBase
    {
        private readonly NoteService _noteService;

        public NotesController(NoteService noteService)
        {
            _noteService = noteService;
        }

        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        [HttpGet]
        public async Task<IActionResult> GetUserNotes()
        {
            var userId = GetUserId();
            var notes = await _noteService.GetUserNotesAsync(userId);
            return Ok(notes);
        }

        [HttpPost]
        public async Task<IActionResult> CreateNote([FromBody] Note note)
        {
            note.UserId = GetUserId();
            note.CreatedAt = DateTime.UtcNow;
            note.UpdatedAt = DateTime.UtcNow;

            await _noteService.CreateAsync(note);
            return Ok(note);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNote(string id, [FromBody] Note note)
        {
            var userId = GetUserId();
            var existing = await _noteService.GetNoteByIdAsync(id);

            if (existing == null || existing.UserId != userId)
                return NotFound("Note not found or access denied.");

            note.Id = id;
            note.UserId = userId;
            note.CreatedAt = existing.CreatedAt;
            note.UpdatedAt = DateTime.UtcNow;

            await _noteService.UpdateAsync(id, note);
            return Ok(note);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNote(string id)
        {
            var userId = GetUserId();
            var existing = await _noteService.GetNoteByIdAsync(id);

            if (existing == null || existing.UserId != userId)
                return NotFound("Note not found or access denied.");

            await _noteService.DeleteAsync(id);
            return Ok("Note deleted.");
        }
    }
}
