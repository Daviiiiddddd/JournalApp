using Microsoft.Extensions.Logging;

namespace JournalApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();
		AppDomain.CurrentDomain.UnhandledException += (_, e) =>
		{
    		try
    		{
        		var path = Path.Combine(FileSystem.AppDataDirectory, "crash.log");
        		File.WriteAllText(path, e.ExceptionObject?.ToString() ?? "Unknown crash");
    		}
    		catch { }
		};

		TaskScheduler.UnobservedTaskException += (_, e) =>
		{
    		try
    		{
        		var path = Path.Combine(FileSystem.AppDataDirectory, "crash.log");
        		File.WriteAllText(path, e.Exception?.ToString() ?? "Unknown task crash");
    		}
    		catch { }
		};

		Console.WriteLine(FileSystem.AppDataDirectory);

		#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		#endif
		// === Dependency Injection registrations ===
		builder.Services.AddSingleton<JournalApp.Data.AppDb>();
		builder.Services.AddSingleton<JournalApp.Data.Repositories.JournalRepository>();
		builder.Services.AddSingleton<JournalApp.Services.JournalService>();
		builder.Services.AddSingleton<JournalApp.Services.AuthService>();
		builder.Services.AddSingleton<JournalApp.Services.AnalyticsService>();
		builder.Services.AddSingleton<JournalApp.Services.PdfExportService>();
		builder.Services.AddMauiBlazorWebView();

		return builder.Build();
	}
	
}

		