using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace EcoRuteando.Modules.Security.Presentation.Controllers
{
    /// <summary>
    /// Endpoints de prueba del esquema de autenticación.
    /// SOLO disponibles en entorno Development: en producción responde 404.
    /// </summary>
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        private readonly IHostEnvironment _env;

        public TestController(IHostEnvironment env)
        {
            _env = env;
        }

        [HttpGet("public")]
        public IActionResult Public()
        {
            if (!_env.IsDevelopment())
            {
                return NotFound();
            }

            return Ok("Endpoint público");
        }

        [Authorize]
        [HttpGet("private")]
        public IActionResult Private()
        {
            if (!_env.IsDevelopment())
            {
                return NotFound();
            }

            return Ok("Entraste con un JWT válido");
        }
    }
}
