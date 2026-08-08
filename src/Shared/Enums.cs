namespace VjWms.Shared.Enums;

/// <summary>
/// Synchronization status for local documents
/// </summary>
public enum SyncStatus
{
    Pending,
    Synced,
    Rejected,
    Conflict
}

/// <summary>
/// Document lifecycle status
/// </summary>
public enum DocumentStatus
{
    Draft,
    PendingSync,
    Synced,
    Approved,
    DigitallySigned,
    Completed,
    Cancelled
}

/// <summary>
/// Types of inventory transactions (ledger entries)
/// </summary>
public enum TransactionType
{
    Import,
    Export,
    TransferIn,
    TransferOut,
    Adjustment,
    StockTake
}

/// <summary>
/// Warehouse category matching business requirements
/// </summary>
public enum WarehouseCategory
{
    /// <summary>Kho hàng nội địa hàng mua Nghi Sơn</summary>
    NoiDiaNghiSon,
    /// <summary>Kho hàng nội địa khác</summary>
    NoiDiaKhac,
    /// <summary>Kho hàng nhập khẩu</summary>
    NhapKhau,
    /// <summary>Kho hàng xuất khẩu</summary>
    XuatKhau,
    /// <summary>Kho hàng chuyển khẩu (Mua bán ngoài lãnh thổ Việt Nam)</summary>
    ChuyenKhau,
    /// <summary>Kho công cụ dụng cụ</summary>
    CongCuDungCu,
    /// <summary>Kho làm dịch vụ logistics</summary>
    Logistics
}

/// <summary>
/// Type of warehouse
/// </summary>
public enum WarehouseType
{
    Physical,
    Virtual
}

/// <summary>
/// Transfer type between warehouses
/// </summary>
public enum TransferType
{
    /// <summary>Luân chuyển vật lý - hàng thay đổi vị trí</summary>
    Physical,
    /// <summary>Luân chuyển logic - chỉ thay đổi kho sổ sách</summary>
    Logical
}

/// <summary>
/// Document type for numbering and categorization
/// </summary>
public enum DocumentType
{
    /// <summary>Phiếu nhập kho (PNK)</summary>
    StockReceipt,
    /// <summary>Phiếu xuất kho kiêm BBGN (PXK)</summary>
    StockIssue,
    /// <summary>Phiếu nhập hàng - kho ảo (PNH)</summary>
    GoodsReceipt,
    /// <summary>Đề nghị luân chuyển kho (ĐNLC)</summary>
    Transfer,
    /// <summary>Biên bản giao nhận (BBGN)</summary>
    DeliveryNote,
    /// <summary>Phiếu bán hàng chuyển khẩu (PBH)</summary>
    SalesNote,
    /// <summary>Đơn đặt hàng (PO)</summary>
    PurchaseOrder
}

/// <summary>
/// Attachment category matching document requirements
/// </summary>
public enum AttachmentCategory
{
    /// <summary>Chứng từ chứng nhận (CDS)</summary>
    CDS,
    /// <summary>Tờ khai hải quan (TKHQ)</summary>
    CustomsDeclaration,
    /// <summary>Vận đơn (B/L)</summary>
    BillOfLading,
    /// <summary>Hóa đơn thương mại (Invoice)</summary>
    Invoice,
    /// <summary>Danh sách đóng gói (Packing list)</summary>
    PackingList,
    /// <summary>Hợp đồng đầu vào</summary>
    InputContract,
    /// <summary>Hợp đồng đầu ra</summary>
    OutputContract,
    /// <summary>Hợp đồng vận chuyển (Debit note)</summary>
    TransportContract,
    /// <summary>Chứng từ bảo hiểm</summary>
    Insurance,
    /// <summary>TDS (Technical Data Sheet)</summary>
    TDS,
    /// <summary>MSDS (Material Safety Data Sheet)</summary>
    MSDS,
    /// <summary>REACH và giấy tờ khác</summary>
    OtherCertificate,
    /// <summary>Khác</summary>
    Other
}

/// <summary>
/// Delivery terms (Incoterms)
/// </summary>
public enum DeliveryTerm
{
    DDP,
    FOB,
    CIF,
    CFR
}

/// <summary>
/// Goods packaging type
/// </summary>
public enum PackagingType
{
    /// <summary>Hàng pallet</summary>
    Pallet,
    /// <summary>Hàng rời</summary>
    Bulk
}

/// <summary>
/// Costing method for inventory valuation
/// </summary>
public enum CostingMethod
{
    FIFO,
    WeightedAverage,
    SpecificIdentification
}

/// <summary>
/// User role in the system
/// </summary>
public enum SystemRole
{
    Admin,
    WarehouseStaff,
    WarehouseManager,
    Accountant,
    ChiefAccountant,
    SalesStaff,
    PurchasingStaff,
    LogisticsStaff,
    Director
}

/// <summary>
/// Digital signature role
/// </summary>
public enum SignatureRole
{
    /// <summary>BP xuất hàng / Kho</summary>
    WarehouseOfficer,
    /// <summary>Kế toán trưởng</summary>
    ChiefAccountant,
    /// <summary>Giám đốc</summary>
    Director
}
