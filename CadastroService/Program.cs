
using CadastroService.Services;

namespace CadastroService {
    public class Program {
        public static void Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);

            // Registrar o HttpClient Factory para conseguir injetar IHttpClientFactory
            builder.Services.AddHttpClient();

            // Registrar o client personalizado
            builder.Services.AddHttpClient("PersistenciaService", client => {
                var persistenciaBaseUrl = builder.Configuration["PERSISTENCIA_SERVICE_BASEURL"];
                if (string.IsNullOrEmpty(persistenciaBaseUrl)) {
                    throw new Exception("Variável de ambiente PERSISTENCIA_SERVICE_BASEURL não configurada.");
                }
                client.BaseAddress = new Uri(persistenciaBaseUrl);
            });

            // Registrar interface e implementação para injeção de dependência
            builder.Services.AddTransient<IPersistenciaServiceClient, PersistenciaServiceClient>();

            // Outros serviços
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment()) {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
