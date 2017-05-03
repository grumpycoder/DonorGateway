using DonorGateway.Domain;

namespace DonorGateway.RSVP.Interfaces
{
    public interface IEventService
    {
        Event GetEvent(string name);
        Event GetEvent(int eventId);
        Guest FindGuestByFinder(string modelPromoCode);
        Guest GetGuest(int dtoGuestId);
    }
}