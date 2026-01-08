namespace Infrastructure.Persistence.Options.Postgres;

public class PostgresOptions
{
    public static string SectionName { get; } = "Infrastructure:Persistence:Postgres";

    public string ConnectionString => $"Host={Host};Port={Port};Database={Database};Username={Username};Password={Password}";

    public string Host { get; set; } = string.Empty;

    public string Database { get; set; } = string.Empty;

    public int Port { get; set; } = 5432;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}