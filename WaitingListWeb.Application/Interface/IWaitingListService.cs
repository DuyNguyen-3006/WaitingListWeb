using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaitingListWeb.Application.DTOs;
using WaitingListWeb.Domain.Abstraction;
using WaitingListWeb.Shared.ApiResponse;

namespace WaitingListWeb.Application.Interface
{
    public interface IWaitingListService
    {
        Task<ApiResponse<object>> CreateAndNotifyAsync(WaitingEntryDTO dto, CancellationToken ct = default);
        Task<ApiResponse<BasePaginatedList<WaitingEntryDTO>>> GetAllEntriesAsync(int pageNumber, int pageSize,CancellationToken ct = default);
    }
}
