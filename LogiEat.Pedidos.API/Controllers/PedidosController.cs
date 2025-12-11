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
    [Authorize]
    public class PedidosController : ControllerBase
    {
        private readonly PedidosDbContext _context;

        // YA NO necesitamos HttpClientFactory porque no vamos a llamar a APIs externas para el stock.
        public PedidosController(PedidosDbContext context)
        {
            _context = context;
        }

        [HttpPost("Crear")]
        public async Task<IActionResult> CrearPedido([FromBody] CrearPedidoDto dto)
        {
            if (dto.Productos == null || !dto.Productos.Any())
                return BadRequest("El carrito no puede estar vacío.");

            var idUsuario = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idUsuario)) return Unauthorized();

            // INICIO DE TRANSACCIÓN: Todo o nada.
            // Si falla el stock, no se crea el pedido. Si falla el pedido, no se descuenta stock.
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. VALIDACIÓN DE STOCK (Lectura Local Directa)
                // Ahora miramos tu tabla 'producto' directamente.
                foreach (var item in dto.Productos)
                {
                    var productoEnDb = await _context.Productos.FindAsync(item.IdProducto);

                    if (productoEnDb == null)
                        throw new Exception($"El producto '{item.Nombre}' (ID: {item.IdProducto}) no existe en el inventario.");

                    if (productoEnDb.Cantidad < item.Cantidad)
                        throw new Exception($"Stock insuficiente para '{item.Nombre}'. Disponible: {productoEnDb.Cantidad}, Solicitado: {item.Cantidad}.");
                }

                // 2. CREAR EL PEDIDO (Cabecera)
                var nuevoPedido = new Pedido
                {
                    IdUsuarioCliente = idUsuario,
                    FechaPedido = DateTime.Now,
                    Estado = "PENDIENTE_PAGO",
                    Detalles = new List<DetallePedido>()
                };

                decimal totalCalculado = 0;

                // 3. PROCESAR DETALLES Y ACTUALIZAR INVENTARIO
                foreach (var item in dto.Productos)
                {
                    var subtotal = item.Precio * item.Cantidad;
                    totalCalculado += subtotal;

                    // A. Agregar a la lista de detalles del PEDIDO (Para que salga en el recibo)
                    nuevoPedido.Detalles.Add(new DetallePedido
                    {
                        IdProducto = item.IdProducto,
                        NombreProductoSnapshot = item.Nombre,
                        PrecioUnitarioSnapshot = item.Precio,
                        Cantidad = item.Cantidad,
                        Subtotal = subtotal
                    });

                    // B. INSERTAR EN TABLA DE MOVIMIENTOS (Para activar el TRIGGER)
                    // Esto reemplaza la llamada a la API del compañero.
                    // Al insertar aquí con 'pedido', el trigger SQL restará el stock automáticamente.
                    var movimientoInventario = new DetallesProducto
                    {
                        IdProducto = item.IdProducto,
                        Cantidad = item.Cantidad,
                        TipoEstado = "pedido", // CLAVE: Esto dispara el Trigger de resta
                        Precio = item.Precio,
                        Fecha = DateTime.Now,
                        // IdTransaccion = nuevoPedido.IdPedido (Opcional, si tienes el campo y quieres vincularlo)
                    };

                    _context.DetallesProductos.Add(movimientoInventario);
                }

                nuevoPedido.Total = totalCalculado;

                // Guardamos el Pedido (y sus DetallePedido por cascada)
                _context.Pedidos.Add(nuevoPedido);

                // Guardamos los cambios (incluyendo los insert en DetallesProductos que actualizan stock)
                await _context.SaveChangesAsync();

                // Confirmamos la transacción
                await transaction.CommitAsync();

                return Ok(new
                {
                    mensaje = "Pedido creado exitosamente",
                    idPedido = nuevoPedido.IdPedido,
                    total = totalCalculado,
                    statusInventario = "ACTUALIZADO_AUTOMATICAMENTE"
                });
            }
            catch (Exception ex)
            {
                // Si algo falla (ej. sin stock), deshacemos todo.
                await transaction.RollbackAsync();
                return BadRequest(new { error = ex.Message });
            }
        }

        // ... RESTO DE MÉTODOS (Get, Put) SE MANTIENEN IGUAL ...
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