using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;


namespace ProyectoGrado.Models
{
    public class Usuario
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int Id { get; set; }
        public string? User { get; set; }
        public string? PasswordHash { get; set; }
        public string? PasswordSalt { get; set; }
        public string? Email { get; set; }
        public string? Nombre { get; set; }
        public string? Apellidos { get; set; }
        public DateTime FechaNac { get; set; }
        public string? Imagen { get; set; }
        public int Rol { get; set; }
        public long ResetPassword { get; set; }


        //Relaciones 

        public ICollection<Ver>? PeliculasVistas { get; set; }
        public ICollection<Fav>? PeliculasFavoritas { get; set; }
        public ICollection<Resena>? Resenas { get; set; }
        public ICollection<Lista>? Listas { get; set; }
    }
}
