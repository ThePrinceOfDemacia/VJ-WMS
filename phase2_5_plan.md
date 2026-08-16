# Phase 2.5 — Transfer Logic, List Actions & Detail Views

## Background

After Phase 2 testing, user identified 3 categories of issues that need addressing:
1. Transfer product selection should be filtered by source warehouse inventory
2. Transfer inventory logic needs to distinguish Logical vs Physical types
3. List views (Receipt, Issue, Transfer) need Sync/Edit/Delete buttons + detail view on double-click

---

## Clarifications (Resolved)

| Question | Answer |
|---|---|
| Warehouse Type values | `"Physical"` and `"Virtual"` (confirmed from SeedData.cs) |
| Inventory display columns | Two columns: "Tồn kho thực tế" + "Tồn kho dự kiến" ✅ |
| Sync button behavior | Invoice starts as `PendingSync`. Click Sync → attempts API → since no API yet → `SyncFailed` |
| Edit history | Local-only edits, uploaded to server later |

---

## Proposed Changes

### Component 1: Transfer Product Filtering by Source Warehouse

When the user selects a source warehouse, the product dropdown should only show products that have available inventory (quantity > 0) in that warehouse.

#### [MODIFY] [TransferCreateViewModel.cs](file:///D:/NgocLongJSC/VJCHEM_WH_Project/src/Desktop/VjWms.Desktop.UI/ViewModels/Transfers/TransferCreateViewModel.cs)

- Add `OnSelectedSourceWarehouseChanged` handler that:
  1. Calculates effective inventory per product in the selected warehouse (snapshot + receipts - issues ± logical transfers)
  2. Filters `Products` collection to only products with available quantity > 0
  3. Stores available quantities in a dictionary so the UI can display "Tồn: X" next to each product
  4. Clears existing line item selections if they're no longer valid
- Add `OnSelectedSourceWarehouseChanged` + `OnSelectedDestWarehouseChanged` handlers that enforce transfer type rules (see Component 2)

---

### Component 2: Transfer Type Logic (Logical vs Physical)

#### Business Rules
| Rule | Description |
|---|---|
| **Logical Transfer** | Immediate effect on inventory. Source decreases, destination increases. Only allowed when BOTH warehouses have `Type = "Virtual"`. |
| **Physical Transfer** | Requires manager digital signature (sync to server). Inventory NOT immediately adjusted. Only reflected in "Tồn kho dự kiến" column. |
| **Type Auto-Detection** | If either source or destination warehouse has `Type = "Physical"`, automatically set transfer type to "Physical" and disable the dropdown. |

#### [MODIFY] [TransferCreateViewModel.cs](file:///D:/NgocLongJSC/VJCHEM_WH_Project/src/Desktop/VjWms.Desktop.UI/ViewModels/Transfers/TransferCreateViewModel.cs)

- Add `IsTransferTypeLocked` observable property
- In `OnSelectedSourceWarehouseChanged` / `OnSelectedDestWarehouseChanged`:
  - Check if either warehouse has `Type == "Physical"`
  - If yes: force `TransferType = "Physical"` and set `IsTransferTypeLocked = true`
  - If no: allow user choice, set `IsTransferTypeLocked = false`

#### [MODIFY] [TransferCreateView.xaml](file:///D:/NgocLongJSC/VJCHEM_WH_Project/src/Desktop/VjWms.Desktop.UI/Views/Pages/Transfers/TransferCreateView.xaml)

- Bind `IsEnabled` of TransferType ComboBox to inverse of `IsTransferTypeLocked`

#### [MODIFY] [TransferValidator.cs](file:///D:/NgocLongJSC/VJCHEM_WH_Project/src/Desktop/VjWms.Desktop.UI/Validation/TransferValidator.cs)

- Add rule: If `TransferType == "Logical"`, neither warehouse can have `Type == "Physical"`

#### [MODIFY] [InventoryViewModel.cs](file:///D:/NgocLongJSC/VJCHEM_WH_Project/src/Desktop/VjWms.Desktop.UI/ViewModels/InventoryViewModel.cs)

- Split transfer adjustments into two categories:
  - **Logical transfers** (non-Draft): Always counted in actual quantity
  - **Physical transfers** (non-Draft): Only counted in expected/projected quantity
- Add `ExpectedQuantity` column to `InventoryRow`
- Update `LoadInventory()` to compute both values

#### [MODIFY] [InventoryView.xaml](file:///D:/NgocLongJSC/VJCHEM_WH_Project/src/Desktop/VjWms.Desktop.UI/Views/Pages/InventoryView.xaml)

- Add "Tồn kho dự kiến" column to the DataGrid

---

### Component 3: List View Actions (Sync, Edit, Delete) + Delete Fix

Apply to all three list views: Receipt, Issue, Transfer.

#### Status Flow
```
Create → PendingSync → [Click Sync] → SyncFailed (no API yet)
                     → [Edit] → PendingSync (updated)
                     → [Delete] ✅

Synced → ❌ Cannot edit or delete (greyed out)
```

#### Row Model Changes

#### [MODIFY] [ReceiptListViewModel.cs](file:///D:/NgocLongJSC/VJCHEM_WH_Project/src/Desktop/VjWms.Desktop.UI/ViewModels/Receipts/ReceiptListViewModel.cs)

- Add `CanEdit` property to `ReceiptRow` (true when Status is Draft/PendingSync/SyncFailed and not IsReadOnly)
- Add `CanSync` property (true when Status is Draft/PendingSync and not IsReadOnly)
- Fix `Delete` command: expand to allow `PendingSync` and `SyncFailed` (not just Draft)
- Add `Sync` command: attempts sync → sets status to `SyncFailed` (no API)
- Fix `Edit` command: navigate with document ID parameter
- Add `ViewDetail` command: opens detail view on double-click

#### [MODIFY] [IssueListViewModel.cs](file:///D:/NgocLongJSC/VJCHEM_WH_Project/src/Desktop/VjWms.Desktop.UI/ViewModels/Issues/IssueListViewModel.cs)

- Same pattern as ReceiptListViewModel above

#### [MODIFY] [TransferListViewModel.cs](file:///D:/NgocLongJSC/VJCHEM_WH_Project/src/Desktop/VjWms.Desktop.UI/ViewModels/Transfers/TransferListViewModel.cs)

- Same pattern as ReceiptListViewModel above

#### View Changes

#### [MODIFY] [ReceiptListView.xaml](file:///D:/NgocLongJSC/VJCHEM_WH_Project/src/Desktop/VjWms.Desktop.UI/Views/Pages/ReceiptListView.xaml)

- Replace single delete button column with action buttons column containing:
  - 📤 Sync button (visible when CanSync=true)
  - ✏️ Edit button (visible when CanEdit=true)
  - 🗑️ Delete button (visible when CanEdit=true)
  - All buttons greyed out / hidden when status is `Synced`
- Add `MouseDoubleClick` event on DataGrid row to trigger `ViewDetail`

#### [MODIFY] [IssueListView.xaml](file:///D:/NgocLongJSC/VJCHEM_WH_Project/src/Desktop/VjWms.Desktop.UI/Views/Pages/IssueListView.xaml)

- Same pattern as ReceiptListView above

#### [MODIFY] [TransferListView.xaml](file:///D:/NgocLongJSC/VJCHEM_WH_Project/src/Desktop/VjWms.Desktop.UI/Views/Pages/Transfers/TransferListView.xaml)

- Same pattern as ReceiptListView above

---

### Component 4: Navigation Service Enhancement (Pass Parameters)

#### [MODIFY] [NavigationService.cs](file:///D:/NgocLongJSC/VJCHEM_WH_Project/src/Desktop/VjWms.Desktop.UI/Services/NavigationService.cs)

- Add `NavigateTo<TViewModel>(object? parameter)` overload to `INavigationService`
- Add `IParameterReceiver` interface with `void ReceiveParameter(object parameter)` method
- After resolving the ViewModel, check if it implements `IParameterReceiver` and call it

---

### Component 5: Edit Functionality

#### [MODIFY] [ReceiptCreateViewModel.cs](file:///D:/NgocLongJSC/VJCHEM_WH_Project/src/Desktop/VjWms.Desktop.UI/ViewModels/Receipts/ReceiptCreateViewModel.cs)

- Implement `IParameterReceiver`
- When receiving a receipt ID parameter, load the existing receipt data into the form
- Change title to "Chỉnh sửa Phiếu Nhập Kho"
- On save, update existing record instead of creating new
- Record edit in `LocalEditHistory`

#### [MODIFY] [IssueCreateViewModel.cs](file:///D:/NgocLongJSC/VJCHEM_WH_Project/src/Desktop/VjWms.Desktop.UI/ViewModels/Issues/IssueCreateViewModel.cs)

- Same pattern as ReceiptCreateViewModel

#### [MODIFY] [TransferCreateViewModel.cs](file:///D:/NgocLongJSC/VJCHEM_WH_Project/src/Desktop/VjWms.Desktop.UI/ViewModels/Transfers/TransferCreateViewModel.cs)

- Same pattern as ReceiptCreateViewModel

---

### Component 6: Transaction Detail View + Edit History

#### [MODIFY] [Entities.cs](file:///D:/NgocLongJSC/VJCHEM_WH_Project/src/Desktop/VjWms.Desktop.Domain/Entities/Entities.cs) — Add LocalEditHistory entity

```csharp
public class LocalEditHistory
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string DocumentId { get; set; } = "";      // Receipt/Issue/Transfer ID
    public string DocumentType { get; set; } = "";     // "Receipt" | "Issue" | "Transfer"
    public string Action { get; set; } = "";           // "Created" | "Edited" | "StatusChanged" | "Synced"
    public string? OldValues { get; set; }             // JSON snapshot before edit
    public string? NewValues { get; set; }             // JSON snapshot after edit
    public string ChangedBy { get; set; } = "";
    public string ChangedAt { get; set; } = "";
    public string? Notes { get; set; }
}
```

#### [MODIFY] [LocalDbContext.cs](file:///D:/NgocLongJSC/VJCHEM_WH_Project/src/Desktop/VjWms.Desktop.Infrastructure/SQLite/LocalDbContext.cs)

- Add `DbSet<LocalEditHistory> EditHistories`

#### [NEW] [TransactionDetailViewModel.cs](file:///D:/NgocLongJSC/VJCHEM_WH_Project/src/Desktop/VjWms.Desktop.UI/ViewModels/TransactionDetailViewModel.cs)

- Generic detail view that works for Receipt, Issue, and Transfer
- Implements `IParameterReceiver` to receive `{ DocumentId, DocumentType }`
- Loads document header info + line items (read-only display)
- Loads edit history from `LocalEditHistory` table
- Displays all information in a scrollable view

#### [NEW] [TransactionDetailView.xaml](file:///D:/NgocLongJSC/VJCHEM_WH_Project/src/Desktop/VjWms.Desktop.UI/Views/Pages/TransactionDetailView.xaml)

- Header section: document number, warehouse, date, status, etc.
- Line items table (read-only)
- Edit history timeline at the bottom
- "Đóng" (Close) button to go back to list

---

## Verification Plan

### Build Test
```bash
dotnet build D:\NgocLongJSC\VJCHEM_WH_Project\src\Desktop\VjWms.Desktop.UI
```

### Manual Verification
1. **Product Filtering**: Select source warehouse → only products with qty > 0 appear in dropdown
2. **Transfer Type Lock**: Select a Physical warehouse (e.g. KNĐ) → transfer type auto-set to Physical, dropdown disabled
3. **Inventory Columns**: Create a Logical transfer → actual qty changes. Create a Physical transfer → only expected qty changes
4. **Delete**: Delete a Draft/PendingSync receipt → should work. Try Synced receipt → button greyed out
5. **Sync**: Click sync on PendingSync receipt → status changes to SyncFailed (no API)
6. **Edit**: Click edit on PendingSync receipt → form opens with existing data pre-filled
7. **Detail View**: Double-click a receipt → detail view shows all info + edit history
