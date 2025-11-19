using Mapster;
using WaitingListWeb.Application.DTOs;
using WaitingListWeb.Application.Interface;
using WaitingListWeb.Domain.Abstraction;
using WaitingListWeb.Domain.Entities;
using Microsoft.Extensions.Logging;

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

        public async Task<Guid> CreateAndNotifyAsync(WaitingEntryDTO dto, CancellationToken ct = default)
        {
            var repo = _unitOfWork.GetRepository<WaitingListEntry>();

            string email = dto.Email.Trim().ToLowerInvariant();

            // 1) check existing
            var existing = await repo.FindByConditionAsync(e => e.Email == email, ct);
            if (existing != null)
            {
                _logger.LogInformation("Email already exists: {Email}", email);
                return existing.Id;
            }

            var entry = new WaitingListEntry(dto.Email.Trim().ToLowerInvariant())
            {
                FirstName = string.IsNullOrWhiteSpace(dto.FirstName) ? null : dto.FirstName.Trim(),
                LastName = string.IsNullOrWhiteSpace(dto.LastName) ? null : dto.LastName.Trim(),
                PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber.Trim(),
                WishMessage = string.IsNullOrWhiteSpace(dto.WishMessage) ? null : dto.WishMessage.Trim()
            };


            // 3) Insert  Save
            await repo.InsertAsync(entry, ct);
            await _unitOfWork.SaveChangeAsync();

            // 4) Send email
            var name = $"{entry.FirstName} {entry.LastName}".Trim();
            if (string.IsNullOrEmpty(name)) name = "Bạn";

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

            return entry.Id;
        }
    }
}
