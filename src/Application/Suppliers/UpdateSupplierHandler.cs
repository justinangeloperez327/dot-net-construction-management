namespace Application.Suppliers;

public sealed record UpdateSupplierRequest(
    string SupplierCode,
    string Name,
    string? Category,
    string? ContactPerson,
    string? Email,
    string? Phone,
    string? Address,
    string? RegistrationNumber,
    string? TaxRegistrationNumber);

public sealed class UpdateSupplierHandler(
    ISupplierRepository suppliers)
{
    public async Task<SupplierActionResult> HandleAsync(
        Guid supplierId,
        UpdateSupplierRequest request,
        CancellationToken cancellationToken = default)
    {
        var supplier = await suppliers.GetByIdAsync(
            supplierId,
            cancellationToken);

        if (supplier is null)
        {
            return SupplierActionResult.Failure(
                "Supplier was not found.");
        }

        var supplierCode = request.SupplierCode.Trim();

        if (await suppliers.SupplierCodeExistsAsync(
                supplierCode,
                supplier.Id,
                cancellationToken))
        {
            return SupplierActionResult.Failure(
                "A supplier with this code already exists.");
        }

        try
        {
            supplier.Update(
                supplierCode,
                request.Name,
                request.Category,
                request.ContactPerson,
                request.Email,
                request.Phone,
                request.Address,
                request.RegistrationNumber,
                request.TaxRegistrationNumber);

            await suppliers.SaveChangesAsync(cancellationToken);

            return SupplierActionResult.Success(supplier.Id);
        }
        catch (ArgumentException exception)
        {
            return SupplierActionResult.Failure(exception.Message);
        }
    }
}
