using AssignmentManagement.Core;
using AssignmentManagement.UI;

using Microsoft.Extensions.DependencyInjection;

using System;

namespace AssignmentManagement.Console
{
    internal class Program
    {
        public static void Main(string[] args) // 🔥 Static Main method — this is required
        {
            var services = new ServiceCollection();

            services.AddSingleton<AssignmentService>();
            services.AddSingleton<ConsoleUI>();

            var serviceProvider = services.BuildServiceProvider();
            var consoleUI = serviceProvider.GetRequiredService<ConsoleUI>();

            consoleUI.Run();
        }
    }
}
