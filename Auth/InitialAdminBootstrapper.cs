namespace JobForge.Auth;

using JobForge.Persistence;
using JobForge.Users;

public sealed class InitialAdminBootstrapper
{
    private readonly UserRepository _repository;
    private readonly UserService _userService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<InitialAdminBootstrapper> _logger;

    public InitialAdminBootstrapper(
        UserRepository repository,
        UserService userService,
        IConfiguration configuration,
        ILogger<InitialAdminBootstrapper> logger)
    {
        _repository = repository;
        _userService = userService;
        _configuration = configuration;
        _logger = logger;
    }

    public void EnsureAdminExists()
    {
        // Once an Admin exists, bootstrap credentials are no longer required.
        if (_repository.AnyAdmin())
            return;

        string username =
            _configuration["BootstrapAdmin:Username"]
            ?? throw new InvalidOperationException(
                "No Admin exists and BootstrapAdmin:Username is not configured."
            );

        string password =
            _configuration["BootstrapAdmin:Password"]
            ?? throw new InvalidOperationException(
                "No Admin exists and BootstrapAdmin:Password is not configured."
            );

        if (_repository.GetByUsername(username) is not null)
        {
            throw new InvalidOperationException(
                "Bootstrap admin username is already in use."
            );
        }

        User admin = _userService.CreateUser(
            username,
            password,
            UserRole.Admin
        );

        _logger.LogInformation(
            "Initial administrator created with id {AdminId}.",
            admin.Id
        );
    }
}