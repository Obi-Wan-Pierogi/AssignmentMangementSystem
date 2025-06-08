using AssignmentManagement.Core.Interfaces;
using AssignmentManagement.Core.Services;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

namespace AssignmentManagement.WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                    // Ensure this line is present and correctly set:
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                    // Optional: If you want your API to *output* JSON with camelCase property names (a common convention):
                    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                });
            builder.Services.AddSingleton<IAssignmentFormatter, AssignmentFormatter>();
            builder.Services.AddSingleton<IAppLogger, ConsoleAppLogger>();
            builder.Services.AddSingleton<IAssignmentService, AssignmentService>();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Assignment Management API", Version = "v1" });
            });
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
