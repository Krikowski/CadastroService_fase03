using CadastroService.Controllers;
using CadastroService.Models;
using CadastroService.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

public class CadastroControllerTests {
    private readonly Mock<IPersistenciaServiceClient> _mockPersistenciaClient;
    private readonly CadastroController _controller;

    public CadastroControllerTests() {
        _mockPersistenciaClient = new Mock<IPersistenciaServiceClient>();
        _controller = new CadastroController(_mockPersistenciaClient.Object);
    }

    [Fact]
    public async Task CriarContato_Sucesso_RetornaCreated() {
        var contato = new ContatoDto { Id = 1, Nome = "Nome", Email = "email@email.com", Telefone = "123456789", DDD = "11" };

        _mockPersistenciaClient.Setup(s => s.EnviarContatoAsync(contato)).ReturnsAsync(true);

        var result = await _controller.CriarContato(contato);

        var createdResult = Assert.IsType<CreatedResult>(result);
        Assert.Equal(contato, createdResult.Value);
    }

    [Fact]
    public async Task CriarContato_Falha_RetornaStatusCode500() {
        var contato = new ContatoDto();

        _mockPersistenciaClient.Setup(s => s.EnviarContatoAsync(contato)).ReturnsAsync(false);

        var result = await _controller.CriarContato(contato);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetAll_RetornaOkComListaDeContatos() {
        var lista = new List<ContatoDto> {
            new ContatoDto { Id = 1, Nome = "Nome", Email = "email@email.com", Telefone = "123456789", DDD = "11" }
        };

        _mockPersistenciaClient.Setup(s => s.ListarContatosAsync()).ReturnsAsync(lista);

        var result = await _controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var contatos = Assert.IsAssignableFrom<List<ContatoDto>>(okResult.Value);
        Assert.Single(contatos);
    }

    [Fact]
    public async Task GetById_ContatoExiste_RetornaOkComContato() {
        var contato = new ContatoDto { Id = 1, Nome = "Nome", Email = "email@email.com", Telefone = "123456789", DDD = "11" };

        _mockPersistenciaClient.Setup(s => s.ObterContatoAsync(1)).ReturnsAsync(contato);

        var result = await _controller.GetById(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(contato, okResult.Value);
    }

    [Fact]
    public async Task GetById_ContatoNaoExiste_RetornaNotFound() {
        _mockPersistenciaClient.Setup(s => s.ObterContatoAsync(1)).ReturnsAsync((ContatoDto?)null);

        var result = await _controller.GetById(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task AtualizarContato_Sucesso_RetornaNoContent() {
        var contato = new ContatoDto();

        _mockPersistenciaClient.Setup(s => s.AtualizarContatoAsync(1, contato)).ReturnsAsync(true);

        var result = await _controller.AtualizarContato(1, contato);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task AtualizarContato_Falha_RetornaNotFound() {
        var contato = new ContatoDto();

        _mockPersistenciaClient.Setup(s => s.AtualizarContatoAsync(1, contato)).ReturnsAsync(false);

        var result = await _controller.AtualizarContato(1, contato);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeletarContato_Sucesso_RetornaNoContent() {
        _mockPersistenciaClient.Setup(s => s.DeletarContatoAsync(1)).ReturnsAsync(true);

        var result = await _controller.DeletarContato(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeletarContato_Falha_RetornaNotFound() {
        _mockPersistenciaClient.Setup(s => s.DeletarContatoAsync(1)).ReturnsAsync(false);

        var result = await _controller.DeletarContato(1);

        Assert.IsType<NotFoundResult>(result);
    }
}
