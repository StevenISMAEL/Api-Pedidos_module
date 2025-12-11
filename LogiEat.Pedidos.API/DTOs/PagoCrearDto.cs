namespace LogiEat.Pedidos.API.DTOs
{
    public class PagoCrearDto
    {
        public int PedidoId { get; set; }
        public decimal Monto { get; set; }
        public int TipoPagoId { get; set; }
        public int EstadoPagoId { get; set; }
    }
}
