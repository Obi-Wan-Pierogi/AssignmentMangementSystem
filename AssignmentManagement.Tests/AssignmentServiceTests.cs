namespace AssignmentManagement.Tests
{
    using Xunit;
    using AssignmentManagement.Core;

    public class AssignmentServiceTests
    {
        [Fact]
        public void ListIncomplete_ShouldReturnOnlyAssignmentsThatAreNotCompleted()
        {
            // Arrange
            var service = new AssignmentService();
            var incompleteAssignment = new Assignment("Incomplete Task", "Do something");
            var completedAssignment = new Assignment("Completed Task", "Do something else");
            completedAssignment.MarkComplete();

            service.AddAssignment(incompleteAssignment);
            service.AddAssignment(completedAssignment);

            // Act
            var result = service.ListIncomplete();

            // Assert
            var singleResult = Assert.Single(result);
            Assert.Single("Incomplete Task", singleResult.Title);
            Assert.False(singleResult.IsCompleted);
        }

        [Fact]
        public void ListIsEmpty_ShouldReturnEmptyList_WhenNoAssignments()
        {
            // Arrange
            var service = new AssignmentService();
            // Act
            var result = service.ListAll();
            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void ListAll_ShouldReturnAllAssignments()
        {
            // Arrange
            var service = new AssignmentService();
            var a1 = new Assignment("Task 1", "Description 1");
            var a2 = new Assignment("Task 2", "Description 2");
            a1.MarkComplete();
            
            service.AddAssignment(a1);
            service.AddAssignment(a2);

            // Act
            var result = service.ListAll();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, a => a.Title == "Task 1");
            Assert.Contains(result, a => a.Title == "Task 2");
        }
    }
}
