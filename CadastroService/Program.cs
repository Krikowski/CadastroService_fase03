using CadastroService.RabbitMQ;
using CadastroService.Services;
using Prometheus;

namespace CadastroService {
    public class Program {
        public static async Task Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);

            builder.WebHost.ConfigureKestrel(options => {
                options.ListenAnyIP(8080);
            });

            // Configuração dos serviços - tudo antes do Build()
            builder.Services.AddHttpClient();
            builder.Services.AddHttpClient("PersistenciaService", client => {
                var persistenciaBaseUrl = builder.Configuration["PERSISTENCIA_SERVICE_BASEURL"];
                if (string.IsNullOrEmpty(persistenciaBaseUrl)) {
                    throw new Exception("Variável de ambiente PERSISTENCIA_SERVICE_BASEURL não configurada.");
                }
                client.BaseAddress = new Uri(persistenciaBaseUrl);
            });

            builder.Services.AddTransient<IPersistenciaServiceClient, PersistenciaServiceClient>();

            // Aqui deve estar ativado para que o RabbitMQ funcione corretamente
            builder.Services.AddSingleton<RabbitMQPublisherService>();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment()) {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseRouting();
            app.UseAuthorization();

            app.UseMetricServer();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
                endpoints.MapMetrics();
            });

            await app.RunAsync();
        }
    }
}
