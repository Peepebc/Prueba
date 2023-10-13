using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoGrado.Models
{
    public class Ver
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int Id { get; set; }
        public int IdPelicula { get; set; }
        public int IdUsuario { get; set; }

        [ForeignKey("IdPelicula")]
        public Pelicula? Pelicula { get; set; }

        [ForeignKey("IdUsuario")]
        public Usuario? Usuario { get; set; }

    }
}
