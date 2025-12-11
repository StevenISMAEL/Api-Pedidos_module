using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LogiEat.Pedidos.API.Models
{
    [Table("detalles_producto")]
    public class DetallesProducto
    {
        [Key]
        [Column("id_detalle")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdDetalle { get; set; }

        [Required]
        [Column("IdProducto")]
        public int IdProducto { get; set; }

        [Required]
        [Column("id_transaccion")]
        public int IdTransaccion { get; set; }

        [Column("Cantidad")]
        [Range(3, int.MaxValue, ErrorMessage = "La cantidad mínima permitida es 3. (Stock)")]
        public int Cantidad { get; set; }

        [Column("precio", TypeName = "decimal(10,2)")]
        //[Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0.")]
        public decimal Precio { get; set; }

        [Column("fecha")]
        public DateTime Fecha { get; set; }

        [Column("tipo_estado")]
        [Required]
        [StringLength(20)]
        public string TipoEstado { get; set; } = "pedido";

        // Navigation
        [ForeignKey("IdProducto")]
        public virtual Producto? Producto { get; set; }
    }
}
