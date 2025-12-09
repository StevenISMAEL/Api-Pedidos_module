using LogiEat.Pedidos.API.Models;
using Microsoft.EntityFrameworkCore;

namespace LogiEat.Pedidos.API.Data
{
    public class PedidosDbContext : DbContext
    {
        public PedidosDbContext(DbContextOptions<PedidosDbContext> options) : base(options) { }

        // MÁGIA DE EF CORE:
        // Al llamar a la propiedad "Pedidos", EF buscará la tabla "Pedidos" automáticamente.
        public DbSet<Pedido> Pedidos { get; set; }

        // Al llamar a la propiedad "DetallesPedido", EF buscará la tabla "DetallesPedido".
        public DbSet<DetallePedido> DetallesPedido { get; set; }
    }
}