using Bytewizer.Backblaze.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoGrado.Models;
using System.Linq;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace ProyectoGrado.Controllers
{

    public class PeliculaForm
    {
        public int? Id { get; set; }
        public string? Titulo { get; set; }
        public DateTime Fecha { get; set; }
        public string? Director { get; set; }
        public string? Descripcion { get; set; }
        public IFormFile? Imagen { get; set; }
        public string? Generos { get; set; }
        public float? Puntuacion { get; set; }
}

    [Route("Peliculas")]
    [ApiController]
    public class PeliculasController : ControllerBase
    {

        private readonly PeliculasContext _peliculasContext;
        private static IStorageClient _storage;
        private readonly IConfiguration _config;


        public PeliculasController(PeliculasContext peliculasContext, IConfiguration config)
        {
            _peliculasContext = peliculasContext;
            _config = config;
        }

        [HttpGet]
        [Route("UltimasPeliculas")]

        public dynamic UltimasPeliculas()
        {
            return  _peliculasContext.Peliculas.Select(p => new { p.Id, p.Imagen }).OrderByDescending(p => p.Id).Take(7).ToList();
        }

        [HttpGet]
        [Route("TodasPeliculas")]

        public dynamic TodasPeliculas()
        {
            return _peliculasContext.Peliculas.Select(p => new { p.Id, p.Imagen, p.Titulo }).OrderByDescending(p => p.Id).Take(50).ToList();
        }

        [HttpGet]
        [Route("PeliculasNoLista/{id}")]

        public dynamic PeliculasNoLista(int id)
        {
            return _peliculasContext.Peliculas.Join(_peliculasContext.Contiene, p => p.Id, v => v.IdPelicula, (p, v) => new { p, v }).Where(x=>x.v.IdLista==id).Select(p => new { p.p.Id, p.p.Imagen, p.p.Titulo }).ToList();
        }

        [HttpGet]
        [Route("UltimasVistasUsuario/{id}")]

        public dynamic UltimasPeliculasVistas(int id)
        {
            return  _peliculasContext.Peliculas.Join(_peliculasContext.Vistas, p=> p.Id, v=>v.IdPelicula, (p,v)=>new { p,v}).Where(x=> x.v.IdUsuario==id).OrderByDescending(pel => pel.v.Id).Select(pel => new { pel.p.Id,pel.p.Imagen }).Take(7).ToList();
        }

        //TODO TODAS LAS VISTAS DEL USUARIO

        [HttpGet]
        [Route("TodasVistasUsuario/{id}")]

        public dynamic TodasistasUsuario(int id)
        {
            return _peliculasContext.Peliculas.Join(_peliculasContext.Vistas, p => p.Id, v => v.IdPelicula, (p, v) => new { p, v }).Where(x => x.v.IdUsuario == id).OrderByDescending(pel => pel.v.Id).Select(pel => new { pel.p.Id, pel.p.Imagen }).ToList();
        }


        [HttpGet]
        [Route("UltimasFavoritasUsuario/{id}")]

        public dynamic UltimasPeliculasFavoritas(int id)
        {
            return  _peliculasContext.Peliculas.Join(_peliculasContext.Favoritos, p => p.Id, f => f.IdPelicula, (p, f) => new { p, f }).Where(x => x.f.IdUsuario == id).OrderByDescending(pel => pel.f.Id).Select(pel => new { pel.p.Id, pel.p.Imagen }).Take(7).ToList();
        }

        //TODO TODAS LAS FAVORITAS DEL USUARIO

        [HttpGet]
        [Route("TodasFavoritasUsuario/{id}")]

        public dynamic TodasFavoritasUsuario(int id)
        {
            return _peliculasContext.Peliculas.Join(_peliculasContext.Favoritos, p => p.Id, f => f.IdPelicula, (p, f) => new { p, f }).Where(x => x.f.IdUsuario == id).OrderByDescending(pel => pel.f.Id).Select(pel => new { pel.p.Id, pel.p.Imagen }).ToList();
        }

        [HttpGet]
        [Route("{id}")]

        public dynamic Pelicula(int id)
        {
            var pelicula =  _peliculasContext.Peliculas.Where(p => p.Id == id).FirstOrDefault();
            int reviews = _peliculasContext.Resenas.Where(r => r.IdPelicula == id).Count();
            float puntuacion = _peliculasContext.Resenas.Where(r => r.IdPelicula == id).Sum(r => r.Valoracion);
            if (pelicula == null) return Ok(false);
            return new{ pelicula,reviews,puntuacion};
        }

        [HttpPost]
        [Route("AnadirPelicula")]
        [Authorize(Roles = "1")]
        public async Task<dynamic> AnadirPelicula([FromForm] PeliculaForm p)
        {
            Pelicula peli = new Pelicula();

            if (p.Id != null) {
                peli = _peliculasContext.Peliculas.Where(peli => peli.Id == p.Id).FirstOrDefault();
            }

            if (p.Imagen != null)
            {
                _storage = new BackblazeClient();
                _storage.Connect(Environment.GetEnvironmentVariable("keyId"), Environment.GetEnvironmentVariable("applicationKey"));
                var result = await _storage.UploadAsync(Environment.GetEnvironmentVariable("bucketId"), "Peliculas/" + p.Titulo + ".jpg", p.Imagen.OpenReadStream());
                peli.Imagen = "https://moviebox-pelis.s3.us-east-005.backblazeb2.com/Peliculas/" + p.Titulo + ".jpg";
            }

            peli.Titulo = p.Titulo;
            peli.Fecha = DateTime.SpecifyKind(p.Fecha, DateTimeKind.Utc); ;
            peli.Director = p.Director;
            peli.Descripcion = p.Descripcion;
            if(p.Puntuacion != 0)
            {
                peli.Puntuacion = p.Puntuacion/10;
            }else peli.Puntuacion = p.Puntuacion;
            peli.Generos = p.Generos;

            if (p.Id == null)  _peliculasContext.Peliculas.Add(peli);
            _peliculasContext.SaveChanges();
            return new
            {
                succes = true,
            };

        }

        [HttpDelete]
        [Route("EliminarPelicula/{id}")]
        [Authorize(Roles = "1")]
        public dynamic EliminarPelicula(int id)
        {
            var p = _peliculasContext.Peliculas.Where(_ => _.Id == id).FirstOrDefault();
            if (p == null) return Ok(false);
            _peliculasContext.Peliculas.Remove(p);
            _peliculasContext.SaveChanges();
            return new
            {
                succes = true,
            };

        }
        //TODO TODAS LAS PELICULAS DE UNA LISTA

        [HttpGet]
        [Route("TodasPeliculasLista/{id}")]

        public dynamic TodasPeliculasLista(int id)
        {
            string titulo = _peliculasContext.Listas.Where(l=> l.Id==id).Select(l=> l.Nombre).FirstOrDefault();
            var peliculas =  _peliculasContext.Peliculas.Join(_peliculasContext.Contiene, p => p.Id, f => f.IdPelicula, (p, f) => new { p, f }).Where(x => x.f.IdLista == id).OrderByDescending(pel => pel.f.Id).Select(pel => new { pel.p.Id, pel.p.Imagen }).ToList();
            return new
            {
                titulo,
                incluye = peliculas
            };
        }
    }
}
