using Domain.Projects;
using Xunit;

namespace Domain.Tests;

public sealed class ProjectTests
{
    [Fact]
    public void Create_sets_active_status_and_normalizes_text()
    {
        var project = Project.Create(
            " P-001 ",
            " Tower Project ",
            " Abu Dhabi ",
            new DateOnly(2026, 9, 23),
            new DateOnly(2027, 9, 23),
            " Main contract ");

        Assert.Equal("P-001", project.ProjectNumber);
        Assert.Equal("Tower Project", project.Name);
        Assert.Equal("Abu Dhabi", project.Location);
        Assert.Equal("Main contract", project.Description);
        Assert.Equal(ProjectStatus.Active, project.Status);
    }

    [Fact]
    public void Create_rejects_target_date_before_start_date()
    {
        Assert.Throws<ArgumentException>(() =>
            Project.Create(
                "P-001",
                "Tower Project",
                null,
                new DateOnly(2026, 9, 23),
                new DateOnly(2026, 9, 22),
                null));
    }

    [Fact]
    public void Closed_project_cannot_be_updated()
    {
        var project = Project.Create(
            "P-001",
            "Tower Project",
            null,
            new DateOnly(2026, 9, 23),
            null,
            null);

        project.Close();

        Assert.Throws<InvalidOperationException>(() =>
            project.Update(
                "P-002",
                "Updated",
                null,
                new DateOnly(2026, 9, 23),
                null,
                null));
    }
}
