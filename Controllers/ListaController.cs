using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoGrado.Models;
using System.Security.Claims;

namespace ProyectoGrado.Controllers
{

    [Route("Listas")]
    [ApiController]
    public class ListaController : ControllerBase
    {

        private readonly PeliculasContext _peliculasContext;

        public ListaController(PeliculasContext peliculasContext)
        {
            _peliculasContext = peliculasContext;
        }

        [HttpPost]
        [Route("CrearLista")]
        [Authorize]

        public dynamic CrearLista([FromBody] string nombre)
        {

            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            Lista l = new Lista();
            l.Nombre = nombre; 
            l.IdUsuario = userId;
            _peliculasContext.Listas.Add(l);
            _peliculasContext.SaveChanges();
            return new { success = true };

        }

        [HttpDelete]
        [Route("EliminarLista/{id}")]
        [Authorize]

        public dynamic EliminarLista(int id)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var lista = _peliculasContext.Listas.Where(e => e.Id == id && e.IdUsuario == userId).FirstOrDefault();
            if (lista == null) return "Error";
            _peliculasContext.Remove<Lista>(lista);
            _peliculasContext.SaveChanges();
            return new { success = true };
        }

        [HttpGet]
        [Route("UltimasListasUsuario/{id}")]
        public  dynamic UltimasListasUsuario(int id)
        {
            return  _peliculasContext.Listas.Where(x => x.IdUsuario == id).OrderByDescending(r => r.Id).Select(l=>new { l.Id, user = l.IdUsuario, l.Nombre,l.Peliculas.Count,poster=l.Peliculas.Select(l=>l.Pelicula.Imagen).Take(3)}).Take(4).ToList();
        }

        [HttpGet]
        [Route("UltimasListas")]
        public dynamic UltimasListas()
        {
            return _peliculasContext.Listas.Where(l=>l.Peliculas.Count()>0).OrderByDescending(r => r.Id).Select(l => new { l.Id, user=l.IdUsuario, l.Nombre, l.Peliculas.Count, poster = l.Peliculas.Select(l => l.Pelicula.Imagen).Take(3) }).Take(4).ToList();
        }

        [HttpGet]
        [Route("TodasListas")]
        public dynamic TodasListas()
        {
            return _peliculasContext.Listas.Where(l => l.Peliculas.Count() > 0).OrderByDescending(r => r.Id).Select(l => new { l.Id, user = l.IdUsuario, l.Nombre, l.Peliculas.Count, poster = l.Peliculas.Select(l => l.Pelicula.Imagen).Take(3) }).ToList();
        }


        [HttpGet]
        [Route("TodasListasUsuario/{id}")]
        public dynamic TodasListasUsuario(int id)
        {
            return  _peliculasContext.Listas.Where(x => x.IdUsuario == id).OrderByDescending(r => r.Id).Select(l => new { l.Id, user = l.IdUsuario, l.Nombre, l.Peliculas.Count, poster = l.Peliculas.Select(l => l.Pelicula.Imagen).Take(3) }).ToList();
        }
    }
}
