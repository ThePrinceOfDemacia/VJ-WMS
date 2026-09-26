namespace VjWms.Desktop.UI.Services.Scanner;

/// <summary>
/// Parses QR code data from NSRP (Nghi Son Refinery) product labels.
/// 
/// Format: "P;{PalletId};{GradePackType};{LotNo};{Classification};{Code}"
/// Example: "P;2608029-A-038;NSY114GP1500;2608029;A;4W12A"
///
/// Field mapping (from physical label):
///   [0] Type       = "P" (Pallet)
///   [1] PalletId   = "2608029-A-038"
///   [2] GradePack  = "NSY114GP1500" (GradeName + PackType concatenated)
///   [3] LotNumber  = "2608029"
///   [4] Class      = "A"
///   [5] Code       = "4W12A"
///
/// The GradePack field is split at the "P" before the numeric pack size:
///   "NSY114GP1500" → GradeName="NSY114G", PackType="P1500"
///
/// From physical label we also know:
///   25 KGS/BAG, 60 PCS, 1500 KGS total (derived from PackType P1500)
/// </summary>
public class QrDataParser
{
    public class QrParseResult
    {
        public bool IsValid { get; set; }
        public string RawData { get; set; } = "";
        
        // Parsed fields
        public string Type { get; set; } = "";          // "P" = Pallet
        public string PalletId { get; set; } = "";      // e.g. "2608029-A-038"
        public string GradeName { get; set; } = "";     // e.g. "NSY114G"
        public string PackType { get; set; } = "";      // e.g. "P1500"
        public string LotNumber { get; set; } = "";     // e.g. "2608029"
        public string Classification { get; set; } = "";// e.g. "A"
        public string Code { get; set; } = "";          // e.g. "4W12A"
        
        // Derived fields
        public double TotalWeight { get; set; }         // e.g. 1500 (from P1500)
        public double BagWeight { get; set; } = 25;     // KGS/BAG (standard)
        public int BagCount { get; set; }               // e.g. 60 (1500/25)
        
        public string ErrorMessage { get; set; } = "";
        
        /// <summary>
        /// Produces a human-readable summary for display in the UI.
        /// </summary>
        public string ToDisplaySummary()
        {
            if (!IsValid)
                return $"Không thể phân tích QR: {ErrorMessage}\nDữ liệu gốc: {RawData}";
            
            return $"✅ Đã đọc QR thành công!\n" +
                   $"Sản phẩm: {GradeName}\n" +
                   $"Số lô: {LotNumber}\n" +
                   $"Pallet: {PalletId}\n" +
                   $"Đóng gói: {PackType} ({TotalWeight:N0} KG)\n" +
                   $"Số bao: {BagCount} x {BagWeight:N0} KG/bao";
        }
    }

    /// <summary>
    /// Parse the raw QR code text into structured data.
    /// Supports the NSRP semicolon-delimited format.
    /// </summary>
    public static QrParseResult Parse(string rawData)
    {
        var result = new QrParseResult { RawData = rawData };
        
        if (string.IsNullOrWhiteSpace(rawData))
        {
            result.ErrorMessage = "Dữ liệu trống";
            return result;
        }

        // Check if it's the NSRP semicolon-delimited format
        if (rawData.Contains(';'))
        {
            return ParseNsrpFormat(rawData, result);
        }
        
        // Fallback: treat as plain product code
        result.GradeName = rawData.Trim();
        result.IsValid = true;
        return result;
    }

    private static QrParseResult ParseNsrpFormat(string rawData, QrParseResult result)
    {
        var parts = rawData.Split(';');
        
        if (parts.Length < 4)
        {
            result.ErrorMessage = $"Định dạng không đúng: cần ít nhất 4 trường, chỉ có {parts.Length}";
            return result;
        }

        result.Type = parts[0].Trim();
        result.PalletId = parts[1].Trim();
        
        // Parse GradePack: "NSY114GP1500" → GradeName="NSY114G", PackType="P1500"
        var gradePack = parts[2].Trim();
        result.GradeName = ExtractGradeName(gradePack);
        result.PackType = ExtractPackType(gradePack);
        
        result.LotNumber = parts[3].Trim();
        
        if (parts.Length > 4) result.Classification = parts[4].Trim();
        if (parts.Length > 5) result.Code = parts[5].Trim();

        // Parse weight from PackType (e.g. "P1500" → 1500 KG)
        if (result.PackType.Length > 1 && double.TryParse(result.PackType[1..], out var totalWeight))
        {
            result.TotalWeight = totalWeight;
            if (result.BagWeight > 0)
                result.BagCount = (int)(totalWeight / result.BagWeight);
        }

        result.IsValid = !string.IsNullOrEmpty(result.GradeName);
        return result;
    }

    /// <summary>
    /// Extract the grade name from the combined GradePack string.
    /// "NSY114GP1500" → "NSY114G"
    /// Strategy: find the last 'P' followed by only digits (the pack type).
    /// </summary>
    private static string ExtractGradeName(string gradePack)
    {
        // Search from the end for 'P' followed by only digits
        for (int i = gradePack.Length - 1; i >= 1; i--)
        {
            if (gradePack[i] == 'P')
            {
                var after = gradePack[(i + 1)..];
                if (after.Length > 0 && after.All(char.IsDigit))
                {
                    return gradePack[..i];
                }
            }
        }
        // If no pack type pattern found, return the whole string
        return gradePack;
    }

    /// <summary>
    /// Extract the pack type from the combined GradePack string.
    /// "NSY114GP1500" → "P1500"
    /// </summary>
    private static string ExtractPackType(string gradePack)
    {
        for (int i = gradePack.Length - 1; i >= 1; i--)
        {
            if (gradePack[i] == 'P')
            {
                var after = gradePack[(i + 1)..];
                if (after.Length > 0 && after.All(char.IsDigit))
                {
                    return gradePack[i..];
                }
            }
        }
        return "";
    }
}
