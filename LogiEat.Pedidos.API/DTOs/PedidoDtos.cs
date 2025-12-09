namespace LogiEat.Pedidos.API.DTOs
{
    public class CrearPedidoDto
    {
        // NO pedimos IdUsuarioCliente aquí.
        // Lo obtenemos del Token en el Controller.
        public List<ProductoItemDto> Productos { get; set; }
    }

    public class ProductoItemDto
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; }
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }
    }
}