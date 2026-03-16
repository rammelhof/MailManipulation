using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using MailKit;
using MailKit.Net.Imap;
using Serilog;

namespace MailManipulation.Lib;

public class MailManipulation
{
    private readonly ILogger _logger;
    private readonly MailManipulationConfiguration? _configuration;

    public MailManipulation(ILogger logger, MailManipulationConfiguration? configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public void ProcessMails()
    {
        _logger.Information("MailManipulation.ProcessMails running at: {time}", DateTimeOffset.Now);

        if (_configuration == null)
        {
            _logger.Information($"No configuration provided");
            return;
        }

        if (_configuration.ImapServer == null)
        {
            _logger.Information($"No valid configuration provided");
            return;
        }

        _logger.Information($"ImapServer: {_configuration.ImapServer}");


        using (var imapClient = new ImapClient())
        {
            imapClient.ServerCertificateValidationCallback = (
                                object sender,
                                X509Certificate? certificate,
                                X509Chain? chain,
                                SslPolicyErrors sslPolicyErrors) =>
                            {
                                return true;
                            };

            imapClient.Connect(_configuration.ImapServer, _configuration.ImapPort ?? 993, MailKit.Security.SecureSocketOptions.Auto);
            imapClient.Authenticate(_configuration.ImapUser, _configuration.ImapPassword);

            DebugImapInfo(imapClient);


            var folder = imapClient.GetFolder(_configuration.ImapFolder);
            folder.Open(FolderAccess.ReadWrite);


            _logger.Information("Total messages: {0} in {1}", folder.Count, _configuration.ImapFolder);

            var query = MailKit.Search.SearchQuery.All;
            var uids = folder.Search(query);

            _logger.Information("Messages after filter: {0} in {1}", uids.Count, _configuration.ImapFolder);

            foreach (var messageId in uids)
            {
                var message = folder.GetMessage(messageId);
                _logger.Information($"{message.Date} {message.Subject}");
            }

            imapClient.Disconnect(true);
        }



        _logger.Information("MailManipulation.ProcessMails finished at: {time}", DateTimeOffset.Now);

    }

    private void DebugImapInfo(ImapClient imapClient)
    {

        foreach (var ns in imapClient.PersonalNamespaces)
            _logger.Information($"PersonalNamespace: \"{ns.Path}\" \"{ns.DirectorySeparator}\"");
        foreach (var ns in imapClient.SharedNamespaces)
            _logger.Information($"SharedNamespaces: \"{ns.Path}\" \"{ns.DirectorySeparator}\"");
        foreach (var ns in imapClient.OtherNamespaces)
            _logger.Information($"OtherNamespaces: \"{ns.Path}\" \"{ns.DirectorySeparator}\"");

        if (_configuration?.ImapNamespace != null)
        {
            var ns = imapClient.PersonalNamespaces.FirstOrDefault(n => n.Path.Equals(_configuration.ImapNamespace));
            if (ns == null)
                ns = imapClient.SharedNamespaces.FirstOrDefault(n => n.Path.Equals(_configuration.ImapNamespace));
            if (ns == null)
                ns = imapClient.OtherNamespaces.FirstOrDefault(n => n.Path.Equals(_configuration.ImapNamespace));

            if (ns != null)
            {
                var namespaceFolder = imapClient.GetFolder(ns);
                var subfolders = namespaceFolder.GetSubfolders();
                foreach (var folder in subfolders)
                    _logger.Information($"{folder.FullName}");
            }
        }

    }
}
