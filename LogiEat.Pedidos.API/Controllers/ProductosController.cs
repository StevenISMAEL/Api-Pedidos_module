using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LogiEat.Pedidos.API.Data;
using LogiEat.Pedidos.API.Models;

namespace LogiEat.Pedidos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly PedidosDbContext _db;
        public ProductosController(PedidosDbContext db) => _db = db;

        // GET: api/Productos
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var productos = await _db.Productos.ToListAsync();
            return Ok(productos);
        }

        // GET: api/Productos/101
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var prod = await _db.Productos.FindAsync(id);
            if (prod == null) return NotFound();
            return Ok(prod);
        }

        // POST: api/Productos
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Producto producto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var exists = await _db.Productos.AnyAsync(p => p.IdProducto == producto.IdProducto);
            if (exists)
                return Conflict(new { message = "IdProducto ya existe." });

            _db.Productos.Add(producto);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = producto.IdProducto }, producto);
        }

        // PUT: api/Productos/101
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Producto producto)
        {
            if (id != producto.IdProducto)
                return BadRequest(new { message = "Id no coincide." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existente = await _db.Productos.FindAsync(id);
            if (existente == null)
                return NotFound();

            existente.NombreProducto = producto.NombreProducto;
            existente.Cantidad = producto.Cantidad;
            existente.Precio = producto.Precio;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Productos/101
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var prod = await _db.Productos.FindAsync(id);
            if (prod == null)
                return NotFound();

            _db.Productos.Remove(prod);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
