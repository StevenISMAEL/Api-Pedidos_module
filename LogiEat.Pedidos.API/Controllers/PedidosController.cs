using LogiEat.Pedidos.API.Data;
using LogiEat.Pedidos.API.DTOs;
using LogiEat.Pedidos.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LogiEat.Pedidos.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Candado general: Nadie entra sin Token
    public class PedidosController : ControllerBase
    {
        private readonly PedidosDbContext _context;

        public PedidosController(PedidosDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // 1. CREAR PEDIDO (CLIENTE)
        // ============================================================
        [HttpPost("Crear")]
        public async Task<IActionResult> CrearPedido([FromBody] CrearPedidoDto dto)
        {
            if (dto.Productos == null || !dto.Productos.Any())
                return BadRequest("El carrito no puede estar vacío.");

            // A. Identificar al usuario desde el Token
            var idUsuario = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idUsuario)) return Unauthorized();

            // B. Crear Cabecera
            var nuevoPedido = new Pedido
            {
                IdUsuarioCliente = idUsuario,
                FechaPedido = DateTime.Now,
                Estado = "PENDIENTE_PAGO",
                Detalles = new List<DetallePedido>()
            };

            decimal totalCalculado = 0;

            // C. Crear Detalles (Con nombres snapshot correctos)
            foreach (var item in dto.Productos)
            {
                var subtotal = item.Precio * item.Cantidad;
                totalCalculado += subtotal;

                nuevoPedido.Detalles.Add(new DetallePedido
                {
                    IdProducto = item.IdProducto,
                    NombreProductoSnapshot = item.Nombre, // Clave: Guardamos el nombre histórico
                    PrecioUnitarioSnapshot = item.Precio, // Clave: Guardamos el precio histórico
                    Cantidad = item.Cantidad,
                    Subtotal = subtotal
                });
            }

            nuevoPedido.Total = totalCalculado;

            // D. Guardar todo en una transacción
            _context.Pedidos.Add(nuevoPedido);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Pedido creado", idPedido = nuevoPedido.IdPedido, total = totalCalculado });
        }

        // ============================================================
        // 2. VER MIS PEDIDOS (CLIENTE)
        // ============================================================
        [HttpGet("MisPedidos")]
        public async Task<ActionResult> MisPedidos()
        {
            var idUsuario = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var lista = await _context.Pedidos
                                .Include(p => p.Detalles) // Traer los platos también
                                .Where(p => p.IdUsuarioCliente == idUsuario)
                                .OrderByDescending(p => p.FechaPedido)
                                .ToListAsync();
            return Ok(lista);
        }

        // ============================================================
        // 3. VER PENDIENTES (ADMINISTRADOR)
        // ============================================================
        [Authorize(Roles = "Admin")] // Solo el Admin puede ver esto
        [HttpGet("Pendientes")]
        public async Task<ActionResult> Pendientes()
        {
            var lista = await _context.Pedidos
                                .Include(p => p.Detalles)
                                .Where(p => p.Estado != "ENTREGADO" && p.Estado != "RECHAZADO") // Vemos todo lo activo
                                .OrderByDescending(p => p.FechaPedido)
                                .ToListAsync();
            return Ok(lista);
        }

        // ============================================================
        // 4. CAMBIAR ESTADO (ADMINISTRADOR) - ¡VITAL PARA TU FLUJO!
        // ============================================================
        [Authorize(Roles = "Admin")]
        [HttpPut("CambiarEstado/{id}")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] string nuevoEstado)
        {
            // Validamos que sea un estado permitido para no romper la lógica
            var estadosValidos = new[] { "PAGADO", "APROBADO", "EN_CAMINO", "ENTREGADO", "RECHAZADO" };
            if (!estadosValidos.Contains(nuevoEstado.ToUpper()))
                return BadRequest($"Estado no válido. Use: {string.Join(", ", estadosValidos)}");

            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null) return NotFound("Pedido no encontrado");

            // Actualizamos estado
            pedido.Estado = nuevoEstado.ToUpper();

            // Auditoría básica: Quién lo aprobó
            var idAdmin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            pedido.IdUsuarioAdminAprobador = idAdmin;
            pedido.FechaAprobacion = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = $"Pedido #{id} actualizado a {nuevoEstado}" });
        }
    }
}