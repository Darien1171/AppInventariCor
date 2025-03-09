using System.Threading.Tasks;

namespace AppInventariCor.Services
{
    public interface IBarcodeScannerService
    {
        Task<string> ScanAsync();
        Task<string> GenerateQRCodeAsync(string content);
        Task<string> GenerateBarcodeAsync(string content, BarcodeFormat format);
    }

    public enum BarcodeFormat
    {
        QR_CODE,
        CODE_128,
        EAN_13,
        EAN_8,
        UPC_A,
        UPC_E
    }
}