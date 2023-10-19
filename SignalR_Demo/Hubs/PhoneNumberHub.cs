using Microsoft.AspNet.SignalR;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace SignalR_Demo.Hubs
{
    public class PhoneNumberHub : Hub<IPhoneNumberClient>
    {
        private const string DEFAULT_PHONE_NUMBER = "1-800-123-4567";

        private static ConcurrentQueue<string> _availablePhoneNumbers = null;

        private static ConcurrentDictionary<string, string> _assignedPhoneNumbers = null;

        public PhoneNumberHub()
        {
            if (_availablePhoneNumbers == null)
            {
                _availablePhoneNumbers = new ConcurrentQueue<string>();
                _assignedPhoneNumbers = new ConcurrentDictionary<string, string>();

                _availablePhoneNumbers.Enqueue("1-800-111-1111");
                _availablePhoneNumbers.Enqueue("1-800-222-2222");
                _availablePhoneNumbers.Enqueue("1-800-333-3333");
                //_availablePhoneNumbers.Enqueue("1-800-444-4444");
            }
        }

        public async Task GenerateNewPhoneNumber()
        {
            string newPhoneNumber = await GetNextAvailablePhoneNumber();

            await Clients.Caller.UpdatePhoneNumber(newPhoneNumber);
        }

        public override async Task OnConnected()
        {
            await GenerateNewPhoneNumber();
            await base.OnConnected();
        }

        public override async Task OnDisconnected(bool stopCalled)
        {
            await ExpireInUsePhoneNumber();
            await base.OnDisconnected(stopCalled);
        }

        private async Task<string> GetNextAvailablePhoneNumber()
        {
            if (_availablePhoneNumbers.Count > 0)
            {
                string phoneNumber = null;
                _availablePhoneNumbers.TryDequeue(out phoneNumber);
                await Groups.Remove(Context.ConnectionId, DEFAULT_PHONE_NUMBER);
                _assignedPhoneNumbers.TryAdd(Context.ConnectionId, phoneNumber);
                return phoneNumber;
            }
            else
            {
                await Groups.Add(Context.ConnectionId, DEFAULT_PHONE_NUMBER);
                return DEFAULT_PHONE_NUMBER;
            }
        }

        private async Task ExpireInUsePhoneNumber()
        {
            string expiredPhoneNumber = null;
            if (_assignedPhoneNumbers.TryRemove(Context.ConnectionId, out expiredPhoneNumber))
            {
                _availablePhoneNumbers.Enqueue(expiredPhoneNumber);
                await Clients.Group(DEFAULT_PHONE_NUMBER).ExpirePhoneNumber();
            }
        }
    }
}