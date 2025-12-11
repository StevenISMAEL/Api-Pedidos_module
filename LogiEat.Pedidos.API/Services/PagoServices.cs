using Microsoft.EntityFrameworkCore;
using LogiEat.Pedidos.API.Data;
using LogiEat.Pedidos.API.Models;
using LogiEat.Pedidos.API.Models.DTOs;
using System.Net.Http;

namespace LogiEat.Pedidos.API.Services
{
    public class PagoServices : IPagoService
    {
        private readonly PedidosDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public PagoServices(PedidosDbContext context, HttpClient httpClient, IConfiguration config)
        {
            _context = context;
            _httpClient = httpClient;
            _config = config;
        }


        public async Task<Pago> CrearPagoAsync(Pago pago, string token)
        {
            if (pago.Monto <= 0)
                throw new Exception("El monto debe ser mayor a 0.");

            if (!await _context.TiposPago.AnyAsync(t => t.Id == pago.TipoPagoId))
                throw new Exception($"TipoPagoId {pago.TipoPagoId} no existe.");

            if (!await _context.EstadosPago.AnyAsync(e => e.Id == pago.EstadoPagoId))
                throw new Exception($"EstadoPagoId {pago.EstadoPagoId} no existe.");

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Validación HTTP de PedidoId
            var baseUrl = _config["PedidosService:BaseUrl"];

            var pedidoResponse = await _httpClient.GetAsync($"{baseUrl}/api/pedidos/{pago.PedidoId}");

            if (!pedidoResponse.IsSuccessStatusCode)
                throw new Exception($"PedidoId {pago.PedidoId} no existe en el microservicio de Pedidos.");

            _context.Pagos.Add(pago);
            await _context.SaveChangesAsync();
            return pago;
        }




        public async Task<Pago?> ObtenerPagoPorIdAsync(int id)
        {
            return await _context.Pagos
                .Include(p => p.TipoPago)
                .Include(p => p.EstadoPago)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<PagoDto>> ObtenerPagosAsync()
        {
            var pagos = await _context.Pagos
                .Include(p => p.TipoPago)
                .Include(p => p.EstadoPago)
                .ToListAsync();

            return pagos.Select(MapToDto);
        }


        public async Task<bool> ActualizarPagoAsync(Pago pago)
        {
            _context.Entry(pago).State = EntityState.Modified;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> EliminarPagoAsync(int id)
        {
            var pago = await _context.Pagos.FindAsync(id);
            if (pago == null) return false;

            _context.Pagos.Remove(pago);
            return await _context.SaveChangesAsync() > 0;
        }

        private PagoDto MapToDto(Pago pago)
        {
            return new PagoDto
            {
                Id = pago.Id,
                PedidoId = pago.PedidoId,
                Monto = pago.Monto,
                FechaPago = pago.FechaPago,
                TipoPago = pago.TipoPago,           // ✅ objeto completo
                EstadoPago = pago.EstadoPago        // ✅ objeto completo
            };
        }


        public async Task<IEnumerable<PagoDto>> ObtenerPagosPorPedidoAsync(int pedidoId)
        {
            var pagos = await _context.Pagos
                .Where(p => p.PedidoId == pedidoId)
                .Include(p => p.TipoPago)
                .Include(p => p.EstadoPago)
                .ToListAsync();

            return pagos.Select(MapToDto);
        }


    }
}
