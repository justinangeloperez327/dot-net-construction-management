using Domain.Clients;
using Domain.Projects;
using Xunit;

namespace Domain.Tests;

public sealed class ClientProjectMemberTests
{
    [Fact]
    public void Client_create_normalizes_values_and_is_active()
    {
        var client = Client.Create(
            " Owner LLC ",
            " Jane Doe ",
            " client@example.com ",
            " +971 2 555 0000 ",
            " Abu Dhabi ");

        Assert.Equal("Owner LLC", client.Name);
        Assert.Equal("Jane Doe", client.ContactPerson);
        Assert.Equal("client@example.com", client.Email);
        Assert.True(client.IsActive);
    }

    [Fact]
    public void Project_can_assign_and_remove_client_while_active()
    {
        var project = Project.Create(
            "P-001",
            "Tower",
            null,
            new DateOnly(2026, 9, 23),
            null,
            null);

        var clientId = Guid.NewGuid();

        project.AssignClient(clientId);
        Assert.Equal(clientId, project.ClientId);

        project.AssignClient(null);
        Assert.Null(project.ClientId);
    }

    [Fact]
    public void Closed_project_rejects_client_change()
    {
        var project = Project.Create(
            "P-001",
            "Tower",
            null,
            new DateOnly(2026, 9, 23),
            null,
            null);

        project.Close();

        Assert.Throws<InvalidOperationException>(() =>
            project.AssignClient(Guid.NewGuid()));
    }

    [Fact]
    public void Project_member_normalizes_responsibility()
    {
        var member = ProjectMember.Assign(
            Guid.NewGuid(),
            Guid.NewGuid(),
            " Site Engineer ");

        Assert.Equal("Site Engineer", member.Responsibility);

        member.UpdateResponsibility(" Project Engineer ");

        Assert.Equal("Project Engineer", member.Responsibility);
    }
}
