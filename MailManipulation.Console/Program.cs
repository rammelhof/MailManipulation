using MailManipulation.Lib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;


string exeDir = System.AppContext.BaseDirectory;
Directory.SetCurrentDirectory(exeDir);


var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile(Path.Combine(exeDir, "appsettings.json"))
    .AddJsonFile(Path.Combine(exeDir, $"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json"), true)
    .Build();


Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Warning)
    .CreateLogger();


var mailManipulationConfigurationSection = configuration.GetSection("MailManipulationConfiguration");
var mailManipulationConfiguration = mailManipulationConfigurationSection.Get<MailManipulationConfiguration>();




//var mailManipulationConfiguration = configuration.GetValue<MailManipulationConfiguration>("MailForwarderConfiguration");
//var mailManipulationConfiguration = configuration.Get<MailManipulationConfiguration>(o =>
//{
//    o.ErrorOnUnknownConfiguration = true;
//});
var mailManipulation = new MailManipulation.Lib.MailManipulation(Log.Logger, mailManipulationConfiguration);
mailManipulation.ProcessMails();