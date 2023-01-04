using System.ComponentModel.DataAnnotations;

namespace ProjetoBanco.Models
{
    public class Conta
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [MaxLength(14)]
        public string CPF { get; set; }

        [Required]
        [MaxLength(60)]
        public string Nome { get; set; }
        public double Saldo { get; set; }

    }
}
