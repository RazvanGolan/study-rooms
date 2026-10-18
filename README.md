# Study Rooms

A web application for booking study rooms at the faculty. We build it together in the optional part of the CFLP C# lab, one step each week.

Slides for every session: https://razvangolan.github.io/cflp-lab/

The rules of the application are in [REQUIREMENTS.md](REQUIREMENTS.md). We agreed on them in class in week 2.

## Following along

After each lab, the code from that session gets a tag with the week number. To get the code as it was at the end of week 5:

```bash
git clone https://github.com/RazvanGolan/study-rooms.git
cd study-rooms
git checkout week-05
```

To see what changed in one week, compare two tags on GitHub, for example https://github.com/RazvanGolan/study-rooms/compare/week-04...week-05.

| Week | What we add |
|---|---|
| 3 | Classes for rooms and bookings, then a first web API that keeps its data in memory |
| 4 | Request and response models, validation, joining a booking, API documentation |
| 5 | A SQLite database with EF Core, and what happens when two people book the same room at once |
| 6 | Accounts with a user and an admin role, and the admin side |
| 8 | The timetable in the browser, as a separate app on GitHub Pages |
| 9 | Reminders and notifications sent in the background |
| 10 | Notifications as a second service, run together with Aspire, and telemetry |
| 11 | The application online, with sign-in through GitHub or Google |

There is no optional part in week 7 or week 12, when the tests take place. The plan may still change as we go.

## Running it

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download). From the repository folder:

```bash
dotnet run --project src/StudyRooms.Api
```

The API listens on http://localhost:5080. Open http://localhost:5080/scalar in a browser to see every endpoint and try it out. You can also open `src/StudyRooms.Api/StudyRooms.Api.http` in VS Code (with the REST Client extension) or Rider and click Send Request above each request, or use `curl`:

```bash
curl -X POST http://localhost:5080/bookings -H "Content-Type: application/json" \
  -d '{"roomId":1,"date":"2026-10-21","start":"10:00","end":"12:00","userId":"ana","joinPolicy":"Open"}'
```

Bookings must start in the next 7 days, so change the date to one that fits. The data lives in a SQLite database, the file `src/StudyRooms.Api/study-rooms.db`. The API creates it on the first run and fills in the sample rooms and users, and it keeps your bookings when you stop and start the API again. To start over, stop the API and delete the file.

## Changing the database

When you change a class that is stored in the database, add a migration, a small C# file that describes the change. The `dotnet ef` tool writes it for you. The repository lists the tool in `dotnet-tools.json`, so one command installs it for this folder:

```bash
dotnet tool restore
dotnet ef migrations add AddRoomFloor --project src/StudyRooms.Api
```

The next `dotnet run` applies the new migration to `study-rooms.db`.

## Using it for your project

If you pick the complex project variant, you can look here to see how the API, the database, the interface and the background work fit together. Your project needs its own domain, so build your own application and use this one as a reference.
