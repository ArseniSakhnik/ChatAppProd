using ChatApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Hubs
{
    [Authorize]
    public class DialogHub : Hub
    {
        private IMessageService _messageService;
        private IUserService _userService;
        public DialogHub(IMessageService messageService, IUserService userService)
        {
            _messageService = messageService;
            _userService = userService;
        }
        /// <summary>
        /// Обновляет диалоги
        /// </summary>
        /// <param name="usernames">Имя пользователей, чьи диалоги необходимо обновить. Если null, то отправляет диалоги пользователю,
        /// который отправил запрос</param>
        /// <returns>Возвращает задачу отправки данных пользователям</returns>
        public async Task SendDialogs(List<string> usernames)
        {
            if (usernames == null)
            {
                var dialogs = _messageService.GetDialogs(Context.UserIdentifier);
                await Clients.User(Context.UserIdentifier).SendAsync("GetDialogs", dialogs);
            }
            else
            {
                foreach (var u in usernames)
                {
                    var dialogs = _messageService.GetDialogs(u);
                    await Clients.User(u).SendAsync("GetDialogs", dialogs);
                }
            }
        }
        /// <summary>
        /// Отправляет сообщение в диалог
        /// </summary>
        /// <param name="usernameSender">Имя пользователя, который отправил сообщение</param>
        /// <param name="dialogId">Id диалога, в который необходимо отправить сообщение</param>
        /// <param name="text">Текст сообщения</param>
        /// <param name="usernames">Имена пользователей, у которых необходимо обновить диалоги</param>
        /// <returns>Задачу обновления диалогов</returns>
        public async Task SendMessage(string usernameSender, int dialogId, string text, List<string> usernames)
        {
            if (_messageService.SendMessage(usernameSender, dialogId, text))
            {
                await SendDialogs(usernames);
            }
        }
        /// <summary>
        /// Создает диалог и отправляет сообщение
        /// </summary>
        /// <param name="userNameRecipient">Имя отправителя</param>
        /// <param name="text">Текст сообщения</param>
        /// <returns>Задачу отправки сообщения</returns>
        public async Task CreateDialogAndSendMessage(string userNameRecipient, string text)
        {
            if (_messageService.CreateDialog(Context.UserIdentifier, userNameRecipient, out int dialogId))
            {
                await SendMessage(Context.UserIdentifier, dialogId, text, new List<string> { Context.UserIdentifier, userNameRecipient });
            }
        }
        /// <summary>
        /// Удаляет пользователя из диалога
        /// </summary>
        /// <param name="username">Имя пользователя, которого необходимо удалить из диалога</param>
        /// <param name="dialogId">Id диалога, из которого необходимо удалить пользователя</param>
        /// <returns>Задачу обновления диалогов</returns>
        public async Task RemoveUserFromDialog(string username, int dialogId)
        {
            if (_userService.RemoveUserFromDialog(username, dialogId))
            {
                var usernames = _userService.GetNamesOfParticipiantsInDialogue(dialogId);
                await SendDialogs(usernames);
            }
        }
        /// <summary>
        /// Добавляет пользователя в диалог
        /// </summary>
        /// <param name="username">Имя пользователя, которого необходимо добавить в диалог</param>
        /// <param name="dialogId">Id диалога, в который необходимо добавить пользователя</param>
        /// <returns>Задачу отправки сообщения пользователям</returns>
        public async Task AddUserToDialog(string username, int dialogId)
        {
            if (_userService.AddUserToDialog(username, dialogId))
            {
                var usernames = _userService.GetNamesOfParticipiantsInDialogue(dialogId);
                await SendDialogs(usernames);
            }
        }
    }
}
