namespace Domain.Clients;

public sealed class Client
{
    private Client()
    {
    }

    private Client(
        Guid id,
        string name,
        string? contactPerson,
        string? email,
        string? phone,
        string? address)
    {
        Id = id;
        IsActive = true;

        SetDetails(
            name,
            contactPerson,
            email,
            phone,
            address);
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? ContactPerson { get; private set; }

    public string? Email { get; private set; }

    public string? Phone { get; private set; }

    public string? Address { get; private set; }

    public bool IsActive { get; private set; }

    public static Client Create(
        string name,
        string? contactPerson,
        string? email,
        string? phone,
        string? address)
    {
        return new Client(
            Guid.NewGuid(),
            name,
            contactPerson,
            email,
            phone,
            address);
    }

    public void Update(
        string name,
        string? contactPerson,
        string? email,
        string? phone,
        string? address)
    {
        SetDetails(
            name,
            contactPerson,
            email,
            phone,
            address);
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }

    private void SetDetails(
        string name,
        string? contactPerson,
        string? email,
        string? phone,
        string? address)
    {
        name = name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Client name is required.",
                nameof(name));
        }

        Name = name;
        ContactPerson = NormalizeOptional(contactPerson);
        Email = NormalizeOptional(email);
        Phone = NormalizeOptional(phone);
        Address = NormalizeOptional(address);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
