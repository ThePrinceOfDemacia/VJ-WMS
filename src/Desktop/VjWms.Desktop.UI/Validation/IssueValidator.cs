using FluentValidation;
using VjWms.Desktop.UI.ViewModels.Issues;

namespace VjWms.Desktop.UI.Validation;

public class IssueValidator : AbstractValidator<IssueCreateViewModel>
{
    public IssueValidator()
    {
        RuleFor(x => x.SelectedWarehouse)
            .NotNull()
            .WithMessage("Vui lòng chọn kho xuất / Please select warehouse.");

        RuleFor(x => x.SelectedCustomer)
            .NotNull()
            .WithMessage("Vui lòng chọn khách hàng / Please select customer.");

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
