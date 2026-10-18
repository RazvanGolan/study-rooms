using Microsoft.EntityFrameworkCore;
using StudyRooms.Api.Domain;

namespace StudyRooms.Api.Data;

// The database, as C# sees it: one DbSet for each table.
// ASP.NET Core creates one AppDbContext for each request (it is a scoped service).
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<User> Users => Set<User>();
}
