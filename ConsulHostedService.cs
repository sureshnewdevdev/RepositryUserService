using Consul;

public class ConsulHostedService : IHostedService
{
    private readonly IConsulClient _consulClient;
    private readonly IConfiguration _configuration;
    private readonly string _serviceId;

    public ConsulHostedService(IConsulClient consulClient, IConfiguration configuration)
    {
        _consulClient = consulClient;
        _configuration = configuration;
        _serviceId = Guid.NewGuid().ToString(); // Unique ID for each instance
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        /*
         * "ConsulConfig": {
    "Address": "http://localhost:8500",
    "ServiceName": "UserService",
    "ServicePort": "5001"
  }*/

        var registration = new AgentServiceRegistration
        {
            ID = _serviceId, // Unique service ID
            Name = _configuration["ConsulConfig:ServiceName"], // Common service name for multiple instances
            Address = "localhost",
            Port = int.Parse(_configuration["ConsulConfig:ServicePort"]), // Dynamic port
            Check = new AgentServiceCheck
            {
                HTTP = $"http://localhost:{_configuration["ConsulConfig:ServicePort"]}/api/health", // Health check URL
                Interval = TimeSpan.FromSeconds(10),
                Timeout = TimeSpan.FromSeconds(5)
            }
        };

        // Register the service with Consul
        Console.WriteLine($"Registering service {registration.Name} on port {registration.Port}");
        await _consulClient.Agent.ServiceRegister(registration, cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // Deregister the service from Consul
        await _consulClient.Agent.ServiceDeregister(_serviceId, cancellationToken);
    }
}
