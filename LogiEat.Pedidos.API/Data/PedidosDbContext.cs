using LogiEat.Pedidos.API.Models;
using Microsoft.EntityFrameworkCore;
using LogiEat.Pedidos.API.Configurations;

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
        // 💳 Pagos
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<TipoPago> TiposPago { get; set; }
        public DbSet<EstadoPago> EstadosPago { get; set; }
        // Inventario
        public DbSet<Producto> Productos { get; set; }
        public DbSet<DetallesProducto> DetallesProductos { get; set; }
        public DbSet<Empresa> Empresas { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 💳 Configuración de Pago
            modelBuilder.Entity<Pago>()
                .Property(p => p.Monto)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Pago>()
                .HasOne(p => p.TipoPago)
                .WithMany(tp => tp.Pagos)
                .HasForeignKey(p => p.TipoPagoId);

            // ✅ Relación pago -> pedido
            modelBuilder.Entity<Pago>()
                .HasOne(p => p.Pedido)
                .WithMany() 
                .HasForeignKey(p => p.PedidoId);


            modelBuilder.Entity<Pago>()
                .HasOne(p => p.EstadoPago)
                .WithMany(ep => ep.Pagos)
                .HasForeignKey(p => p.EstadoPagoId);

            // ✅ Tablas personalizadas
            modelBuilder.Entity<Producto>().ToTable("producto");
            modelBuilder.Entity<DetallesProducto>().ToTable("detalles_producto");
            modelBuilder.Entity<Empresa>().ToTable("empresa");

            // ✅ Relación 1-N: Producto -> DetallesProducto
            modelBuilder.Entity<DetallesProducto>()
                .HasOne(d => d.Producto)
                .WithMany(p => p.DetallesProductos)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ CHECK constraint opcional
            modelBuilder.Entity<DetallesProducto>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_DetallesProducto_TipoEstado", 
                    "tipo_estado IN ('pedido','compra')"));



            // 🌱 Seeding
            modelBuilder.ApplyConfiguration(new TipoPagoSeedConfiguration());
            modelBuilder.ApplyConfiguration(new EstadoPagoSeedConfiguration());
        }
    }
}