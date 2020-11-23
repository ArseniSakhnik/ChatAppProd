using ChatApp.Entities;
using ChatApp.Models;
using ChatApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;


namespace ChatApp.Controllers
{

    /// <summary>
    /// Контроллер пользователей ASP.NET Core определяет и обрабатывает все маршруты / 
    /// конечные точки для API, которые относятся к пользователям, включая аутентификацию, 
    /// обновление и отзыв токенов, а также получение данных пользователя и обновления токенов.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private IUserService _userService { get; set; }
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UsersController(IUserService userService, IHttpContextAccessor httpContextAccessor)
        {
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
        }
        [AllowAnonymous]
        [HttpPost("registration")]
        public IActionResult Registration([FromBody] RegistrationRequest model) 
        {
            if (_userService.Registration(model))
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }


        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody] AuthenticateRequest model)
        {

            var response = _userService.Authenticate(model, ipAddress());

            if (response == null)
            {
                return BadRequest(new { message = "Username or password is incorrect" });
            }

            setTokenCookie(response.RefreshToken);

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public IActionResult RefreshToken()
        {
            var refreshToken = _httpContextAccessor.HttpContext.Request.Cookies["refreshToken"];
            var response = _userService.RefreshToken(refreshToken, ipAddress());

            if (response == null)
                return Unauthorized(new { message = "Invalid token" });

            setTokenCookie(response.RefreshToken);

            return Ok(response);
        }

        [HttpPost("revoke-token")]
        public IActionResult RevokeToken([FromBody] RevokeTokenRequest model)
        {
            // accept token from request body or cookie
            var token = model.Token ?? _httpContextAccessor.HttpContext.Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(token))
                return BadRequest(new { message = "Token is required" });

            var response = _userService.RevokeToken(token, ipAddress());

            if (!response)
                return NotFound(new { message = "Token not found" });

            return Ok(new { message = "Token revoked" });
        }

        [AllowAnonymous]
        [HttpPost("get")]
        public IActionResult Get([FromBody] UserExitstsRequest request)
        {
            if (_userService.UserExists(request.Username))
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }


        //Методы помощники

        /// <summary>
        /// Вспомогательный метод присоединяет HTTP только печенье с обновлением маркера ответа для повышения безопасности. 
        /// Файлы cookie только HTTP недоступны для клиентского javascript, 
        /// который предотвращает XSS (межсайтовый скриптинг), 
        /// а токен обновления можно использовать только для получения нового токена из /users/refresh-tokenмаршрута, 
        /// который предотвращает CSRF (подделку межсайтовых запросов).
        /// </summary>
        /// <param name="token"></param>
        private void setTokenCookie(string token)
        {
            //хотим чтобы куки не были доступны приложению юзера и хранились 7 дней, как рефреш токен
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7)
            };

            _httpContextAccessor.HttpContext.Response.Cookies.Append("refreshToken", token, cookieOptions);
        }

        private string ipAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                //Нестандартный заголовок, используемый неанонимными прокси-серверами для передачи реального IP клиента
                return Request.Headers["X-Forwarded-For"];
            }
            else
            {
                //получить ip адрес Client
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
        }


    }
}
