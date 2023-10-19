using Microsoft.AspNet.SignalR;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SignalR_Demo.Hubs
{
    public class PhoneNumberHub : Hub<IPhoneNumberClient>
    {
        private const string DEFAULT_PHONE_NUMBER = "1-800-123-4567";

        private static ConcurrentQueue<string> _availablePhoneNumbers = null;

        private static ConcurrentQueue<PhoneNumberAssignment> _assignedPhoneNumbers = null;

        private static List<string> _defaultPhoneNumberConnectionIds = null;

        public PhoneNumberHub()
        {
            if (_availablePhoneNumbers == null)
            {
                _availablePhoneNumbers = new ConcurrentQueue<string>();
                _assignedPhoneNumbers = new ConcurrentQueue<PhoneNumberAssignment>();
                _defaultPhoneNumberConnectionIds = new List<string>();

                _availablePhoneNumbers.Enqueue("1-800-111-1111");
                _availablePhoneNumbers.Enqueue("1-800-222-2222");
                _availablePhoneNumbers.Enqueue("1-800-333-3333");
                //_availablePhoneNumbers.Enqueue("1-800-444-4444");
            }
        }

        public async Task AssignNewPhoneNumber()
        {
            string newPhoneNumber = await AssignNextAvailablePhoneNumber();

            await Clients.Caller.UpdatePhoneNumber(newPhoneNumber);
        }

        public override async Task OnConnected()
        {
            await AssignNewPhoneNumber();
            await base.OnConnected();
        }

        public override async Task OnDisconnected(bool stopCalled)
        {
            await UnassignInUsePhoneNumber();
            await base.OnDisconnected(stopCalled);
        }

        private async Task<string> AssignNextAvailablePhoneNumber()
        {
            string phoneNumber = null;
            if (_availablePhoneNumbers.TryDequeue(out phoneNumber))
            {
                _defaultPhoneNumberConnectionIds.Remove(Context.ConnectionId);
            }
            else
            {
                // dequeue assigned phone number
                PhoneNumberAssignment oldAssignment = null;
                _assignedPhoneNumbers.TryDequeue(out oldAssignment);
                phoneNumber = oldAssignment.PhoneNumber;

                // Expire old user's phone number
                await Clients.Client(oldAssignment.ConnectionId).UpdatePhoneNumber(DEFAULT_PHONE_NUMBER);
                _defaultPhoneNumberConnectionIds.Add(oldAssignment.ConnectionId);
            }

            _assignedPhoneNumbers.Enqueue(new PhoneNumberAssignment()
            {
                ConnectionId = Context.ConnectionId,
                PhoneNumber = phoneNumber
            });

            return phoneNumber;
        }

        private async Task UnassignInUsePhoneNumber()
        {
            var disconnectedAssignment = _assignedPhoneNumbers.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (disconnectedAssignment != null)
            {
                _assignedPhoneNumbers =
                    new ConcurrentQueue<PhoneNumberAssignment>(
                        _assignedPhoneNumbers.Where(a =>
                            a != disconnectedAssignment)); // Ugly way to do a RemoveAt from queue

                //_defaultPhoneNumberConnectionIds.Remove(disconnectedAssignment.ConnectionId);

                _availablePhoneNumbers.Enqueue(disconnectedAssignment.PhoneNumber);

                //Give newly unassigned number to first user with a default phone number:
                var defaultPhoneNumberConnectionId = _defaultPhoneNumberConnectionIds.FirstOrDefault();
                _defaultPhoneNumberConnectionIds.RemoveAt(0);
                if (!string.IsNullOrEmpty(defaultPhoneNumberConnectionId))
                {
                    await Clients.Client(defaultPhoneNumberConnectionId).ExpirePhoneNumber();
                }
            }
            else
            {
                _defaultPhoneNumberConnectionIds.Remove(Context.ConnectionId);
            }

        }
    }
}