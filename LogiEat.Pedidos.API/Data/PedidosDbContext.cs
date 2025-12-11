using LogiEat.Pedidos.API.Models;
using Microsoft.EntityFrameworkCore;
using LogiEat.Pedidos.API.Configurations; // Si tienes configs separadas

namespace LogiEat.Pedidos.API.Data
{
    public class PedidosDbContext : DbContext
    {
        public PedidosDbContext(DbContextOptions<PedidosDbContext> options) : base(options) { }

        // --- TU API DE PEDIDOS ---
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<DetallePedido> DetallesPedido { get; set; }

        // --- PAGOS ---
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<TipoPago> TiposPago { get; set; }
        public DbSet<EstadoPago> EstadosPago { get; set; }

        // --- INVENTARIO (AHORA LOCAL) ---
        public DbSet<Producto> Productos { get; set; }
        // Asegúrate de que este nombre coincida con la clase del archivo Models/DetallesProducto.cs
        public DbSet<DetallesProducto> DetallesProductos { get; set; }
        public DbSet<Empresa> Empresas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuraciones de Pago
            modelBuilder.Entity<Pago>().Property(p => p.Monto).HasPrecision(18, 2);
            modelBuilder.Entity<Pago>().HasOne(p => p.TipoPago).WithMany(tp => tp.Pagos).HasForeignKey(p => p.TipoPagoId);
            modelBuilder.Entity<Pago>().HasOne(p => p.Pedido).WithMany().HasForeignKey(p => p.PedidoId);
            modelBuilder.Entity<Pago>().HasOne(p => p.EstadoPago).WithMany(ep => ep.Pagos).HasForeignKey(p => p.EstadoPagoId);

            // Mapeo de Tablas de Inventario
            modelBuilder.Entity<Producto>().ToTable("producto");
            modelBuilder.Entity<Empresa>().ToTable("empresa");

            // ✅ CORRECCIÓN CRÍTICA AQUÍ:
            // Configuramos la tabla, el trigger Y el check constraint en el mismo bloque.
            modelBuilder.Entity<DetallesProducto>()
                .ToTable("detalles_producto", table =>
                {
                    // 1. Avisamos a EF Core que hay un Trigger (Esto arregla el error del OUTPUT)
                    table.HasTrigger("TR_ActualizarStock_Producto");

                    // 2. Definimos el Check Constraint (Esto valida los datos a nivel de BD)
                    table.HasCheckConstraint("CK_DetallesProducto_TipoEstado", "tipo_estado IN ('pedido','compra')");
                });

            // Relación 1-N: Producto -> DetallesProducto
            modelBuilder.Entity<DetallesProducto>()
                .HasOne(d => d.Producto)
                .WithMany(p => p.DetallesProductos) // Asegúrate que la clase Producto tenga esta lista
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}