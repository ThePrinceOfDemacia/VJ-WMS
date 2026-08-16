using System.Collections.Generic;
using System.Linq;
using VjWms.Desktop.Infrastructure.SQLite;

namespace VjWms.Desktop.UI.Services;

/// <summary>
/// Shared warehouse inventory math used by the Inventory lookup screen, the transfer product
/// filter, and the negative-stock guard on Issue/Transfer confirm.
///
/// Actual = the last server-confirmed snapshot, plus any local document that has actually
/// reached "Synced" (no code path sets this today - there is no live sync API yet - but the
/// math is ready for when there is).
///
/// Expected = Actual + the effect of every locally pending ("PendingSync"/"SyncFailed")
/// Receipt, Issue, and Transfer (Logical or Physical alike) that hasn't synced yet - i.e. what
/// stock will look like once everything currently waiting to sync goes through.
/// </summary>
public static class InventoryCalculator
{
    public readonly record struct Snapshot(double Actual, double Expected);

    /// <summary>
    /// Identifies one local document so its own not-yet-saved effect can be excluded while it
    /// is being edited - otherwise its old committed quantities would double-count against the
    /// new ones currently being entered in the form.
    /// </summary>
    public readonly record struct DocumentRef(string DocumentType, string DocumentId);

    public static Dictionary<(string WarehouseId, string ProductId), Snapshot> CalculateAll(
        LocalDbContext db, DocumentRef? exclude = null)
    {
        var result = new Dictionary<(string, string), Snapshot>();

        void AddActual((string WarehouseId, string ProductId) key, double delta)
        {
            result.TryGetValue(key, out var s);
            result[key] = new Snapshot(s.Actual + delta, s.Expected + delta);
        }

        void AddExpectedOnly((string WarehouseId, string ProductId) key, double delta)
        {
            result.TryGetValue(key, out var s);
            result[key] = new Snapshot(s.Actual, s.Expected + delta);
        }

        foreach (var inv in db.CachedInventories.ToList())
            AddActual((inv.WarehouseId, inv.ProductId), inv.Quantity);

        bool IsExcluded(string documentType, string documentId) =>
            exclude is { } ex && ex.DocumentType == documentType && ex.DocumentId == documentId;

        var receiptItems = db.LocalStockReceiptItems
            .Where(ri => ri.Receipt.Status != "Draft")
            .Select(ri => new { ri.ReceiptId, ri.Receipt.WarehouseId, ri.Receipt.Status, ri.ProductId, ri.Quantity })
            .ToList();

        foreach (var ri in receiptItems)
        {
            if (IsExcluded("Receipt", ri.ReceiptId)) continue;
            var key = (ri.WarehouseId, ri.ProductId);
            if (ri.Status == "Synced") AddActual(key, ri.Quantity);
            else AddExpectedOnly(key, ri.Quantity);
        }

        var issueItems = db.LocalStockIssueItems
            .Where(ii => ii.Issue.Status != "Draft")
            .Select(ii => new { ii.IssueId, ii.Issue.WarehouseId, ii.Issue.Status, ii.ProductId, ii.Quantity })
            .ToList();

        foreach (var ii in issueItems)
        {
            if (IsExcluded("Issue", ii.IssueId)) continue;
            var key = (ii.WarehouseId, ii.ProductId);
            if (ii.Status == "Synced") AddActual(key, -ii.Quantity);
            else AddExpectedOnly(key, -ii.Quantity);
        }

        var transferItems = db.LocalTransferItems
            .Where(ti => ti.Transfer.Status != "Draft")
            .Select(ti => new { ti.TransferId, ti.Transfer.SourceWarehouseId, ti.Transfer.DestWarehouseId, ti.Transfer.Status, ti.ProductId, ti.Quantity })
            .ToList();

        foreach (var ti in transferItems)
        {
            if (IsExcluded("Transfer", ti.TransferId)) continue;
            var srcKey = (ti.SourceWarehouseId, ti.ProductId);
            var dstKey = (ti.DestWarehouseId, ti.ProductId);
            if (ti.Status == "Synced")
            {
                AddActual(srcKey, -ti.Quantity);
                AddActual(dstKey, ti.Quantity);
            }
            else
            {
                AddExpectedOnly(srcKey, -ti.Quantity);
                AddExpectedOnly(dstKey, ti.Quantity);
            }
        }

        return result;
    }

    public static double GetExpected(LocalDbContext db, string warehouseId, string productId, DocumentRef? exclude = null)
    {
        var all = CalculateAll(db, exclude);
        return all.TryGetValue((warehouseId, productId), out var s) ? s.Expected : 0;
    }
}
