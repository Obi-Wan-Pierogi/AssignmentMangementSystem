using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssignmentManagement.Core.Interfaces;

namespace AssignmentManagement.Core.Services
{
    public class AssignmentFormatter : IAssignmentFormatter
    {
        public string Format(Assignment assignment)
        {
            if (assignment == null)
            {
                return "Error: Cannot format a null assignment.";
            }
            return $"[{assignment.Id}] {assignment.Title} - {(assignment.IsCompleted ? "Completed" : "Incomplete")}";
        }

        public string FormatList(IEnumerable<Assignment> assignments)
        {
            if (assignments == null || !assignments.Any())
            {
                return "No assignments to display.";
            }

            var sb = new StringBuilder();

            var assignmentList = assignments.ToList();
            for (int i = 0; i < assignmentList.Count; i++)
            {
                sb.Append(Format(assignmentList[i]));
                if (i < assignmentList.Count - 1)
                {
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }
    }
}
