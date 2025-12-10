namespace SistemaPedidos.Pagos.API.Models.DTOs
{
    public class PagoDto
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public decimal Monto { get; set; }
        public string TipoPago { get; set; }
        public string EstadoPago { get; set; }
        public DateTime FechaPago { get; set; }
    }
}
