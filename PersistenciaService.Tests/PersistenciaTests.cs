using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using PersistenciaService.Controllers;
using PersistenciaService.Data;
using PersistenciaService.Models;
using PersistenciaService.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace PersistenciaService.Tests
{
    public class ContatoTests
    {
        // ------- Serviço --------
        private async Task<ApplicationDbContext> GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "ContatoDbTest")
                .Options;

            var context = new ApplicationDbContext(options);
            context.Contatos.RemoveRange(context.Contatos);
            await context.SaveChangesAsync();

            return context;
        }

        [Fact]
        public async Task CreateContatoAsync_ShouldAddContato()
        {
            var context = await GetInMemoryDbContext();
            var service = new ContatoService(context);

            var contato = new Contato { Nome = "Teste", Email = "teste@email.com", Telefone = "123456789", DDD = "11" };

            var result = await service.CreateContatoAsync(contato);

            Assert.NotNull(result);
            Assert.True(result.Id > 0);
        }

        [Fact]
        public async Task GetAllContatosAsync_ShouldReturnAll()
        {
            var context = await GetInMemoryDbContext();
            var service = new ContatoService(context);

            context.Contatos.Add(new Contato { Nome = "Contato1", Email = "c1@email.com", Telefone = "1111", DDD = "11" });
            context.Contatos.Add(new Contato { Nome = "Contato2", Email = "c2@email.com", Telefone = "2222", DDD = "22" });
            await context.SaveChangesAsync();

            var result = await service.GetAllContatosAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task UpdateContatoAsync_ShouldReturnTrue_WhenExists()
        {
            var context = await GetInMemoryDbContext();
            var service = new ContatoService(context);

            var contato = new Contato { Nome = "Contato", Email = "c@email.com", Telefone = "1234", DDD = "11" };
            context.Contatos.Add(contato);
            await context.SaveChangesAsync();

            contato.Nome = "Nome Atualizado";

            var result = await service.UpdateContatoAsync(contato);

            Assert.True(result);
            var updatedContato = await context.Contatos.FindAsync(contato.Id);
            Assert.Equal("Nome Atualizado", updatedContato.Nome);
        }

        [Fact]
        public async Task DeleteContatoAsync_ShouldReturnTrue_WhenExists()
        {
            var context = await GetInMemoryDbContext();
            var service = new ContatoService(context);

            var contato = new Contato { Nome = "Contato", Email = "c@email.com", Telefone = "1234", DDD = "11" };
            context.Contatos.Add(contato);
            await context.SaveChangesAsync();

            var result = await service.DeleteContatoAsync(contato.Id);

            Assert.True(result);
            var deletedContato = await context.Contatos.FindAsync(contato.Id);
            Assert.Null(deletedContato);
        }

        // ------- Controller --------
        // Controller refatorado para receber serviço via DI
        public class ContatosControllerRefatorado : ControllerBase
        {
            private readonly ContatoService _service;

            public ContatosControllerRefatorado(ContatoService service)
            {
                _service = service;
            }

            [HttpGet]
            public async Task<IActionResult> GetAll()
            {
                var contatos = await _service.GetAllContatosAsync();
                return Ok(contatos);
            }

            [HttpGet("{id}")]
            public async Task<IActionResult> GetById(int id)
            {
                var contato = await _service.GetContatoByIdAsync(id);
                if (contato == null) return NotFound();
                return Ok(contato);
            }

            [HttpPost]
            public async Task<IActionResult> Create([FromBody] Contato contato)
            {
                var criado = await _service.CreateContatoAsync(contato);
                return CreatedAtAction(nameof(GetById), new { id = criado.Id }, criado);
            }

            [HttpPut("{id}")]
            public async Task<IActionResult> Update(int id, [FromBody] Contato contatoAtualizado)
            {
                if (id != contatoAtualizado.Id)
                    return BadRequest("ID do contato incompatível.");

                var atualizado = await _service.UpdateContatoAsync(contatoAtualizado);
                if (!atualizado) return NotFound();

                return NoContent();
            }

            [HttpDelete("{id}")]
            public async Task<IActionResult> Delete(int id)
            {
                var deletado = await _service.DeleteContatoAsync(id);
                if (!deletado) return NotFound();

                return NoContent();
            }
        }

        [Fact]
        public async Task Controller_GetAll_ReturnsOk()
        {
            var context = await GetInMemoryDbContext();
            var service = new ContatoService(context);
            var controller = new ContatosControllerRefatorado(service);

            context.Contatos.Add(new Contato { Nome = "Contato1", Email = "c1@email.com", Telefone = "1111", DDD = "11" });
            context.Contatos.Add(new Contato { Nome = "Contato2", Email = "c2@email.com", Telefone = "2222", DDD = "22" });
            await context.SaveChangesAsync();

            var result = await controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var contatos = Assert.IsAssignableFrom<List<Contato>>(okResult.Value);
            Assert.Equal(2, contatos.Count);
        }

        [Fact]
        public async Task Controller_Create_ReturnsCreated()
        {
            var context = await GetInMemoryDbContext();
            var service = new ContatoService(context);
            var controller = new ContatosControllerRefatorado(service);

            var contato = new Contato { Nome = "Novo", Email = "novo@email.com", Telefone = "123456", DDD = "11" };

            var result = await controller.Create(contato);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var createdContato = Assert.IsType<Contato>(createdResult.Value);
            Assert.Equal("Novo", createdContato.Nome);
        }

        [Fact]
        public async Task Controller_Update_ReturnsNoContent()
        {
            var context = await GetInMemoryDbContext();
            var service = new ContatoService(context);
            var controller = new ContatosControllerRefatorado(service);

            var contato = new Contato { Nome = "Para Atualizar", Email = "atualizar@email.com", Telefone = "1234", DDD = "11" };
            context.Contatos.Add(contato);
            await context.SaveChangesAsync();

            contato.Nome = "Atualizado";

            var result = await controller.Update(contato.Id, contato);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Controller_Delete_ReturnsNoContent()
        {
            var context = await GetInMemoryDbContext();
            var service = new ContatoService(context);
            var controller = new ContatosControllerRefatorado(service);

            var contato = new Contato { Nome = "Para Deletar", Email = "deletar@email.com", Telefone = "4321", DDD = "11" };
            context.Contatos.Add(contato);
            await context.SaveChangesAsync();

            var result = await controller.Delete(contato.Id);

            Assert.IsType<NoContentResult>(result);
        }
    }
}
