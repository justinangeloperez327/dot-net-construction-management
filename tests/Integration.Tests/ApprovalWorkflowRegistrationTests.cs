using Application.Approvals;
using Domain.Approvals;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Integration.Tests;

public sealed class ApprovalWorkflowRegistrationTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApprovalWorkflowRegistrationTests(
        WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Approval_services_are_registered()
    {
        using var scope = _factory.Services.CreateScope();

        Assert.NotNull(
            scope.ServiceProvider.GetService<IApprovalRequestRepository>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<CreateApprovalRequestHandler>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<DecideApprovalStepHandler>());

        Assert.NotNull(
            scope.ServiceProvider.GetService<CancelApprovalRequestHandler>());
    }

    [Fact]
    public void Approval_entities_are_mapped()
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var request = dbContext.Model.FindEntityType(
            typeof(ApprovalRequest));

        var step = dbContext.Model.FindEntityType(
            typeof(ApprovalStep));

        Assert.NotNull(request);
        Assert.NotNull(step);

        Assert.Equal(
            "ApprovalRequests",
            request.GetTableName());

        Assert.Equal(
            "ApprovalSteps",
            step.GetTableName());

        var stepIndex = step.GetIndexes()
            .Single(index =>
                index.Properties.Select(property => property.Name)
                    .SequenceEqual(
                        [
                            nameof(ApprovalStep.ApprovalRequestId),
                            nameof(ApprovalStep.StepNumber)
                        ]));

        Assert.True(stepIndex.IsUnique);
    }
}
