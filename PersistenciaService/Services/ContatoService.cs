using PersistenciaService.Data;
using PersistenciaService.Models;
using Microsoft.EntityFrameworkCore;

namespace PersistenciaService.Services {
    public class ContatoService {
        private readonly ApplicationDbContext _context;

        public ContatoService(ApplicationDbContext context) {
            _context = context;
        }

        public async Task<List<Contato>> GetAllContatosAsync() {
            return await _context.Contatos.ToListAsync();
        }

        public async Task<Contato?> GetContatoByIdAsync(int id) {
            return await _context.Contatos.FindAsync(id);
        }

        public async Task<Contato> CreateContatoAsync(Contato contato) {
            _context.Contatos.Add(contato);
            await _context.SaveChangesAsync();
            return contato;
        }

        public async Task<bool> UpdateContatoAsync(Contato contatoAtualizado) {
            var contatoExistente = await _context.Contatos.FindAsync(contatoAtualizado.Id);
            if (contatoExistente == null) return false;

            contatoExistente.Nome = contatoAtualizado.Nome;
            contatoExistente.Email = contatoAtualizado.Email;
            contatoExistente.Telefone = contatoAtualizado.Telefone;
            contatoExistente.DDD = contatoAtualizado.DDD;

            _context.Contatos.Update(contatoExistente);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteContatoAsync(int id) {
            var contato = await _context.Contatos.FindAsync(id);
            if (contato == null) return false;

            _context.Contatos.Remove(contato);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
