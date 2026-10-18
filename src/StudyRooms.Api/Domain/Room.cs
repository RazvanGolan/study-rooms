using Microsoft.EntityFrameworkCore;

namespace StudyRooms.Api.Domain;

// Two rooms cannot have the same name. The database checks this with a unique index.
[Index(nameof(Name), IsUnique = true)]
public class Room
{
    public int Id { get; private set; }    // the database picks it when the room is first saved
    public string Name { get; private set; }
    public string Building { get; private set; }
    public int Capacity { get; private set; }
    public Equipment Equipment { get; private set; }
    public bool IsActive { get; private set; } = true;

    public Room(string name, string building, int capacity, Equipment equipment)
    {
        CheckCapacity(capacity);
        Name = name;
        Building = building;
        Capacity = capacity;
        Equipment = equipment;
    }

    // An admin may lower the capacity below the size of a booking. That booking keeps its people,
    // but nobody new can join it until it fits again.
    public void Edit(string name, string building, int capacity, Equipment equipment)
    {
        CheckCapacity(capacity);
        Name = name;
        Building = building;
        Capacity = capacity;
        Equipment = equipment;
    }

    // A removed room is hidden, not deleted, so past bookings can still show it.
    public void Remove()
    {
        IsActive = false;
    }

    private static void CheckCapacity(int capacity)
    {
        if (capacity < 1)
        {
            throw new ArgumentException("Capacity must be at least 1.", nameof(capacity));
        }
    }
}
