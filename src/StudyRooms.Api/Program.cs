using System.Text.Json.Serialization;
using Scalar.AspNetCore;
using StudyRooms.Api;
using StudyRooms.Api.Domain;

var builder = WebApplication.CreateBuilder(args);

// Find every controller in the project, and write enums as text in JSON ("Open" instead of 1).
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// A description of every endpoint, in the OpenAPI format, for the Scalar page.
builder.Services.AddOpenApi();

// The "Booking" section of appsettings.json, read into a BookingOptions object.
builder.Services.Configure<BookingOptions>(builder.Configuration.GetSection("Booking"));

// One Timetable and one BookingRules for the whole application.
builder.Services.AddSingleton<Timetable>();
builder.Services.AddSingleton<BookingRules>();

var app = builder.Build();

AddSampleData(app.Services.GetRequiredService<Timetable>());

app.MapOpenApi();               // the description, at /openapi/v1.json
app.MapScalarApiReference();    // a page to read it and try every endpoint, at /scalar
app.MapControllers();

app.Run();

static void AddSampleData(Timetable timetable)
{
    timetable.AddRoom("A101", "Library", 3, Equipment.Whiteboard);
    timetable.AddRoom("A102", "Library", 6, Equipment.Whiteboard | Equipment.PowerOutlets);
    timetable.AddRoom("B204", "Electrical Engineering", 10, Equipment.Projector | Equipment.Tv | Equipment.PowerOutlets);

    timetable.AddUser("ana", "Ana P.", "ana.popescu@example.com");
    timetable.AddUser("mihai", "Mihai I.", "mihai.ionescu@example.com");
    timetable.AddUser("ioana", "Ioana D.", "ioana.dumitru@example.com");
    timetable.AddUser("radu", "Radu S.", "radu.stan@example.com");
}
