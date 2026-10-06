using DimDim.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace DimDim.Web.Data;

// O esquema é criado pelo DDL em scripts/ddl.sql; aqui só fazemos o mapeamento.
public class DimDimContext : DbContext
{
    public DimDimContext(DbContextOptions<DimDimContext> options) : base(options) { }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Conta> Contas => Set<Conta>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>(e =>
        {
            e.ToTable("Clientes", "dbo");
            e.HasKey(c => c.Id);
            e.Property(c => c.Nome).HasMaxLength(100).IsRequired();
            e.Property(c => c.Cpf).HasColumnType("char(11)").IsRequired();
            e.HasIndex(c => c.Cpf).IsUnique();
            e.Property(c => c.Email).HasMaxLength(150).IsRequired();
            e.Property(c => c.Telefone).HasMaxLength(20);
            e.Property(c => c.DataCadastro).HasColumnType("datetime2");
        });

        modelBuilder.Entity<Conta>(e =>
        {
            e.ToTable("Contas", "dbo");
            e.HasKey(c => c.Id);
            e.Property(c => c.Numero).HasMaxLength(20).IsRequired();
            e.HasIndex(c => c.Numero).IsUnique();
            e.Property(c => c.Agencia).HasMaxLength(10).IsRequired();
            e.Property(c => c.Tipo).HasMaxLength(20).IsRequired();
            e.Property(c => c.Saldo).HasPrecision(18, 2);
            e.Property(c => c.DataAbertura).HasColumnType("datetime2");

            e.HasOne(c => c.Cliente)
             .WithMany(cl => cl.Contas)
             .HasForeignKey(c => c.ClienteId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
