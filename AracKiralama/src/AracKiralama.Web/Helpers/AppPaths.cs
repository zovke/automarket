namespace AracKiralama.Web.Helpers;

/// <summary>
/// Uygulamanın yazma yaptığı klasörler (veritabanı, yüklenen görseller, oturum anahtarları).
/// Varsayılan: proje klasörü. Sunucuda (Docker / Render) DATA_DIR ortam değişkeniyle değiştirilir,
/// böylece kod klasörüne yazma izni gerekmez.
/// </summary>
public class AppPaths
{
    public string DataDir { get; }
    public string UploadsDir => Path.Combine(DataDir, "uploads");
    public string KeysDir => Path.Combine(DataDir, "keys");
    public string DatabaseFile => Path.Combine(DataDir, "arackiralama.db");

    public AppPaths(string contentRoot)
    {
        var fromEnv = Environment.GetEnvironmentVariable("DATA_DIR");
        DataDir = string.IsNullOrWhiteSpace(fromEnv) ? contentRoot : fromEnv;
        Directory.CreateDirectory(DataDir);
        Directory.CreateDirectory(UploadsDir);
    }
}
