using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using api.Data;
using api.Services;
using api.Middleware;
using OfficeOpenXml;

// Configure EPPlus license for non-commercial use
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

// Add Entity Framework with SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Add Controllers
builder.Services.AddControllers();

// Add custom services
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<DatabaseSeeder>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("AllowAngular");

app.UseAuthentication();
app.UseMiddleware<UserActiveMiddleware>();
app.UseAuthorization();

app.MapControllers();

// Debug endpoints
app.MapGet("/api/debug/add-isactive-column", async (ApplicationDbContext context) => 
{
    try 
    {
        // Check if column exists
        var checkQuery = @"PRAGMA table_info(Users);";
        var connection = context.Database.GetDbConnection();
        await connection.OpenAsync();
        
        using var checkCommand = connection.CreateCommand();
        checkCommand.CommandText = checkQuery;
        
        bool hasIsActiveColumn = false;
        using (var reader = await checkCommand.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                var columnName = reader["name"].ToString();
                if (columnName == "IsActive")
                {
                    hasIsActiveColumn = true;
                    break;
                }
            }
        }
        
        if (!hasIsActiveColumn)
        {
            // Add the IsActive column
            using var alterCommand = connection.CreateCommand();
            alterCommand.CommandText = "ALTER TABLE Users ADD COLUMN IsActive INTEGER NOT NULL DEFAULT 1;";
            await alterCommand.ExecuteNonQueryAsync();
            
            // Mark migrations as applied
            using var insertCommand = connection.CreateCommand();
            insertCommand.CommandText = @"
            INSERT OR IGNORE INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES 
            ('20250723071234_AddSchoolsTable', '9.0.0'),
            ('20250723075106_AddSchoolSettings', '9.0.0'), 
            ('20250723081435_UpdateSchoolSettingsConstraints', '9.0.0'),
            ('20250723095215_AddBranchesAndCoursesSimplified', '9.0.0'),
            ('20250725073326_AddIsActiveToUsers', '9.0.0');";
            await insertCommand.ExecuteNonQueryAsync();
            
            return new { success = true, message = "IsActive column added successfully" };
        }
        else
        {
            return new { success = true, message = "IsActive column already exists" };
        }
    }
    catch (Exception ex)
    {
        return new { success = false, message = ex.Message };
    }
});

app.MapGet("/api/debug/test", () => new { message = "API is working", timestamp = DateTime.UtcNow });

app.MapGet("/api/debug/users", async (ApplicationDbContext context) => 
{
    var users = await context.Users.Select(u => new { u.Username, u.Role }).ToListAsync();
    var branches = await context.Branches.Select(b => new { b.BranchName, b.SchoolHeadUsername, b.Location }).ToListAsync();
    
    return new { 
        users = users,
        branches = branches,
        message = "Debug info for users and branches"
    };
});

app.MapGet("/api/debug/auth", (HttpContext context) => 
{
    var hasAuthHeader = context.Request.Headers.ContainsKey("Authorization");
    var authHeader = context.Request.Headers["Authorization"].ToString();
    var userClaims = context.User?.Claims?.Select(c => new { c.Type, c.Value }).ToList();
    
    return new { 
        hasAuthorizationHeader = hasAuthHeader,
        authorizationHeader = authHeader.Length > 50 ? authHeader.Substring(0, 50) + "..." : authHeader,
        isAuthenticated = context.User?.Identity?.IsAuthenticated ?? false,
        userName = context.User?.Identity?.Name,
        claims = userClaims
    };
});

app.MapGet("/api/debug/decode-token", (HttpContext context) => 
{
    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
    if (authHeader == null || !authHeader.StartsWith("Bearer "))
    {
        return Results.Json(new { error = "No valid Bearer token found" });
    }
    
    var token = authHeader.Substring("Bearer ".Length).Trim();
    
    try 
    {
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);
        var claims = jsonToken.Claims.Select(c => new { c.Type, c.Value }).ToList();
        
        return Results.Json(new { 
            success = true,
            claims = claims,
            expiry = jsonToken.ValidTo
        });
    }
    catch (Exception ex)
    {
        return Results.Json(new { error = ex.Message });
    }
});

app.MapGet("/api/debug/auth-test", (HttpContext context) => 
{
    var hasAuthHeader = context.Request.Headers.ContainsKey("Authorization");
    var authHeader = context.Request.Headers["Authorization"].ToString();
    var userClaims = context.User?.Claims?.Select(c => new { c.Type, c.Value }).ToList();
    
    return new { 
        hasAuthorizationHeader = hasAuthHeader,
        authorizationHeader = authHeader,
        isAuthenticated = context.User?.Identity?.IsAuthenticated ?? false,
        userName = context.User?.Identity?.Name,
        claims = userClaims
    };
}).RequireAuthorization();

app.Run();
