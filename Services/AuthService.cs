using BCrypt.Net;

namespace JournalApp.Services;

public class AuthService
{
    private const string PinHashKey = "journalapp_pin_hash";

    public async Task<bool> HasPinAsync()
    {
        var hash = await SecureStorage.GetAsync(PinHashKey);
        return !string.IsNullOrWhiteSpace(hash);
    }

    public async Task SetPinAsync(string pin)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(pin);
        await SecureStorage.SetAsync(PinHashKey, hash);
    }

    public async Task<bool> VerifyPinAsync(string pin)
    {
        var hash = await SecureStorage.GetAsync(PinHashKey);
        if (string.IsNullOrWhiteSpace(hash)) return true; // no pin set
        return BCrypt.Net.BCrypt.Verify(pin, hash);
    }

    public void ClearPin()
    {
        SecureStorage.Remove(PinHashKey);
    }
}