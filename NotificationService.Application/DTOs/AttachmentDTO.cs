namespace NotificationService.Application.DTOs
{
    public class AttachmentDTO
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string MimeType { get; set; } = "application/pdf";
        public long FileSize { get; set; }
        public string StorageType { get; set; } = "FileSystem";
    }
}
