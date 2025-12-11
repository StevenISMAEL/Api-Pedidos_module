using LogiEat.Pedidos.API.Data;
using LogiEat.Pedidos.API.DTOs;
using LogiEat.Pedidos.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace LogiEat.Pedidos.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PedidosController : ControllerBase
    {
        private readonly PedidosDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        // ⚠️ URL DEL POST DE TU COMPAÑERO (Ajustada a la ruta de Detalles)
        private const string UrlApiPartnerPost = "https://app-inventario.azurewebsites.net/api/DetallesProducto";

        public PedidosController(PedidosDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost("Crear")]
        public async Task<IActionResult> CrearPedido([FromBody] CrearPedidoDto dto)
        {
            if (dto.Productos == null || !dto.Productos.Any())
                return BadRequest("El carrito no puede estar vacío.");

            var idUsuario = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idUsuario)) return Unauthorized();

            // 1. CREAR EL PEDIDO EN TU BASE DE DATOS
            var nuevoPedido = new Pedido
            {
                IdUsuarioCliente = idUsuario,
                FechaPedido = DateTime.Now,
                Estado = "PENDIENTE_PAGO",
                Detalles = new List<DetallePedido>()
            };

            decimal totalCalculado = 0;

            foreach (var item in dto.Productos)
            {
                var subtotal = item.Precio * item.Cantidad;
                totalCalculado += subtotal;

                nuevoPedido.Detalles.Add(new DetallePedido
                {
                    IdProducto = item.IdProducto,
                    NombreProductoSnapshot = item.Nombre,
                    PrecioUnitarioSnapshot = item.Precio,
                    Cantidad = item.Cantidad,
                    Subtotal = subtotal
                });
            }

            nuevoPedido.Total = totalCalculado;

            _context.Pedidos.Add(nuevoPedido);
            await _context.SaveChangesAsync(); // Se genera el ID del Pedido

            // 2. ENVIAR REPORTE A LA API DEL COMPAÑERO (JSON LIMPIO)
            var clienteHttp = _httpClientFactory.CreateClient();

            // Lista para guardar errores y mostrártelos en Swagger
            var erroresDeInventario = new List<string>();

            foreach (var item in dto.Productos)
            {
                // AQUÍ ESTÁ EL TRUCO: Creamos un objeto anónimo SOLO con los campos planos.
                // Ignoramos el objeto "producto" anidado que muestra Swagger.
                var reporteVenta = new
                {
                    idProducto = item.IdProducto,
                    cantidad = item.Cantidad,
                    precio = item.Precio,       // Él pide precio en su tabla

                    // CORRECCIÓN: Cambiado de "SALIDA" a "pedido" según el error de tu compañero
                    nombreProducto = item.Nombre,
                    tipoEstado = "pedido",

                    // Fechas (las ponemos nosotros por si su API no las genera sola)
                    fechaIngreso = DateTime.Now,
                    fechaEgreso = DateTime.Now,

                    // (Opcional) Si él tiene un campo para referencia, úsalo. Si no, omítelo.
                    // idTransaccion = nuevoPedido.IdPedido 
                };

                try
                {
                    var jsonContent = JsonSerializer.Serialize(reporteVenta);
                    var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                    // Enviamos el POST
                    var responsePost = await clienteHttp.PostAsync(UrlApiPartnerPost, content);

                    if (!responsePost.IsSuccessStatusCode)
                    {
                        var errorMsg = await responsePost.Content.ReadAsStringAsync();
                        // Guardamos el error para que tú lo veas
                        erroresDeInventario.Add($"Error Prod {item.IdProducto}: {responsePost.StatusCode} - {errorMsg}");
                        Console.WriteLine($"[WARNING] Falló reporte a inventario ({responsePost.StatusCode}): {errorMsg}");
                    }
                }
                catch (Exception ex)
                {
                    erroresDeInventario.Add($"Excepción Prod {item.IdProducto}: {ex.Message}");
                    Console.WriteLine($"[ERROR] No se pudo conectar con inventario: {ex.Message}");
                }
            }

            // Retornamos el resultado incluyendo los errores de inventario si hubo
            return Ok(new
            {
                mensaje = "Pedido creado exitosamente",
                idPedido = nuevoPedido.IdPedido,
                total = totalCalculado,
                statusInventario = erroresDeInventario.Any() ? "CON ERRORES" : "EXITOSO",
                detallesErrorInventario = erroresDeInventario
            });
        }

        // ... MÉTODOS GET Y PUT (Iguales que antes, no cambian) ...
        [HttpGet("MisPedidos")]
        public async Task<ActionResult> MisPedidos()
        {
            var idUsuario = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Ok(await _context.Pedidos.Include(p => p.Detalles).Where(p => p.IdUsuarioCliente == idUsuario).OrderByDescending(p => p.FechaPedido).ToListAsync());
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("Pendientes")]
        public async Task<ActionResult> Pendientes()
        {
            return Ok(await _context.Pedidos.Include(p => p.Detalles).Where(p => p.Estado != "ENTREGADO" && p.Estado != "RECHAZADO").OrderByDescending(p => p.FechaPedido).ToListAsync());
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("CambiarEstado/{id}")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] string nuevoEstado)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null) return NotFound();
            pedido.Estado = nuevoEstado.ToUpper();
            pedido.IdUsuarioAdminAprobador = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            pedido.FechaAprobacion = DateTime.Now;
            await _context.SaveChangesAsync();
            return Ok(new { mensaje = "Estado actualizado" });
        }

        [HttpPut("Pagar/{id}")]
        public async Task<IActionResult> PagarPedido(int id)
        {
            var idUsuario = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null) return NotFound();
            if (pedido.IdUsuarioCliente != idUsuario) return Forbid();
            pedido.Estado = "PAGADO";
            pedido.IdTransaccionPago = Guid.NewGuid().ToString();
            await _context.SaveChangesAsync();
            return Ok(new { mensaje = "Pagado" });
        }
    }
}