namespace StudyRooms.Api.Domain;

public class Room
{
    public int Id { get; }
    public string Name { get; private set; }
    public string Building { get; private set; }
    public int Capacity { get; private set; }
    public Equipment Equipment { get; private set; }
    public bool IsActive { get; private set; } = true;

    public Room(int id, string name, string building, int capacity, Equipment equipment)
    {
        if (capacity < 1)
        {
            throw new ArgumentException("Capacity must be at least 1.", nameof(capacity));
        }

        Id = id;
        Name = name;
        Building = building;
        Capacity = capacity;
        Equipment = equipment;
    }
}
