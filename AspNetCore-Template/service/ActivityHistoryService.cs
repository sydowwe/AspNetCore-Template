// using AspNetCore_Template.model.DTO.request.history;
// using AspNetCore_Template.model.DTO.response.history;
// using AspNetCore_Template.model.entity;
// using AspNetCore_Template.repository;
// using AspNetCore_Template.security;
// using AspNetCore_Template.service.abs;
// using AutoMapper;
// using AutoMapper.QueryableExtensions;
// using Microsoft.EntityFrameworkCore;
//
// namespace AspNetCore_Template.service;
//
// public interface IActivityHistoryService : IEntityWithActivityService<ActivityHistory, ActivityHistoryRequest, ActivityHistoryResponse>
// {
//     Task<List<ActivityHistoryListGroupedByDateResponse>> FilterAsync(ActivityHistoryFilterRequest filterRequest);
// }
//
// public class ActivityHistoryService(
//     IActivityHistoryRepository repository,
//     ILoggedUserService loggedUserService,
//     IMapper mapper)
//     : EntityWithActivityService<ActivityHistory, ActivityHistoryRequest, ActivityHistoryResponse, IActivityHistoryRepository>(repository,
//         activityService, loggedUserService, mapper), IActivityHistoryService
// {
//     public async Task<List<ActivityHistoryListGroupedByDateResponse>> FilterAsync(ActivityHistoryFilterRequest filterRequest)
//     {
//         var query = repository.ApplyFilters(loggedUserService.GetLoggedUserId(), filterRequest);
//
//         var historyResponses = await query.OrderBy(h => h.StartTimestamp)
//             .ProjectTo<ActivityHistoryResponse>(mapper.ConfigurationProvider).ToListAsync();
//
//         return historyResponses
//             .GroupBy(hr => hr.StartTimestamp.ToUniversalTime().Date)
//             .Select(group => new ActivityHistoryListGroupedByDateResponse(group.Key, group.OrderBy(h => h.StartTimestamp).ToList()))
//             .OrderBy(response => response.Date)
//             .ToList();
//     }
//
//     // public async Task<ActivityFormSelectsResponse> UpdateFilterSelectsAsync(ActivitySelectForm request)
//     // {
//     //     var loggedUserId = (await _userService.GetLoggedUserAsync()).Id;
//     //     var query = context.Histories.AsQueryable();
//     //
//     //     // Apply filters
//     //     query = applyFilters(query, loggedUserId, request);
//     //
//     //     var activityList = await query
//     //         .Select(h => h.Activity)
//     //         .Distinct()
//     //         .ToListAsync();
//     //
//     //     return await activityService.GetActivityFormSelectsFromActivityListAsync(activityList);
//     // }
// }