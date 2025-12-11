namespace LogiEat.Pedidos.API.Models
{
    public class Pago
    {
        public int Id { get; set; }

        // Relación con Pedido (FK)
        public int PedidoId { get; set; }

        public decimal Monto { get; set; }

        public int TipoPagoId { get; set; }

        public int EstadoPagoId { get; set; }

        public DateTime FechaPago { get; set; } = DateTime.UtcNow;

        // Relaciones
        public Pedido? Pedido { get; set; }
        public TipoPago? TipoPago { get; set; }
        public EstadoPago? EstadoPago { get; set; }
    }
}
