
using EventSystem.ViewModels.EventViewModel;

namespace EventSystem.Services.Interfaces
{
    public interface IEventService
    {
        Task<IOrderedEnumerable<DetailsEventViewModel>> GetEventsByFilters(string? searchTerm, string? location,
            DateTime? date);
        Task<IOrderedEnumerable<DetailsEventViewModel>> GetEventsByDate(string searchTerm);
        Task<bool> CreateEvent(CreateEventViewModel vieMmodel);
        Task<EditEventViewModel?> GetEventForEdit(Guid id);
        Task<bool> EditEvent(EditEventViewModel viewModel);
        Task<bool> Attend(string eventId);
        Task<DetailsEventViewModel?> GetEventDetails(string id);
        Task<DeleteEventViewModel?> GetEventForDelete(Guid id);
        Task<ICollection<PersonInfo>?> GetUsersInfo(string term, string eventId);
        Task<bool> AddPersonToEvent(string personId, string eventId);
        Task<UserList> GetUsersInEvent(string eventId);
        Task<bool> RemoveUser(string userId, string eventId);

    }
}
