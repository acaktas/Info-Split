using InfoWebApp.Models;
using Microsoft.AspNet.SignalR.Hubs;

namespace InfoWebApp.Hub
{
    [HubName("ArticleHub")]
    public class ArticleHub : Microsoft.AspNet.SignalR.Hub
    {
        public void AddNewMessageToPage(string name, Article article)
        {
            Clients.All.addNewMessageToPage(name, article);
        }
    }
}