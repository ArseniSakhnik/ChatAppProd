using ChatApp.Entities;
using ChatApp.Helpers;
using ChatApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Services
{

    public interface IUserService
    {
        AuthenticateResponse Authenticate(AuthenticateRequest model, string ipAddress);
        AuthenticateResponse RefreshToken(string token, string ipAddress);
        bool RevokeToken(string token, string ipAddress);
        IEnumerable<User> GetAll();
        User GetById(int id);
        bool Registration(RegistrationRequest model);
        bool UserExists(string username);
        bool AddUserToDialog(string username, int dialogId);
        bool RemoveUserFromDialog(string username, int dialogId);
        List<string> GetNamesOfParticipiantsInDialogue(int dialogId);
    }

    public class UserService : IUserService
    {
        private DataContext _context;
        private readonly AppSettings _appSettings;

        private readonly int jwtTokenLifeTimeExpires = 60;

        public UserService(DataContext context, IOptions<AppSettings> appSettings)
        {
            _context = context;
            _appSettings = appSettings.Value;
        }

        public bool Registration(RegistrationRequest model)
        {
            var user = _context.Users.SingleOrDefault(u => u.Username == model.Username);

            if (user != null)
            {
                return false;
            }

            _context.Users.Add(new User
            {
                Username = model.Username,
                Password = model.Password,
            });

            _context.SaveChanges();
            return true;
        }

        public AuthenticateResponse Authenticate(AuthenticateRequest model, string ipAddress)
        {
            //получаем пользователя из базы данных
            var user = _context.Users.SingleOrDefault(x => x.Username == model.Username && x.Password == model.Password);

            if (user == null)
            {
                return null;
            }


            //проверка подлинности прошла успешно, поэтому сгенерируйте токены jwt и обновите
            var jwtToken = generateJwtToken(user);
            var refreshToken = generateRefreshToken(ipAddress);

            //сохраняем токен обновления
            user.RefreshTokens.Add(refreshToken);
            _context.Update(user);
            _context.SaveChanges();

            
            return new AuthenticateResponse(user, jwtToken, refreshToken.Token);
        }

        public AuthenticateResponse RefreshToken(string token, string ipAddress)
        {
            //находим пользователя, у которого имеется токен, сохраненный в куках контекса
            var user = _context.Users.SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));

            if (user == null) return null;

            //находим интересующий нас рефреш токен
            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

            //вернуть null, если токен больше не активен
            if (!refreshToken.IsActive) return null;

            
            //создаем новый рефреш токен
            var newRefreshToken = generateRefreshToken(ipAddress);
            // добавим запись о том, когда рефреш токен был отозван, свойство isActive становится false
            refreshToken.Revoked = DateTime.UtcNow;
            // с какого ip был отозван 
            refreshToken.RevokedByIp = ipAddress;
            // предыдущй рефреш токен
            refreshToken.ReplacedByToken = newRefreshToken.Token;
            // заменяем старый рефреш токен новым и сохраняем
            user.RefreshTokens.Add(newRefreshToken);
            _context.Update(user);
            _context.SaveChanges();

            // также создаем новый джвт 
            var jwtToken = generateJwtToken(user);

            return new AuthenticateResponse(user, jwtToken, newRefreshToken.Token);
        }

        public bool RevokeToken(string token, string ipAddress)
        {
            var user = _context.Users.SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));

            if (user == null) return false;

            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

            //вернуть null, если токен больше не активен
            if (!refreshToken.IsActive) return false;

            //отозвать токен и сохранить
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            _context.Update(user);
            _context.SaveChanges();

            return true;
        }

        public bool UserExists(string username)
        {
            var user = _context.Users.SingleOrDefault(u => u.Username == username);
            if (user == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool AddUserToDialog(string username, int dialogId)
        {
            var user = _context.Users.SingleOrDefault(u => u.Username == username);
            if (user == null)
            {
                return false;
            }

            var dialog = _context.Dialogs.Where(d => d.Id == dialogId).Include(d => d.UserDialog).SingleOrDefault();

            if (dialog == null)
            {
                return false;
            }
            
            

            foreach (var ud in dialog.UserDialog)
            {
                if (ud.Username == username)
                {
                    return false;
                }
            }

            dialog.Name += $", {username}";

            dialog.UserDialog.Add(new UserDialog
            {
                DialogId = dialogId,
                Username = username
            });
            _context.SaveChanges();
            return true;
        }

        public bool RemoveUserFromDialog(string username, int dialogId)
        {
            var user = _context.Users.SingleOrDefault(u => u.Username == username);
            if (user == null)
            {
                return false;
            }

            var dialog = _context.Dialogs.Where(d => d.Id == dialogId).Include(d => d.UserDialog).SingleOrDefault();

            if (dialog == null)
            {
                return false;
            }

            var ud = dialog.UserDialog.Where(ud => ud.Username == username && ud.DialogId == dialogId).SingleOrDefault();

            if (ud == null)
            {
                return false;
            }

            if (dialog.Name.Contains($"{username}, "))
            {
                dialog.Name = dialog.Name.Replace($"{username}, ", "");
            }
            else 
            {
                dialog.Name = dialog.Name.Replace($", {username}", "");
            }

            dialog.UserDialog.Remove(ud);

            _context.SaveChanges();

            return true;
        }


        public List<string> GetNamesOfParticipiantsInDialogue(int dialogId)
        {
            var dialog = _context.Dialogs.Where(d => d.Id == dialogId).Include(d => d.UserDialog)
                .ThenInclude(ud => ud.User).SingleOrDefault();

            if (dialog == null)
            {
                return null;
            }

            var usernames = dialog.UserDialog.Select(ud => ud.User.Username);

            return usernames.ToList();
        }

        public IEnumerable<User> GetAll()
        {
            return _context.Users;
        }

        public User GetById(int id)
        {
            return _context.Users.Find(id);
        }

        //Методы помощники
        private string generateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

           //кодируем секретный ключ в набор байтов
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);

            //настройки токена
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                //вставляем в токен информацию о юзере в виде его id
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Username)
                }),

                //продолжительность жизни токена - 15 минут
                Expires = DateTime.UtcNow.AddMinutes(jwtTokenLifeTimeExpires),
                //Получает или задает учетные данные, используемые для подписывания токена.
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };


            //создаем токен
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private RefreshToken generateRefreshToken(string ipAddress)
        {
            //
            using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                var randomBytes = new byte[64];
                //заполняем массив randomBytes криптостойкой последовательностью случайных значений
                rngCryptoServiceProvider.GetBytes(randomBytes);
                return new RefreshToken
                {
                    Token = Convert.ToBase64String(randomBytes),
                    Expires = DateTime.UtcNow.AddDays(7),
                    Created = DateTime.UtcNow,
                    CreatedByIp = ipAddress
                };
            }
        }

        
    }
}
