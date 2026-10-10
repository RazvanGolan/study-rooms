using System.Text.Json.Serialization;
using StudyRooms.Api;
using StudyRooms.Api.Domain;

var builder = WebApplication.CreateBuilder(args);

// Find every controller in the project, and write enums as text in JSON ("Open" instead of 1).
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// One Timetable for the whole application. Every controller that asks for it gets the same object.
builder.Services.AddSingleton<Timetable>();

var app = builder.Build();

AddSampleRooms(app.Services.GetRequiredService<Timetable>());

app.MapControllers();

app.Run();

static void AddSampleRooms(Timetable timetable)
{
    timetable.AddRoom("A101", "Library", 4, Equipment.Whiteboard);
    timetable.AddRoom("A102", "Library", 6, Equipment.Whiteboard | Equipment.PowerOutlets);
    timetable.AddRoom("B204", "Electrical Engineering", 10, Equipment.Projector | Equipment.Tv | Equipment.PowerOutlets);
}
