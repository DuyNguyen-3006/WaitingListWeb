using Mapster;
using Microsoft.Extensions.Logging;
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

        public async Task<ApiResponse<IList<WaitingEntryDTO>>> GetAllEntriesAsync(CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Retrieving all waiting list entries.");

                var repo = _unitOfWork.GetRepository<WaitingListEntry>();
                var entries = await repo.GetAllAsync();

                var dtoList = entries.Adapt<IList<WaitingEntryDTO>>();

                return ApiResponse<IList<WaitingEntryDTO>>.Success(
                    data: dtoList,
                    message: "Retrieved all waiting list entries successfully."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving waiting list entries.");

                return ApiResponse<IList<WaitingEntryDTO>>.Fail(
                    status: System.Net.HttpStatusCode.InternalServerError,
                    code: "SERVER_ERROR",
                    message: "An error occurred while retrieving the data."
                );
            }
        }

    }
}
