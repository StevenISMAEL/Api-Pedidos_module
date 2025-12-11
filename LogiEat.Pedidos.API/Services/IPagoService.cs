using LogiEat.Pedidos.API.Models;
using LogiEat.Pedidos.API.Models.DTOs;

namespace LogiEat.Pedidos.API.Services
{
    public interface IPagoService
    {
        Task<IEnumerable<PagoDto>> ObtenerPagosAsync();

        Task<Pago?> ObtenerPagoPorIdAsync(int id);
        Task<Pago> CrearPagoAsync(Pago pago, string token);
        Task<bool> ActualizarPagoAsync(Pago pago);
        Task<bool> EliminarPagoAsync(int id);

        Task<IEnumerable<PagoDto>> ObtenerPagosPorPedidoAsync(int pedidoId);

    }
}
