using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoGrado.Models
{
    public class Pelicula
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? Titulo { get; set; }
        public DateTime? Fecha { get; set; }
        public string? Director { get; set; }
        public string? Descripcion { get; set; }
        public string? Imagen { get; set; }
        public string? Generos { get; set; }
        public float? Puntuacion { get; set; }

        //Relaciones 
        public List<Ver>? Vistas { get; set; }
        public List<Fav>? Favoritos { get; set; }
        public List<Resena>? Resenas { get; set; }
        public List<Contiene>? Listas { get; set; }
    }
}
