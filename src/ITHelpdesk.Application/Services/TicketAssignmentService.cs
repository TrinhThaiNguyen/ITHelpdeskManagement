using ITHelpdesk.Application.Interfaces.Repositories;
using ITHelpdesk.Application.Interfaces.Services;
using ITHelpdesk.Application.DTOs.Tickets;
using ITHelpdesk.Domain.Entities;

namespace ITHelpdesk.Application.Services;

public class TicketAssignmentService : ITicketAssignmentService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ITicketHistoryRepository _ticketHistoryRepository;
    private readonly INotificationService _notificationService;
    private readonly INotificationRecipientRepository _notificationRecipientRepository;

    public TicketAssignmentService(
        ITicketRepository ticketRepository,
        IEmployeeRepository employeeRepository,
        ITicketHistoryRepository ticketHistoryRepository,
        INotificationService notificationService,
        INotificationRecipientRepository notificationRecipientRepository)
    {
        _ticketRepository = ticketRepository;
        _employeeRepository = employeeRepository;
        _ticketHistoryRepository = ticketHistoryRepository;
        _notificationService = notificationService;
        _notificationRecipientRepository = notificationRecipientRepository;
    }

    public async Task AssignAsync(
        Guid ticketId,
        AssignTicketRequest request,
        CancellationToken cancellationToken = default)
    {
        // 1. Lấy Ticket cần cập nhật
        var ticket = await _ticketRepository.GetByIdForUpdateAsync(
            ticketId, cancellationToken);

        if (ticket == null)
            throw new KeyNotFoundException("Ticket not found.");

        // 2. Kiểm tra nhân viên được phân công
        var employee = await _employeeRepository.GetByIdAsync(
            request.AssigneeId, cancellationToken);

        if (employee == null)
            throw new KeyNotFoundException("Employee not found.");

        if (!employee.IsActive)
            throw new ArgumentException("Cannot assign ticket to an inactive employee.");

        // 3. Không ghi lịch sử nếu người phụ trách không thay đổi
        if (ticket.AssigneeId == employee.Id)
            return;

        // 4. Lưu người phụ trách cũ
        var oldAssigneeId = ticket.AssigneeId;

        // 5. Cập nhật người phụ trách mới
        ticket.AssigneeId = employee.Id;
        ticket.UpdatedAt = DateTime.UtcNow;

        // 6. Ghi nhận lịch sử phân công
        var history = new TicketHistory
        {
            TicketId = ticket.Id,
            Action = "AssigneeChanged",
            OldValue = oldAssigneeId?.ToString() ?? "Unassigned",
            NewValue = employee.Id.ToString(),
            ChangedBy = "System"
        };

        await _ticketHistoryRepository.AddAsync(
            history, cancellationToken);

        // 7. Lưu Ticket và History
        await _ticketRepository.UpdateAsync(
            ticket, cancellationToken);

        // 8. Send notification to assigned employee
        var assigneeUserId =
            await _notificationRecipientRepository.GetUserIdByEmployeeIdAsync(
                employee.Id);

        if (assigneeUserId.HasValue)
        {
            await _notificationService.NotifyTicketAssignedAsync(
                assigneeUserId.Value,
                ticket.Id,
                ticket.Title);
        }
    }

    public async Task UnassignAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default)
    {
        // 1. Lấy Ticket cần cập nhật
        var ticket = await _ticketRepository.GetByIdForUpdateAsync(
            ticketId, cancellationToken);

        if (ticket == null)
            throw new KeyNotFoundException("Ticket not found.");

        // 2. Nếu Ticket chưa có người phụ trách thì không cần xử lý
        if (ticket.AssigneeId == null)
            return;

        // 3. Lưu ID nhân viên cũ
        var oldAssigneeId = ticket.AssigneeId;

        // 4. Hủy phân công
        ticket.AssigneeId = null;
        ticket.UpdatedAt = DateTime.UtcNow;

        // 5. Ghi lịch sử thay đổi
        var history = new TicketHistory
        {
            TicketId = ticket.Id,
            Action = "AssigneeChanged",
            OldValue = oldAssigneeId.ToString(),
            NewValue = "Unassigned",
            ChangedBy = "System"
        };

        await _ticketHistoryRepository.AddAsync(
            history, cancellationToken);

        // 6. Lưu thay đổi
        await _ticketRepository.UpdateAsync(
            ticket, cancellationToken);
    }

    public async Task<TicketAssignmentResponse> GetAssignmentAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default)
    {
        // 1. Kiểm tra Ticket
        var ticket = await _ticketRepository.GetByIdAsync(
            ticketId, cancellationToken);

        if (ticket == null)
            throw new KeyNotFoundException("Ticket not found.");

        // 2. Ticket chưa có người phụ trách
        if (ticket.AssigneeId == null)
        {
            return new TicketAssignmentResponse
            {
                TicketId = ticket.Id,
                AssigneeId = null,
                AssigneeName = null,
                AssigneeEmail = null,
                AssignedAt = null
            };
        }

        // 3. Lấy thông tin nhân viên
        var employee = await _employeeRepository.GetByIdAsync(
            ticket.AssigneeId.Value, cancellationToken);

        // 4. Lấy lịch sử phân công gần nhất
        var histories = await _ticketHistoryRepository.GetByTicketIdAsync(
            ticket.Id, cancellationToken);

        var assignedHistory = histories
            .Where(h =>
                h.Action == "AssigneeChanged" &&
                h.NewValue == ticket.AssigneeId.Value.ToString())
            .OrderByDescending(h => h.CreatedAt)
            .FirstOrDefault();

        // 5. Trả về kết quả
        return new TicketAssignmentResponse
        {
            TicketId = ticket.Id,
            AssigneeId = employee?.Id,
            AssigneeName = employee?.FullName,
            AssigneeEmail = employee?.Email,
            AssignedAt = assignedHistory?.CreatedAt
        };
    }
}