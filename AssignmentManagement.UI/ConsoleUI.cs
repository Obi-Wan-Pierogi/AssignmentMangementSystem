using AssignmentManagement.Core;
using AssignmentManagement.Core.Interfaces;
using AssignmentManagement.Core.Enums;
using System;

namespace AssignmentManagement.UI
{
    public class ConsoleUI
    {
        private readonly IAssignmentService _assignmentService;
        private readonly IAppLogger _logger;

        public ConsoleUI(IAssignmentService assignmentService, IAppLogger logger)
        {
            _assignmentService = assignmentService ?? throw new ArgumentNullException(nameof(assignmentService));
            _logger = logger;
        }

        public void Run()
        {
            _logger.LogInformation("ConsoleUI Run method started.");

            // The dictionary maps user input directly to the corresponding method.
            var menuActions = new Dictionary<string, Action>
            {
                { "1", AddAssignment },
                { "2", ListAllAssignments },
                { "3", ListIncompleteAssignments },
                { "4", MarkAssignmentComplete },
                { "5", SearchAssignmentByTitle },
                { "6", UpdateAssignment },
                { "7", DeleteAssignment }
            };

            while (true)
            {
                DisplayMenu();
                var input = Console.ReadLine();
                _logger.LogInformation($"User chose option: {input}");

                if (input == "0")
                {
                    _logger.LogInformation("User chose to exit.");
                    Console.WriteLine("Goodbye!");
                    return;
                }

                // Use TryGetValue for a clean lookup without needing a default case.
                if (menuActions.TryGetValue(input, out var action))
                {
                    action.Invoke();
                }
                else
                {
                    _logger.LogWarning($"Invalid user input: {input}");
                    Console.WriteLine("Invalid choice. Try again.");
                }
            }
        }


        internal void AddAssignment()
        {
            _logger.LogInformation("Attempting to add a new assignment via ConsoleUI.");
            Console.Write("Enter assignment title: ");
            var title = Console.ReadLine();
            Console.Write("Enter assignment description: ");
            var description = Console.ReadLine();

            Console.Write("Enter priority (Low, Medium, High - defaults to Medium if empty or invalid): ");
            var priorityInput = Console.ReadLine();
            Priority chosenPriority;

            if (string.IsNullOrWhiteSpace(priorityInput) ||
                !Enum.TryParse<Priority>(priorityInput, true, out chosenPriority)) // true to ignore case
            {
                chosenPriority = Priority.Medium; // Default value
                _logger.LogInformation("Invalid or empty priority input, defaulting to Medium.");
                Console.WriteLine("Invalid or empty priority, defaulting to Medium.");
            }

            Console.Write("Enter optional due date (e.g., yyyy-MM-dd, or leave blank if none): ");
            var dueDateInput = Console.ReadLine();
            DateTime? dueDate = null;
            if (!string.IsNullOrWhiteSpace(dueDateInput))
            {
                // Using TryParse with current culture. For specific format, use TryParseExact.
                if (DateTime.TryParse(dueDateInput, out DateTime parsedDate))
                {
                    dueDate = parsedDate;
                }
                else
                {
                    _logger.LogWarning($"Invalid due date format entered: {dueDateInput}. Due date will not be set.");
                    Console.WriteLine("Invalid date format. Due date will not be set.");
                }
            }

            Console.Write("Enter optional notes for the assignment: ");
            var notes = Console.ReadLine();

            try
            {
                var assignment = new Assignment(title, description, dueDate, chosenPriority, notes);
                if (_assignmentService.AddAssignment(assignment))
                {
                    Console.WriteLine($"Assignment added successfully with Priority: {chosenPriority}.");
                }
                else
                {
                    Console.WriteLine("An assignment with this title already exists.");
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogError($"Error adding assignment in ConsoleUI: {ex.Message}");
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        internal void ListAllAssignments()
        {
            _logger.LogInformation("Listing all assignments in ConsoleUI.");
            var assignments = _assignmentService.ListAll();
            if (assignments.Count == 0)
            {
                Console.WriteLine("No assignments found.");
                return;
            }
            Console.WriteLine("\n--- All Assignments ---");
            foreach (var assignment in assignments)
            {
                Console.WriteLine(assignment.ToString());
                Console.WriteLine($"  Status: {(assignment.IsCompleted ? "Completed" : "Incomplete")}{(assignment.IsOverdue(_logger) ? " - OVERDUE" : "")}");
                Console.WriteLine();
            }
            Console.WriteLine("-----------------------");
        }

        internal void ListIncompleteAssignments()
        {
            _logger.LogInformation("Listing incomplete assignments in ConsoleUI.");
            var assignments = _assignmentService.ListIncomplete();
            if (assignments.Count == 0)
            {
                Console.WriteLine("No incomplete assignments found.");
                return;
            }
            Console.WriteLine("\n--- Incomplete Assignments ---");
            foreach (var assignment in assignments)
            {
                Console.WriteLine(assignment.ToString());
                Console.WriteLine($"  Status: {(assignment.IsCompleted ? "Completed" : "Incomplete")}{(assignment.IsOverdue(_logger) ? " - OVERDUE" : "")}");
                Console.WriteLine(); 
            }
            Console.WriteLine("----------------------------");
        }

        internal void MarkAssignmentComplete()
        {
            _logger.LogInformation("Attempting to mark an assignment as complete via ConsoleUI.");
            var incompleteAssignments = _assignmentService.ListIncomplete();
            if (incompleteAssignments.Count == 0)
            {
                Console.WriteLine("No incomplete assignments to mark as complete.");
                return;
            }

            var assignmentToMark = SelectAssignmentFromList(incompleteAssignments, "\n--- Select an Assignment to Mark as Complete ---");

            if (assignmentToMark != null)
            {
                if (_assignmentService.MarkAssignmentComplete(assignmentToMark.Title))
                {
                    Console.WriteLine($"Assignment '{assignmentToMark.Title}' marked as complete.");
                }
                else
                {
                    // Service should log details, UI message can be simpler
                    Console.WriteLine($"Could not mark '{assignmentToMark.Title}' as complete. It might have already been completed or an error occurred.");
                }
            }
        }
        internal void SearchAssignmentByTitle()
        {
            Console.Write("Enter the title to search: ");
            var title = Console.ReadLine();
            _logger.LogInformation($"Searching for assignment by title: '{title}' in ConsoleUI.");
            var assignment = _assignmentService.FindAssignmentByTitle(title);

            if (assignment == null)
            {
                Console.WriteLine("Assignment not found.");
            }
            else
            {
                Console.WriteLine($"\n--- Found Assignment ---");
                Console.WriteLine(assignment.ToString()); // Uses the overridden ToString()
                Console.WriteLine($"  Status: {(assignment.IsCompleted ? "Completed" : "Incomplete")}{(assignment.IsOverdue(_logger) ? " - OVERDUE" : "")}");
                Console.WriteLine($"----------------------");
            }
        }
        internal void UpdateAssignment()
        {
            _logger.LogInformation("Attempting to update an assignment via ConsoleUI.");
            var allAssignments = _assignmentService.ListAll();
            if (allAssignments.Count == 0)
            {
                Console.WriteLine("No assignments available to update.");
                return;
            }

            var assignmentToUpdate = SelectAssignmentFromList(allAssignments, "\n--- Select an Assignment to Update ---");

            if (assignmentToUpdate != null)
            {
                Console.WriteLine($"\nUpdating assignment:");
                Console.WriteLine(assignmentToUpdate.ToString()); // Display current details using ToString
                Console.WriteLine($"  Status: {(assignmentToUpdate.IsCompleted ? "Completed" : "Incomplete")}{(assignmentToUpdate.IsOverdue(_logger) ? " - OVERDUE" : "")}");

                Console.Write($"Enter new title (leave blank to keep '{assignmentToUpdate.Title}'): ");
                var newTitle = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(newTitle))
                {
                    newTitle = assignmentToUpdate.Title;
                }

                Console.Write($"Enter new description (leave blank to keep '{assignmentToUpdate.Description}'): ");
                var newDescription = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(newDescription))
                {
                    newDescription = assignmentToUpdate.Description;
                }
                _logger.LogInformation($"User provided update details - New Title: '{newTitle}', New Description: '{(string.IsNullOrWhiteSpace(newDescription) ? "[No Change]" : newDescription)}' for original title '{assignmentToUpdate.Title}'.");

                // Note: Priority update is not handled here.
                // The IAssignmentService.UpdateAssignment currently only takes oldTitle, newTitle, newDescription.

                if (_assignmentService.UpdateAssignment(assignmentToUpdate.Title, newTitle, newDescription))
                {
                    Console.WriteLine("Assignment updated successfully.");
                }
                else
                {
                    Console.WriteLine("Update failed. The new title may conflict, the original assignment was not found, or new data was invalid.");
                }
            }
        }
        internal void DeleteAssignment()
        {
            _logger.LogInformation("Attempting to delete an assignment via ConsoleUI.");
            var allAssignments = _assignmentService.ListAll();
            if (allAssignments.Count == 0)
            {
                Console.WriteLine("No assignments available to delete.");
                return;
            }

            var assignmentToDelete = SelectAssignmentFromList(allAssignments, "\n--- Select an Assignment to Delete ---");

            if (assignmentToDelete != null)
            {
                Console.Write($"Are you sure you want to delete '{assignmentToDelete.Title}'? (y/n): ");
                string confirmation = Console.ReadLine()?.Trim().ToLower();

                if (confirmation == "y")
                {
                    _logger.LogInformation($"User confirmed deletion for assignment: '{assignmentToDelete.Title}'.");
                    if (_assignmentService.DeleteAssignment(assignmentToDelete.Title))
                    {
                        Console.WriteLine($"Assignment '{assignmentToDelete.Title}' deleted successfully.");
                    }
                    else
                    {
                        Console.WriteLine($"Could not delete assignment '{assignmentToDelete.Title}'. It may have already been deleted or an error occurred.");
                    }
                }
                else
                {
                    _logger.LogInformation($"User cancelled deletion for assignment: '{assignmentToDelete.Title}'.");
                    Console.WriteLine("Deletion cancelled.");
                }
            }
        }

        // Helper method to keep the main loop clean.
        private void DisplayMenu()
        {
            Console.WriteLine("\nAssignment Manager Menu:");
            Console.WriteLine("1. Add Assignment");
            Console.WriteLine("2. List All Assignments");
            Console.WriteLine("3. List Incomplete Assignments");
            Console.WriteLine("4. Mark Assignment as Complete");
            Console.WriteLine("5. Search Assignment by Title");
            Console.WriteLine("6. Update Assignment");
            Console.WriteLine("7. Delete Assignment");
            Console.WriteLine("0. Exit");
            Console.Write("Choose an option: ");
        }

        // Helper method to select an assignment from a list
        private Assignment SelectAssignmentFromList(List<Assignment> assignments, string promptMessage)
        {
            if (assignments == null || assignments.Count == 0)
            {
                _logger.LogInformation("SelectAssignmentFromList: No assignments available.");
                Console.WriteLine("No assignments available for this action.");
                return null;
            }

            Console.WriteLine(promptMessage);
            PrintAssignmentsForSelection(assignments); // Responsibility 1: Display

            int? selectedIndex = GetUserSelectionIndex(assignments.Count); // Responsibility 2: Get Input

            if (selectedIndex.HasValue)
            {
                var selectedAssignment = assignments[selectedIndex.Value];
                _logger.LogInformation($"User selected assignment: ID {selectedAssignment.Id}, Title '{selectedAssignment.Title}'.");
                return selectedAssignment;
            }

            _logger.LogInformation("User cancelled assignment selection.");
            Console.WriteLine("Selection cancelled.");
            return null;
        }

        // Helper method for displaying the list
        private void PrintAssignmentsForSelection(List<Assignment> assignments)
        {
            for (int i = 0; i < assignments.Count; i++)
            {
                var assignment = assignments[i];
                string dueDateDisplay = assignment.DueDate?.ToShortDateString() ?? "N/A";
                string overdueMarker = assignment.IsOverdue(_logger) ? " (OVERDUE)" : "";
                Console.WriteLine($"{i + 1}. {assignment.Title} (Due: {dueDateDisplay}{overdueMarker}, Priority: {assignment.Priority}, Status: {(assignment.IsCompleted ? "Completed" : "Incomplete")})");
            }
        }

        // Helper method for handling the user input loop
        private int? GetUserSelectionIndex(int assignmentCount)
        {
            while (true)
            {
                Console.Write($"Enter the number of the assignment (or 0 to cancel): ");
                string input = Console.ReadLine();
                if (int.TryParse(input, out int choice))
                {
                    if (choice == 0)
                    {
                        return null; // User cancelled
                    }
                    if (choice >= 1 && choice <= assignmentCount)
                    {
                        return choice - 1; // Return the zero-based index
                    }
                }
                _logger.LogWarning($"Invalid selection input: '{input}'.");
                Console.WriteLine($"Invalid choice. Please enter a number from 1 to {assignmentCount} or 0 to cancel.");
            }
        }
    }
}
