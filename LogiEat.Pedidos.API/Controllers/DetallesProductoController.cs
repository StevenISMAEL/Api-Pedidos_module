using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LogiEat.Pedidos.API.Data;
using LogiEat.Pedidos.API.Models;

namespace LogiEat.Pedidos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DetallesProductoController : ControllerBase
    {
        private readonly PedidosDbContext _db;

        public DetallesProductoController(PedidosDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Lista todos los movimientos (compras y pedidos).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var lista = await _db.DetallesProductos
                .Include(d => d.Producto)
                .ToListAsync();

            return Ok(lista);
        }

        /// <summary>
        /// Obtiene un movimiento específico por id_detalle.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var detalle = await _db.DetallesProductos
                .Include(d => d.Producto)
                .FirstOrDefaultAsync(d => d.IdDetalle == id);

            if (detalle == null)
                return NotFound();

            return Ok(detalle);
        }

        /// <summary>
        /// Crea un detalle de producto (compra o pedido).
        /// TipoEstado = "compra" suma stock, "pedido" descuenta stock.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DetallesProducto detalle)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var producto = await _db.Productos.FindAsync(detalle.IdProducto);
            if (producto == null)
                return BadRequest(new { message = "Producto no encontrado." });

            if (detalle.Cantidad < 3)
                return BadRequest(new { message = "La cantidad mínima permitida es 3." });

            // Usamos el precio actual del producto
            detalle.Precio = producto.Precio;

            // Si no mandan fecha desde afuera, puedes forzarla a hoy:
            if (detalle.Fecha == default)
                detalle.Fecha = DateTime.Today;

            using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                if (detalle.TipoEstado.Equals("compra", StringComparison.OrdinalIgnoreCase))
                {
                    // COMPRA → ingresa stock
                    producto.Cantidad += detalle.Cantidad;
                }
                else if (detalle.TipoEstado.Equals("pedido", StringComparison.OrdinalIgnoreCase))
                {
                    // PEDIDO → egresa stock
                    if (producto.Cantidad < detalle.Cantidad)
                    {
                        return Conflict(new
                        {
                            message = "Stock insuficiente para realizar el pedido.",
                            stockActual = producto.Cantidad
                        });
                    }

                    producto.Cantidad -= detalle.Cantidad;
                }
                else
                {
                    return BadRequest(new { message = "tipo_estado inválido. Use 'pedido' o 'compra'." });
                }

                _db.Productos.Update(producto);
                _db.DetallesProductos.Add(detalle);

                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                return CreatedAtAction(nameof(Get), new { id = detalle.IdDetalle }, detalle);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return StatusCode(500, new { message = "Error al registrar detalle.", detail = ex.Message });
            }
        }

        /// <summary>
        /// Elimina un detalle (no ajusta stock, se puede mejorar luego).
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var detalle = await _db.DetallesProductos.FindAsync(id);
            if (detalle == null)
                return NotFound();

            _db.DetallesProductos.Remove(detalle);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
