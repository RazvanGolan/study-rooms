using System.Text.Json.Serialization;
using StudyRooms.Api;
using StudyRooms.Api.Domain;

var builder = WebApplication.CreateBuilder(args);

// One Timetable for the whole application. Any endpoint that asks for it gets the same object.
builder.Services.AddSingleton<Timetable>();

// Write enums as text in JSON ("Open" instead of 1), and accept them as text too.
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

AddSampleRooms(app.Services.GetRequiredService<Timetable>());

app.MapGet("/rooms", (Timetable timetable) => timetable.GetRooms());

app.MapGet("/rooms/{id}", (int id, Timetable timetable) =>
{
    Room? room = timetable.FindRoom(id);
    return room is null ? Results.NotFound() : Results.Ok(room);
});

// GET /timetable?date=2026-10-20
app.MapGet("/timetable", (DateOnly date, Timetable timetable) => timetable.GetBookings(date));

app.MapGet("/bookings/{id}", (int id, Timetable timetable) =>
{
    Booking? booking = timetable.FindBooking(id);
    return booking is null ? Results.NotFound() : Results.Ok(booking);
});

app.MapPost("/bookings", (BookingRequest request, Timetable timetable, ILogger<Program> logger) =>
{
    if (timetable.FindRoom(request.RoomId) is null)
    {
        return Results.NotFound($"Room {request.RoomId} does not exist.");
    }
    if (request.End <= request.Start)
    {
        return Results.BadRequest("A booking must end after it starts.");
    }

    if (!timetable.TryBook(request.RoomId, request.Date, request.Start, request.End, request.UserId,
            request.JoinPolicy, out Booking? booking, out string? error))
    {
        logger.LogInformation("Booking refused for {UserId}: {Reason}", request.UserId, error);
        return Results.Conflict(error);
    }

    logger.LogInformation("Booking {BookingId} created by {UserId} for room {RoomId}, {Date} {Start}-{End}",
        booking.Id, booking.OwnerId, booking.RoomId, booking.Date, booking.Start, booking.End);
    return Results.Created($"/bookings/{booking.Id}", booking);
});

app.Run();

static void AddSampleRooms(Timetable timetable)
{
    timetable.AddRoom("A101", "Library", 4, Equipment.Whiteboard);
    timetable.AddRoom("A102", "Library", 6, Equipment.Whiteboard | Equipment.PowerOutlets);
    timetable.AddRoom("B204", "Electrical Engineering", 10, Equipment.Projector | Equipment.Tv | Equipment.PowerOutlets);
}
