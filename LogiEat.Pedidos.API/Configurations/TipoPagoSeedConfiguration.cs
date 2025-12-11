using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LogiEat.Pedidos.API.Models;

namespace LogiEat.Pedidos.API.Configurations
{
    public class TipoPagoSeedConfiguration : IEntityTypeConfiguration<TipoPago>
    {
        public void Configure(EntityTypeBuilder<TipoPago> builder)
        {
            builder.HasData(
                new TipoPago { Id = 1, Nombre = "Efectivo" },
                new TipoPago { Id = 2, Nombre = "Tarjeta de crédito" },
                new TipoPago { Id = 3, Nombre = "Tarjeta de débito" },
                new TipoPago { Id = 4, Nombre = "Transferencia bancaria" }
            );
        }
    }
}
