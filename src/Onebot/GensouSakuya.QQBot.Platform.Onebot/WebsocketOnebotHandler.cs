using Mliybs.OneBot.V11.Data.Receivers.Messages;
using Mliybs.OneBot.V11.Data.Receivers.Metas;
using Mliybs.OneBot.V11.Data.Receivers.Notices;
using Mliybs.OneBot.V11.Data.Receivers.Requests;
using Mliybs.OneBot.V11.Data.Receivers;
using Mliybs.OneBot.V11.Utils;
using Mliybs.OneBot.V11;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Reactive.Subjects;
using Websocket.Client;
using GensouSakuya.QQBot.Core;
using System.Text.Json;
using System.Reactive.Linq;

namespace GensouSakuya.QQBot.Platform.Onebot
{
    public class WebsocketOneBotHandler : IOneBotHandler
    {
        private static Logger _logger = Logger.GetLogger<WebsocketOneBotHandler>();

        private CancellationTokenSource _source;

        private WebsocketClient _client;

        private readonly Subject<MessageReceiver> messageReceived = new();

        private readonly Subject<NoticeReceiver> noticeReceived = new();

        private readonly Subject<RequestReceiver> requestReceived = new();

        private readonly Subject<MetaReceiver> metaReceived = new();

        private readonly Subject<UnknownReceiver> unknownReceived = new();

        private readonly ConcurrentDictionary<string, Action<ReplyResult>> onReply = new();

        public WebsocketOneBotHandler(Uri uri, string? token = null)
        {
            _source = new();
            _client = new WebsocketClient(uri, () =>
            {
                var nativeClient = new ClientWebSocket();
                if (!string.IsNullOrWhiteSpace(token))
                {
                    nativeClient.Options.SetRequestHeader("Authorization", "Bearer " + token);
                }
                return nativeClient;
            });
            _client.DisconnectionHappened.Subscribe(async p => await ReconnectAsync(p, _source.Token));
            _client.MessageReceived.Subscribe(async p => await Received(p));
            _client.Start().ConfigureAwait(false).GetAwaiter().GetResult();
        }
        
        public WebsocketOneBotHandler(string uri, string? token = null):this(new Uri(uri), token) { }

        private async Task Received(ResponseMessage p)
        {
            if(p.MessageType== WebSocketMessageType.Text)
                UtilHelpers.Handle(p.Text!, messageReceived, noticeReceived, requestReceived, metaReceived, unknownReceived, onReply);
        }

        private async Task ReconnectAsync(DisconnectionInfo di, CancellationToken token)
        {
            try
            {
                _logger.Error(di.Exception, "websocket disconnected");
                await Task.Delay(1000, token);
                if (token.IsCancellationRequested)
                    return;
                _logger.Info("websocket reconnecting...");
                await _client.Reconnect();
            }
            catch (Exception ex) 
            {
                _logger.Error(ex, "websocket reconnect failed");
            }
        }

        private static readonly JsonSerializerOptions Options = new()
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                new StringIntConverter(),
                new NullableStringIntConverter(),
                new StringLongConverter(),
                new NullableStringLongConverter(),
                new BooleanConverter(),
                new NullableBooleanConverter()
            }
        };

        public async Task<ReplyResult> SendAsync(string action, object data)
        {
            var id = Guid.NewGuid().ToString();
            var json = JsonSerializer.Serialize(new
            {
                action,
                @params = data,
                echo = id
            }, Options);
            var task = this.WaitForReply(id);
            await _client.SendInstant(json).ConfigureAwait(false);
            return await task.ConfigureAwait(false);
        }

        public void Dispose()
        {
            _source.Cancel();
            _source.Dispose();
            _client.Dispose();
        }

        public IObservable<MessageReceiver> MessageReceived => messageReceived.AsObservable();

        public IObservable<NoticeReceiver> NoticeReceived => noticeReceived.AsObservable();

        public IObservable<RequestReceiver> RequestReceived => requestReceived.AsObservable();

        public IObservable<MetaReceiver> MetaReceived => metaReceived.AsObservable();

        public IObservable<UnknownReceiver> UnknownReceived => unknownReceived.AsObservable();

        public IDictionary<string, Action<ReplyResult>> OnReply => onReply;
    }
}
