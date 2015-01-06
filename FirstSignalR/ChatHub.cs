using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;

namespace FirstSignalR
{
    public class ChatHub : Hub
    {
        //public void Send(string name, string message)
        //{
        //    Clients.All.addNewMessageToPage(name, message);
        //}

        //声明静态变量存储当前在线用户
        public static class UserHandler
        {
            public static Dictionary<string, string> ConnectedIds = new Dictionary<string, string>();
        }  

        //用户进入页面时执行的（连接操作）
        public void userConnected(string name)
        {
            //进行编码，防止XSS攻击
            name = HttpUtility.HtmlEncode(name);
            string message = "用户" + name + "登录";

            //新增目前使用者的在线清单
            UserHandler.ConnectedIds.Add(Context.ConnectionId, name);

            //发送消息给其他人
            Clients.Others.addList(Context.ConnectionId, name);
            Clients.Others.hello(message);

            //发送消息给自己,并显示上线清单
            Clients.Caller.getList(UserHandler.ConnectedIds.Select(p => new { id = p.Key, name = p.Value }).ToList());

            
        }
        //发送消息给所有人
        public void sendAllMessage(string message)
        {
            message = HttpUtility.HtmlEncode(message);
            var name = UserHandler.ConnectedIds.Where(p => p.Key == Context.ConnectionId).FirstOrDefault().Value;
            message = name + "说：" + message;
            Clients.All.sendAllMessage(message);
        }

        //发送消息给特定的人
        public void sendMessage(string ToId,string message)
        {
            message = HttpUtility.HtmlEncode(message);
            var fromName = UserHandler.ConnectedIds.Where(p => p.Key == Context.ConnectionId).FirstOrDefault().Value;
            var toName = UserHandler.ConnectedIds.Where(p => p.Key == ToId).FirstOrDefault().Value;
            var frommessage = "<span style='color:red'>你悄悄对" + toName + "说</span>：" + message;
            var tomessage = fromName + "<span style='color:red'>悄悄对你说</span>：" + message;
           
            Clients.Client(Context.ConnectionId).sendMessage(frommessage); // 显示给发送人

            Clients.Client(ToId).sendMessage(tomessage); //显示给接收人
        }

        //当前使用者断线时执行
        public override Task OnDisconnected(bool stopCalled)
        {
            //当使用者离开时，移除在清单内的ConnectionId
            Clients.All.removeList(Context.ConnectionId);
            UserHandler.ConnectedIds.Remove(Context.ConnectionId);
            return base.OnDisconnected(stopCalled);
        }



      
        
    }
}