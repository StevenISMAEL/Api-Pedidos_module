using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LogiEat.Pedidos.API.Models;

namespace LogiEat.Pedidos.API.Configurations
{
    public class EstadoPagoSeedConfiguration : IEntityTypeConfiguration<EstadoPago>
    {
        public void Configure(EntityTypeBuilder<EstadoPago> builder)
        {
            builder.HasData(
                new EstadoPago { Id = 1, Estado = "Pendiente" },
                new EstadoPago { Id = 2, Estado = "Completado" },
                new EstadoPago { Id = 3, Estado = "Fallido" }
            );
        }
    }
}
