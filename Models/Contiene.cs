using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoGrado.Models
{
    public class Contiene
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int Id { get; set; }
        public int IdPelicula { get; set; }
        public int IdLista { get; set; }

        [ForeignKey("IdLista")]
        public Lista? Lista { get; set; }

        [ForeignKey("IdPelicula")]
        public Pelicula? Pelicula { get; set; }
    }
}
