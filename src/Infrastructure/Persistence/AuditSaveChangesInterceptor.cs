using System.Globalization;
using System.Text.Json;
using Application.Auditing;
using Application.Common.Authentication;
using Domain.Projects;
using Infrastructure.Persistence.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Infrastructure.Persistence;

public sealed class AuditSaveChangesInterceptor(
    ICurrentUser currentUser,
    TimeProvider timeProvider)
    : SaveChangesInterceptor
{
    private static readonly JsonSerializerOptions JsonOptions =
        new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is DbContext context)
        {
            EnsureAppendOnly(context);

            AppendAuditLogs(
                context,
                actor: null,
                timeProvider.GetUtcNow());
        }

        return result;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not DbContext context)
        {
            return result;
        }

        EnsureAppendOnly(context);

        context.ChangeTracker.DetectChanges();

        if (!context.ChangeTracker.Entries().Any(IsAuditable))
        {
            return result;
        }

        var actor = await currentUser.GetAsync(cancellationToken);

        AppendAuditLogs(
            context,
            actor,
            timeProvider.GetUtcNow());

        return result;
    }

    private static void EnsureAppendOnly(DbContext context)
    {
        var invalidAuditChanges = context.ChangeTracker
            .Entries<AuditLog>()
            .Any(entry =>
                entry.State is EntityState.Modified
                    or EntityState.Deleted);

        if (invalidAuditChanges)
        {
            throw new InvalidOperationException(
                "Audit log entries are append-only and cannot be modified or deleted.");
        }
    }

    private static void AppendAuditLogs(
        DbContext context,
        CurrentUserInfo? actor,
        DateTimeOffset occurredAt)
    {
        context.ChangeTracker.DetectChanges();

        var entries = context.ChangeTracker
            .Entries()
            .Where(IsAuditable)
            .ToArray();

        if (entries.Length == 0)
        {
            return;
        }

        var auditLogs = entries
            .Select(entry => BuildAuditLog(
                context,
                entry,
                actor,
                occurredAt))
            .Where(audit => audit is not null)
            .Cast<AuditLog>()
            .ToArray();

        if (auditLogs.Length > 0)
        {
            context.Set<AuditLog>().AddRange(auditLogs);
        }
    }

    private static bool IsAuditable(EntityEntry entry)
    {
        if (entry.State is not EntityState.Added
            and not EntityState.Modified
            and not EntityState.Deleted)
        {
            return false;
        }

        var entityNamespace = entry.Metadata.ClrType.Namespace;

        if (entityNamespace is null ||
            !entityNamespace.StartsWith(
                "Domain.",
                StringComparison.Ordinal))
        {
            return false;
        }

        return entry.State != EntityState.Modified ||
            entry.Properties.Any(property => property.IsModified);
    }

    private static AuditLog? BuildAuditLog(
        DbContext context,
        EntityEntry entry,
        CurrentUserInfo? actor,
        DateTimeOffset occurredAt)
    {
        var action = entry.State switch
        {
            EntityState.Added => AuditAction.Created,
            EntityState.Modified => AuditAction.Updated,
            EntityState.Deleted => AuditAction.Deleted,
            _ => (AuditAction?)null
        };

        if (action is null)
        {
            return null;
        }

        var changes = BuildChanges(entry);

        if (changes.Count == 0)
        {
            return null;
        }

        return new AuditLog(
            ResolveProjectId(
                context,
                entry,
                new HashSet<object>(
                    ReferenceEqualityComparer.Instance)),
            actor?.UserId,
            actor?.Email,
            action.Value.ToString(),
            entry.Metadata.ClrType.Name,
            BuildEntityId(entry),
            JsonSerializer.Serialize(
                changes,
                JsonOptions),
            occurredAt);
    }

    private static SortedDictionary<string, AuditPropertyChange> BuildChanges(
        EntityEntry entry)
    {
        var changes = new SortedDictionary<
            string,
            AuditPropertyChange>(
            StringComparer.Ordinal);

        foreach (var property in entry.Properties
                     .Where(property =>
                         !property.Metadata.IsShadowProperty())
                     .OrderBy(property => property.Metadata.Name))
        {
            if (entry.State == EntityState.Modified &&
                !property.IsModified)
            {
                continue;
            }

            var originalValue = entry.State == EntityState.Added
                ? null
                : NormalizeValue(property.OriginalValue);

            var currentValue = entry.State == EntityState.Deleted
                ? null
                : NormalizeValue(property.CurrentValue);

            changes[property.Metadata.Name] =
                new AuditPropertyChange(
                    originalValue,
                    currentValue);
        }

        return changes;
    }

    private static string BuildEntityId(EntityEntry entry)
    {
        var key = entry.Metadata.FindPrimaryKey();

        if (key is null)
        {
            return "(no key)";
        }

        var parts = key.Properties
            .Select(property =>
            {
                var value = GetEntryValue(
                    entry,
                    property);

                var formatted = FormatIdentifier(value);

                return key.Properties.Count == 1
                    ? formatted
                    : $"{property.Name}={formatted}";
            });

        return string.Join(";", parts);
    }

    private static Guid? ResolveProjectId(
        DbContext context,
        EntityEntry entry,
        HashSet<object> visited)
    {
        if (!visited.Add(entry.Entity))
        {
            return null;
        }

        if (entry.Metadata.ClrType == typeof(Project))
        {
            return TryGuid(
                entry,
                "Id");
        }

        var directProjectId = TryGuid(
            entry,
            "ProjectId");

        if (directProjectId is not null)
        {
            return directProjectId;
        }

        foreach (var foreignKey in entry.Metadata.GetForeignKeys())
        {
            if (foreignKey.PrincipalEntityType.ClrType.Namespace
                    ?.StartsWith(
                        "Domain.",
                        StringComparison.Ordinal)
                != true)
            {
                continue;
            }

            var dependentValues = foreignKey.Properties
                .Select(property => GetEntryValue(
                    entry,
                    property))
                .ToArray();

            if (dependentValues.Any(value => value is null))
            {
                continue;
            }

            var principalEntry = context.ChangeTracker
                .Entries()
                .FirstOrDefault(candidate =>
                    candidate.Metadata ==
                        foreignKey.PrincipalEntityType &&
                    KeyMatches(
                        candidate,
                        foreignKey.PrincipalKey,
                        dependentValues));

            if (principalEntry is null)
            {
                continue;
            }

            var projectId = ResolveProjectId(
                context,
                principalEntry,
                visited);

            if (projectId is not null)
            {
                return projectId;
            }
        }

        return null;
    }

    private static bool KeyMatches(
        EntityEntry principalEntry,
        IKey principalKey,
        IReadOnlyList<object?> dependentValues)
    {
        if (principalKey.Properties.Count != dependentValues.Count)
        {
            return false;
        }

        for (var index = 0;
             index < principalKey.Properties.Count;
             index++)
        {
            var principalValue = GetEntryValue(
                principalEntry,
                principalKey.Properties[index]);

            if (!Equals(
                    principalValue,
                    dependentValues[index]))
            {
                return false;
            }
        }

        return true;
    }

    private static Guid? TryGuid(
        EntityEntry entry,
        string propertyName)
    {
        var property = entry.Metadata.FindProperty(propertyName);

        if (property is null)
        {
            return null;
        }

        var value = GetEntryValue(
            entry,
            property);

        return value is Guid guid &&
            guid != Guid.Empty
                ? guid
                : null;
    }

    private static object? GetEntryValue(
        EntityEntry entry,
        IProperty property)
    {
        var propertyEntry = entry.Property(property.Name);

        return entry.State == EntityState.Deleted
            ? propertyEntry.OriginalValue
            : propertyEntry.CurrentValue;
    }

    private static object? NormalizeValue(object? value) =>
        value switch
        {
            null => null,
            Enum enumValue => enumValue.ToString(),
            DateOnly dateOnly => dateOnly.ToString(
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture),
            TimeOnly timeOnly => timeOnly.ToString(
                "HH:mm:ss.fffffff",
                CultureInfo.InvariantCulture),
            DateTimeOffset dateTimeOffset =>
                dateTimeOffset.ToString(
                    "O",
                    CultureInfo.InvariantCulture),
            DateTime dateTime => dateTime.ToString(
                "O",
                CultureInfo.InvariantCulture),
            Guid guid => guid.ToString(),
            byte[] bytes => $"<binary:{bytes.Length}>",
            _ => value
        };

    private static string FormatIdentifier(object? value) =>
        value switch
        {
            null => "null",
            Guid guid => guid.ToString(),
            IFormattable formattable =>
                formattable.ToString(
                    null,
                    CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty
        };

    private sealed record AuditPropertyChange(
        object? Old,
        object? New);
}
