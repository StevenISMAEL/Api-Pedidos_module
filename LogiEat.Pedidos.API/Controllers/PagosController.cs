using LogiEat.Pedidos.API.Data;
using LogiEat.Pedidos.API.DTOs;
using LogiEat.Pedidos.API.Models;
using LogiEat.Pedidos.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LogiEat.Pedidos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PagosController : ControllerBase
    {
        private readonly IPagoService _pagoService;
        private readonly PedidosDbContext _context;


        public PagosController(PedidosDbContext context, IPagoService pagoService)
        {
            _context = context;
            _pagoService = pagoService;
        }

        // GET: api/pagos
        [HttpGet]
        public async Task<IActionResult> GetPagos()
        {
            var pagos = await _pagoService.ObtenerPagosAsync();
            return Ok(pagos);
        }

        // GET: api/pagos/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetPago(int id)
        {
            var pago = await _pagoService.ObtenerPagoPorIdAsync(id);
            if (pago == null) return NotFound();
            return Ok(pago);
        }

        // POST: api/pagos
        [HttpPost]
        public async Task<IActionResult> CrearPago([FromBody] Pago pago)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Aquí en una arquitectura completa deberías validar PedidoId consultando microservicio de Pedidos

            try
            {
                var nuevoPago = await _pagoService.CrearPagoAsync(pago);
                return CreatedAtAction(nameof(GetPago), new { id = nuevoPago.Id }, nuevoPago);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("registrar")]
        public async Task<IActionResult> RegistrarPago([FromBody] PagoCrearDto dto)
        {
            // 1. Validar que el pedido exista
            var pedidoExiste = await _context.Pedidos
                .AnyAsync(p => p.IdPedido == dto.PedidoId);

            if (!pedidoExiste)
                return BadRequest("El Pedido no existe.");

            // 2. Crear el pago
            var pago = new Pago
            {
                PedidoId = dto.PedidoId,
                Monto = dto.Monto,
                TipoPagoId = dto.TipoPagoId,
                EstadoPagoId = 1, // Pendiente
                FechaPago = DateTime.Now
            };

            // 3. Guardar en BD
            _context.Pagos.Add(pago);
            await _context.SaveChangesAsync();

            // 4. Devolver respuesta
            return Ok(pago);
        }


        [HttpGet("pedido/{pedidoId}")]
        public async Task<IActionResult> GetPagosPorPedido(int pedidoId)
        {
            var pagos = await _pagoService.ObtenerPagosPorPedidoAsync(pedidoId);
            return Ok(pagos);
        }




        // PUT: api/pagos/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> ActualizarPago(int id, [FromBody] Pago pago)
        {
            if (id != pago.Id)
                return BadRequest("El ID del pago no coincide.");

            var resultado = await _pagoService.ActualizarPagoAsync(pago);
            if (!resultado) return NotFound();

            return NoContent();
        }

        // DELETE: api/pagos/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> EliminarPago(int id)
        {
            var resultado = await _pagoService.EliminarPagoAsync(id);
            if (!resultado) return NotFound();

            return NoContent();
        }
    }
}
