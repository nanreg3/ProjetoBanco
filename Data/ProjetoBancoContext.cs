using Microsoft.EntityFrameworkCore;
using ProjetoBanco.Models;

namespace ProjetoBanco.Data
{
    public class ProjetoBancoContext : DbContext
    {
        public ProjetoBancoContext (DbContextOptions<ProjetoBancoContext> options)
            : base(options)
        {
        }

        public DbSet<Conta> Conta { get; set; }
    }
}
