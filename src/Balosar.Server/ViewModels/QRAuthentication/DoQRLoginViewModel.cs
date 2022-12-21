namespace Balosar.Server.ViewModels.QRAuthentication;

public class DoQRLoginViewModel
{
    public string DeviceCode { get; set; }
    public string? ReturnUrl { get; set; }
}