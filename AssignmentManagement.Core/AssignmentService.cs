using System;
using System.Collections.Generic;
using System.Linq;

namespace AssignmentManagement.Core
{
    public class AssignmentService : IAssignmentService
    {
        private readonly Dictionary<string, Assignment> _assignments =
            new Dictionary<string, Assignment>(StringComparer.OrdinalIgnoreCase);

        public bool AddAssignment(Assignment assignment)
        {
            if (assignment == null)
            {
                return false;
            }
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

        public Assignment FindAssignmentByTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return null;

            _assignments.TryGetValue(title, out var assignment);
            return assignment; 
        }

        public bool MarkAssignmentComplete(string title)
        {
            var assignment = FindAssignmentByTitle(title);
            if (assignment != null)
            {
                assignment.MarkComplete(); 
            }
            return false;
        }

        public bool DeleteAssignment(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return false;

            
            return _assignments.Remove(title);
        }

        public bool UpdateAssignment(string oldTitle, string newTitle, string newDescription)
        {
            if (string.IsNullOrWhiteSpace(oldTitle) || string.IsNullOrWhiteSpace(newTitle))
            {
                return false;
            }

            if (!_assignments.TryGetValue(oldTitle, out var assignment))
            {
                return false; 
            }

            bool titleIsChanging = !StringComparer.OrdinalIgnoreCase.Equals(oldTitle, newTitle);
            if (titleIsChanging && _assignments.ContainsKey(newTitle))
            {
                return false;
            }

            try
            {
                assignment.Update(newTitle, newDescription);
            }
            catch (ArgumentException)
            {
                return false;
            }

            if (titleIsChanging)
            {
                _assignments.Remove(oldTitle);
                _assignments.Add(newTitle, assignment);
            }

            return true;
        }
    }
}
