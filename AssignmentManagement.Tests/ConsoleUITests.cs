using Xunit;
using Moq;
using AssignmentManagement.Core; 
using AssignmentManagement.UI;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace AssignmentManagement.Tests
{
    public class ConsoleUITests : IDisposable
    {
        private readonly Mock<IAssignmentService> _mockService;
        private readonly ConsoleUI _consoleUI;
        private readonly StringWriter _stringWriter;
        private readonly TextWriter _originalOutput;
        private readonly TextReader _originalInput;

        public ConsoleUITests()
        {
            _mockService = new Mock<IAssignmentService>();

            // Redirect Console.Out
            _originalOutput = Console.Out;
            _stringWriter = new StringWriter();
            Console.SetOut(_stringWriter);

            // Store Console.In (will be replaced per-test)
            _originalInput = Console.In;

            // Create ConsoleUI instance with the MOCKED service
            _consoleUI = new ConsoleUI(_mockService.Object);
        }

        private void SetConsoleInput(params string[] lines)
        {
            var inputString = string.Join(Environment.NewLine, lines);
            var stringReader = new StringReader(inputString);
            Console.SetIn(stringReader);
        }

        private string GetConsoleOutput()
        {
            return _stringWriter.ToString();
        }

        [Fact]
        public void AddAssignment_WhenSuccessful_CallsServiceAndWritesSuccessMessage()
        {
            // Arrange
            string title = "New Task";
            string description = "A good description";
            SetConsoleInput(title, description);

            // Setup the mock service's AddAssignment method:
            // Expect an Assignment with matching title/desc, and return true (success)
            _mockService.Setup(s => s.AddAssignment(It.Is<Assignment>(a =>
                            a.Title == title && a.Description == description)))
                        .Returns(true);

            // Act
            _consoleUI.AddAssignment();

            // Assert
            var output = GetConsoleOutput();
            Assert.Contains("Enter assignment title:", output);
            Assert.Contains("Enter assignment description:", output);
            Assert.Contains("Assignment added successfully.", output);

            // Verify that the service's AddAssignment method was called exactly once
            // with an Assignment object matching the criteria.
            _mockService.Verify(s => s.AddAssignment(It.Is<Assignment>(a =>
                            a.Title == title && a.Description == description)), Times.Once);
        }

        [Fact]
        public void SearchAssignmentByTitle_WhenFound_WritesAssignmentDetails()
        {
            // Arrange
            string titleToFind = "Find This Task";
            SetConsoleInput(titleToFind);

            // Create the assignment object the mock service should return
            var foundAssignment = new Assignment(titleToFind, "This is the description");
            foundAssignment.MarkComplete();

            // Setup the mock service's FindAssignmentByTitle method:
            // When called with titleToFind, return the prepared assignment object
            _mockService.Setup(s => s.FindAssignmentByTitle(titleToFind))
                        .Returns(foundAssignment);

            // Act
            _consoleUI.SearchAssignmentByTitle(); // Call the method under test

            // Assert
            var output = GetConsoleOutput();
            Assert.Contains("Enter the title to search:", output); // Check prompt
            Assert.Contains("--- Found Assignment ---", output);   // Check header
            // Check that the details of the found assignment are printed correctly
            Assert.Contains($"Found: {foundAssignment.Title}: {foundAssignment.Description} (Completed: {foundAssignment.IsCompleted})", output);

            // Verify the service's FindAssignmentByTitle method was called once with the correct title
            _mockService.Verify(s => s.FindAssignmentByTitle(titleToFind), Times.Once);
        }

        [Fact]
        public void DeleteAssignment_WhenSuccessful_CallsServiceAndWritesSuccessMessage()
        {
            // Arrange
            string titleToDelete = "Task To Delete";
            SetConsoleInput(titleToDelete); 

            // Setup the mock service's DeleteAssignment method:
            // When called with titleToDelete, return true (simulating successful deletion)
            _mockService.Setup(s => s.DeleteAssignment(titleToDelete))
                        .Returns(true);

            // Act
            _consoleUI.DeleteAssignment();

            // Assert
            var output = GetConsoleOutput();
            Assert.Contains("Enter the title of the assignment to delete:", output); // Check prompt
            Assert.Contains("Assignment deleted successfully.", output);             // Check success message

            // Verify the service's DeleteAssignment method was called once with the correct title
            _mockService.Verify(s => s.DeleteAssignment(titleToDelete), Times.Once);
        }

        public void Dispose()
        {
            Console.SetOut(_originalOutput); // Restore original Console.Out
            Console.SetIn(_originalInput);   // Restore original Console.In
            _stringWriter.Dispose();
        }
    }
}
