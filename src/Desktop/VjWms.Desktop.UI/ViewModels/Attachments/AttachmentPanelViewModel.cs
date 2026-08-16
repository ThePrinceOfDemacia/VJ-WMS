using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using VjWms.Desktop.Domain.Entities;
using VjWms.Desktop.UI.Services;

namespace VjWms.Desktop.UI.ViewModels.Attachments;

/// <summary>
/// Reusable ViewModel for the attachment panel that can be embedded
/// into Receipt, Issue, or Transfer create/edit views.
/// </summary>
public partial class AttachmentPanelViewModel : ObservableObject
{
    private readonly AttachmentService _attachmentService;
    private string _documentId;
    private string _documentType;

    [ObservableProperty] private string _statusMessage = "";

    public ObservableCollection<AttachmentRow> Attachments { get; } = new();

    public AttachmentPanelViewModel(AttachmentService attachmentService, string documentId, string documentType)
    {
        _attachmentService = attachmentService;
        _documentId = documentId;
        _documentType = documentType;
        LoadAttachments();
    }

    /// <summary>
    /// Update the document ID after saving a new document (since we may not know the ID before save).
    /// </summary>
    public void SetDocumentId(string documentId)
    {
        _documentId = documentId;
        LoadAttachments();
    }

    private void LoadAttachments()
    {
        Attachments.Clear();
        foreach (var a in _attachmentService.GetAttachments(_documentId))
        {
            Attachments.Add(new AttachmentRow
            {
                Id = a.Id,
                FileName = a.FileName,
                FileSize = FormatFileSize(a.FileSize),
                MimeType = a.MimeType,
                CreatedAt = a.CreatedAt,
                IsVerified = a.IsVerifiedOnServer,
                Icon = GetIcon(a.MimeType)
            });
        }
    }

    [RelayCommand]
    private void AddFile()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Chọn tệp đính kèm / Select attachment",
            Filter = "All files (*.*)|*.*|Images (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|PDF (*.pdf)|*.pdf",
            Multiselect = true
        };

        if (dialog.ShowDialog() != true) return;

        foreach (var file in dialog.FileNames)
        {
            _attachmentService.AddAttachment(_documentId, _documentType, file);
        }

        LoadAttachments();
        StatusMessage = $"Đã thêm {dialog.FileNames.Length} tệp / {dialog.FileNames.Length} file(s) added";
    }

    [RelayCommand]
    private void DeleteFile(AttachmentRow? row)
    {
        if (row == null || row.IsVerified) return;

        var deleted = _attachmentService.DeleteAttachment(row.Id);
        if (deleted)
        {
            LoadAttachments();
            StatusMessage = $"Đã xóa: {row.FileName}";
        }
    }

    [RelayCommand]
    private void OpenFile(AttachmentRow? row)
    {
        if (row == null) return;
        _attachmentService.OpenAttachment(row.Id);
    }

    private static string FormatFileSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        return $"{bytes / (1024.0 * 1024.0):F1} MB";
    }

    private static string GetIcon(string mimeType)
    {
        if (mimeType.StartsWith("image/")) return "🖼️";
        if (mimeType == "application/pdf") return "📄";
        if (mimeType.Contains("spreadsheet") || mimeType.Contains("excel")) return "📊";
        if (mimeType.Contains("word")) return "📝";
        return "📎";
    }
}

public class AttachmentRow
{
    public string Id { get; set; } = "";
    public string FileName { get; set; } = "";
    public string FileSize { get; set; } = "";
    public string MimeType { get; set; } = "";
    public string CreatedAt { get; set; } = "";
    public bool IsVerified { get; set; }
    public string Icon { get; set; } = "📎";
}
