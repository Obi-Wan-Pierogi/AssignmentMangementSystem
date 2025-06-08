namespace AssignmentManagement.Tests
{
    using AssignmentManagement.Core;
    using AssignmentManagement.Core.Enums;
    using Moq;
    using AssignmentManagement.Core.Interfaces;
    using System;

    public class AssignmentTests
    {
        private readonly Mock<IAppLogger> _mockLogger;

        public AssignmentTests() 
        {
            _mockLogger = new Mock<IAppLogger>();   
        }

        [Fact]
        public void Constructor_ValidInput_ShouldCreateAssignment()
        {
            var assignment = new Assignment("Read Chapter 2", "Summarize key points", null, Priority.Medium, null);
            Assert.Equal("Read Chapter 2", assignment.Title);
            Assert.Equal("Summarize key points", assignment.Description);
            Assert.False(assignment.IsCompleted);
            Assert.Null(assignment.DueDate);
            Assert.Equal(Priority.Medium, assignment.Priority); // Check default or passed priority
            Assert.Null(assignment.Notes);
        }

        [Fact]
        public void Constructor_BlankTitle_ShouldThrowException()
        {
            Assert.Throws<ArgumentException>(() => new Assignment("", "Valid description", null, Priority.Medium, null));
        }

        [Fact]
        public void Update_BlankDescription_ShouldThrowException()
        {
            var assignment = new Assignment("Read Chapter 2", "Summarize key points", null, Priority.Medium, null);
            Assert.Throws<ArgumentException>(() => assignment.Update("Valid title", ""));
        }

        [Fact]
        public void MarkComplete_SetsIsCompletedToTrue()
        {
            var assignment = new Assignment("Task", "Complete the lab", null, Priority.Medium, null);
            assignment.MarkComplete();
            Assert.True(assignment.IsCompleted);
        }

        [Fact]
        public void Assignment_HasDefaultPriority()
        {
            var assignment = new Assignment("Task 1", "Details", null, Priority.Medium, null);
            Assert.Equal(Priority.Medium, assignment.Priority);
        }

        [Fact]
        public void Assignment_AcceptsHighPriority()
        {
            var assignment = new Assignment("Urgent Task", "Do it now", null, Priority.High, null);
            Assert.Equal(Priority.High, assignment.Priority);
        }

        [Fact]
        public void Constructor_WithNotes_ShouldSetNotesProperty()
        {
            // Arrange
            string title = "Test Task with Notes";
            string description = "Description";
            DateTime? dueDate = null;
            Priority priority = Priority.High;
            string notes = "These are some important notes.";

            // Act
            var assignment = new Assignment(title, description, dueDate, priority, notes);

            // Assert
            Assert.Equal(notes, assignment.Notes);
        }

        [Fact]
        public void Constructor_WithoutNotes_ShouldHaveNullNotesProperty()
        {
            // Arrange
            string title = "Test Task without Notes";
            string description = "Description";
            DateTime? dueDate = null;
            Priority priority = Priority.Low;

            // Act
            var assignment = new Assignment(title, description, dueDate, priority);

            // Assert
            Assert.Null(assignment.Notes);
        }

        [Fact]
        public void JsonConstructor_WithNotes_ShouldSetAllPropertiesIncludingNotes()
        {
            // Arrange
            int id = 101;
            string title = "JSON Task";
            string description = "JSON Desc";
            DateTime? dueDate = DateTime.Now;
            bool isCompleted = false;
            Priority priority = Priority.Medium;
            string notes = "Notes for JSON constructor";

            // Act
            var assignment = new Assignment(id, title, description, dueDate.Value, isCompleted, priority, notes);

            // Assert
            Assert.Equal(id, assignment.Id);
            Assert.Equal(title, assignment.Title);
            Assert.Equal(description, assignment.Description);
            Assert.Equal(dueDate, assignment.DueDate); // Assert DueDate
            Assert.False(assignment.IsCompleted);
            Assert.Equal(priority, assignment.Priority);
            Assert.Equal(notes, assignment.Notes);
        }

        [Fact]
        public void Bug_Constructor_ShouldAssignNotes()
        {
            // Arrange
            string title = "Test Title with Notes";
            string description = "Test Description";
            DateTime? dueDate = DateTime.Now.AddDays(7);
            string expectedNotes = "These are important test notes.";

            // Act
            var assignment = new Assignment(title, description, dueDate, Priority.Medium, expectedNotes);

            // Assert
            Assert.Equal(expectedNotes, assignment.Notes);
        }

        [Fact]
        public void Bug_IsOverdue_WhenDueDateIsNull_ShouldReturnFalseAndNotThrow()
        {
            // Arrange
            var assignment = new Assignment("No DueDate Task", "This task has no due date.", null, Priority.Low);

            // Act
            bool isOverdue = false;
            Exception caughtException = null;

            try
            {
                isOverdue = assignment.IsOverdue(_mockLogger.Object);
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.Null(caughtException);
            Assert.False(isOverdue, "Assignment with no due date should not be considered overdue.");
        }

        [Fact]
        public void Bug_IsOverdue_WhenCompletedAndPastDueDate_ShouldReturnFalse()
        {
            // Arrange
            var assignment = new Assignment("Completed Late Task", "This task was completed after its due date.", DateTime.Now.AddDays(-1), Priority.High);
            assignment.MarkComplete();

            // Act
            bool isOverdue = assignment.IsOverdue(_mockLogger.Object);

            // Assert
            Assert.False(isOverdue, "Completed assignments should not be considered overdue.");
        }

        [Fact]
        public void Bug_ToString_WhenNotesArePresent_ShouldIncludeNotesInOutput()
        {
            // Arrange
            string title = "Project Report";
            string description = "Finalize and submit the project report.";
            DateTime? dueDate = DateTime.Today.AddDays(5);
            string notesContent = "Ensure all references are cited correctly.";
            var assignment = new Assignment(title, description, dueDate, Priority.High, notesContent);

            // Act
            string result = assignment.ToString();

            // Assert
            Assert.Contains(notesContent, result);
        }
    }
}
