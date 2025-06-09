using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using AssignmentManagement.WebAPI;
using AssignmentManagement.WebAPI.Dtos;
using AssignmentManagement.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Linq;
using Microsoft.AspNetCore.TestHost;
using AssignmentManagement.Core.Services;
using AssignmentManagement.Core.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;
using static AssignmentManagement.Tests.AssignmentServiceTests;
using AssignmentManagement.Core.Enums;


public class AssignmentApiTests : IClassFixture<WebApplicationFactory<Program>> 
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly JsonSerializerOptions _jsonOptions;

    public AssignmentApiTests(WebApplicationFactory<Program> factory)
    {
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services => 
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IAssignmentService));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                services.RemoveAll<IAppLogger>(); 
                services.AddSingleton<IAppLogger, ConsoleAppLogger>(); // Use the real console logger

                services.AddSingleton<IAssignmentFormatter, StubAssignmentFormatter>(); 
                services.AddSingleton<IAssignmentService, AssignmentService>();
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreateAssignment_WithValidData_ReturnsCreatedAndAssignment()
    {
        // Arrange
        var assignmentDto = new AssignmentCreationDto
        {
            Title = "Test API Create Notes " + System.Guid.NewGuid().ToString(),
            Description = "Description for API notes test",
            Priority = Priority.High,
            Notes = "Important API notes here" // Add notes to DTO
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/assignment", assignmentDto, _jsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        response.EnsureSuccessStatusCode();

        var createdAssignment = await response.Content.ReadFromJsonAsync<Assignment>(_jsonOptions);
        Assert.NotNull(createdAssignment);
        Assert.Equal(assignmentDto.Title, createdAssignment.Title);
        Assert.Equal(assignmentDto.Description, createdAssignment.Description);
        Assert.Equal(Priority.High, createdAssignment.Priority);
        Assert.Equal(assignmentDto.Notes, createdAssignment.Notes); // Verify notes
        Assert.True(createdAssignment.Id > 0, "ID should be a positive integer.");
        Assert.False(createdAssignment.IsCompleted);

        // Check Location header
        Assert.NotNull(response.Headers.Location);

        string expectedTitleInPath = System.Uri.EscapeDataString(createdAssignment.Title);
        string expectedPathFragment = $"/api/assignment/{expectedTitleInPath}";

        Assert.Contains(expectedPathFragment, response.Headers.Location.OriginalString);
    }

    [Fact]
    public async Task CreateAssignment_WithoutPriority_DefaultsToMediumAndReturnsAssignment()
    {
        // Arrange
        var assignmentDto = new AssignmentCreationDto
        {
            Title = "Test API Default Prio " + Guid.NewGuid(), // Using Guid here just for unique title generation
            Description = "Description for API default priority test"

        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/assignment", assignmentDto, _jsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        response.EnsureSuccessStatusCode();

        var createdAssignment = await response.Content.ReadFromJsonAsync<Assignment>(_jsonOptions);
        Assert.NotNull(createdAssignment);
        Assert.Equal(assignmentDto.Title, createdAssignment.Title);
        Assert.Equal(assignmentDto.Description, createdAssignment.Description);
        Assert.Equal(Priority.Medium, createdAssignment.Priority); // Verify default priority
        Assert.True(createdAssignment.Id > 0, "ID should be a positive integer.");
    }

    [Fact]
    public async Task CreateAssignment_WithoutNotes_ReturnsCreatedAndAssignmentWithNullNotes()
    {
        // Arrange
        var assignmentDto = new AssignmentCreationDto
        {
            Title = "Test API No Notes " + System.Guid.NewGuid().ToString(),
            Description = "Description for API no notes test",
            Priority = Priority.Low
            // Notes property is omitted, so it should be null
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/assignment", assignmentDto, _jsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        response.EnsureSuccessStatusCode();

        var createdAssignment = await response.Content.ReadFromJsonAsync<Assignment>(_jsonOptions);
        Assert.NotNull(createdAssignment);
        Assert.Equal(assignmentDto.Title, createdAssignment.Title);
        Assert.Equal(assignmentDto.Description, createdAssignment.Description);
        Assert.Equal(Priority.Low, createdAssignment.Priority);
        Assert.Null(createdAssignment.Notes); // Verify notes are null
        Assert.True(createdAssignment.Id > 0, "ID should be a positive integer.");
    }

    [Fact]
    public async Task GetAllAssignments_ReturnsOkAndListOfAssignmentsWithPriority()
    {
        // Arrange: Create two assignments with different priorities
        var title1 = "API GetAll Prio 1 " + Guid.NewGuid(); // Using Guid here just for unique title generation
        var notesContent = "Specific notes for this fetched assignment.";
        var dto1 = new AssignmentCreationDto
        {
            Title = title1,
            Description = "Item 1 for Get All test",
            Priority = Priority.Low,
            Notes = notesContent

        };
        var createResponse1 = await _client.PostAsJsonAsync("/api/assignment", dto1, _jsonOptions);
        createResponse1.EnsureSuccessStatusCode();

        var title2 = "API GetAll Prio 2 " + Guid.NewGuid(); // Using Guid here just for unique title generation
        var dto2 = new AssignmentCreationDto
        {
            Title = title2,
            Description = "Item 2 for Get All test",
            Priority = Priority.High,
            Notes = notesContent

        };
        var createResponse2 = await _client.PostAsJsonAsync("/api/assignment", dto2, _jsonOptions);
        createResponse2.EnsureSuccessStatusCode();

        // Act
        var response = await _client.GetAsync("/api/assignment");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        response.EnsureSuccessStatusCode();

        var assignments = await response.Content.ReadFromJsonAsync<List<Assignment>>(_jsonOptions);
        Assert.NotNull(assignments);
        Assert.True(assignments.Count >= 2); // Check for at least the two we added

        var foundItem1 = assignments.FirstOrDefault(a => a.Title == title1);
        Assert.NotNull(foundItem1);
        Assert.Equal(dto1.Description, foundItem1.Description);
        Assert.Equal(Priority.Low, foundItem1.Priority); // Verify priority for item 1
        Assert.Equal(notesContent, foundItem1.Notes);
        Assert.True(foundItem1.Id > 0, "Item 1 ID should be a positive integer."); // Check Id type

        var foundItem2 = assignments.FirstOrDefault(a => a.Title == title2);
        Assert.NotNull(foundItem2);
        Assert.Equal(dto2.Description, foundItem2.Description);
        Assert.Equal(Priority.High, foundItem2.Priority);
        Assert.Equal(notesContent, foundItem2.Notes);// Verify priority for item 2
        Assert.True(foundItem2.Id > 0, "Item 2 ID should be a positive integer."); // Check Id type
    }

    [Fact]
    public async Task GetAssignmentByTitle_ReturnsAssignmentWithNotes()
    {
        // Arrange
        var uniqueTitle = "API Get Single Notes " + Guid.NewGuid();
        var notesContent = "Specific notes for this fetched assignment.";
        var creationDto = new AssignmentCreationDto
        {
            Title = uniqueTitle,
            Description = "Test for GET single with notes",
            Priority = Priority.High,
            Notes = notesContent
        };
        var postResponse = await _client.PostAsJsonAsync("/api/assignment", creationDto, _jsonOptions);
        postResponse.EnsureSuccessStatusCode();
        var postedAssignment = await postResponse.Content.ReadFromJsonAsync<Assignment>(_jsonOptions);
        Assert.NotNull(postedAssignment);

        // Act
        var response = await _client.GetAsync($"/api/assignment/{uniqueTitle}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var retrievedAssignment = await response.Content.ReadFromJsonAsync<Assignment>(_jsonOptions);
        Assert.NotNull(retrievedAssignment);
        Assert.Equal(uniqueTitle, retrievedAssignment.Title);
        Assert.Equal(Priority.High, retrievedAssignment.Priority);
        Assert.Equal(notesContent, retrievedAssignment.Notes);
        Assert.Equal(postedAssignment.Id, retrievedAssignment.Id);
    }
}