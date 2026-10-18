using System.Text.Json.Serialization;
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
builder.Services.AddOpenApi();

// The "Booking" section of appsettings.json, read into a BookingOptions object.
builder.Services.Configure<BookingOptions>(builder.Configuration.GetSection("Booking"));

// The SQLite database, in the file named in appsettings.json. AddDbContext registers AppDbContext as scoped.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("StudyRooms")));

// The services use the AppDbContext of the request, so they are scoped too: a new one for every request.
builder.Services.AddScoped<BookingService>();
builder.Services.AddScoped<RoomService>();

// BookingRules only reads the settings, so one object can serve every request.
builder.Services.AddSingleton<BookingRules>();

var app = builder.Build();

// At startup there is no request, so we make a scope of our own to get an AppDbContext.
using (IServiceScope scope = app.Services.CreateScope())
{
    AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();    // create the database, or bring it up to the latest migration
    if (db.Rooms.Count() == 0)
    {
        AddSampleData(db);
    }
}

app.MapOpenApi();               // the description, at /openapi/v1.json
app.MapScalarApiReference();    // a page to read it and try every endpoint, at /scalar
app.MapControllers();

app.Run();

// Only when the database is new. To start over, stop the API and delete study-rooms.db.
static void AddSampleData(AppDbContext db)
{
    db.Rooms.Add(new Room("A101", "Library", 3, Equipment.Whiteboard));
    db.Rooms.Add(new Room("A102", "Library", 6, Equipment.Whiteboard | Equipment.PowerOutlets));
    db.Rooms.Add(new Room("B204", "Electrical Engineering", 10, Equipment.Projector | Equipment.Tv | Equipment.PowerOutlets));

    db.Users.Add(new User("ana", "Ana P.", "ana.popescu@example.com"));
    db.Users.Add(new User("mihai", "Mihai I.", "mihai.ionescu@example.com"));
    db.Users.Add(new User("ioana", "Ioana D.", "ioana.dumitru@example.com"));
    db.Users.Add(new User("radu", "Radu S.", "radu.stan@example.com"));

    db.SaveChanges();
}
