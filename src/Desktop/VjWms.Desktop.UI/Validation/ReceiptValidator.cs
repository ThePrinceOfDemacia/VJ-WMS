using FluentValidation;
using VjWms.Desktop.UI.ViewModels.Receipts;

namespace VjWms.Desktop.UI.Validation;

public class ReceiptValidator : AbstractValidator<ReceiptCreateViewModel>
{
    public ReceiptValidator()
    {
        RuleFor(x => x.SelectedWarehouse)
            .NotNull()
            .WithMessage("Vui lòng chọn kho nhập / Please select warehouse.");

        RuleFor(x => x.SelectedSupplier)
            .NotNull()
            .WithMessage("Vui lòng chọn nhà cung cấp / Please select supplier.");

        RuleFor(x => x.LineItems)
            .Must(lines => lines.Any(l => l.SelectedProduct != null && l.Quantity > 0))
            .WithMessage("Cần ít nhất 1 dòng hàng hóa hợp lệ / At least 1 valid line item required.");

        RuleForEach(x => x.LineItems).ChildRules(items =>
        {
            items.RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .When(x => x.SelectedProduct != null)
                .WithMessage("Số lượng phải lớn hơn 0 / Quantity must be > 0.");
        });
    }
}
