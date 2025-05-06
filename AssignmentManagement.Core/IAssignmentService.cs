using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssignmentManagement.Core
{
    public interface IAssignmentService
    {
        bool AddAssignment(Assignment assignment);
        List<Assignment> ListAll();
        List<Assignment> ListIncomplete();
        bool MarkAssignmentComplete(string title);
        Assignment FindAssignmentByTitle(string title);
        bool UpdateAssignment(string oldTitle, string newTitle, string newDescription);
        bool DeleteAssignment(string title);
    }
}
