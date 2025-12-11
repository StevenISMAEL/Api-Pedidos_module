using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LogiEat.Pedidos.API.Models
{
    // ¡Adiós al atributo [Table]! Ya no hace falta.
    public class Pedido
    {
        [Key]
        public int IdPedido { get; set; }
        public string IdUsuarioCliente { get; set; } = string.Empty;
        public DateTime FechaPedido { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Total { get; set; }

        public string Estado { get; set; } = "PENDIENTE_PAGO";

        // Campos opcionales
        public string? IdUsuarioAdminAprobador { get; set; }
        public DateTime? FechaAprobacion { get; set; }
        public string? IdTransaccionPago { get; set; }

        public List<DetallePedido> Detalles { get; set; } = new();
    }
}