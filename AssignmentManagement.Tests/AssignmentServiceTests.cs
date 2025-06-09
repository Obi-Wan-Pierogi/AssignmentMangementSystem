namespace AssignmentManagement.Tests
{
    using Xunit;
    using AssignmentManagement.Core;
    using AssignmentManagement.Core.Services;
    using AssignmentManagement.Core.Interfaces;
    using System.Collections.Generic;

    public class AssignmentServiceTests
    {
        // Dependencies
        private readonly IAssignmentFormatter _stubFormatter;
        private readonly IAppLogger _stubLogger;
        private readonly AssignmentService _service;

        // Constructor
        public AssignmentServiceTests()
        {
            _stubFormatter = new StubAssignmentFormatter();
            _stubLogger = new StubAppLogger();
            _service = new AssignmentService(_stubFormatter, _stubLogger);
        }

        [Fact]
        public void ListIncomplete_ShouldReturnOnlyAssignmentsThatAreNotCompleted()
        {
            // Arrange
            var incompleteAssignment = new Assignment("Incomplete Task", "Do something", null);
            var completedAssignment = new Assignment("Completed Task", "Do something else", null);
            completedAssignment.MarkComplete();

            _service.AddAssignment(incompleteAssignment);
            _service.AddAssignment(completedAssignment);

            // Act
            var result = _service.ListIncomplete();

            // Assert
            var singleResult = Assert.Single(result);
            Assert.Equal("Incomplete Task", singleResult.Title);
            Assert.False(singleResult.IsCompleted);
        }

        [Fact]
        public void ListIsEmpty_ShouldReturnEmptyList_WhenNoAssignments()
        {
            // Arrange
            
            // Act
            var result = _service.ListAll();
            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void ListAll_ShouldReturnAllAssignments()
        {
            // Arrange
            var a1 = new Assignment("Task 1", "Description 1", null);
            var a2 = new Assignment("Task 2", "Description 2", null);
            a1.MarkComplete();
            
            _service.AddAssignment(a1);
            _service.AddAssignment(a2);

            // Act
            var result = _service.ListAll();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, a => a.Title == "Task 1");
            Assert.Contains(result, a => a.Title == "Task 2");
        }

        [Fact]
        public void UpdateAssignment_WhenSuccessful_ShouldChangeTitleAndDescription()
        {
            // Arrange
            var originalAssignment = new Assignment("Old Title", "Old Desc", null);
            _service.AddAssignment(originalAssignment);
            var newTitle = "New Title";
            var newDesc = "New Desc";

            // Act
            var result = _service.UpdateAssignment("Old Title", newTitle, newDesc);
            var updatedAssignment = _service.FindAssignmentByTitle(newTitle);
            var oldAssignment = _service.FindAssignmentByTitle("Old Title");


            // Assert
            Assert.True(result);
            Assert.Null(oldAssignment); // The old key should be gone
            Assert.NotNull(updatedAssignment);
            Assert.Equal(newTitle, updatedAssignment.Title);
            Assert.Equal(newDesc, updatedAssignment.Description);
        }

        // A minimal stub for IAssignmentFormatter
        public class StubAssignmentFormatter : IAssignmentFormatter
        {

            public string Format(Assignment assignment)
            {
                return assignment?.Title ?? string.Empty;
            }

            public string FormatList(IEnumerable<Assignment> assignments)
            {
                throw new NotImplementedException();
            }
        }

        // A minimal stub for IAppLogger
        public class StubAppLogger : IAppLogger
        {
            public void LogInformation(string message)
            {
                // Do nothing
            }

            public void LogWarning(string message)
            {
                // Do nothing
            }

            public void Log(string message)
            {
                // Do nothing
            }

            public void LogError(string v)
            {
                // Do nothing
            }
        }
    }
}

