using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LogiEat.Pedidos.API.Data;
using LogiEat.Pedidos.API.Models;

namespace LogiEat.Pedidos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TiposPagoController : ControllerBase
    {
        private readonly PedidosDbContext _context;

        public TiposPagoController(PedidosDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TipoPago>>> GetTiposPago()
        {
            return await _context.TiposPago.ToListAsync();

        }

    }
}
