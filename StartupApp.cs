using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using ConsumerAPI.Config;
using ConsumerAPI.Services;
using ConsumerAPI.Models;
using Microsoft.Extensions.Options;

namespace ConsumerAPI
{
    public class StartupApp
    {
        public static void Run(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add DI services to the container.

            // Configuración de RabbitMQSettings desde appsettings.json
            //builder.Services.AddScoped<IRabbitMQService, RabbitMQService>();
            builder.Services.AddScoped(typeof(IRabbitMQService<>), typeof(RabbitMQService<>));

            builder.Services.Configure<RabbitMQSettings>(
                builder.Configuration.GetSection("RabbitMQ")
            );

            builder.Services.AddScoped<IRabbitMQService<ClassMessageRMQ>, RabbitMQService<ClassMessageRMQ>>();

            builder.Services.AddControllers();

            // builder.WebHost.ConfigureKestrel(options =>
            // {
            //     options.ListenAnyIP(8080); // El contenedor escucha en 8080
            // });

            // Agregar Swagger
            builder.Services.AddSwaggerGen();

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseCors();
                app.UseSwagger();              // genera /swagger/v1/swagger.json
                app.UseSwaggerUI(c =>          // UI en /swagger/index.html
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mi API V1");
                    c.RoutePrefix = "swagger";
                });
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
