using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LogiEat.Pedidos.API.Data;
using LogiEat.Pedidos.API.Models;

namespace LogiEat.Pedidos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EstadosPagoController : ControllerBase
    {
        private readonly PedidosDbContext _context;

        public EstadosPagoController(PedidosDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EstadoPago>>> GetEstadosPago()
        {
            return await _context.EstadosPago.ToListAsync();
        }
    }
}
