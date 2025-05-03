using System;
using System.Collections.Generic;
using System.Linq;

namespace AssignmentManagement.Core
{
    public class AssignmentService
    {
        private readonly Dictionary<string, Assignment> _assignments =
            new Dictionary<string, Assignment>(StringComparer.OrdinalIgnoreCase);

        public bool AddAssignment(Assignment assignment)
        {
            if (assignment == null)
            {
                // Or throw ArgumentNullException depending on desired behavior
                return false;
            }
            // Assignment constructor already validated title/description.
            // TryAdd handles duplicate title check efficiently.
            return _assignments.TryAdd(assignment.Title, assignment);
        }

        public List<Assignment> ListAll()
        {
            return new List<Assignment>(_assignments.Values);
        }

        public List<Assignment> ListIncomplete()
        {
            return _assignments.Values.Where(a => !a.IsCompleted).ToList();
        }

        // TODO: Implement method to find an assignment by title
        public Assignment FindAssignmentByTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return null;

            _assignments.TryGetValue(title, out var assignment);
            return assignment; 
        }

        // TODO: Implement method to mark an assignment complete
        public bool MarkAssignmentComplete(string title)
        {
            var assignment = FindAssignmentByTitle(title);
            if (assignment != null)
            {
                assignment.MarkComplete(); // Call Assignment's method
                return true;
            }
            return false;
        }

        // TODO: Implement method to delete an assignment by title
        public bool DeleteAssignment(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return false;

            // Dictionary.Remove is efficient (O(1) average)
            return _assignments.Remove(title);
        }

        // TODO: Implement method to update an assignment (title and description)
        public bool UpdateAssignment(string oldTitle, string newTitle, string newDescription)
        {
            if (string.IsNullOrWhiteSpace(oldTitle) || string.IsNullOrWhiteSpace(newTitle))
            {
                return false;
            }

            if (!_assignments.TryGetValue(oldTitle, out var assignment))
            {
                return false; // Assignment with oldTitle not found
            }

            bool titleIsChanging = !StringComparer.OrdinalIgnoreCase.Equals(oldTitle, newTitle);
            if (titleIsChanging && _assignments.ContainsKey(newTitle))
            {
                return false; // New title conflicts with another existing assignment
            }

            try
            {
                assignment.Update(newTitle, newDescription);
            }
            catch (ArgumentException)
            {
                // Invalid data passed for update (caught from Assignment.Update validation)
                return false;
            }

            if (titleIsChanging)
            {
                _assignments.Remove(oldTitle);
                // Use Add; we know it doesn't exist because of the ContainsKey check earlier.
                _assignments.Add(newTitle, assignment);
            }

            return true;
        }
    }
}
