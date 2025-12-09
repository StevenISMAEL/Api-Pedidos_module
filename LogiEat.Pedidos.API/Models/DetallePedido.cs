using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LogiEat.Pedidos.API.Models
{
    public class DetallePedido
    {
        [Key]
        public int IdDetalle { get; set; }

        public int IdPedido { get; set; } // La llave foránea

        public int IdProducto { get; set; }

        // --- CORRECCIÓN 1: Usar los nombres EXACTOS de la BD ---
        // Antes tenías "NombreProducto", ahora debe ser "NombreProductoSnapshot"
        public string NombreProductoSnapshot { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal PrecioUnitarioSnapshot { get; set; }
        // -------------------------------------------------------

        public int Cantidad { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Subtotal { get; set; }

        // --- CORRECCIÓN 2: Explicarle a EF cuál es la llave ---
        [JsonIgnore]
        [ForeignKey("IdPedido")] // Le decimos: "Oye, la llave es la propiedad IdPedido de arriba"
        public Pedido? Pedido { get; set; }
    }
}