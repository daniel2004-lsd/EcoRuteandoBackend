using EcoRuteando.Modules.Security.Application;
using EcoRuteando.Modules.Security.Infrastructure;
using EcoRuteando.Modules.Security.Presentation.Controllers;

namespace EcoRuteando.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Registrar Application
            builder.Services.AddSecurityApplication();

            // Registrar Infrastructure
            builder.Services.AddSecurityInfrastructure(builder.Configuration);

            // Controllers + Swagger
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddEndpointsApiExplorer();
            



            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}