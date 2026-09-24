using Domain.Suppliers;
using Xunit;

namespace Domain.Tests;

public sealed class SupplierTests
{
    [Fact]
    public void Create_normalizes_supplier_details()
    {
        var supplier = Supplier.Create(
            " SUP-001 ",
            " Gulf Technical Supplies ",
            " MEP ",
            " Ahmed ",
            " sales@example.com ",
            " +971 2 000 0000 ",
            " Abu Dhabi ",
            " CN-123 ",
            " 100000000000001 ");

        Assert.Equal("SUP-001", supplier.SupplierCode);
        Assert.Equal("Gulf Technical Supplies", supplier.Name);
        Assert.Equal("MEP", supplier.Category);
        Assert.Equal("Ahmed", supplier.ContactPerson);
        Assert.Equal("sales@example.com", supplier.Email);
        Assert.True(supplier.IsActive);
    }

    [Fact]
    public void Supplier_requires_code_and_name()
    {
        Assert.Throws<ArgumentException>(() =>
            Supplier.Create(
                " ",
                "Supplier",
                null,
                null,
                null,
                null,
                null,
                null,
                null));

        Assert.Throws<ArgumentException>(() =>
            Supplier.Create(
                "SUP-001",
                " ",
                null,
                null,
                null,
                null,
                null,
                null,
                null));
    }

    [Fact]
    public void Supplier_can_be_deactivated_and_reactivated()
    {
        var supplier = Supplier.Create(
            "SUP-001",
            "Supplier",
            null,
            null,
            null,
            null,
            null,
            null,
            null);

        supplier.SetActive(false);
        Assert.False(supplier.IsActive);

        supplier.SetActive(true);
        Assert.True(supplier.IsActive);
    }
}
