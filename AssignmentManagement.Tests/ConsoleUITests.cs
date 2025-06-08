using Xunit;
using Moq;
using AssignmentManagement.Core;
using AssignmentManagement.UI;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using AssignmentManagement.Core.Interfaces;
using AssignmentManagement.Core.Enums;

namespace AssignmentManagement.Tests
{
    public class ConsoleUITests : IDisposable
    {
        private readonly Mock<IAssignmentService> _mockService;
        private readonly ConsoleUI _consoleUI;
        private readonly StringWriter _stringWriter;
        private readonly TextWriter _originalOutput;
        private readonly TextReader _originalInput;
        private readonly Mock<IAppLogger> _mockLogger;

        public ConsoleUITests()
        {
            _mockService = new Mock<IAssignmentService>();
            _mockLogger = new Mock<IAppLogger>();

            // Redirect Console.Out
            _originalOutput = System.Console.Out;
            _stringWriter = new StringWriter();
            System.Console.SetOut(_stringWriter);

            // Store Console.In (will be replaced per-test)
            _originalInput = System.Console.In;

            // Create ConsoleUI instance with the MOCKED service
            _consoleUI = new ConsoleUI(_mockService.Object, _mockLogger.Object);
        }

        private void SetConsoleInput(params string[] lines)
        {
            var inputString = string.Join(Environment.NewLine, lines);
            var stringReader = new StringReader(inputString);
            System.Console.SetIn(stringReader);
        }

        private string GetConsoleOutput()
        {
            return _stringWriter.ToString();
        }

        [Fact]
        public void AddAssignment_WhenSuccessful_IncludesDueDatePromptAndData()
        {
            // Arrange
            string title = "New Task with DueDate";
            string description = "Description here";
            string priorityInput = "High";
            string dueDateInput = "2025-12-31"; // Valid due date input
            string notesInput = "Optional notes";
            Priority expectedPriority = Priority.High;
            DateTime expectedDueDate = DateTime.Parse(dueDateInput);

            // Inputs: Title, Description, Priority, DueDate, Notes
            SetConsoleInput(title, description, priorityInput, dueDateInput, notesInput);

            _mockService.Setup(s => s.AddAssignment(It.Is<Assignment>(a =>
                            a.Title == title &&
                            a.Description == description &&
                            a.Priority == expectedPriority &&
                            a.DueDate == expectedDueDate && // Verify DueDate is passed
                            a.Notes == notesInput)))
                        .Returns(true);

            // Act
            _consoleUI.AddAssignment();

            // Assert
            var output = GetConsoleOutput();
            Assert.Contains("Enter assignment title:", output);
            Assert.Contains("Enter assignment description:", output);
            Assert.Contains("Enter priority (Low, Medium, High - defaults to Medium if empty or invalid):", output);
            Assert.Contains("Enter optional due date (e.g., yyyy-MM-dd, or leave blank if none):", output);
            Assert.Contains("Enter optional notes for the assignment:", output);
            Assert.Contains($"Assignment added successfully with Priority: {expectedPriority}.", output);

            _mockService.Verify(s => s.AddAssignment(It.Is<Assignment>(a =>
                            a.Title == title &&
                            a.Description == description &&
                            a.Priority == expectedPriority &&
                            a.DueDate == expectedDueDate &&
                            a.Notes == notesInput)), Times.Once);
        }

        [Fact]
        public void AddAssignment_WithInvalidDueDateInput_SetsDueDateToNullAndNotifiesUser()
        {
            // Arrange
            string title = "Task Invalid DueDate";
            string description = "Description";
            string priorityInput = "Medium";
            string invalidDueDateInput = "not-a-date"; // Invalid input
            string notesInput = "Some notes";
            Priority expectedPriority = Priority.Medium;


            SetConsoleInput(title, description, priorityInput, invalidDueDateInput, notesInput);

            _mockService.Setup(s => s.AddAssignment(It.Is<Assignment>(a =>
                            a.Title == title &&
                            a.Description == description &&
                            a.Priority == expectedPriority &&
                            a.DueDate == null && // Expect DueDate to be null
                            a.Notes == notesInput)))
                        .Returns(true);
            // Act
            _consoleUI.AddAssignment();

            // Assert
            var output = GetConsoleOutput();
            Assert.Contains("Invalid date format. Due date will not be set.", output);
            Assert.Contains($"Assignment added successfully with Priority: {expectedPriority}.", output);
            _mockService.Verify(s => s.AddAssignment(It.Is<Assignment>(a =>
                            a.Title == title &&
                            a.DueDate == null)), Times.Once);
        }

        [Fact]
        public void AddAssignment_WithEmptyDueDateInput_SetsDueDateToNull()
        {
            // Arrange
            string title = "Task No DueDate";
            string description = "Description";
            string priorityInput = "Low";
            string emptyDueDateInput = ""; // Empty input for due date
            string notesInput = "";
            Priority expectedPriority = Priority.Low;

            SetConsoleInput(title, description, priorityInput, emptyDueDateInput, notesInput);

            _mockService.Setup(s => s.AddAssignment(It.Is<Assignment>(a =>
                            a.Title == title &&
                            a.Description == description &&
                            a.Priority == expectedPriority &&
                            a.DueDate == null && // Expect DueDate to be null
                            string.IsNullOrEmpty(a.Notes))))
                        .Returns(true);
            // Act
            _consoleUI.AddAssignment();

            // Assert
            var output = GetConsoleOutput();
            Assert.DoesNotContain("Invalid date format. Due date will not be set.", output); // Should not show error for blank
            Assert.Contains($"Assignment added successfully with Priority: {expectedPriority}.", output);
            _mockService.Verify(s => s.AddAssignment(It.Is<Assignment>(a =>
                            a.Title == title &&
                            a.DueDate == null)), Times.Once);
        }

        [Fact]
        public void SearchAssignmentByTitle_WhenFound_DisplaysDetailsWithDueDateAndStatus()
        {
            // Arrange
            string titleToFind = "Findable Task With DueDate";
            DateTime? dueDate = DateTime.Now.AddDays(5);
            string notes = "Searchable notes content";
            // Using the constructor that takes nullable DateTime for DueDate
            var foundAssignment = new Assignment(titleToFind, "Details of found task...", dueDate, Priority.High, notes);
            // foundAssignment.MarkComplete(); // Uncomment to test completed status display

            _mockService.Setup(s => s.FindAssignmentByTitle(titleToFind)).Returns(foundAssignment);
            SetConsoleInput(titleToFind);

            // Act
            _consoleUI.SearchAssignmentByTitle();

            // Assert
            var output = GetConsoleOutput();
            Assert.Contains("--- Found Assignment ---", output);

            // Check for elements from assignment.ToString()
            Assert.Contains($"- {foundAssignment.Title} ({foundAssignment.Priority}) due {foundAssignment.DueDate?.ToShortDateString()}", output);
            Assert.Contains($"Description: {foundAssignment.Description}", output);
            if (!string.IsNullOrWhiteSpace(foundAssignment.Notes))
            {
                Assert.Contains($"Notes: {foundAssignment.Notes}", output);
            }
            else // If notes are null/empty, ToString() doesn't add the "Notes:" line
            {
                Assert.DoesNotContain("\n  Notes:", output);
            }

            // Check for appended status line from ConsoleUI
            string expectedStatus = foundAssignment.IsCompleted ? "Completed" : "Incomplete";
            string overdueMarker = foundAssignment.IsOverdue(_mockLogger.Object) ? " - OVERDUE" : "";
            Assert.Contains($"Status: {expectedStatus}{overdueMarker}", output);
        }

        [Fact]
        public void DeleteAssignment_WhenSuccessful_UsesSelectionListWithDueDate()
        {
            // Arrange
            var assignmentToDelete = new Assignment("Task To Delete", "Desc Del", DateTime.Now.AddDays(7), Priority.Medium, "Notes for delete test");
            var assignmentsList = new List<Assignment> { assignmentToDelete };

            _mockService.Setup(s => s.ListAll()).Returns(assignmentsList);
            _mockService.Setup(s => s.DeleteAssignment(assignmentToDelete.Title)).Returns(true);

            SetConsoleInput("1", "y"); // Select first item, confirm "y"

            // Act
            _consoleUI.DeleteAssignment();

            // Assert
            var output = GetConsoleOutput();
            string dueDateDisplay = assignmentToDelete.DueDate?.ToShortDateString() ?? "N/A";
            string overdueMarker = assignmentToDelete.IsOverdue(_mockLogger.Object) ? " (OVERDUE)" : "";
            Assert.Contains($"1. {assignmentToDelete.Title} (Due: {dueDateDisplay}{overdueMarker}, Priority: {assignmentToDelete.Priority}, Status: {(assignmentToDelete.IsCompleted ? "Completed" : "Incomplete")})", output);
            Assert.Contains($"Are you sure you want to delete '{assignmentToDelete.Title}'? (y/n):", output);
            Assert.Contains($"Assignment '{assignmentToDelete.Title}' deleted successfully.", output);
            _mockService.Verify(s => s.DeleteAssignment(assignmentToDelete.Title), Times.Once);
        }
        [Fact]
        public void ListAllAssignments_DisplaysAssignmentsCorrectly()
        {
            // Arrange
            var assignment1 = new Assignment("Task 1 Due Soon", "Desc 1", DateTime.Now.AddDays(2), Priority.Low, "Notes 1");
            var assignment2 = new Assignment("Task 2 No DueDate", "Desc 2", null, Priority.Medium, null);
            var assignment3 = new Assignment("Task 3 Overdue", "Desc 3", DateTime.Now.AddDays(-2), Priority.High, "This one is late");
            var assignment4 = new Assignment("Task 4 Completed", "Desc 4", DateTime.Now.AddDays(-5), Priority.Medium, "Done");
            assignment4.MarkComplete(); // Mark as completed (and overdue, but completed takes precedence for IsOverdue())

            var assignmentsList = new List<Assignment> { assignment1, assignment2, assignment3, assignment4 };
            _mockService.Setup(s => s.ListAll()).Returns(assignmentsList);

            // Act
            _consoleUI.ListAllAssignments();

            // Assert
            var output = GetConsoleOutput();
            Assert.Contains("--- All Assignments ---", output);

            // Verify output for assignment1 (not overdue, incomplete)
            Assert.Contains($"- {assignment1.Title} ({assignment1.Priority}) due {assignment1.DueDate?.ToShortDateString()}", output);
            Assert.Contains($"Description: {assignment1.Description}", output.Substring(output.IndexOf(assignment1.Title)));
            Assert.Contains($"Notes: {assignment1.Notes}", output.Substring(output.IndexOf(assignment1.Title)));
            Assert.Contains($"Status: Incomplete", output.Substring(output.IndexOf(assignment1.Title)));
            Assert.DoesNotContain("OVERDUE", output.Substring(output.IndexOf(assignment1.Title), output.IndexOf(assignment2.Title) - output.IndexOf(assignment1.Title)));


            // Verify output for assignment2 (no due date, incomplete)
            Assert.Contains($"- {assignment2.Title} ({assignment2.Priority}) due N/A", output);
            Assert.Contains($"Status: Incomplete", output.Substring(output.IndexOf(assignment2.Title)));

            // Verify output for assignment3 (overdue, incomplete)
            Assert.Contains($"- {assignment3.Title} ({assignment3.Priority}) due {assignment3.DueDate?.ToShortDateString()}", output);
            Assert.Contains($"Status: Incomplete - OVERDUE", output.Substring(output.IndexOf(assignment3.Title)));

            // Verify output for assignment4 (completed, was overdue but IsOverdue() returns false if completed)
            Assert.Contains($"- {assignment4.Title} ({assignment4.Priority}) due {assignment4.DueDate?.ToShortDateString()}", output);
            Assert.Contains($"Status: Completed", output.Substring(output.IndexOf(assignment4.Title)));
            // Assert.DoesNotContain("OVERDUE", output.Substring(output.IndexOf(assignment4.Title))); // IsOverdue() should be false
        }

        [Fact]
        public void SelectAssignmentFromList_DisplaysCorrectFormatIncludingDueDateAndOverdueStatus()
        {
            // Arrange
            var assignmentNormal = new Assignment("Normal Task", "Desc", DateTime.Now.AddDays(3), Priority.Medium, null);
            var assignmentOverdue = new Assignment("Overdue Task", "Desc", DateTime.Now.AddDays(-3), Priority.High, "Late!");
            var assignmentNoDueDate = new Assignment("No DueDate Task", "Desc", null, Priority.Low, null);
            var assignmentCompleted = new Assignment("Completed Task", "Desc", DateTime.Now.AddDays(-7), Priority.Medium, null);
            assignmentCompleted.MarkComplete(); // IsOverdue() will be false

            var assignmentsList = new List<Assignment> { assignmentNormal, assignmentOverdue, assignmentNoDueDate, assignmentCompleted };
            _mockService.Setup(s => s.ListAll()).Returns(assignmentsList); // Using DeleteAssignment to trigger SelectAssignmentFromList

            SetConsoleInput("0"); // User cancels selection

            // Act
            _consoleUI.DeleteAssignment(); // This method uses SelectAssignmentFromList

            // Assert
            var output = GetConsoleOutput();
            Assert.Contains("--- Select an Assignment to Delete ---", output);
            Assert.Contains($"1. {assignmentNormal.Title} (Due: {assignmentNormal.DueDate?.ToShortDateString()}, Priority: {assignmentNormal.Priority}, Status: Incomplete)", output);
            Assert.Contains($"2. {assignmentOverdue.Title} (Due: {assignmentOverdue.DueDate?.ToShortDateString()} (OVERDUE), Priority: {assignmentOverdue.Priority}, Status: Incomplete)", output);
            Assert.Contains($"3. {assignmentNoDueDate.Title} (Due: N/A, Priority: {assignmentNoDueDate.Priority}, Status: Incomplete)", output);
            Assert.Contains($"4. {assignmentCompleted.Title} (Due: {assignmentCompleted.DueDate?.ToShortDateString()}, Priority: {assignmentCompleted.Priority}, Status: Completed)", output);
            Assert.Contains("Selection cancelled.", output);
        }

        [Fact]
        public void DeleteAssignment_WhenSelectionCancelled_DoesNotCallService()
        {
            // Arrange
            var assignmentInList = new Assignment("Task To Keep", "Desc Del", DateTime.Now.AddDays(7), Priority.Medium, "Notes for delete test");
            var assignmentsList = new List<Assignment> { assignmentInList };
            _mockService.Setup(s => s.ListAll()).Returns(assignmentsList);

            SetConsoleInput("0"); // User inputs "0" to cancel selection in SelectAssignmentFromList

            // Act
            _consoleUI.DeleteAssignment();

            // Assert
            var output = GetConsoleOutput();
            Assert.Contains("Selection cancelled.", output);
            _mockService.Verify(s => s.DeleteAssignment(It.IsAny<string>()), Times.Never); // Ensure service method not called
        }

        [Fact]
        public void MarkAssignmentComplete_WhenSuccessful_UsesSelectionListWithDueDate()
        {
            // Arrange
            var assignmentToMark = new Assignment("Task To Mark", "Desc Mark", DateTime.Now.AddDays(-1), Priority.Low, "Important notes"); // Potentially overdue
            var incompleteAssignments = new List<Assignment> { assignmentToMark };

            _mockService.Setup(s => s.ListIncomplete()).Returns(incompleteAssignments);
            _mockService.Setup(s => s.MarkAssignmentComplete(assignmentToMark.Title)).Returns(true);

            SetConsoleInput("1");

            // Act
            _consoleUI.MarkAssignmentComplete();

            // Assert
            var output = GetConsoleOutput();
            string dueDateDisplay = assignmentToMark.DueDate?.ToShortDateString() ?? "N/A";
            // Note: IsOverdue() might be true here, but the selection list primarily shows current status.
            // The main thing is that the list item displayed matches what SelectAssignmentFromList produces.
            string overdueMarker = assignmentToMark.IsOverdue(_mockLogger.Object) ? " (OVERDUE)" : "";
            Assert.Contains($"1. {assignmentToMark.Title} (Due: {dueDateDisplay}{overdueMarker}, Priority: {assignmentToMark.Priority}, Status: Incomplete)", output);
            Assert.Contains($"Assignment '{assignmentToMark.Title}' marked as complete.", output);
            _mockService.Verify(s => s.MarkAssignmentComplete(assignmentToMark.Title), Times.Once);
        }

        [Fact]
        public void UpdateAssignment_WhenSuccessful_UsesSelectionListWithDueDate()
        {
            // Arrange
            var originalAssignment = new Assignment("Old Title", "Old Desc", null, Priority.High, "Original Notes"); // No DueDate
            var assignmentsList = new List<Assignment> { originalAssignment };
            string newTitle = "New Updated Title";
            string newDescription = "New Updated Description";

            _mockService.Setup(s => s.ListAll()).Returns(assignmentsList);
            _mockService.Setup(s => s.UpdateAssignment(originalAssignment.Title, newTitle, newDescription))
                        .Returns(true);

            SetConsoleInput("1", newTitle, newDescription);

            // Act
            _consoleUI.UpdateAssignment();

            // Assert
            var output = GetConsoleOutput();
            string dueDateDisplay = originalAssignment.DueDate?.ToShortDateString() ?? "N/A";
            string overdueMarker = originalAssignment.IsOverdue(_mockLogger.Object) ? " (OVERDUE)" : "";
            Assert.Contains($"1. {originalAssignment.Title} (Due: {dueDateDisplay}{overdueMarker}, Priority: {originalAssignment.Priority}, Status: Incomplete)", output);
            Assert.Contains($"Updating assignment:", output);
            Assert.Contains($"- {originalAssignment.Title} ({originalAssignment.Priority}) due N/A", output); // From ToString()
            Assert.Contains($"Status: Incomplete", output); // From appended status line in UpdateAssignment
            Assert.Contains($"Enter new title (leave blank to keep '{originalAssignment.Title}'):", output);
            Assert.Contains("Assignment updated successfully.", output);
            _mockService.Verify(s => s.UpdateAssignment(originalAssignment.Title, newTitle, newDescription), Times.Once);
        }

        public void Dispose()
        {
            System.Console.SetOut(_originalOutput); // Restore original Console.Out
            System.Console.SetIn(_originalInput);   // Restore original Console.In
            _stringWriter.Dispose();
        }
    }
}
