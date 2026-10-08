using AracKiralama.Web.Helpers;

namespace AracKiralama.Web.Services;

public interface IImageService
{
    /// <summary>Görseli DATA_DIR/uploads altına kaydeder ve tarayıcıda kullanılacak yolu ("/uploads/...") döndürür.</summary>
    Task<string> SaveAsync(IFormFile file);
}

public class ImageService(AppPaths paths) : IImageService
{
    private const long MaxBytes = 5 * 1024 * 1024; // 5 MB
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    public async Task<string> SaveAsync(IFormFile file)
    {
        if (file.Length == 0) throw new BusinessRuleException("Boş dosya yüklenemez.");
        if (file.Length > MaxBytes) throw new BusinessRuleException("Görsel en fazla 5 MB olabilir.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext) || !file.ContentType.StartsWith("image/"))
            throw new BusinessRuleException("Sadece JPG, PNG veya WEBP görsel yükleyebilirsiniz.");

        // DATA_DIR/uploads (bkz. Helpers/AppPaths.cs); Program.cs bu klasörü /uploads adresinden sunar.
        var folder = paths.UploadsDir;
        Directory.CreateDirectory(folder);

        // Kullanıcının verdiği dosya adını kullanmıyoruz; çakışma ve güvenlik riskine karşı rastgele ad.
        var fileName = $"{Guid.NewGuid():N}{ext}";
        await using var stream = File.Create(Path.Combine(folder, fileName));
        await file.CopyToAsync(stream);

        return $"/uploads/{fileName}";
    }
}
