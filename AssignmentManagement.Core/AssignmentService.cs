
namespace AssignmentManagement.Core
{
    public class AssignmentService
    {
        private readonly List<Assignment> assignments = new();

        public void AddAssignment(Assignment assignment)
        {
            assignments.Add(assignment);
        }

        public List<Assignment> ListAll()
        {
            return new List<Assignment>(assignments);
        }

        public List<Assignment> ListIncomplete()
        {
            return assignments.Where(a => !a.IsCompleted).ToList();
        }
    }
}
