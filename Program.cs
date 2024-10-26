
//Örnek url=http://newmanager.smg.com.tr/files/00011f99-63e2-4515-8b8f-d7d4d0a69437.mp3
class Program
{
    static async Task Main(string[] args)
    {
        string url;

        // Daha önce kaydedilmiş URL'yi oku

        Console.WriteLine("İndirmek istediğiniz dosyanın URL'sini girin:");
        url = Console.ReadLine(); // Kullanıcıdan URL al

        // Kullanıcının Downloads klasörünü al
        string downloadsFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
        string fileName = Path.GetFileName(url); // URL'den dosya adını al
        fileName = new FileDownloader().CleanFileName(fileName); // Geçersiz karakterleri temizle
        string destinationPath = Path.Combine(downloadsFolder, fileName); // Tam dosya yolu

        FileDownloader downloader = new FileDownloader(); // FileDownloader sınıfından bir nesne oluştur
        downloader.onDownloadStatus += indirmeDurumu;
        await downloader.DownloadFileAsync(url, destinationPath); // Dosyayı indir
    }

    private static void indirmeDurumu(object? sender, DownloadStatusEventArgs e)
    {
        Console.WriteLine($"yüzde {e.Percentage}");
    }
}