using System.ComponentModel.DataAnnotations;
using System.Globalization;

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


        public override string ToString()
        {
            return "Conta de Numero: "
                + ID 
                + ", Nome: "
                + Nome
                + ", CPF: "
                + CPF
                + " e saldo de R$: "
                + Saldo.ToString("F2")
                + ".";
        }
    }
}
