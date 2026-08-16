# Phase 2 Implementation Walkthrough

Tất cả các tính năng của Phase 2 (Luân chuyển kho, Quét mã vạch/QR, Đính kèm tài liệu và FluentValidation) đã được hoàn thiện. 

## 1. Tính năng Luân chuyển (Transfer)
Đã bổ sung **Luân chuyển kho nội bộ (Transfer)**. 
- **[TransferListViewModel.cs](file:///D:/NgocLongJSC/VJCHEM_WH_Project/src/Desktop/VjWms.Desktop.UI/ViewModels/Transfers/TransferListViewModel.cs)** & **[TransferListView.xaml](file:///D:/NgocLongJSC/VJCHEM_WH_Project/src/Desktop/VjWms.Desktop.UI/Views/Pages/Transfers/TransferListView.xaml)**: Màn hình danh sách với bộ lọc trạng thái.
- **[TransferCreateViewModel.cs](file:///D:/NgocLongJSC/VJCHEM_WH_Project/src/Desktop/VjWms.Desktop.UI/ViewModels/Transfers/TransferCreateViewModel.cs)** & **[TransferCreateView.xaml](file:///D:/NgocLongJSC/VJCHEM_WH_Project/src/Desktop/VjWms.Desktop.UI/Views/Pages/Transfers/TransferCreateView.xaml)**: Màn hình tạo phiếu luân chuyển mới, với chọn kho nguồn, kho đích, chọn loại hàng và số lượng.
- Tính năng tồn kho (`InventoryViewModel`) cũng đã được nâng cấp để cộng/trừ số lượng từ phiếu luân chuyển.
- Sidebar menu đã được thêm mục **🔄 Luân chuyển kho**.

## 2. Quét mã QR / Barcode
Thêm khả năng quét mã cực kỳ thông minh với 2 chế độ (Dual-mode):
- **📷 Quét từ ảnh màn hình**: Sử dụng thư viện `ZXing.Net` để đọc mã QR/Barcode từ ảnh tải lên.
- **📱 Quét từ máy quét USB (HID)**: Thuật toán nhận diện tốc độ gõ bàn phím (<80ms giữa các phím) để tự động lắng nghe thiết bị USB Scanner mà không cần người dùng trỏ chuột vào ô text nào.
- *Xem cách hoạt động ở [ScannerService.cs](file:///D:/NgocLongJSC/VJCHEM_WH_Project/src/Desktop/VjWms.Desktop.UI/Services/Scanner/ScannerService.cs)*.

## 3. Tệp đính kèm (Attachments)
- Tạo mới **[AttachmentService.cs](file:///D:/NgocLongJSC/VJCHEM_WH_Project/src/Desktop/VjWms.Desktop.UI/Services/AttachmentService.cs)** lưu trữ các file vào thư mục `%APPDATA%/vj-wms/users/admin/attachments/`.
- Tạo một UserControl dùng chung **[AttachmentPanel.xaml](file:///D:/NgocLongJSC/VJCHEM_WH_Project/src/Desktop/VjWms.Desktop.UI/Views/Controls/AttachmentPanel.xaml)**.
- Người dùng giờ có thể dễ dàng đính kèm Hóa đơn, Chứng từ (Ảnh, PDF, Excel) vào phiếu Nhập, Xuất, Luân chuyển ngay từ khi đang tạo mới.

## 4. Kiểm tra hợp lệ (FluentValidation)
- Chuyển toàn bộ logic kiểm tra bằng tay sang dùng gói Nuget `FluentValidation`.
- Mã được tái cấu trúc thành các Validator class cực kỳ quy chuẩn:
  - `ReceiptValidator.cs`
  - `IssueValidator.cs`
  - `TransferValidator.cs`
- Các thông báo lỗi chi tiết, hỗ trợ cả Tiếng Việt và Tiếng Anh để thân thiện với nhiều người dùng.

> [!TIP]
> Hãy chạy thử phần mềm qua Visual Studio hoặc lệnh `dotnet run --project src/Desktop/VjWms.Desktop.UI` để xem các Form tạo mới cực kỳ "xịn sò" với công cụ Quét mã và Đính kèm file!
