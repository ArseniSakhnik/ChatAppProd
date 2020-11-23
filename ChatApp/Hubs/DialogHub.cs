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
    //[Authorize]
    public class DialogHub : Hub
    {
        private IMessageService _messageService;
        private IUserService _userService;
        public DialogHub(IMessageService messageService, IUserService userService)
        {
            _messageService = messageService;
            _userService = userService;
        }

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

        public async Task SendMessage(string usernameSender, int dialogId, string text, List<string> usernames)
        {
            if (_messageService.SendMessage(usernameSender, dialogId, text))
            {
                await SendDialogs(usernames);
            }
        }

        public async Task CreateDialogAndSendMessage(string userNameRecipient, string text)
        {
            if (_messageService.CreateDialog(Context.UserIdentifier, userNameRecipient, out int dialogId))
            {
                await SendMessage(Context.UserIdentifier, dialogId, text, new List<string> { Context.UserIdentifier, userNameRecipient });
            }
        }

        public async Task RemoveUserFromDialog(string username, int dialogId)
        {
            if (_userService.RemoveUserFromDialog(username, dialogId))
            {
                var usernames = _userService.GetNamesOfParticipiantsInDialogue(dialogId);
                await SendDialogs(usernames);
            }
        }

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
