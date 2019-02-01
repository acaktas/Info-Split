using Models;
using System.Threading.Tasks;

namespace Messengers
{
    interface IMessenger
    {
        Task SendMessageAsync(Article article);
    }
}
