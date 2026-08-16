# Phase 2 — Warehouse Transactions Offline: Implementation Plan

## Goal

Hoàn thiện Desktop Client offline với **tất cả loại giao dịch kho**, thêm **quét QR/Barcode**, **đính kèm file**, và **validation chuyên nghiệp** song ngữ. Khi Phase 2 hoàn thành, thủ kho có thể thực hiện mọi nghiệp vụ kho hàng ngày mà không cần mạng.

> [!IMPORTANT]
> Phase 1 đã xây dựng xong Receipt (Nhập kho) và Issue (Xuất kho) cơ bản. Phase 2 sẽ **bổ sung** Transfer (Luân chuyển), nâng cấp Receipt/Issue với scanning + attachments, và thêm FluentValidation.

---

## What Already Exists (Phase 1)

| Feature | Status | Notes |
|---|---|---|
| Receipt CRUD (Create/List/Delete) | ✅ Done | `ReceiptCreateViewModel`, `ReceiptListViewModel` |
| Issue CRUD (Create/List/Delete) | ✅ Done | `IssueCreateViewModel`, `IssueListViewModel` |
| Inventory Lookup | ✅ Done | `InventoryViewModel` |
| Master Data (Warehouse/Product List) | ✅ Done | `WarehouseListViewModel`, `ProductListViewModel` |
| Dashboard | ✅ Done | `DashboardViewModel` |
| MVVM + Navigation + DI | ✅ Done | `NavigationService`, `ShellViewModel` |
| Domain Entities (Transfer, Attachment) | ✅ Done | `LocalTransfer`, `LocalAttachment` in `Entities.cs` |
| DbContext (Transfer/Attachment tables) | ✅ Done | `LocalDbContext.cs` has DbSets |

---

## Proposed Changes

### Component 1: Transfer (Luân chuyển kho) — Full CRUD

Tạo module Luân chuyển nội bộ giữa các kho. Đây là tính năng mới hoàn toàn.

#### [NEW] ViewModels/Transfers/TransferListViewModel.cs
- Load danh sách phiếu luân chuyển từ SQLite
- Filter theo trạng thái (All/Draft/PendingSync/Rejected)
- Lệnh: Tạo mới, Xóa (chỉ Draft), Làm mới

#### [NEW] ViewModels/Transfers/TransferCreateViewModel.cs
- Form tạo phiếu: chọn Kho nguồn, Kho đích, Loại (Logical/Physical), Ngày, Ghi chú
- Thêm/xóa dòng hàng hóa: chọn Sản phẩm, SL, ĐVT, Số lô
- Validation: 2 kho khác nhau, ít nhất 1 dòng, SL > 0
- Lưu = Draft, Xác nhận = PendingSync
- Local number: `LOCAL/LC/{ngày}/{STT}`

#### [NEW] Views/Pages/Transfers/TransferListView.xaml + .cs
- DataGrid: Số phiếu, Kho nguồn, Kho đích, Loại, Ngày, Trạng thái
- Badge màu cho trạng thái

#### [NEW] Views/Pages/Transfers/TransferCreateView.xaml + .cs
- Form header (Kho nguồn, Kho đích, Loại, Ngày) + DataGrid dòng hàng
- Nút: Lưu nháp, Xác nhận, Hủy

#### [MODIFY] ViewModels/ShellViewModel.cs
- Thêm `NavigateToTransfersCommand`

#### [MODIFY] Views/ShellWindow.xaml
- Thêm sidebar button "🔄 Luân chuyển kho"
- Thêm DataTemplate cho `TransferListViewModel` → `TransferListView`
- Thêm DataTemplate cho `TransferCreateViewModel` → `TransferCreateView`

#### [MODIFY] App.xaml.cs
- Register `TransferListViewModel` và `TransferCreateViewModel` trong DI

---

### Component 2: Dual-Mode QR/Barcode Scanner

Hai chế độ quét:
1. **Screen QR**: Chọn file ảnh → decode bằng ZXing.Net
2. **HID USB Scanner**: Lắng nghe keyboard input từ máy quét cầm tay

#### [NEW] Services/Scanner/IScannerService.cs
```csharp
public interface IScannerService
{
    Task<string?> ScanFromImageAsync();  // Mode 1: file picker → ZXing decode
    void StartHidListening(Action<string> onScanned);  // Mode 2: HID
    void StopHidListening();
}
```

#### [NEW] Services/Scanner/ScannerService.cs
- **Mode 1 (ScreenQR)**: `OpenFileDialog` → load image → `ZXing.Net` `BarcodeReader` decode
- **Mode 2 (HID USB)**: Lắng nghe keyboard PreviewKeyDown trên Window, accumulate buffer, detect Enter → fire callback
  - USB barcode scanners act as keyboard: type characters + Enter
  - Buffer timer: if characters come < 50ms apart = scanner input, not human typing

#### [MODIFY] ViewModels/Receipts/ReceiptCreateViewModel.cs
- Thêm `ScanQrCommand` (Mode 1) và `StartHidScanCommand` / `StopHidScanCommand` (Mode 2)
- Khi scan thành công → auto-fill product code, lot number, quantity nếu có trong QR data
- Parse QR data format: `PRODUCT_CODE|LOT|QTY` hoặc plain barcode = product code

#### [MODIFY] ViewModels/Issues/IssueCreateViewModel.cs
- Tương tự Receipt: thêm scan commands

#### [MODIFY] ViewModels/Transfers/TransferCreateViewModel.cs
- Tương tự: thêm scan commands

#### [MODIFY] Views/Pages/ReceiptCreateView.xaml
- Thêm 2 buttons bên cạnh DataGrid: "📷 Quét QR từ ảnh" + "📱 Máy quét USB"
- Status indicator cho HID mode (đang lắng nghe / tắt)

#### [MODIFY] Views/Pages/IssueCreateView.xaml
- Tương tự

#### [MODIFY] VjWms.Desktop.UI.csproj
- Thêm NuGet: `ZXing.Net.Windows.Compatibility` (for WPF/Windows image support)

---

### Component 3: Local Attachment Management

Cho phép đính kèm tài liệu (ảnh, PDF, scan) vào phiếu nhập/xuất/chuyển. File lưu local trong AppData.

#### [NEW] Services/AttachmentService.cs
- `AddAttachment(documentId, documentType, filePath)` → copy file to `AppData/vj-wms/users/{userId}/attachments/{documentId}/`
- `GetAttachments(documentId)` → query `LocalAttachments` table
- `DeleteAttachment(attachmentId)` → remove file + DB record (chỉ khi chưa sync)
- `OpenAttachment(attachmentId)` → `Process.Start` mở file bằng app mặc định

#### [NEW] ViewModels/Attachments/AttachmentPanelViewModel.cs
- Reusable ViewModel dùng chung cho Receipt, Issue, Transfer
- Load attachments cho 1 document
- Commands: AddFile, DeleteFile, OpenFile
- Drag & drop support (optional)

#### [NEW] Views/Controls/AttachmentPanel.xaml + .cs
- UserControl hiển thị danh sách file đính kèm
- Nút: "📎 Thêm tệp", Xóa, Mở
- Hiển thị: tên file, kích thước, loại, ngày tạo, icon theo MIME type

#### [MODIFY] ViewModels/Receipts/ReceiptCreateViewModel.cs
- Thêm `AttachmentPanelViewModel` property
- Sau khi save receipt → liên kết attachments với receipt ID

#### [MODIFY] ViewModels/Issues/IssueCreateViewModel.cs
- Tương tự

#### [MODIFY] ViewModels/Transfers/TransferCreateViewModel.cs
- Tương tự

#### [MODIFY] Views/Pages/ReceiptCreateView.xaml
- Thêm `AttachmentPanel` control phía dưới form

---

### Component 4: FluentValidation (Bilingual)

Thay validation thủ công bằng FluentValidation với lỗi song ngữ VI/EN.

#### [NEW] Validators/ReceiptValidator.cs
```csharp
public class ReceiptValidator : AbstractValidator<ReceiptCreateViewModel>
{
    public ReceiptValidator()
    {
        RuleFor(x => x.SelectedWarehouse)
            .NotNull()
            .WithMessage("Vui lòng chọn kho nhập / Please select a warehouse");
        
        RuleFor(x => x.LineItems)
            .Must(items => items.Any(l => l.SelectedProduct != null && l.Quantity > 0))
            .WithMessage("Cần ít nhất 1 dòng hàng hóa / At least 1 line item required");
        
        RuleFor(x => x.ReceiptDate)
            .NotEmpty()
            .WithMessage("Vui lòng chọn ngày nhập / Please select a date");
    }
}
```

#### [NEW] Validators/IssueValidator.cs
- Tương tự Receipt + thêm rules cho Customer (optional)

#### [NEW] Validators/TransferValidator.cs
- Kho nguồn ≠ Kho đích
- Ít nhất 1 dòng
- Số lượng > 0

#### [NEW] Validators/LineItemValidator.cs
- Validate từng dòng: ProductId required, Quantity > 0

#### [MODIFY] ViewModels/Receipts/ReceiptCreateViewModel.cs
- Replace manual `Validate()` method with `ReceiptValidator`
- Show errors per-field (not just single ErrorMessage)

#### [MODIFY] ViewModels/Issues/IssueCreateViewModel.cs
- Replace manual validation

#### [MODIFY] VjWms.Desktop.UI.csproj
- Thêm NuGet: `FluentValidation` (latest stable)

---

### Component 5: Inventory Upgrade

#### [MODIFY] ViewModels/InventoryViewModel.cs
- Include Transfer adjustments in inventory calculation:
  - Source warehouse: subtract quantity
  - Dest warehouse: add quantity
- Fix current calculation to handle transfers

---

## NuGet Packages to Add

| Package | Purpose |
|---|---|
| `ZXing.Net.Windows.Compatibility` | QR/Barcode decode from image files |
| `FluentValidation` | Form validation framework |

---

## Verification Plan

### Build Test
```bash
dotnet build D:\NgocLongJSC\VJCHEM_WH_Project\src\Desktop\VjWms.Desktop.UI
dotnet run --project D:\NgocLongJSC\VJCHEM_WH_Project\src\Desktop\VjWms.Desktop.UI
```

### Manual Verification
1. **Transfer**: Tạo phiếu luân chuyển từ KNĐ → KXK, thêm 2 sản phẩm, lưu Draft → thấy trong list
2. **QR Scan**: Chọn file ảnh QR → sản phẩm tự điền vào dòng hàng
3. **HID Scanner**: Bật chế độ USB → quét barcode → sản phẩm tự tìm theo mã
4. **Attachments**: Đính kèm 2 file vào phiếu nhập → xem danh sách → mở file
5. **Validation**: Thử submit phiếu không chọn kho → hiện lỗi song ngữ
6. **Inventory**: Sau khi tạo phiếu chuyển kho → tồn kho kho nguồn giảm, kho đích tăng

---

## Open Questions

> [!NOTE]
> **QR Data Format**: Hiện tại dùng format `PRODUCT_CODE|LOT_NUMBER|QUANTITY`. Nếu VJCHEM có format QR riêng, cần cập nhật parser. Tạm thời hỗ trợ cả plain barcode (= product code lookup) và structured QR.
