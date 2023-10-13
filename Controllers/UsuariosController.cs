using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProyectoGrado.Models;
using ProyectoGrado.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using Bytewizer.Backblaze.Client;
using System;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace ProyectoGrado.Controllers
{

    public class LoginModel
    {
        public string usuario { get; set; }
        public string password { get; set; }
    }

    public class RegisterModel
    {
        public string usuario { get; set; }
        public string password { get; set; }
        public string email { get; set; }
        public string nombre { get; set; }
        public string apellidos { get; set; }
        public DateTime fechaNac { get; set; }
        public IFormFile Imagen { get; set; }

    }
    public class NuevaContrasenaModal
    {
        public string nuevaContrasena { get; set; }
        public long codigo { get; set; }


    }


    [Route("Usuarios")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {

        private readonly PeliculasContext _peliculasContext;
        private readonly IConfiguration _config;
        private static IStorageClient _storage;
        private readonly IEmailService _emailService;
        private Usuario user;

        public UsuariosController(PeliculasContext peliculasContext,IConfiguration config, IEmailService emailService)
        {
            _peliculasContext = peliculasContext;
            _config = config;
            _emailService = emailService;
        }

        [HttpGet]
        [Route("TodosUsuarios")]

        public dynamic TodosUsuarios()
        {
            return _peliculasContext.Usuarios.Select(p => new { p.Id, p.Imagen,p.User}).OrderByDescending(p => p.Id).Take(50).ToList();
        }

        [HttpPost]
        [Route("Register")]
        public async Task<dynamic> RegisterUsuario([FromForm] RegisterModel r)
        {
            if (_peliculasContext.Usuarios.Where(e => e.User == r.usuario).Count() > 0) return new {    error = "El usuario ya existe" };
            if (_peliculasContext.Usuarios.Where(e => e.Email == r.email).Count() > 0) return new {    error = "El correo ya esta en uso" };

            byte[] salt = RandomNumberGenerator.GetBytes(128 / 8); // divide by 8 to convert bits to bytes

            // derive a 256-bit subkey (use HMACSHA256 with 100,000 iterations)
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: r.password!,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            _storage = new BackblazeClient();
            _storage.Connect(Environment.GetEnvironmentVariable("keyId"), Environment.GetEnvironmentVariable("applicationKey"));
            var result = await _storage.UploadAsync(Environment.GetEnvironmentVariable("bucketId"), "Pfps/" + r.usuario+".jpg", r.Imagen.OpenReadStream());

            Usuario u = new Usuario();

            u.Nombre = r.nombre;
            u.User = r.usuario;
            u.PasswordHash = hashed;
            u.PasswordSalt = Convert.ToBase64String(salt);
            u.Email = r.email;
            u.FechaNac = DateTime.SpecifyKind(r.fechaNac, DateTimeKind.Utc);
            u.Rol = 0;
            u.ResetPassword = 0;
            u.Apellidos = r.apellidos;
            u.Imagen = "https://moviebox-pelis.s3.us-east-005.backblazeb2.com/Pfps/" + r.usuario+".jpg"; ;

            _peliculasContext.Usuarios.Add(u);
            _peliculasContext.SaveChanges();
            return new
            {
                succes = true,
            };

        }

        [HttpPost]
        [Route("Login")]
        public async Task<ActionResult<dynamic>> LoginUsuario(LoginModel l)
        {

            var u = await _peliculasContext.Usuarios.Where(e => e.User == l.usuario).FirstOrDefaultAsync();

            if (u == null)
            {
                return new { error= "Usuario o contraseña incorrectos" };

            }

            byte[] salt = Convert.FromBase64String(u.PasswordSalt);

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: l.password!,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));


            if (u.PasswordHash.Equals(hashed))
            {
                string token = CreateToken(u);
                return new { 
                    success= true,
                    id = u.Id,
                    jwt = token };
            }

            return new { error = "Usuario o contraseña incorrectos" };
        }

        [HttpGet]
        [Route("Validame")]
        [Authorize]

        public async Task<ActionResult<dynamic>> Validame()
        {
            int id = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var u = await _peliculasContext.Usuarios.Where(e => e.Id == id).FirstOrDefaultAsync();
            return new
            {
                u.Id,
                u.Rol,
                u.User,
                u.Imagen
            };
        }

        [HttpGet]
        [Route("DatosUsuario/{id}")]
        public dynamic DatosUsuario(int id)
        {
            return _peliculasContext.Usuarios.Where(u => u.Id == id).Select(r => new { r.User, r.Imagen }).FirstOrDefault();
        }

        [HttpPost]
        [Route("OlvidarContraseña")]
        public dynamic OlvidarContraseña([FromBody]  string email)
        {
            if (_peliculasContext.Usuarios.Where(e => e.Email == email).Count() > 0) {
            var usuario = _peliculasContext.Usuarios.Where(u => u.Email == email).FirstOrDefault();
            var codigo = new Random();
            long codigoReset = codigo.NextInt64();
            usuario.ResetPassword = codigoReset;
            _peliculasContext.SaveChanges();
            _emailService.SendEmail(new EmailDTO { Para = email, Codigo = codigoReset });
                return new { success = true };
            }
            else
            {
                return new { error = "El correo no existe" };

            }
        }
        [HttpPost]
        [Route("CambiarContraseña")]
        public dynamic CambiarContraseña(NuevaContrasenaModal r)
        {
            if(r.codigo == 0 ) return new { error = "Se ha producido un error" };
            if (_peliculasContext.Usuarios.Where(e => e.ResetPassword == r.codigo).Count() > 0)
            {
                var usuario = _peliculasContext.Usuarios.Where(u => u.ResetPassword == r.codigo).FirstOrDefault();

                byte[] salt = Convert.FromBase64String(usuario.PasswordSalt);

                string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: r.nuevaContrasena!,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 100000,
                    numBytesRequested: 256 / 8));
                usuario.PasswordHash = hashed;
                usuario.ResetPassword = 0;
                _peliculasContext.SaveChanges();
                return new { success = true };
            }
            else
            {
                return new { error = "Se ha producido un error" };

            }
        }

        private string CreateToken(Usuario usuario) 
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Role, usuario.Rol.ToString())
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims : claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: cred
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
    }
}
