using LogiEat.Pedidos.API.Data;
using LogiEat.Pedidos.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LogiEat.Pedidos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmpresasController : ControllerBase
    {
        private readonly PedidosDbContext _db;

        public EmpresasController(PedidosDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Obtiene todas las empresas.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var lista = await _db.Empresas.ToListAsync();
            return Ok(lista);
        }

        /// <summary>
        /// Obtiene una empresa por su ID.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var emp = await _db.Empresas.FindAsync(id);
            if (emp == null) return NotFound();
            return Ok(emp);
        }

        /// <summary>
        /// Crea una nueva empresa.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Empresa empresa)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var exists = await _db.Empresas.AnyAsync(e => e.IdEmpresa == empresa.IdEmpresa);
            if (exists) return Conflict(new { message = "id_empresa ya existe." });

            _db.Empresas.Add(empresa);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = empresa.IdEmpresa }, empresa);
        }

        /// <summary>
        /// Actualiza una empresa existente.
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Empresa empresa)
        {
            if (id != empresa.IdEmpresa)
                return BadRequest(new { message = "Id no coincide." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existente = await _db.Empresas.FindAsync(id);
            if (existente == null) return NotFound();

            existente.NombreEmpresa = empresa.NombreEmpresa;
            existente.Direccion = empresa.Direccion;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Elimina una empresa.
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var emp = await _db.Empresas.FindAsync(id);
            if (emp == null) return NotFound();

            _db.Empresas.Remove(emp);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
