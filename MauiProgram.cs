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

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
