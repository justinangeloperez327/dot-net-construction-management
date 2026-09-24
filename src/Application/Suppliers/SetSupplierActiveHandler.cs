namespace Application.Suppliers;

public sealed class SetSupplierActiveHandler(
    ISupplierRepository suppliers)
{
    public async Task<SupplierActionResult> HandleAsync(
        Guid supplierId,
        bool isActive,
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

        supplier.SetActive(isActive);

        await suppliers.SaveChangesAsync(cancellationToken);

        return SupplierActionResult.Success(supplier.Id);
    }
}
