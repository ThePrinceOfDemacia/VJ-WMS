using System.Diagnostics;
using System.IO;
using VjWms.Desktop.Domain.Entities;
using VjWms.Desktop.Infrastructure.SQLite;

namespace VjWms.Desktop.UI.Services;

/// <summary>
/// Manages local file attachments for documents (receipts, issues, transfers).
/// Files are stored in: %APPDATA%/vj-wms/users/{userId}/attachments/{documentId}/
/// </summary>
public class AttachmentService
{
    private readonly LocalDbContext _db;

    public AttachmentService(LocalDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Copy a file into the attachments directory and record it in the database.
    /// </summary>
    public LocalAttachment AddAttachment(string documentId, string documentType, string sourceFilePath, string category = "General")
    {
        var fileName = Path.GetFileName(sourceFilePath);
        var attachDir = GetAttachmentDir(documentId);
        Directory.CreateDirectory(attachDir);

        // Generate unique filename to prevent collisions
        var uniqueName = $"{Guid.NewGuid():N}_{fileName}";
        var destPath = Path.Combine(attachDir, uniqueName);

        File.Copy(sourceFilePath, destPath, overwrite: true);

        var attachment = new LocalAttachment
        {
            DocumentId = documentId,
            DocumentType = documentType,
            Category = category,
            FileName = fileName,
            LocalFilePath = destPath,
            MimeType = GetMimeType(fileName),
            FileSize = new FileInfo(destPath).Length,
            SyncStatus = "Pending",
            CreatedAt = DateTime.UtcNow.ToString("o")
        };

        _db.LocalAttachments.Add(attachment);
        _db.SaveChanges();

        return attachment;
    }

    /// <summary>
    /// Get all attachments for a given document.
    /// </summary>
    public List<LocalAttachment> GetAttachments(string documentId)
    {
        return _db.LocalAttachments
            .Where(a => a.DocumentId == documentId)
            .OrderByDescending(a => a.CreatedAt)
            .ToList();
    }

    /// <summary>
    /// Delete an attachment (only if not yet synced to server).
    /// </summary>
    public bool DeleteAttachment(string attachmentId)
    {
        var attachment = _db.LocalAttachments.Find(attachmentId);
        if (attachment == null) return false;

        // Don't delete attachments that have been synced and verified
        if (attachment.IsVerifiedOnServer) return false;

        // Delete the physical file
        if (File.Exists(attachment.LocalFilePath))
        {
            try { File.Delete(attachment.LocalFilePath); }
            catch { /* ignore file deletion errors */ }
        }

        _db.LocalAttachments.Remove(attachment);
        _db.SaveChanges();
        return true;
    }

    /// <summary>
    /// Open an attachment file with the default OS application.
    /// </summary>
    public void OpenAttachment(string attachmentId)
    {
        var attachment = _db.LocalAttachments.Find(attachmentId);
        if (attachment == null || !File.Exists(attachment.LocalFilePath)) return;

        Process.Start(new ProcessStartInfo
        {
            FileName = attachment.LocalFilePath,
            UseShellExecute = true
        });
    }

    private string GetAttachmentDir(string documentId)
    {
        var username = App.CurrentUsername ?? "admin";
        return Path.Combine(App.AppDataPath, "users", username, "attachments", documentId);
    }

    private static string GetMimeType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }
}
