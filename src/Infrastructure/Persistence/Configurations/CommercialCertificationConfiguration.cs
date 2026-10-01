using Domain.Approvals;
using Domain.Commercial;
using Domain.PaymentApplications;
using Domain.Projects;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class CommercialCertificationConfiguration
    : IEntityTypeConfiguration<CommercialCertification>
{
    public void Configure(
        EntityTypeBuilder<CommercialCertification> builder)
    {
        builder.ToTable("CommercialCertifications");

        builder.HasKey(certification => certification.Id);

        builder.Property(certification => certification.CertificateNumber)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(certification => certification.CertificateDate)
            .HasColumnType("date");

        builder.Property(certification => certification.ClaimedAmount)
            .HasPrecision(18, 2);

        builder.Property(certification => certification.CertifiedAmount)
            .HasPrecision(18, 2);

        builder.Property(certification => certification.RetentionPercent)
            .HasPrecision(5, 2);

        builder.Property(
                certification =>
                    certification.AdvancePaymentRecoveryAmount)
            .HasPrecision(18, 2);

        builder.Property(certification => certification.Notes)
            .HasMaxLength(2000);

        builder.Property(certification => certification.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(certification => new
        {
            certification.ProjectId,
            certification.CertificateNumber
        }).IsUnique();

        builder.HasIndex(certification => certification.PaymentApplicationId)
            .IsUnique();

        builder.HasIndex(certification => certification.CreatedByUserId);
        builder.HasIndex(certification => certification.ApprovalRequestId);

        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(certification => certification.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<PaymentApplication>()
            .WithMany()
            .HasForeignKey(certification => certification.PaymentApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(certification => certification.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApprovalRequest>()
            .WithMany()
            .HasForeignKey(certification => certification.ApprovalRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(certification => certification.OtherDeductions)
            .WithOne()
            .HasForeignKey(deduction => deduction.CommercialCertificationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(certification => certification.OtherDeductions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class CommercialDeductionConfiguration
    : IEntityTypeConfiguration<CommercialDeduction>
{
    public void Configure(
        EntityTypeBuilder<CommercialDeduction> builder)
    {
        builder.ToTable("CommercialDeductions");

        builder.HasKey(deduction => deduction.Id);

        builder.Property(deduction => deduction.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(deduction => deduction.Amount)
            .HasPrecision(18, 2);

        builder.HasIndex(deduction =>
            deduction.CommercialCertificationId);
    }
}
