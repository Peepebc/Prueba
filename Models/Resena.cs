using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoGrado.Models
{
    public class Resena
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int Id { get; set; }
        public string? Descripcion { get; set; }

        [ForeignKey(nameof(Descripcion))]
        public int IdPelicula { get; set; }
        public int IdUsuario { get; set; }
        public float Valoracion { get; set; }

        [ForeignKey("IdUsuario")]    
        public Usuario? Usuario { get; set; }

        [ForeignKey("IdPelicula")]
        public Pelicula? Pelicula { get; set; }
    }
}
