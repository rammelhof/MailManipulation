namespace MailManipulation.Lib;

public class MailManipulationConfiguration
{
    public MailManipulationConfiguration()
    {
        
    }

    public String? ImapServer { get; set; }
    public String? ImapUser { get; set; }
    public String? ImapPassword { get; set; }
    public int? ImapPort { get; set; }
    public String? ImapNamespace { get; set; }
    public String? ImapFolder { get; set; }
    
}