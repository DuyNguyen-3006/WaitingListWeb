using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaitingListWeb.Application.DTOs;

namespace WaitingListWeb.Application.Interface
{
    public interface IWaitingListService
    {
        Task<Guid> CreateAndNotifyAsync(WaitingEntryDTO dto, CancellationToken ct = default);

    }
}
