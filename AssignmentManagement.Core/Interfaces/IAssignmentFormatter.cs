using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssignmentManagement.Core.Interfaces
{
    public interface IAssignmentFormatter
    {
        string Format(Assignment assignment);
        string FormatList(IEnumerable<Assignment> assignments);
    }
}
