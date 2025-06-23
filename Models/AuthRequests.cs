﻿namespace NotesApp.Api.Models
{
    public class RegisterRequest
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
    }

    public class LoginRequest
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
}
