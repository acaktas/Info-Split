using Info.Models;
using System.Threading.Tasks;

namespace Info.Messengers
{
    public interface IMessenger
    {
        Task SendMessageAsync(Article article);
    }
}
