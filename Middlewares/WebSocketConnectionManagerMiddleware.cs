using FileTracking.Services.BO;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using static FileTracking.Client.Common.WSCommonObject;

namespace FileTracking.Services.Middlewares
{
    public class WebSocketConnectionManagerMiddleware
    {

        private readonly RequestDelegate _next;

        public WebSocketConnectionManagerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    var ws = await context.WebSockets.AcceptWebSocketAsync();
                    if (ws == null || ws.State != WebSocketState.Open) { return; }
                    var newconnection = new WebSocketConnectionManagerMiddleware(ws); idc++; newconnection.id = idc;
                    if (wss.TryAdd(idc, newconnection)) { await newconnection.floop(); }
                }
                else
                    await _next(context);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"The following error happened: {e.Message}");
            }
        }

        WebSocket _ws;
        Authentication UserInfor;

        int id = 0;

        static ConcurrentDictionary<int, WebSocketConnectionManagerMiddleware> wss = new ConcurrentDictionary<int, WebSocketConnectionManagerMiddleware>();
        static int idc = 0;

        public WebSocketConnectionManagerMiddleware(WebSocket ws) { _ws = ws; }
        async Task floop()
        {
            while (_ws.State == WebSocketState.Open)
            {
                byte[] arrbytebuf = new byte[4096];
                ArraySegment<byte> arrseg = new ArraySegment<byte>(arrbytebuf);
                var incoming = await _ws.ReceiveAsync(arrseg, CancellationToken.None);
                try
                {
                    MessageObject objSend = new MessageObject();
                    switch (incoming.MessageType)
                    {
                        case WebSocketMessageType.Text:
                            var sdata = System.Text.Encoding.UTF8.GetString(arrseg.Array, arrseg.Offset, arrseg.Count);
                            var obj = JsonConvert.DeserializeObject<csockdata>(sdata);
                            foreach (var s in wss)
                            {
                                if ((s.Value.UserInfor != null) && (s.Value.UserInfor.Username != null) && (s.Value.UserInfor.Username == "loguser"))
                                {

                                    objSend.UserName = "Server log";
                                    objSend.FullName = objSend.UserName;
                                    objSend.Message = JsonConvert.SerializeObject(obj);
                                    await fsend(s.Key, MessageType.Message, objSend);
                                    objSend = new MessageObject();
                                }
                            }
                            switch (obj.MessageType.ToLower())
                            {
                                case MessageType.Login:
                                    var objLogin = JsonConvert.DeserializeObject<LoginObject>(obj.Value);
                                    UserInfor = new Authentication(objLogin.Username, objLogin.Password, objLogin.FullName, objLogin.token);
                                    if (!UserInfor.IsAuthenticated)
                                    {
                                        foreach (var s in wss)
                                        {
                                            if ((s.Value.UserInfor != null) && (s.Value.UserInfor.Username != null) && (s.Value.UserInfor.Username == "loguser"))
                                            {

                                                objSend.UserName = "Server log";
                                                objSend.FullName = objSend.UserName;
                                                objSend.Message = "Login fail: " + JsonConvert.SerializeObject(objLogin);
                                                await fsend(s.Key, MessageType.Message, objSend);
                                                objSend = new MessageObject();
                                            }
                                        }
                                        await fsendLoginFail(id);
                                        WebSocketConnectionManagerMiddleware cNewLogin;
                                        if (wss.TryRemove(id, out cNewLogin))
                                        {
                                            await cNewLogin.fclr();
                                        }
                                    }
                                    else
                                    {
                                        objSend.UserName = "Server";
                                        objSend.FullName = "Server";
                                        objSend.Message = "Login OK";
                                        await fsend(id, MessageType.Message, objSend);
                                    }
                                    break;
                                case MessageType.FileAction:
                                    FileMessage objAction = null;
                                    try
                                    {
                                        objAction = JsonConvert.DeserializeObject<FileMessage>(obj.Value);
                                    }
                                    catch { }
                                    if (objAction != null)
                                    {
                                        FileMessage fileMessageResult = new FileMessage();
                                        string userCheckouted = UserInfor.FilePermission(objAction.FilePath);
                                        fileMessageResult.FileName = objAction.FilePath;
                                        fileMessageResult.FilePath = objAction.FilePath;
                                        fileMessageResult.UserName = UserInfor.Username;
                                        //TODO: chamge fullname
                                        fileMessageResult.FullName = UserInfor.FullName;
                                        fileMessageResult.Action = FileAction.All;
                                        fileMessageResult.LockedByUserName = userCheckouted;
                                        //TODO: chamge fullname
                                        fileMessageResult.LockedByFullName = userCheckouted;
                                        fileMessageResult.FileMD5 = objAction.FileMD5;

                                        switch (objAction.Action)
                                        {
                                            case FileAction.CheckEditable:
                                                await fsend(id, MessageType.FileAction, fileMessageResult);
                                                break;
                                            default:
                                                foreach (var s in wss)
                                                {
                                                    await fsend(s.Key, MessageType.FileAction, fileMessageResult);
                                                }
                                                break;
                                        }
                                    }
                                    break;
                                case MessageType.Message:
                                    objSend.UserName = UserInfor.Username;
                                    objSend.FullName = UserInfor.FullName;
                                    objSend.Message = obj.Value;
                                    foreach (var s in wss)
                                    {
                                        if (s.Value.id != id) await fsend(s.Key, MessageType.Message, objSend);
                                    }
                                    break;
                                case MessageType.APICommand:
                                    var objAPICommands = JsonConvert.DeserializeObject<APIMessageObject>(obj.Value);
                                    objSend.UserName = UserInfor.Username;
                                    objSend.FullName = UserInfor.FullName;
                                    objSend.Message = objAPICommands.Message;
                                    if ((objAPICommands.ToList == null) || (objAPICommands.ToList.Count == 0))
                                    {
                                        foreach (var s in wss)
                                        {
                                            await fsend(s.Key, MessageType.Message, objSend);
                                        }
                                    }
                                    else
                                    {
                                        foreach (var s in wss)
                                        {
                                            if (objAPICommands.ToList.Contains(s.Value.UserInfor.Username)) await fsend(s.Key, MessageType.Message, objSend);
                                        }
                                    }
                                    break;
                                case MessageType.GetConcurrentUsers:
                                    var userList = new List<LoginObject>();
                                    foreach (var s in wss)
                                    {
                                        //if (userList.Find(x => x.Username == s.Value.UserInfor.Username) )
                                        var temp = new LoginObject();
                                        temp.Username = s.Value.UserInfor.Username;
                                        temp.FullName = s.Value.UserInfor.FullName;
                                        temp.token = s.Value.UserInfor.Token;
                                        userList.Add(temp);
                                    }
                                    await fsend(id, MessageType.GetConcurrentUsers, userList);
                                    break;
                                case MessageType.GetAllFileInformation:
                                default: break;
                            }
                            break;
                        case WebSocketMessageType.Close:
                            WebSocketConnectionManagerMiddleware ctemp;
                            if (wss.TryRemove(id, out ctemp))
                            {
                                await ctemp.fclr();
                            }
                            break;
                        default: break;
                    }
                }
                catch(Exception ex)
                {
                    throw new Exception("Web Socket: Error");
                }
            }
        }
        async Task<bool> fbsenda(ArraySegment<byte> parrseg)
        {
            bool b = true;
            try
            {
                await _ws.SendAsync(parrseg, WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch
            {
                b = false;
            }
            return b;
        }
        private async Task fclr()
        {
            try
            {
                await _ws.CloseOutputAsync(WebSocketCloseStatus.Empty, "idle", CancellationToken.None);
                _ws.Dispose();
                id = 0;
            }
            catch { }
        }
        //statics
        static async Task fsend(int iid, string mt, object obj)
        {
            var ws = wss[iid];//maychangeto:wss.where..and static to
            var stosend = JsonConvert.SerializeObject(obj);

            if (!await ws.fbsenda(farrsegb(mt, stosend)))
            {
                WebSocketConnectionManagerMiddleware cst;
                if (wss.TryRemove(ws.id, out cst))
                {
                    await cst.fclr();
                }
            }
        }
        static async Task fsendLoginFail(int iid)
        {
            var ws = wss[iid];//maychangeto:wss.where..and static to            
            if (!await ws.fbsenda(farrsegb("loginfail", "")))
            {
                WebSocketConnectionManagerMiddleware cst;
                if (wss.TryRemove(ws.id, out cst))
                {
                    await cst.fclr();
                }
            }
        }

        #region helpers
        static ArraySegment<byte> farrsegb(string skey, string sval) { csockdata csd = new csockdata { MessageType = skey, Value = sval }; var csdj = JsonConvert.SerializeObject(csd); byte[] arrb = System.Text.Encoding.UTF8.GetBytes(csdj); return new ArraySegment<byte>(arrb);/*deleteifyouincludein fsenda*/ }
        #endregion

    }
}
