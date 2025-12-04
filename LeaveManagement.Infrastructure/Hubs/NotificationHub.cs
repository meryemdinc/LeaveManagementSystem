using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace LeaveManagement.Infrastructure.Hubs
{
    // Bu sınıf bizim "Yayın Kulemiz" olacak.
    public class NotificationHub : Hub
    {
        // İstemciler (Frontend) bağlandığında çalışır
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        // İstemciler koptuğunda çalışır
        public override async Task OnDisconnectedAsync(System.Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}