using Polymind.Domain.Common;
using Polymind.Domain.Enums;

namespace Polymind.Domain.Entities;

/// <summary>Phiếu thu/chi, sinh mã tự động + xuất PDF (mục 7.4).</summary>
public class Receipt : BaseEntity
{
    public string Code { get; set; } = default!; // RC-YYYYMMDD-XXXX
    public ReceiptType ReceiptType { get; set; }
    public Guid? CandidateId { get; set; }
    public Guid? AgentId { get; set; }
    public Guid? PaymentId { get; set; } // nguồn: khoản thu (nếu phiếu thu)
    public Guid? ExpenseId { get; set; } // nguồn: khoản chi (nếu phiếu chi)
    public Guid? LoanId { get; set; } // nguồn: lần thu nợ công ty
    public Guid? LoanRepaymentId { get; set; } // kỳ trả nợ cụ thể; null khi thu hết nhiều kỳ
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateOnly ReceiptDate { get; set; }
    public string? PdfUrl { get; set; }
    public Guid? SignedBy { get; set; }
    public Guid CreatedBy { get; set; }

    // Kho lưu trữ của RIÊNG phiếu thu/chi — độc lập với kho lưu trữ khoản thu.
    // Lưu trữ khoản thu của một ứng viên KHÔNG kéo theo phiếu của họ (chứng từ có vòng đời riêng).
    public DateTimeOffset? ArchivedAt { get; set; }
    public Guid? ArchivedBy { get; set; }
}
