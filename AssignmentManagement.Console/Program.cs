using AssignmentManagement.Core.Interfaces;
using AssignmentManagement.Core.Services;
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

            services.AddSingleton<IAssignmentService, AssignmentService>();
            services.AddSingleton<ConsoleUI>();
            services.AddSingleton<IAppLogger, ConsoleAppLogger>();
            services.AddSingleton<IAssignmentFormatter, AssignmentFormatter>();
            services.AddSingleton<ConsoleUI>();

            var serviceProvider = services.BuildServiceProvider();
            var logger = serviceProvider.GetRequiredService<IAppLogger>();

            logger.LogInformation("Application starting...");
            try
            {
                var consoleUI = serviceProvider.GetRequiredService<ConsoleUI>();
                consoleUI.Run();
            }
            catch (Exception ex)
            {
                logger.LogError("An unhandled exception occurred in Main");
                System.Console.WriteLine($"An error occurred: {ex.Message}");
                System.Console.WriteLine(ex.StackTrace);
            }
            logger.LogInformation("Application finished.");
        }
    }
}
