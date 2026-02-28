using System;
using System.Threading;
using System.Threading.Tasks;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Security;
using MimeKit;



namespace Models
{
    public class EmailListener
    {
        private readonly string _host = "imap.gmail.com"; 
        private readonly int _port = 993;
        private readonly string _email;
        private readonly string _password;

        
        public event Action<MimeMessage>? OnNewEmailReceived;

        public EmailListener(string email, string password)
        {
            _email = email;
            _password = password;
        }

        public async Task StartListeningAsync(CancellationToken ct)
        {
            using var client = new ImapClient();

            try
            {
                
                await client.ConnectAsync(_host, _port, SecureSocketOptions.SslOnConnect, ct);
                await client.AuthenticateAsync(_email, _password, ct);

                
                var inbox = client.Inbox;
                await inbox.OpenAsync(FolderAccess.ReadOnly, ct);

                
                inbox.CountChanged += (sender, e) =>
                {
                    
                    var message = inbox.GetMessage(inbox.Count - 1);
                    OnNewEmailReceived?.Invoke(message);
                };

                Console.WriteLine("Listening for new emails...");

                
                while (!ct.IsCancellationRequested)
                {
                    
                    await client.IdleAsync(ct);
                }

                await client.DisconnectAsync(true, ct);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}