using CleanArchitecture.Northwind.Web.Services;
using Microsoft.AspNetCore.Http;

namespace CleanArchitecture.Northwind.Web.FunctionalTests.Controllers;

public class ProductImageValidatorTests
{
    [TestCase("image/jpeg", ".jpg", ImageKind.Jpeg)]
    [TestCase("image/jpeg", ".jpeg", ImageKind.Jpeg)]
    [TestCase("image/jpeg", ".jfif", ImageKind.Jpeg)]
    [TestCase("image/png", ".png", ImageKind.Png)]
    [TestCase("image/webp", ".webp", ImageKind.WebP)]
    public void TryValidate_accepts_matching_allowed_extension_content_type_and_signature(
        string contentType,
        string extension,
        ImageKind imageKind)
    {
        var bytes = CreateImageBytes(imageKind);
        var file = CreateFile(bytes, $"product{extension}", contentType);

        var valid = ProductImageValidator.TryValidate(file, out var image, out var error);

        valid.ShouldBeTrue();
        error.ShouldBeNull();
        image.ShouldNotBeNull();
        image.ContentType.ShouldBe(contentType);
        image.Content.ShouldBe(bytes);
    }

    [TestCase("image/gif", ".gif", ImageKind.Gif)]
    [TestCase("image/png", ".jpg", ImageKind.Png)]
    [TestCase("image/jpeg", ".png", ImageKind.Jpeg)]
    [TestCase("image/webp", ".webp", ImageKind.Png)]
    public void TryValidate_rejects_disallowed_or_mismatched_image_metadata(
        string contentType,
        string extension,
        ImageKind imageKind)
    {
        var file = CreateFile(
            CreateImageBytes(imageKind),
            $"product{extension}",
            contentType);

        var valid = ProductImageValidator.TryValidate(file, out var image, out var error);

        valid.ShouldBeFalse();
        image.ShouldBeNull();
        error.ShouldNotBeNullOrWhiteSpace();
    }

    [Test]
    public void TryValidate_rejects_null_and_empty_files()
    {
        ProductImageValidator.TryValidate(null, out var nullImage, out var nullError)
            .ShouldBeFalse();
        nullImage.ShouldBeNull();
        nullError.ShouldNotBeNullOrWhiteSpace();

        var emptyFile = CreateFile([], "product.png", "image/png");

        ProductImageValidator.TryValidate(emptyFile, out var emptyImage, out var emptyError)
            .ShouldBeFalse();
        emptyImage.ShouldBeNull();
        emptyError.ShouldNotBeNullOrWhiteSpace();
    }

    [Test]
    public void TryValidate_rejects_a_file_larger_than_two_mebibytes()
    {
        var bytes = new byte[ProductImageValidator.MaxFileSize + 1];
        CreateImageBytes(ImageKind.Png).CopyTo(bytes, 0);
        var file = CreateFile(bytes, "product.png", "image/png");

        var valid = ProductImageValidator.TryValidate(file, out var image, out var error);

        valid.ShouldBeFalse();
        image.ShouldBeNull();
        error.ShouldNotBeNullOrWhiteSpace();
    }

    [Test]
    public void TryValidate_rejects_a_stream_that_contains_more_data_than_its_declared_length()
    {
        var bytes = new byte[ProductImageValidator.MaxFileSize + 1];
        CreateImageBytes(ImageKind.Png).CopyTo(bytes, 0);
        var file = new DeceptiveFormFile(
            bytes,
            ProductImageValidator.MaxFileSize,
            "product.png",
            "image/png");

        var valid = ProductImageValidator.TryValidate(file, out var image, out var error);

        valid.ShouldBeFalse();
        image.ShouldBeNull();
        error.ShouldNotBeNullOrWhiteSpace();
    }

    private static FormFile CreateFile(
        byte[] bytes,
        string fileName,
        string contentType)
    {
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "Picture", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    private static byte[] CreateImageBytes(ImageKind imageKind)
        => imageKind switch
        {
            ImageKind.Jpeg => [0xFF, 0xD8, 0xFF, 0xE0, 0x00],
            ImageKind.Png => [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00],
            ImageKind.WebP =>
            [
                0x52, 0x49, 0x46, 0x46,
                0x04, 0x00, 0x00, 0x00,
                0x57, 0x45, 0x42, 0x50,
                0x00
            ],
            ImageKind.Gif => [0x47, 0x49, 0x46, 0x38, 0x39, 0x61],
            _ => throw new ArgumentOutOfRangeException(nameof(imageKind))
        };

    public enum ImageKind
    {
        Jpeg,
        Png,
        WebP,
        Gif
    }

    private sealed class DeceptiveFormFile(
        byte[] content,
        long declaredLength,
        string fileName,
        string contentType) : IFormFile
    {
        public string ContentType { get; } = contentType;

        public string ContentDisposition => string.Empty;

        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();

        public long Length { get; } = declaredLength;

        public string Name => "Picture";

        public string FileName { get; } = fileName;

        public void CopyTo(Stream target)
            => target.Write(content);

        public Task CopyToAsync(
            Stream target,
            CancellationToken cancellationToken = default)
            => target.WriteAsync(content, cancellationToken).AsTask();

        public Stream OpenReadStream()
            => new MemoryStream(content, writable: false);
    }
}
