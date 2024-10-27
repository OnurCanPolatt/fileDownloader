class Program
{
    static async Task Main(string[] args)
    {
        string url;

        // Daha önce kaydedilmiş URL'yi oku
        if (File.Exists(FileDownloader.UrlFileName))
        {
            url = File.ReadAllText(FileDownloader.UrlFileName); // URL'yi dosyadan oku
            Console.WriteLine($"Devam eden indirme için URL: {url}");
        }
        else
        {
            Console.WriteLine("İndirmek istediğiniz dosyanın URL'sini girin:");
            url = Console.ReadLine(); // Kullanıcıdan URL al
            File.WriteAllText(FileDownloader.UrlFileName, url); // URL'yi kaydet
        }

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
        Console.WriteLine($"Yüzde: {e.Percentage:F2}% - İndirilen: {e.bytesDownloaded / 1024.0:F2} KB / Toplam: {e.totalBytes / 1024.0:F2} KB");
    }
}
