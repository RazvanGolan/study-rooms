using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using StudyRooms.Api;
using StudyRooms.Api.Data;
using StudyRooms.Api.Domain;
using StudyRooms.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Find every controller in the project, and write enums as text in JSON ("Open" instead of 1).
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// A description of every endpoint, in the OpenAPI format, for the Scalar page.
// The transformer tells Scalar that requests carry a bearer token, so the page shows a box to paste it in.
builder.Services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());

// The "Booking" section of appsettings.json, read into a BookingOptions object.
builder.Services.Configure<BookingOptions>(builder.Configuration.GetSection("Booking"));

// The SQLite database, in the file named in appsettings.json. AddDbContext registers AppDbContext as scoped.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("StudyRooms")));

// Passwords are stored as hashes. PasswordHasher comes with ASP.NET Core.
builder.Services.AddSingleton<PasswordHasher<AppUser>>();

// Authentication: who is this? Every request may carry "Authorization: Bearer <token>", and this reads it.
// Authorization: may they do this? [Authorize] on a controller decides, using what authentication found.
builder.Services.AddAuthentication(BearerTokenDefaults.AuthenticationScheme)
    .AddBearerToken();
builder.Services.AddAuthorization();

// The browser lets a page call an API on another site only if the API allows that page's origin.
// The interface runs on GitHub Pages and, while we develop it, on localhost.
string[] origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod()));

// The services use the AppDbContext of the request, so they are scoped too: a new one for every request.
builder.Services.AddScoped<BookingService>();
builder.Services.AddScoped<RoomService>();
builder.Services.AddScoped<AccountService>();

// BookingRules only reads the settings, so one object can serve every request.
builder.Services.AddSingleton<BookingRules>();

var app = builder.Build();

// At startup there is no request, so we make a scope of our own to get an AppDbContext.
using (IServiceScope scope = app.Services.CreateScope())
{
    AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();    // create the database, or bring it up to the latest migration

    // Sample rooms and accounts, only on our own computers, and only when there are none yet.
    if (app.Environment.IsDevelopment())
    {
        if (db.Rooms.Count() == 0)
        {
            AddSampleRooms(db);
        }
        if (db.Users.Count() == 0)
        {
            AddSampleAccounts(db, scope.ServiceProvider.GetRequiredService<PasswordHasher<AppUser>>());
        }
    }
}

app.MapOpenApi();               // the description, at /openapi/v1.json
app.MapScalarApiReference();    // a page to read it and try every endpoint, at /scalar

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// To start over, stop the API and delete study-rooms.db.
static void AddSampleRooms(AppDbContext db)
{
    db.Rooms.Add(new Room("A101", "Library", 3, Equipment.Whiteboard));
    db.Rooms.Add(new Room("A102", "Library", 6, Equipment.Whiteboard | Equipment.PowerOutlets));
    db.Rooms.Add(new Room("B204", "Electrical Engineering", 10, Equipment.Projector | Equipment.Tv | Equipment.PowerOutlets));
    db.SaveChanges();
}

// Every sample account has the password Password1!
// Real accounts get a GUID as their id. The sample ones get short ids, so the .http file can name them.
static void AddSampleAccounts(AppDbContext db, PasswordHasher<AppUser> passwordHasher)
{
    List<AppUser> users =
    [
        new AppUser("admin", "admin@example.com", "Admin", true),
        new AppUser("ana", "ana.popescu@example.com", "Ana P.", false),
        new AppUser("mihai", "mihai.ionescu@example.com", "Mihai I.", false),
        new AppUser("ioana", "ioana.dumitru@example.com", "Ioana D.", false),
        new AppUser("radu", "radu.stan@example.com", "Radu S.", false),
    ];

    foreach (AppUser user in users)
    {
        user.SetPasswordHash(passwordHasher.HashPassword(user, "Password1!"));
        db.Users.Add(user);
    }
    db.SaveChanges();
}
