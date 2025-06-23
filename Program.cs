using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using NotesApp.Api.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();


// Load configuration values
var jwtSecret = builder.Configuration["JwtSecret"];
var jwtIssuer = builder.Configuration["JwtIssuer"];
var jwtAudience = builder.Configuration["JwtAudience"];
var jwtExpiryMinutes = builder.Configuration["JwtExpiryMinutes"];
var mongoConnection = builder.Configuration["MongoConnectionString"];
var mongoDatabase = builder.Configuration["MongoDatabase"];

// Validate JWT secret
if (string.IsNullOrWhiteSpace(jwtSecret))
{
    throw new Exception("JwtSecret is missing in environment variables. App cannot start.");
}

// Configure Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MongoDB services
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<NoteService>();
builder.Services.AddSingleton<MongoService>();


// CORS (allow everything for now — you can tighten later)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });
Console.WriteLine($"JwtSecret: {jwtSecret?.Length} chars, JwtIssuer: {jwtIssuer}, JwtAudience: {jwtAudience}");
try { 
var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"[FATAL ERROR] {ex.GetType()}: {ex.Message}");
    throw;
}
