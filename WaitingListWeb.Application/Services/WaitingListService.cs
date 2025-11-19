using Mapster;
using Microsoft.Extensions.Logging;
using System.Net;
using WaitingListWeb.Application.DTOs;
using WaitingListWeb.Application.Interface;
using WaitingListWeb.Domain.Abstraction;
using WaitingListWeb.Domain.Entities;
using WaitingListWeb.Shared.ApiResponse;

namespace WaitingListWeb.Infrastructure.Implementation
{
    public class WaitingListService : IWaitingListService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly ILogger<WaitingListService> _logger;

        public WaitingListService(
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            ILogger<WaitingListService> logger)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<ApiResponse<object>> CreateAndNotifyAsync(WaitingEntryDTO dto, CancellationToken ct = default)
        {
            var repo = _unitOfWork.GetRepository<WaitingListEntry>();
            string email = dto.Email.Trim().ToLowerInvariant();

            // 1) Check existing
            var existing = await repo.FindByConditionAsync(e => e.Email == email, ct);
            if (existing != null)
            {
                _logger.LogInformation("Email already exists: {Email}", email);

                return ApiResponse<object>.Success(
                    data: null,
                    message: "This email is already registered."
                );
            }

            // 2) Create entry
            var entry = new WaitingListEntry(email)
            {
                FirstName = string.IsNullOrWhiteSpace(dto.FirstName) ? null : dto.FirstName.Trim(),
                LastName = string.IsNullOrWhiteSpace(dto.LastName) ? null : dto.LastName.Trim(),
                PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber.Trim(),
                WishMessage = string.IsNullOrWhiteSpace(dto.WishMessage) ? null : dto.WishMessage.Trim()
            };

            await repo.InsertAsync(entry, ct);
            await _unitOfWork.SaveChangeAsync();

            // 3) Send email
            var name = $"{entry.FirstName} {entry.LastName}".Trim();
            if (string.IsNullOrEmpty(name)) name = "Friend";

            var templateData = new Dictionary<string, string>
            {
                ["FullName"] = name,
                ["CreatedAtUtc"] = entry.CreatedAtUtc.ToString("yyyy-MM-dd HH:mm:ss"),
                ["WishMessage"] = entry.WishMessage ?? ""
            };

            bool emailOK = await _emailService.SendEmailAsync(
                entry.Email,
                name,
                "WelcomeTemplate.html",
                templateData
            );

            if (!emailOK)
                _logger.LogWarning("Failed to send welcome email to {Email}", entry.Email);

            return ApiResponse<object>.Success(
                data: null,
                message: emailOK
                    ? "Successfully joined the waiting list."
                    : "Joined the waiting list, but failed to send confirmation email."
            );
        }

        public async Task<ApiResponse<BasePaginatedList<WaitingEntryDTO>>> GetAllEntriesAsync(
    int pageNumber,
    int pageSize,
    CancellationToken ct = default)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            try
            {
                _logger.LogInformation("Retrieving waiting list entries page {Page} size {Size}.",
                    pageNumber, pageSize);

                var repo = _unitOfWork.GetRepository<WaitingListEntry>();
                var entries = await repo.GetAllAsync(); // nếu sau này có IQueryable thì tối ưu sau

                var ordered = entries.OrderByDescending(x => x.CreatedAtUtc).ToList();
                var count = ordered.Count;

                var pageItems = ordered
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Adapt<IReadOnlyCollection<WaitingEntryDTO>>();

                var pagedResult = new BasePaginatedList<WaitingEntryDTO>(
                    pageItems, count, pageNumber, pageSize);

                return ApiResponse<BasePaginatedList<WaitingEntryDTO>>.Success(
                    pagedResult,
                    "Retrieved paged waiting list entries successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged waiting list entries.");
                return ApiResponse<BasePaginatedList<WaitingEntryDTO>>.Fail(
                    HttpStatusCode.InternalServerError,
                    "INTERNAL_ERROR",
                    "An error occurred while retrieving paged waiting list entries.");
            }
        }


    }
}
