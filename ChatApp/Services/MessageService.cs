using ChatApp.Entities;
using ChatApp.Helpers;
using ChatApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Services
{

    public interface IMessageService
    {
        //bool SendMessage(int senderId, int recipientId, string text);
        //List<Message> GetLastMessages(int senderId);
        //List<Message> GetSenderAndRecipientMessages(int senderId, int recipientId);
        bool SendMessage(string username, int dialogId, string text);
        List<Dialog> GetDialogs(string username);
        bool CreateDialog(string firstUsername, string secondUsername, out int dialogId);
    }

    public class MessageService : IMessageService
    {
        private DataContext _context;
        public MessageService(DataContext context)
        {
            _context = context;
        }

        public List<Dialog> GetDialogs(string username)
        {
            var user = _context.Users.Where(u => u.Username == username)
                .Include(u => u.UserDialog)
                .ThenInclude(ud => ud.Dialog)
                .ThenInclude(d => d.Messages)
                .ThenInclude(m => m.Sender)
                .SingleOrDefault();

            if (user == null)
            {
                return null;
            }

            var dialogs = user.UserDialog.Select(ud => ud.Dialog).ToList();


            return dialogs;
        }

        public bool SendMessage(string username, int dialogId, string text)
        {

            var user = _context.Users.Where(u => u.Username == username)
                .Include(u => u.UserDialog)
                .ThenInclude(ud => ud.Dialog)
                .ThenInclude(d => d.Messages)
                .SingleOrDefault();

            if (user == null)
            {
                return false;
            }

            var userDialog = user.UserDialog.Where(ud => ud.DialogId == dialogId).Select(ud => ud.Dialog).SingleOrDefault();

            if (userDialog == null)
            {
                return false;
            }

            var message = new Message
            {
                Dialog = userDialog,
                SenderUsername = user.Username,
                Sender = user,
                Text = text
            };

            userDialog.Messages.Add(message);

            _context.SaveChanges();

            return true;
        }

        public bool CreateDialog(string firstUsername, string secondUsername, out int dialogId)
        {
            var firstUser = _context.Users.SingleOrDefault(u => u.Username == firstUsername);
            if (firstUser == null)
            {
                dialogId = -1;
                return false;
            }
            var secondUser = _context.Users.SingleOrDefault(u => u.Username == secondUsername);
            if (secondUser == null)
            {
                dialogId = -1;
                return false;
            }

            var dialog = new Dialog();

            dialog.UserDialog.Add(new UserDialog
            {
                Username = firstUser.Username,
                User = firstUser,
                Dialog = dialog,
                DialogId = dialog.Id
            });

            dialog.UserDialog.Add(new UserDialog
            {
                Username = secondUser.Username,
                User = secondUser,
                Dialog = dialog,
                DialogId = dialog.Id
            });

            dialog.Name = $"{firstUser.Username}, {secondUser.Username}";

            _context.Dialogs.Add(dialog);
            _context.SaveChanges();

            dialogId = dialog.Id;

            return true;
        }

    }
}
