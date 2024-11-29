using EventSystem.Data.Enum;
using EventSystem.Data.Models;
using EventSystem.Repositories.Interfaces;
using EventSystem.Services.Interfaces;
using EventSystem.ViewModels.EventViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSystem.Services
{
    public class EventService : BaseService, IEventService
    {
        private readonly IRepository<Event> _eventRepository;
        private readonly IRepository<UserEvent> _userEventRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public EventService(
            IRepository<Event> eventRepository
            ,IHttpContextAccessor httpContextAccessor
            ,IRepository<UserEvent> userEventRepository
            ,UserManager<ApplicationUser> userManager) : base(httpContextAccessor) 
        {
            _eventRepository = eventRepository;
            _userEventRepository = userEventRepository;
            _userManager = userManager;
        }
        public async Task<IOrderedEnumerable<DetailsEventViewModel>> GetEventsByFilters(string? searchTerm, string? location, DateTime? date)
        {
            
            var query = _eventRepository
                .GetAllAttached()
                .Include(e => e.Host)
                .Where(e => !e.IsDeleted);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(e =>
                    e.Name.ToLower().Contains(searchTerm.ToLower()) ||
                    e.Description.ToLower().Contains(searchTerm.ToLower()) ||
                    e.LocationName.ToLower().Contains(searchTerm.ToLower()));
            }

            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(e => e.LocationName.ToLower() == location.ToLower());
            }

            if (date.HasValue)
            {
                query = query.Where(e => e.Date.Date == date.Value.Date); 
            }

            var entities = await query.ToListAsync();

            var events = entities.Select(e => new DetailsEventViewModel()
            {
                id = e.id,
                Name = e.Name,
                HostName = e.Host.UserName ?? string.Empty,
                Description = e.Description,
                Date = e.Date,
                LocationName = e.LocationName
            }).OrderBy(e => e.Date);

            return events;
        }

        public async Task<IOrderedEnumerable<DetailsEventViewModel>> GetEventsByDate(string searchTerm)
        {
                var entities = string.IsNullOrEmpty(searchTerm)
          ? await _eventRepository.GetAllAttached().Include(e => e.Host).ToListAsync()
          : await _eventRepository
          .GetAllAttached()
          .Include(e => e.Host)
          .Where(e =>
              e.Name.ToLower().Contains(searchTerm.ToLower()) ||
              e.Description.ToLower().Contains(searchTerm.ToLower()) ||
              e.LocationName.ToLower().Contains(searchTerm.ToLower()))
          .ToListAsync();

            var events = entities.Select(e => new DetailsEventViewModel()
            {
                id = e.id,
                Name = e.Name,
                HostName = e.Host.UserName ?? string.Empty,
                Description = e.Description,
                Date = e.Date,
                LocationName = e.LocationName
            }).OrderBy(e => e.Date);

            return events;
        }

        public async Task<bool> CreateEvent(CreateEventViewModel viewModel)
        {

            bool isDateValid = DateTime.TryParseExact(viewModel.Date, "yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime eventDate);
            if (!isDateValid)
            {
                return false;
            }
            else if (eventDate < DateTime.Now)
            {

                return false;
            }

            Event newEvent = new Event()
            {
                Name = viewModel.Name,
                Description = viewModel.Description,
                Date = eventDate,
                LocationName = viewModel.LocationName,
                HostId = GetUserId(),
                IsDeleted = false,
                Latitude = viewModel.Latitude,
                Longitude = viewModel.Longitude,
            };

            UserEvent newUserEvent = new()
            {
                EventId = newEvent.id,
                UserId = GetUserId(),
                AttendStatus = AttendStatus.Host
            };

            await _eventRepository.AddAsync(newEvent);
            await _userEventRepository.AddAsync(newUserEvent);


            return true;
        }

        public async Task<bool> EditEvent(EditEventViewModel viewModel)
        {
            var eventToUpdate = await _eventRepository.FirstOrDefaultAsync(e=> e.id == viewModel.id);

            bool isDateValid = DateTime.TryParseExact(viewModel.Date, "yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime eventDate);
            if (!isDateValid)
            {
                return false;
            }

            eventToUpdate.Name = viewModel.Name;
            eventToUpdate.Description = viewModel.Description;
            eventToUpdate.Date = eventDate;
            eventToUpdate.LocationName = viewModel.Location;
            eventToUpdate.Latitude = viewModel.Latitude;
            eventToUpdate.Longitude = viewModel.Longitude;

            await _eventRepository.AddAsync(eventToUpdate);

            return true;
        }

        public async Task<EditEventViewModel?> GetEventForEdit(Guid id)
        {


            Event? eventToEdit = await _eventRepository.FirstOrDefaultAsync(e=> e.id == id);

            if (eventToEdit == null)
            {
                return null;
            }


            EditEventViewModel viewModel = new EditEventViewModel()
            {
                id = eventToEdit.id,
                Name = eventToEdit.Name,
                Description = eventToEdit.Description,
                Date = eventToEdit.Date.ToString("yyyy-MM-ddTHH:mm"),
                Location = eventToEdit.LocationName,
                Latitude = eventToEdit.Latitude,
                Longitude = eventToEdit.Longitude,
            };

            return viewModel;
        }


        public async Task<bool> Attend(string eventId)
        {
            bool isEventGuidValid = Guid.TryParse(eventId, out var eventGuid);

            if (!isEventGuidValid)
            {
                return false;
            }


            var userId = GetUserId();

            if (string.IsNullOrEmpty(userId.ToString()))
            {
                return false;
            }

            var isAttending = await _userEventRepository.GetAllAttached()
                .Where(ue => ue.UserId == userId && ue.EventId == eventGuid)
                .FirstOrDefaultAsync();

            if (isAttending != null)
            {
                return false;
            }

            UserEvent userEvent = new()
            {
                UserId = userId,
                EventId = eventGuid,
                AttendStatus = AttendStatus.Attending
            };

            await _userEventRepository.AddAsync(userEvent);


            return true;
        }

       public async Task<DetailsEventViewModel?> GetEventDetails(string id)
        {
            bool isEventGuidValid = Guid.TryParse(id, out var eventId);

            if (!isEventGuidValid)
            {
                return null;
            }

            var eventDetails = await _eventRepository.GetAllAttached()
                .Include(e => e.Host)
                .FirstOrDefaultAsync(e => e.id == eventId);

            if (eventDetails == null)
            {
                return null;
            }

            DetailsEventViewModel viewModel = new()
            {
                id = eventId,
                Name = eventDetails.Name,
                Description = eventDetails.Description,
                Date = eventDetails.Date,
                LocationName = eventDetails.LocationName,
                HostName = eventDetails.Host.UserName ?? string.Empty,
                Longitude = eventDetails.Longitude,
                Latitude = eventDetails.Latitude,
            };
            var userId = GetUserId();

            viewModel.PopleAttending = await _userEventRepository.GetAllAttached()
                .Include(e => e.User)
                .Where(e => e.UserId == userId && e.EventId == viewModel.id)
                .Select(e => new PersonInfo()
                {
                    Id = e.UserId,
                    Name = e.User.UserName ?? string.Empty,
                    AttendStatus = (int)e.AttendStatus
                }).ToListAsync();

            viewModel.Atendees = viewModel.PopleAttending.Count;

            var isAttending = await _userEventRepository.GetAllAttached()
                .Where(ue => ue.UserId == userId && ue.EventId == eventId)
                .FirstOrDefaultAsync();

            if (isAttending != null)
            {
                viewModel.IsAttending = true;
            }

            return viewModel;
        }

        public async Task<DeleteEventViewModel?> GetEventForDelete(Guid id)
        {
            Event? eventToDelete = await _eventRepository.FirstOrDefaultAsync(e=> e.id == id);

            if (eventToDelete == null)
            {
                return null;
            }

            DeleteEventViewModel viewModel = new DeleteEventViewModel
            {
                Id = eventToDelete.id,
                Name = eventToDelete.Name,
                Description = eventToDelete.Description,
                Date = eventToDelete.Date,
                Location = eventToDelete.LocationName
            };

            return viewModel;
        }

        public async Task<ICollection<PersonInfo>?> GetUsersInfo(string term, string eventId)
        {
            bool isEventGuidValid = Guid.TryParse(eventId, out var eventGuid);

            if (!isEventGuidValid)
            {
                return null;
            }

            var usersGuidInEvent = await _userEventRepository.GetAllAttached()
                .Where(ue => ue.EventId == eventGuid)
                .Select(ue => ue.UserId)
                .ToListAsync();

            var users = _userManager.Users
         .Where(u => u.Id != GetUserId() 
         && (u.UserName.ToLower().Contains(term) 
         || u.Email.ToLower().Contains(term)) 
         && usersGuidInEvent.Contains(u.Id) == false)
         .Select(u => new PersonInfo()
         {
             Id = u.Id,
             Name = u.UserName ?? string.Empty
         })
         .ToList();

            return users;
        }

        public async Task<bool> AddPersonToEvent(string personId, string eventId)
        {
            bool isEventGuidValid = Guid.TryParse(eventId, out var eventGuid);

            if (!isEventGuidValid)
            {
                return false;
            }

            bool isUserGuidValid = Guid.TryParse(personId, out var userGuid);

            if (!isUserGuidValid)
            {
                return false;
            }
            var isEventValid = await _eventRepository.GetAllAttached().FirstOrDefaultAsync(e => e.id == eventGuid);

            if (isEventValid == null)
            {
                return false;
            }

            var person = _userManager.Users.FirstOrDefault(u => u.Id == userGuid);
            if (person == null)
            {
                return false;
            }

            UserEvent userEventToAdd = new()
            {
                UserId = userGuid,
                EventId = eventGuid,
                AttendStatus = AttendStatus.Invited
            };

            await _userEventRepository.AddAsync(userEventToAdd);

            return true;
        }
    }
}
