using Consul;

public class ConsulHostedService : IHostedService
{
    private readonly IConsulClient _consulClient;
    private readonly IConfiguration _configuration;
    private readonly string _serviceId;

    public ConsulHostedService(IConsulClient consulClient, IConfiguration configuration)
    {
        _consulClient = consulClient ?? throw new ArgumentNullException(nameof(consulClient));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _serviceId = Guid.NewGuid().ToString(); // Ensure unique service ID
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Retrieve port from configuration, default to 5001 if null
        var servicePortString = _configuration["ConsulConfig:ServicePort"];
        var servicePort = int.TryParse(servicePortString, out var port) ? port : 5001;

        var registration = new AgentServiceRegistration
        {
            ID = _serviceId,
            Name = _configuration["ConsulConfig:ServiceName"] ?? "UserService", // Fallback to "UserService" if null
            Address = "localhost",
            Port = servicePort,
            Check = new AgentServiceCheck
            {
                HTTP = $"http://localhost:{servicePort}/api/health", // Use the dynamically retrieved port
                Interval = TimeSpan.FromSeconds(10),
                Timeout = TimeSpan.FromSeconds(5)
            }
        };

        await _consulClient.Agent.ServiceRegister(registration, cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _consulClient.Agent.ServiceDeregister(_serviceId, cancellationToken);
    }
}
