// using AspNetCore_Template.model.DTO.request.history;
// using AspNetCore_Template.model.entity;
// using AspNetCore_Template.repository.abs;
//
// namespace AspNetCore_Template.repository;
//
// public interface IActivityHistoryRepository : IEntityWithActivityRepository<ActivityHistory>
// {
//     IQueryable<ActivityHistory> ApplyFilters(long userId, ActivityHistoryFilterRequest filter);
// }
//
// public class ActivityHistoryRepository(AppDbContext context) : EntityWithActivityRepository<ActivityHistory>(context), IActivityHistoryRepository
// {
//     public IQueryable<ActivityHistory> ApplyFilters(long userId, ActivityHistoryFilterRequest filter)
//     {
//         var query = context.ActivityHistories.AsQueryable();
//         query = query.Where(h => h.UserId == userId);
//
//         if (filter.ActivityId.HasValue)
//             query = query.Where(h => h.ActivityId == filter.ActivityId);
//
//         if (filter.RoleId.HasValue)
//             query = query.Where(h => h.Activity.CategoryId == filter.RoleId);
//
//         if (filter.CategoryId.HasValue)
//             query = query.Where(h => h.Activity.RoleId == filter.CategoryId);
//
//         if (filter.IsFromToDoList.HasValue)
//             query = query.Where(h => h.Activity.IsOnToDoList == filter.IsFromToDoList);
//
//         if (filter.IsUnavoidable.HasValue)
//             query = query.Where(h => h.Activity.IsUnavoidable == filter.IsUnavoidable);
//
//         if (filter.DateFrom.HasValue)
//             query = query.Where(h => h.StartTimestamp >= filter.DateFrom);
//
//         if (filter.DateTo.HasValue)
//             query = query.Where(h => h.StartTimestamp <= filter.DateTo);
//
//         if (filter.HoursBack.HasValue)
//         {
//             var cutoffTime = DateTime.UtcNow.AddHours(-filter.HoursBack.Value);
//             query = query.Where(h => h.StartTimestamp >= cutoffTime);
//         }
//
//         return query;
//     }
// }