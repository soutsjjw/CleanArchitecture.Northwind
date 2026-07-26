using Microsoft.AspNetCore.Http;
using MimeKit;

namespace CleanArchitecture.Northwind.Web.Services;

public static class ProductImageValidator
{
    public const int MaxFileSize = 2 * 1024 * 1024;

    private const int MaximumSignatureLength = 12;

    public static bool TryValidate(
        IFormFile? file,
        out ValidatedProductImage? image,
        out string? error)
    {
        image = null;
        error = null;

        if (file is null || file.Length <= 0)
        {
            error = "請選擇非空白的圖片檔案。";
            return false;
        }

        if (file.Length > MaxFileSize)
        {
            error = "圖片大小不可超過 2 MiB。";
            return false;
        }

        var format = FindFormat(file.FileName, file.ContentType);
        if (format is null)
        {
            error = "圖片格式必須是 JPEG、PNG 或 WebP，且副檔名與內容類型必須一致。";
            return false;
        }

        byte[] content;
        try
        {
            content = ReadWithinLimit(file);
        }
        catch (InvalidDataException)
        {
            error = "圖片大小不可超過 2 MiB。";
            return false;
        }
        catch (IOException)
        {
            error = "無法讀取圖片檔案。";
            return false;
        }

        if (content.Length == 0 || content.LongLength != file.Length)
        {
            error = "圖片檔案內容不完整。";
            return false;
        }

        if (!format.HasValidSignature(content))
        {
            error = "圖片內容與所選格式不一致。";
            return false;
        }

        image = new ValidatedProductImage(content, format.ContentType);
        return true;
    }

    public static bool TryValidateStored(
        byte[]? content,
        string? contentType,
        out ValidatedProductImage? image)
    {
        image = null;
        if (content is null
            || content.Length == 0
            || content.Length > MaxFileSize
            || string.IsNullOrWhiteSpace(contentType)
            || !ContentType.TryParse(contentType, out var parsedContentType))
        {
            return false;
        }

        var normalizedContentType = parsedContentType.MimeType.ToLowerInvariant();
        var format = Formats.FirstOrDefault(candidate =>
            candidate.ContentType == normalizedContentType);
        if (format is null || !format.HasValidSignature(content))
        {
            return false;
        }

        image = new ValidatedProductImage(content, format.ContentType);
        return true;
    }

    private static ProductImageFormat? FindFormat(
        string fileName,
        string contentType)
    {
        if (string.IsNullOrWhiteSpace(fileName)
            || !ContentType.TryParse(contentType, out var declaredContentType))
        {
            return null;
        }

        var extensionContentType = MimeTypes.GetMimeType(fileName);
        var normalizedDeclaredContentType =
            declaredContentType.MimeType.ToLowerInvariant();

        return Formats.FirstOrDefault(format =>
            format.ContentType == normalizedDeclaredContentType
            && format.ContentType.Equals(
                extensionContentType,
                StringComparison.OrdinalIgnoreCase));
    }

    private static byte[] ReadWithinLimit(IFormFile file)
    {
        using var source = file.OpenReadStream();
        using var destination = new MemoryStream(
            capacity: (int)Math.Min(file.Length, MaxFileSize));
        var buffer = new byte[64 * 1024];
        var totalRead = 0;

        while (true)
        {
            var remaining = MaxFileSize + 1 - totalRead;
            if (remaining <= 0)
            {
                throw new InvalidDataException();
            }

            var read = source.Read(
                buffer,
                0,
                Math.Min(buffer.Length, remaining));
            if (read == 0)
            {
                break;
            }

            destination.Write(buffer, 0, read);
            totalRead += read;
        }

        if (totalRead > MaxFileSize)
        {
            throw new InvalidDataException();
        }

        return destination.ToArray();
    }

    private static readonly ProductImageFormat[] Formats =
    [
        new(
            "image/jpeg",
            content => content.Length >= 3
                && content[0] == 0xFF
                && content[1] == 0xD8
                && content[2] == 0xFF),
        new(
            "image/png",
            content => content.Length >= 8
                && content.AsSpan(0, 8).SequenceEqual(
                    new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A })),
        new(
            "image/webp",
            content => content.Length >= MaximumSignatureLength
                && content.AsSpan(0, 4).SequenceEqual("RIFF"u8)
                && content.AsSpan(8, 4).SequenceEqual("WEBP"u8))
    ];

    private sealed record ProductImageFormat(
        string ContentType,
        Func<byte[], bool> HasValidSignature);
}

public sealed record ValidatedProductImage(
    byte[] Content,
    string ContentType);
