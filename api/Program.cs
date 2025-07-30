using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using api.Data;
using api.Services;
using api.Middleware;
using OfficeOpenXml;

// Configure EPPlus license for non-commercial use
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

// Add Entity Framework with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                Console.WriteLine($"🔑 JWT - Token received: {context.Token?[..50] ?? "null"}...");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("✅ JWT - Token validated successfully");
                var claims = context.Principal?.Claims.Select(c => $"{c.Type}={c.Value}");
                Console.WriteLine($"✅ JWT - Claims: {string.Join(", ", claims ?? new string[0])}");
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"❌ JWT - Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            }
        };
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.Name
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

app.MapPost("/api/debug/create-bgss-user", async (ApplicationDbContext context) => 
{
    try 
    {
        // Check if bgss user already exists
        var existingStudent = await context.Students.FirstOrDefaultAsync(s => s.Username == "bgss");
        if (existingStudent != null)
        {
            return Results.Json(new { success = true, message = "User bgss already exists", userId = existingStudent.Id });
        }
        
        // Get first school
        var school = await context.Schools.FirstOrDefaultAsync();
        if (school == null)
        {
            return Results.Json(new { success = false, message = "No school found" });
        }
        
        // Create bgss student
        var newStudent = new api.Models.Student
        {
            Username = "bgss",
            Email = "bgss@school.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            FullName = "BGSS Student",
            Grade = "Grade 10",
            RollNumber = "BGSS001",
            ParentName = "BGSS Parent",
            ParentPhone = "555-9999",
            Phone = "555-8888",
            IsActive = true,
            SchoolId = school.Id,
            CreatedAt = DateTime.UtcNow
        };
        
        context.Students.Add(newStudent);
        await context.SaveChangesAsync();
        
        return Results.Json(new { success = true, message = "User bgss created successfully", userId = newStudent.Id });
    }
    catch (Exception ex)
    {
        return Results.Json(new { success = false, message = ex.Message });
    }
});

app.Run();
