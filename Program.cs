//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.IdentityModel.Tokens;
//using NotesApp.Api.Services;
//using System;
//using System.Text;

//var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddControllers();
//builder.Services.AddSingleton<MongoService>();
//builder.Services.AddScoped<UserService>();
//builder.Services.AddScoped<NoteService>();

//var mongoConn = builder.Configuration["MongoDb:ConnectionString"] ?? Environment.GetEnvironmentVariable("MongoConnectionString");
//var jwtSecret = builder.Configuration["Jwt:Secret"] ?? Environment.GetEnvironmentVariable("JwtSecret");

//// JWT Auth Setup
//var key = Encoding.ASCII.GetBytes(jwtSecret);

//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//})
//.AddJwtBearer(options =>
//{
//    options.RequireHttpsMetadata = false;
//    options.SaveToken = true;
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateIssuerSigningKey = true,
//        ValidIssuer = builder.Configuration["Jwt:Issuer"],
//        ValidAudience = builder.Configuration["Jwt:Audience"],
//        IssuerSigningKey = new SymmetricSecurityKey(key)
//    };
//});

//var app = builder.Build();

//app.UseAuthentication();
//app.UseAuthorization();
//app.MapControllers();
//app.Run();

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using NotesApp.Api.Services;

var builder = WebApplication.CreateBuilder(args);

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
