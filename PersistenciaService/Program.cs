using PersistenciaService.Data;
using Microsoft.EntityFrameworkCore;
using PersistenciaService.Services;
using Prometheus;

namespace PersistenciaService {
    public class Program {
        public static async Task Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);

            builder.WebHost.ConfigureKestrel(options => {
                options.ListenAnyIP(80);
                options.ListenAnyIP(9090);
            });

            // Serviços
            builder.Services.AddControllers();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    sqlServerOptions => sqlServerOptions.EnableRetryOnFailure()
                )
            );

            builder.Services.AddHostedService<RabbitMQConsumerService>();


            builder.Services.AddScoped<ContatoService>();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Swagger
            app.UseSwagger();
            app.UseSwaggerUI();

            // Prometheus
            app.UseMetricServer(); 

            app.UseHttpsRedirection();
            app.UseAuthorization();

            app.UseRouting();

            app.UseHttpMetrics();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
                endpoints.MapMetrics(); // /metrics
            });

            // Aplica as migrations automaticamente
            using (var scope = app.Services.CreateScope()) {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.Migrate();
            }

            await app.RunAsync();
        }
    }
}
