
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Part_2.Data;
using System.Text;

// Creates the application builder.
var builder = WebApplication.CreateBuilder(args);

// Register API controllers
builder.Services.AddControllers();

// Connects to the SQL Server database
builder.Services.AddDbContext<RaceDayContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("RaceDayConnection")));

// Adds Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Adds password hashing service
builder.Services.AddScoped<
    Microsoft.AspNetCore.Identity.IPasswordHasher<Part_2.Models.UserAccount>,
    Microsoft.AspNetCore.Identity.PasswordHasher<Part_2.Models.UserAccount>>();

// Configures JWT authentication
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key is missing.");

// Converts the secret key into bytes for token validation.
var key = Encoding.UTF8.GetBytes(jwtKey);

// Sets up authentication using JWT bearer tokens.
builder.Services.AddAuthentication(options =>
{
    // Uses JWT tokens to identify logged-in users.
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;
    // Requests without valid authentication are challenged.
    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Defines how the API checks incoming JWT tokens.
    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            // These values must match the token settings.
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            // Checks that the token was signed with our secret key
            IssuerSigningKey = new SymmetricSecurityKey(key),

            // Does not allow extra time after token expiry.
            ClockSkew = TimeSpan.Zero
        };
});
// Enables role-based access restrictions.
builder.Services.AddAuthorization();

// Builds the web application.
var app = builder.Build();

// Enables Swagger during development.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Redirects HTTP requests to HTTPS.
app.UseHttpsRedirection();

// Identifies the user from their authentication token.
app.UseAuthentication();

// Enforces the [Authorize] and role restrictions on controllers.
app.UseAuthorization();

// Makes controller endpoints available.
app.MapControllers();

// Starts the API.
app.Run();

// Defines a partial class for the program, allowing for future extensions.
public partial class Program {}
