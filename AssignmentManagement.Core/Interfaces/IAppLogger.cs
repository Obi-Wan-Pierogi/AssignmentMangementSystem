using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssignmentManagement.Core.Interfaces
{
    public interface IAppLogger
    {
        void Log(string message);
        void LogError(string v);
        void LogInformation(string v);
        void LogWarning(string v);
    }
}
