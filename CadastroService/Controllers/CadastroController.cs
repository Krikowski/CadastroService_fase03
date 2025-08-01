using CadastroService.Models;
using CadastroService.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using CadastroService.RabbitMQ;


namespace CadastroService.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class CadastroController : ControllerBase {
        private readonly IPersistenciaServiceClient _persistenciaClient;

        private readonly RabbitMQPublisherService _publisher;

        public CadastroController(IPersistenciaServiceClient persistenciaClient, RabbitMQPublisherService publisher) {
            _persistenciaClient = persistenciaClient;
            _publisher = publisher;
        }

        // CREATE
        [HttpPost]
        public async Task<IActionResult> CriarContato([FromBody] ContatoDto contato) {
            var sucesso = await _persistenciaClient.EnviarContatoAsync(contato);
            if (!sucesso)
                return StatusCode(500, "Erro ao criar o contato.");

            _publisher.PublishContato(contato);
            return Created("", contato);
        }

        // READ - Listar todos
        [HttpGet]
        public async Task<IActionResult> GetAll() {
            var contatos = await _persistenciaClient.ListarContatosAsync();
            return Ok(contatos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id) {
            var contato = await _persistenciaClient.ObterContatoAsync(id);
            if (contato == null) return NotFound();
            return Ok(contato);
        }

        // UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarContato(int id, [FromBody] ContatoDto contato) {
            var sucesso = await _persistenciaClient.AtualizarContatoAsync(id, contato);
            if (!sucesso)
                return NotFound();

            return NoContent(); // 204
        }

        // DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletarContato(int id) {
            var sucesso = await _persistenciaClient.DeletarContatoAsync(id);
            if (!sucesso)
                return NotFound();

            return NoContent(); // 204
        }
    }
}
