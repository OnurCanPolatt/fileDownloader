using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

public class DownloadStatusEventArgs : EventArgs
{
    public double totalBytes { get; set; }
    public double bytesDownloaded { get; set; }
    public double Percentage { get; set; }
}
class FileDownloader
{
    private const string ProgressFileName = "download_progress.txt"; // İndirme durumu dosyası
    private const string UrlFileName = "download_url.txt"; // İndirme URL'sini saklamak için dosya
    public event EventHandler<DownloadStatusEventArgs> onDownloadStatus;
    public async Task DownloadFileAsync(string url, string destinationPath)
    {
        using (HttpClient httpClient = new HttpClient())
        {
            long totalReadBytes = 0;
            long totalBytes = -1;

            // İndirme durumunu dosyadan oku
            if (File.Exists(ProgressFileName))
            {
                string progress = File.ReadAllText(ProgressFileName);
                if (!string.IsNullOrWhiteSpace(progress)) // Boş değilse
                {
                    totalReadBytes = long.Parse(progress); // Okunan byte sayısını al
                }
            }

            // Dosyanın mevcut boyutunu kontrol et
            if (File.Exists(destinationPath))
            {
                totalReadBytes = new FileInfo(destinationPath).Length; // Mevcut dosyanın boyutunu al
            }

            // URL'den dosya boyutunu sorgula
            var headResponse = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
            headResponse.EnsureSuccessStatusCode(); // Başarılı bir yanıt alındığından emin ol

            // Toplam byte sayısını al
            totalBytes = headResponse.Content.Headers.ContentLength ?? -1L;
            if (totalBytes == -1)
            {
                Console.WriteLine("Toplam dosya boyutu belirlenemedi.");
                return;
            }

            // Dosya boyutunu konsola yazdır
            Console.WriteLine($"İnecek olan dosya boyutu: {totalBytes / 1024.0:F2} KB");

            // Dosya akışını oluştur
            using (var fileStream = new FileStream(destinationPath, FileMode.Append, FileAccess.Write, FileShare.None))
            {
                var buffer = new byte[8192]; // 8 KB'lık bir tampon
                int readBytes;

                // Eğer toplam okunan byte sayısı toplam byte sayısından büyükse, sıfırla
                if (totalReadBytes >= totalBytes)
                {
                    Console.WriteLine("İndirme tamamlanmış. Dosya zaten mevcut.");
                    return;
                }

                // HTTP isteği için Range başlığını ayarla (kaldığı yerden devam etmek için)
                httpClient.DefaultRequestHeaders.Range = new System.Net.Http.Headers.RangeHeaderValue(totalReadBytes, null);

                // URL'den dosyayı indir
                var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode(); // Başarılı bir yanıt alındığından emin ol

                using (var contentStream = await response.Content.ReadAsStreamAsync())
                {
                    while ((readBytes = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, readBytes); // Tamponu dosyaya yaz
                        totalReadBytes += readBytes; // Toplam okunan byte sayısını güncelle

                        // İndirme durumunu kaydet
                        File.WriteAllText(ProgressFileName, totalReadBytes.ToString());

                        // İlerleme yüzdesini hesapla ve göster
                        var progressPercentage = (double)totalReadBytes / totalBytes * 100;
                        //Console.WriteLine($"İlerleme: {progressPercentage:F2}%");
                        onDownloadStatus?.Invoke(this, new DownloadStatusEventArgs
                        {
                            bytesDownloaded = totalReadBytes,
                            totalBytes = totalBytes,
                            Percentage = progressPercentage
                        });
                    }
                }
            }

            // İndirme tamamlandığında kontrol et
            if (totalReadBytes == totalBytes)
            {
                Console.WriteLine($"İndirme tamamlandı: {destinationPath}");
                File.Delete(ProgressFileName); // İndirme tamamlandığında durum dosyasını sil
                File.Delete(UrlFileName); // URL dosyasını sil
            }
            else
            {
                Console.WriteLine("İndirme tamamlanmadı. Lütfen kontrol edin.");
            }
        }
    }

    // Geçersiz karakterleri temizleyen metot
    public string CleanFileName(string fileName)
    {
        // Geçersiz karakterleri temizle
        return Regex.Replace(fileName, @"[<>:""/\\|?*]", "_");
    }
}