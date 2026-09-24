using Domain.Suppliers;

namespace Application.Suppliers;

public sealed record CreateSupplierRequest(
    string SupplierCode,
    string Name,
    string? Category,
    string? ContactPerson,
    string? Email,
    string? Phone,
    string? Address,
    string? RegistrationNumber,
    string? TaxRegistrationNumber);

public sealed class CreateSupplierHandler(
    ISupplierRepository suppliers)
{
    public async Task<SupplierActionResult> HandleAsync(
        CreateSupplierRequest request,
        CancellationToken cancellationToken = default)
    {
        var supplierCode = request.SupplierCode.Trim();

        if (await suppliers.SupplierCodeExistsAsync(
                supplierCode,
                cancellationToken: cancellationToken))
        {
            return SupplierActionResult.Failure(
                "A supplier with this code already exists.");
        }

        try
        {
            var supplier = Supplier.Create(
                supplierCode,
                request.Name,
                request.Category,
                request.ContactPerson,
                request.Email,
                request.Phone,
                request.Address,
                request.RegistrationNumber,
                request.TaxRegistrationNumber);

            await suppliers.AddAsync(
                supplier,
                cancellationToken);

            await suppliers.SaveChangesAsync(cancellationToken);

            return SupplierActionResult.Success(supplier.Id);
        }
        catch (ArgumentException exception)
        {
            return SupplierActionResult.Failure(exception.Message);
        }
    }
}
