using Microsoft.Maui.Storage;

namespace JournalApp.Services;

public class AuthService
{
    private const string LoggedInKey = "auth_logged_in";
    private const string UsernameKey = "auth_username";

    // Stored credentials (local demo)
    private const string UserKey = "auth_user";
    private const string PassKey = "auth_pass";

    public bool IsLoggedIn => Preferences.Get(LoggedInKey, false);
    public string CurrentUsername => Preferences.Get(UsernameKey, "");

    // ✅ Called by Login page (creates admin/admin123 once)
    public void EnsureSeedUser()
    {
        var u = Preferences.Get(UserKey, "");
        var p = Preferences.Get(PassKey, "");

        if (string.IsNullOrWhiteSpace(u) || string.IsNullOrWhiteSpace(p))
        {
            Preferences.Set(UserKey, "admin");
            Preferences.Set(PassKey, "admin123");
        }
    }

    public bool Login(string username, string password)
    {
        var u = Preferences.Get(UserKey, "");
        var p = Preferences.Get(PassKey, "");

        var ok =
            !string.IsNullOrWhiteSpace(username) &&
            !string.IsNullOrWhiteSpace(password) &&
            username == u &&
            password == p;

        // Only set logged in = true on success
        if (ok)
        {
            Preferences.Set(LoggedInKey, true);
            Preferences.Set(UsernameKey, username);
        }
        // Do NOT set LoggedInKey = false here (avoid auto-logout)
        return ok;
    }

    public void Logout()
    {
        Preferences.Set(LoggedInKey, false);
        Preferences.Remove(UsernameKey);
    }

    // Optional: allow changing credentials later from Settings
    public bool ChangeCredentials(string newUsername, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newUsername) || string.IsNullOrWhiteSpace(newPassword))
            return false;

        Preferences.Set(UserKey, newUsername.Trim());
        Preferences.Set(PassKey, newPassword);
        return true;
    }

    public bool VerifyPassword(string password)
    {
        var saved = Preferences.Get(PassKey, "");
        return !string.IsNullOrWhiteSpace(password) && password == saved;
    }

    public string GetSavedUsername()
    {
        return Preferences.Get(UserKey, "");
    }   
}