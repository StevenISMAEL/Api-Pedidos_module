namespace SistemaPedidos.Pagos.API.Models
{
    public class EstadoPago
    {
        public int Id { get; set; }

        public string Estado { get; set; } = string.Empty;

        public ICollection<Pago>? Pagos { get; set; }
    }
}
