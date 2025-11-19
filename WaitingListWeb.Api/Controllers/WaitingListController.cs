using Mailjet.Client.TransactionalEmails;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WaitingListWeb.Application.DTOs;
using WaitingListWeb.Application.Interface;

namespace WaitingListWeb.Api.Controllers
{
    [ApiController]
    [Route("api/waiting-list")]
    public class WaitingListController : ControllerBase
    {
        private readonly IWaitingListService _service;

        public WaitingListController(IWaitingListService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Join([FromForm] WaitingEntryDTO dto)
        {
            var id = await _service.CreateAndNotifyAsync(dto);
            return Ok(new { id });
        }
        [HttpGet]
        public async Task<IActionResult> GetPagedEntries([FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            try
            {
                var entries = await _service.GetAllEntriesAsync(page, size);
                return Ok(entries);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the waiting list entries.");
            }
        }
    }
}