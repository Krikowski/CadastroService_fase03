using System.ComponentModel.DataAnnotations;

namespace PersistenciaService.Models {
    public class Contato {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O email é obrigatório.")]
        [EmailAddress(ErrorMessage = "O email informado não é válido.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "O telefone é obrigatório.")]
        [Phone(ErrorMessage = "O telefone informado não é válido.")]
        public string Telefone { get; set; }

        [Required(ErrorMessage = "O DDD é obrigatório.")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "O DDD deve ter 2 caracteres.")]
        public string DDD { get; set; }

        // Construtor vazio que inicializa as propriedades para valores padrão não nulos
        public Contato() {
            Nome = string.Empty;
            Email = string.Empty;
            Telefone = string.Empty;
            DDD = string.Empty;
        }
    }
}