# Phase 1 — Implementation Plan

## Goal

Biến VJ-WMS từ giao diện "trình diễn" (Phase 0) thành phần mềm **hoạt động thực tế**: người dùng có thể đăng nhập, xem danh mục kho/sản phẩm/NCC/KH, tạo Phiếu Nhập Kho và Phiếu Xuất Kho **offline** với dữ liệu lưu vào SQLite.

> [!IMPORTANT]
> Phase 1 tập trung **100% vào Desktop Client (Offline)**. Chưa dựng Server, chưa cần Docker. Tất cả dữ liệu master (kho, sản phẩm, NCC, KH) sẽ được **seed trực tiếp** vào SQLite để có dữ liệu làm việc ngay lập tức.

---

## Proposed Changes

### Component 1: MVVM Foundation + Navigation

Chuyển từ code-behind thuần sang **MVVM pattern** chuẩn WPF bằng `CommunityToolkit.Mvvm`. Đây là nền tảng cho mọi tính năng sau này.

#### [NEW] ViewModels/BaseViewModel.cs
- Base class sử dụng `ObservableObject` từ CommunityToolkit.Mvvm

#### [NEW] ViewModels/ShellViewModel.cs
- Quản lý navigation giữa các trang (Dashboard, Receipts, Issues, Inventory...)
- Thuộc tính `CurrentView` binding với ContentControl trên ShellWindow

#### [NEW] ViewModels/LoginViewModel.cs
- Xử lý login logic (offline auth từ SQLite `cached_users`)

#### [NEW] Services/NavigationService.cs
- Dịch vụ điều hướng (navigate giữa các ViewModel)

#### [MODIFY] VjWms.Desktop.UI.csproj
- Thêm NuGet: `CommunityToolkit.Mvvm`

---

### Component 2: Seed Data + Database

Tạo dữ liệu mẫu thực tế cho VJCHEM (7 kho, ~20 sản phẩm hóa chất, NCC, KH) để phần mềm có dữ liệu ngay khi mở.

#### [NEW] Infrastructure/SQLite/SeedData.cs
- Seed 7 kho: KNĐ (Kho Nghi Đồng), KNK (Kho Nhập Khẩu), KXK (Kho Xuất Khẩu), KCK (Kho Chuyển Khẩu), KCCDC (Kho CC-DC), KLOG (Kho Logistics), KVR (Kho Ảo)
- Seed ~20 sản phẩm hóa chất (dựa trên mẫu PXN: Caustic Soda, HCl, NaOH...)
- Seed ~5 NCC, ~5 KH
- Seed user `admin` (password hash `admin123`) + user `thukho` (password `thukho123`)

#### [MODIFY] Infrastructure/SQLite/LocalDbContext.cs
- Thêm `LocalTransfer` + `LocalTransferItem` DbSets (thiếu từ Phase 0)
- Gọi `SeedData.Seed()` trong `OnModelCreating`

---

### Component 3: Dashboard View (Real Data)

Thay thế Dashboard "trình diễn" bằng Dashboard thực tế đọc dữ liệu từ SQLite.

#### [NEW] ViewModels/DashboardViewModel.cs
- Đếm số phiếu Draft, PendingSync, Rejected từ SQLite
- Đếm số kho đang hoạt động
- Hiển thị 5 phiếu gần nhất (Receipt + Issue)

#### [NEW] Views/DashboardView.xaml
- 4 stat cards: Kho hoạt động, Phiếu Draft, Chờ đồng bộ, Bị từ chối
- Bảng "Hoạt động gần đây" (danh sách phiếu mới nhất)

---

### Component 4: Master Data Views (Xem danh mục)

Cho phép người dùng xem danh mục Kho, Sản phẩm, NCC, KH đã được seed.

#### [NEW] ViewModels/MasterData/WarehouseListViewModel.cs
#### [NEW] ViewModels/MasterData/ProductListViewModel.cs
#### [NEW] Views/MasterData/WarehouseListView.xaml
#### [NEW] Views/MasterData/ProductListView.xaml
- DataGrid hiển thị danh sách (readonly trong Phase 1)
- Ô tìm kiếm (filter theo tên/mã)

---

### Component 5: Phiếu Nhập Kho (Receipt) — CRUD Offline

Đây là tính năng cốt lõi đầu tiên: tạo Phiếu Nhập Kho lưu vào SQLite.

#### [NEW] ViewModels/Receipts/ReceiptListViewModel.cs
- Load danh sách phiếu từ SQLite
- Filter theo trạng thái (Draft/PendingSync/All)
- Lệnh: Tạo mới, Xem chi tiết, Xóa (chỉ Draft)

#### [NEW] ViewModels/Receipts/ReceiptCreateViewModel.cs
- Form tạo phiếu: chọn Kho, NCC, ngày nhập, ghi chú
- Thêm/xóa dòng hàng hóa: chọn Sản phẩm, SL, ĐVT, Số lô, Số bao
- Validation: kho bắt buộc, ít nhất 1 dòng hàng, số lượng > 0
- Lưu = Draft, Xác nhận = PendingSync
- Tự sinh local number: `LOCAL/NK/{ngày}/{STT}`

#### [NEW] Views/Receipts/ReceiptListView.xaml
- DataGrid với cột: Số phiếu, Kho, NCC, Ngày, Trạng thái, Đồng bộ
- Badge màu cho trạng thái

#### [NEW] Views/Receipts/ReceiptCreateView.xaml
- Form header (Kho, NCC, Ngày) + DataGrid dòng hàng
- Nút: Lưu nháp, Xác nhận, Hủy

---

### Component 6: Phiếu Xuất Kho (Issue) — CRUD Offline

Tương tự Receipt nhưng cho xuất kho.

#### [NEW] ViewModels/Issues/IssueListViewModel.cs
#### [NEW] ViewModels/Issues/IssueCreateViewModel.cs
#### [NEW] Views/Issues/IssueListView.xaml
#### [NEW] Views/Issues/IssueCreateView.xaml
- Tương tự Phiếu nhập nhưng thêm: Khách hàng, Lý do xuất, Số cont, Seal, Xe
- Local number: `LOCAL/XK/{ngày}/{STT}`

---

### Component 7: Inventory View (Tra cứu tồn kho)

Xem tồn kho từ dữ liệu seed + phiếu đã tạo.

#### [NEW] ViewModels/InventoryViewModel.cs
- Tính tồn kho = Seed quantity + SUM(nhập) - SUM(xuất) cho mỗi sản phẩm/kho
- Filter theo Kho, Sản phẩm

#### [NEW] Views/InventoryView.xaml
- DataGrid: Kho, Mã hàng, Tên hàng, Tồn kho, ĐVT

---

## Verification Plan

### Build Test
```bash
dotnet build D:\NgocLongJSC\VJCHEM_WH_Project\src\Desktop\VjWms.Desktop.UI
dotnet run --project D:\NgocLongJSC\VJCHEM_WH_Project\src\Desktop\VjWms.Desktop.UI
```

### Manual Verification
- Đăng nhập `admin`/`admin123` → thấy Dashboard với dữ liệu thực
- Xem danh sách Kho (7 kho VJCHEM) và Sản phẩm (~20 hóa chất)
- Tạo Phiếu Nhập Kho → thêm 3 dòng hàng → Lưu nháp → thấy trong danh sách
- Tạo Phiếu Xuất Kho → thêm 2 dòng → Xác nhận → trạng thái = PendingSync
- Tra cứu tồn kho → thấy số lượng cập nhật sau khi nhập/xuất
- Tắt app → mở lại → dữ liệu vẫn còn (SQLite persistent)

---

## Open Questions

> [!NOTE]
> Không có câu hỏi mở. Tất cả dữ liệu master sẽ dùng seed data theo thông tin VJCHEM từ mẫu phiếu đã phân tích.
