namespace Domain.Suppliers;

public sealed class Supplier
{
    private Supplier()
    {
    }

    private Supplier(
        string supplierCode,
        string name,
        string? category,
        string? contactPerson,
        string? email,
        string? phone,
        string? address,
        string? registrationNumber,
        string? taxRegistrationNumber)
    {
        Id = Guid.NewGuid();
        IsActive = true;

        SetDetails(
            supplierCode,
            name,
            category,
            contactPerson,
            email,
            phone,
            address,
            registrationNumber,
            taxRegistrationNumber);
    }

    public Guid Id { get; private set; }

    public string SupplierCode { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Category { get; private set; }

    public string? ContactPerson { get; private set; }

    public string? Email { get; private set; }

    public string? Phone { get; private set; }

    public string? Address { get; private set; }

    public string? RegistrationNumber { get; private set; }

    public string? TaxRegistrationNumber { get; private set; }

    public bool IsActive { get; private set; }

    public static Supplier Create(
        string supplierCode,
        string name,
        string? category,
        string? contactPerson,
        string? email,
        string? phone,
        string? address,
        string? registrationNumber,
        string? taxRegistrationNumber)
    {
        return new Supplier(
            supplierCode,
            name,
            category,
            contactPerson,
            email,
            phone,
            address,
            registrationNumber,
            taxRegistrationNumber);
    }

    public void Update(
        string supplierCode,
        string name,
        string? category,
        string? contactPerson,
        string? email,
        string? phone,
        string? address,
        string? registrationNumber,
        string? taxRegistrationNumber)
    {
        SetDetails(
            supplierCode,
            name,
            category,
            contactPerson,
            email,
            phone,
            address,
            registrationNumber,
            taxRegistrationNumber);
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }

    private void SetDetails(
        string supplierCode,
        string name,
        string? category,
        string? contactPerson,
        string? email,
        string? phone,
        string? address,
        string? registrationNumber,
        string? taxRegistrationNumber)
    {
        supplierCode = supplierCode.Trim();
        name = name.Trim();

        if (string.IsNullOrWhiteSpace(supplierCode))
        {
            throw new ArgumentException(
                "Supplier code is required.",
                nameof(supplierCode));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Supplier name is required.",
                nameof(name));
        }

        SupplierCode = supplierCode;
        Name = name;
        Category = NormalizeOptional(category);
        ContactPerson = NormalizeOptional(contactPerson);
        Email = NormalizeOptional(email);
        Phone = NormalizeOptional(phone);
        Address = NormalizeOptional(address);
        RegistrationNumber = NormalizeOptional(registrationNumber);
        TaxRegistrationNumber = NormalizeOptional(taxRegistrationNumber);
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
}
