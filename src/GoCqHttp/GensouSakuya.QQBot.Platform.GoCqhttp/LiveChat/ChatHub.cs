using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace GensouSakuya.QQBot.Platform.GoCqhttp.LiveChat
{
    public class ChatHub : Hub<IChatClient>
    {
        private QQHelper _qqHelper;
        private ILogger<ChatHub> _logger;
        public ChatHub(QQHelper qQHelper, ILogger<ChatHub> logger)
        {
            _qqHelper = qQHelper;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                var param = Context.GetHttpContext().Request.Query;
                var roomname = param["room"];
                var guildName = param["guildName"];
                var connectionId = Context.ConnectionId;
                await _qqHelper.AttachRoomConnection(connectionId, guildName, roomname);
                await base.OnConnectedAsync();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "connection error");
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                var connectionId = Context.ConnectionId;
                _qqHelper.DetachRoomConnection(connectionId);
                await base.OnDisconnectedAsync(exception);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "disconnection error");
            }
        }
    }

    public interface IChatClient
    {
        Task SendChatMessage(DanmuMessage msg);

        Task SendSuperChatMessage(SuperChatMessage msg);
    }

    public class DanmuMessage
    {
        public string AvatarUrl { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }
        public bool IsOwner { get; set; }
    }

    public class SuperChatMessage
    {
        public string AvatarUrl { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }
        public bool IsOwner { get; set; }
    }
}
