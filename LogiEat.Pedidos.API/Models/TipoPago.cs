namespace LogiEat.Pedidos.API.Models
{
    public class TipoPago
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public ICollection<Pago>? Pagos { get; set; }
    }
}
