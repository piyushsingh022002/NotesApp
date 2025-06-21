using MongoDB.Driver;
using NotesApp.Api.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NotesApp.Api.Services
{
    public class NoteService
    {
        private readonly IMongoCollection<Note> _notes;

        public NoteService(MongoService mongoService)
        {
            _notes = mongoService.GetCollection<Note>("notes");
        }

        public async Task<List<Note>> GetUserNotesAsync(string userId)
        {
            return await _notes.Find(n => n.UserId == userId).ToListAsync();
        }

        public async Task<Note?> GetNoteByIdAsync(string id)
        {
            return await _notes.Find(n => n.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Note note)
        {
            await _notes.InsertOneAsync(note);
        }

        public async Task UpdateAsync(string id, Note updatedNote)
        {
            await _notes.ReplaceOneAsync(n => n.Id == id, updatedNote);
        }

        public async Task DeleteAsync(string id)
        {
            await _notes.DeleteOneAsync(n => n.Id == id);
        }
    }
}
