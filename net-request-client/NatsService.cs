using NATS.Client;

public class NatsService : IDisposable
{
    private IConnection? nc;
    private SemaphoreSlim connectionLock = new SemaphoreSlim(1);
    public NatsService(IConfiguration configuration)
    {
        Servers = configuration["NatsServers"]!.Split(",");
    }

    public string[] Servers { get; }
    public IConnection Connection
    {
        get
        {
            if(nc is null)
            {
                try
                {
                    connectionLock.Wait();
                    if(nc is null)
                    {
                        var options = ConnectionFactory.GetDefaultOptions();
                        options.Url = string.Join(",", Servers);
                        options.AsyncErrorEventHandler += OnError;
                        options.ClosedEventHandler += OnClosed;
                        options.DisconnectedEventHandler += OnDisconnected; 
                        options.ReconnectedEventHandler += OnReconnected;
                        nc = new ConnectionFactory().CreateConnection(options);
                    }
                }
                finally
                {
                    connectionLock.Release();
                }
            }
            return nc;
        }
    }

    public bool IsConnected => (Connection.IsClosed() == false 
    && Connection.IsDraining() == false 
    && Connection.IsReconnecting() == false 
    && Connection.ConnectedId is not null);
    

    private void OnError(object? sender, ErrEventArgs args)
    {
        Console.WriteLine("Error: ");
        Console.WriteLine("   Server: " + args.Conn.ConnectedUrl);
        Console.WriteLine("   Message: " + args.Error);
        Console.WriteLine("   Subject: " + args.Subscription.Subject);
    }

    private void OnClosed(object? sender, ConnEventArgs args)
    {
        Console.WriteLine("Connection Closed: ");
        Console.WriteLine("   Server: " + args.Conn.ConnectedUrl);
    }

    private void OnDisconnected(object? sender, ConnEventArgs args)
    {
        Console.WriteLine("Connection Disconnected: ");
        Console.WriteLine("   Server: " + args.Conn.ConnectedUrl);
    }

    private void OnReconnected(object? sender, ConnEventArgs args)
    {
        Console.WriteLine("Connection Reconnected: ");
        Console.WriteLine("   Server: " + args.Conn.ConnectedUrl);
    }

    public void Dispose()
    {
        if(nc is not null)
        {
            nc.Drain();
            nc.Close();
            nc.Dispose();
        }
    }
}