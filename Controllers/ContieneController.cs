using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoGrado.Models;
using System.Security.Claims;

namespace ProyectoGrado.Controllers
{

    public class ContieneLista
    {
        public int IdPelicula { get; set; }
        public int IdLista { get; set; }
    }

    [Route("Contiene")]
    [ApiController]
    [Authorize]
    public class ContieneController : ControllerBase
    {

        private readonly PeliculasContext _peliculasContext;

        public ContieneController(PeliculasContext peliculasContext)
        {
            _peliculasContext = peliculasContext;
        }

        [HttpPost]
        [Route("AnadirPeliculaLista")]
        public dynamic AnadirPeliculaLista(ContieneLista p)
        {

            Contiene c = new Contiene();
            c.IdPelicula = p.IdPelicula;
            c.IdLista = p.IdLista;
            _peliculasContext.Contiene.Add(c);
            _peliculasContext.SaveChanges();
            return new {succes =true};

        }

        [HttpDelete]
        [Route("EliminarPeliculaLista")]
        public dynamic EliminarPeliculaLista(ContieneLista p)
        {
            var contiene =  _peliculasContext.Contiene.Where(e => e.IdPelicula == p.IdPelicula && e.IdLista == p.IdLista).FirstOrDefault();
            if (contiene == null) return "Error";
            _peliculasContext.Remove<Contiene>(contiene);
            _peliculasContext.SaveChanges();
            return new { succes = true };
        }
    }
}
