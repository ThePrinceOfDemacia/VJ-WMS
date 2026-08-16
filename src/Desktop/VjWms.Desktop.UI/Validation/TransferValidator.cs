using FluentValidation;
using VjWms.Desktop.UI.ViewModels.Transfers;

namespace VjWms.Desktop.UI.Validation;

public class TransferValidator : AbstractValidator<TransferCreateViewModel>
{
    public TransferValidator()
    {
        RuleFor(x => x.SelectedSourceWarehouse)
            .NotNull()
            .WithMessage("Vui lòng chọn kho nguồn / Please select source warehouse.");

        RuleFor(x => x.SelectedDestWarehouse)
            .NotNull()
            .WithMessage("Vui lòng chọn kho đích / Please select destination warehouse.");

        RuleFor(x => x)
            .Must(x => x.SelectedSourceWarehouse?.Id != x.SelectedDestWarehouse?.Id)
            .When(x => x.SelectedSourceWarehouse != null && x.SelectedDestWarehouse != null)
            .WithMessage("Kho nguồn và kho đích không được trùng nhau / Source and destination must differ.");

        RuleFor(x => x)
            .Must(x => !(x.TransferType == "Logical" && (x.SelectedSourceWarehouse?.Type == "Physical" || x.SelectedDestWarehouse?.Type == "Physical")))
            .WithMessage("Kho vật lý không được luân chuyển trên phần mềm (Logical) / Physical warehouses cannot use logical transfers.");

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
