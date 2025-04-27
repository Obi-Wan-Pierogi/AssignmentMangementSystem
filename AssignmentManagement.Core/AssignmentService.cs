
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
            // TODO: Return only assignments where IsCompleted is false
            throw new NotImplementedException();
        }
    }
}
