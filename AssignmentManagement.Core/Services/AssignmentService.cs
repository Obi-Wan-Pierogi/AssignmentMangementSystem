using AssignmentManagement.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AssignmentManagement.Core.Services
{
    public class AssignmentService : IAssignmentService
    {
        private readonly Dictionary<string, Assignment> _assignments =
            new Dictionary<string, Assignment>(StringComparer.OrdinalIgnoreCase);

        private readonly IAssignmentFormatter _formatter;
        private readonly IAppLogger _logger;

        public AssignmentService(IAssignmentFormatter formatter, IAppLogger logger)
        {
            _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logger.LogInformation("AssignmentService initialized.");
        }

        public bool AddAssignment(Assignment assignment)
        {
            if (assignment == null)
            {
                _logger.LogWarning("Attempted to add a null assignment.");
                return false;
            }

            bool added = _assignments.TryAdd(assignment.Title, assignment);
            if (added)
            {
                // More detailed log message for successful addition
                _logger.LogInformation($"Added assignment: ID {assignment.Id}, Title '{assignment.Title}', Priority: {assignment.Priority}, Due: {assignment.DueDate?.ToShortDateString() ?? "N/A"}.");
            }
            else
            {
                _logger.LogWarning($"Failed to add assignment: Title '{assignment.Title}' already exists.");
            }
            return added;
        }

        public List<Assignment> ListAll()
        {
            _logger.LogInformation($"Listing all {_assignments.Count} assignments.");
            return _assignments.Values.ToList();
        }

        public List<Assignment> ListIncomplete()
        {
            var incomplete = _assignments.Values.Where(a => !a.IsCompleted).ToList();
            _logger.LogInformation($"Listing {incomplete.Count} incomplete assignments out of {_assignments.Count} total.");
            return incomplete;
        }

        public Assignment FindAssignmentByTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                _logger.LogWarning("Attempted to find an assignment with a null or empty title.");
                return null;
            }

            _assignments.TryGetValue(title, out var assignment);
            if (assignment != null)
            {
                _logger.LogInformation($"Found assignment: ID {assignment.Id}, Title '{assignment.Title}'.");

            }
            else
            {
                _logger.LogWarning($"Assignment with title '{title}' not found.");
            }
            return assignment;
        }

        public bool MarkAssignmentComplete(string title)
        {
            var assignment = FindAssignmentByTitle(title);
            if (assignment != null)
            {
                if (!assignment.IsCompleted)
                {
                    assignment.MarkComplete();
                    _logger.LogInformation($"Marked assignment '{assignment.Title}' (ID: {assignment.Id}) as complete.");
                    return true;
                }
                else
                {
                    _logger.LogInformation($"Assignment '{assignment.Title}' (ID: {assignment.Id}) is already completed.");
                    return true;
                }

            }
            return false;
        }

        public bool DeleteAssignment(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                _logger.LogWarning("Attempted to delete an assignment with a null or empty title.");
                return false;
            }

            bool removed = _assignments.Remove(title);
            if (removed)
            {
                _logger.LogInformation($"Deleted assignment with title: '{title}'.");
            }
            else
            {
                _logger.LogWarning($"Failed to delete assignment: Title '{title}' not found.");
            }


            return removed;
        }

        public bool UpdateAssignment(string oldTitle, string newTitle, string newDescription)
        {
            if (string.IsNullOrWhiteSpace(oldTitle) || string.IsNullOrWhiteSpace(newTitle))
            {
                _logger.LogWarning("Attempted to update an assignment with a null or empty title.");
                return false;
            }

            if (!_assignments.TryGetValue(oldTitle, out var assignment))
            {
                _logger.LogWarning($"Failed to update assignment: Original title '{oldTitle}' not found.");
                return false;
            }

            bool titleIsChanging = !StringComparer.OrdinalIgnoreCase.Equals(oldTitle, newTitle);
            if (titleIsChanging && _assignments.ContainsKey(newTitle))
            {
                _logger.LogWarning($"Failed to update assignment: New title '{newTitle}' already exists.");
                return false;
            }

            try
            {
                string assignmentIdForLog = $"ID: {assignment.Id}";
                assignment.Update(newTitle, newDescription);

                if (titleIsChanging)
                {
                    _assignments.Remove(oldTitle);
                    _assignments.Add(newTitle, assignment);
                    _logger.LogInformation($"Assignment {assignmentIdForLog} title changed from '{oldTitle}' to '{newTitle}'. Description also updated.");
                }
                else
                {
                    _logger.LogInformation($"Assignment {assignmentIdForLog} ('{newTitle}') description updated.");
                }
                return true;

            }
            catch (ArgumentException ex)
            {
                _logger.LogError($"Error updating assignment '{oldTitle}': {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error updating assignment '{oldTitle}': {ex.ToString()}");
                return false;
            }
        }

        public string GetFormattedAssignment(string title)
        {
            _logger.LogInformation($"Attempting to get formatted assignment for title: '{title}'");
            var assignment = FindAssignmentByTitle(title); 
                                                          
            if (assignment == null)
            {
                return "Assignment not found for formatting."; 
            }
            return _formatter.Format(assignment);
        }

        public string GetFormattedAllAssignments()
        {
            _logger.LogInformation("Attempting to get all assignments formatted.");
            var allAssignments = ListAll();
            return _formatter.FormatList(allAssignments);
        }
        public string GetFormattedIncompleteAssignments()
        {
            _logger.LogInformation("Attempting to get incomplete assignments formatted.");
            var incompleteAssignments = ListIncomplete();
            return _formatter.FormatList(incompleteAssignments);
        }
    }
}
