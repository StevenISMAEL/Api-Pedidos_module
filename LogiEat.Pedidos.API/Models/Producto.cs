using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LogiEat.Pedidos.API.Models
{
    [Table("producto")]
    public class Producto
    {
        [Key]
        [Column("IdProducto")]
        public int IdProducto { get; set; }

        [Required]
        [StringLength(100)]
        [Column("nombre_producto")]
        public string NombreProducto { get; set; } = string.Empty;

        [Column("Cantidad")]
        [Range(3, int.MaxValue, ErrorMessage = "La cantidad mínima permitida es 3.")]
        public int Cantidad { get; set; }

        [Column("precio", TypeName = "decimal(10,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0.")]
        public decimal Precio { get; set; }

        [JsonIgnore]
        public virtual ICollection<DetallesProducto>? DetallesProductos { get; set; }
    }

}
