using Aspire.Hosting;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var sharedVaultPath = Path.Combine(Path.GetTempPath(), "vault-aspire");
Directory.CreateDirectory(Path.Combine(sharedVaultPath, "identity"));
Directory.CreateDirectory(Path.Combine(sharedVaultPath, "kyc"));
Directory.CreateDirectory(Path.Combine(sharedVaultPath, "document"));
Directory.CreateDirectory(Path.Combine(sharedVaultPath, "profile"));
Directory.CreateDirectory(Path.Combine(sharedVaultPath, "procedure"));


var pgPassword = builder.AddParameter("postgres-password", secret: true);
var pgUser = builder.AddParameter("postgres-user", secret: true);

var rabbitPassword = builder.AddParameter("rabbitmq-password", secret: true);
var rabbitUser = builder.AddParameter("rabbitmq-user", secret: true);

var elasticPassword = builder.AddParameter("elastic-password", secret: true);
var kibanaPassword = builder.AddParameter("kibana-password", secret: true);

var gatewaySecret = builder.AddParameter("gateway-secret", secret: true);
var jwtSecret = builder.AddParameter("jwt-secret", secret: true);

var vault = builder.AddContainer("vault", "hashicorp/vault", "1.21.1")
    .WithHttpEndpoint(targetPort: 8200, name: "http")
    .WithVolume("vault_data", "/vault/data")
    .WithVolume("vault_logs", "/vault/logs")
    .WithBindMount("../../vault/config.hcl", "/vault/config.hcl", isReadOnly: true)
    .WithEntrypoint("/bin/sh")
    .WithArgs("-c", @"
        mkdir -p /vault/data /vault/logs && \
        chown -R 100:100 /vault/data /vault/logs && \
        chmod 700 /vault/data /vault/logs && \
        exec vault server -config=/vault/config.hcl
    ")
    .WithHttpHealthCheck("/", 200);

var vaultInit = builder.AddContainer("vault-init", "hashicorp/vault", "1.21.1")
    .WithBindMount("../../scripts/init-vault.sh", "/init-vault.sh", isReadOnly: true)
    .WithVolume("vault_credentials", "/vault-credentials")
    .WithEnvironment("VAULT_ADDR", vault.GetEndpoint("http"))
    .WithEntrypoint("/bin/sh")
    .WithParentRelationship(vault)
    .WithArgs("-c", "/init-vault.sh")
    .WaitFor(vault);

var identityVaultAgent = builder.AddContainer("identity-vault-agent", "hashicorp/vault", "1.21.1")
    .WithBindMount("../../configs/vault-agent.hcl", "/vault-agent.hcl", isReadOnly: true)
    .WithBindMount(Path.Combine(sharedVaultPath, "identity"), "/vault")
    .WithVolume("vault_credentials", "/vault-credentials", isReadOnly: true)
    .WithEntrypoint("/bin/sh")
    .WithParentRelationship(vaultInit)
    .WithArgs("-c", "mkdir -p /vault/creds && cp /vault-credentials/identity/role-id /vault/creds/role-id && cp /vault-credentials/identity/secret-id /vault/creds/secret-id && vault agent -config=/vault-agent.hcl")
    .WaitForCompletion(vaultInit);

var kycVaultAgent = builder.AddContainer("kyc-vault-agent", "hashicorp/vault", "1.21.1")
    .WithBindMount("../../configs/vault-agent.hcl", "/vault-agent.hcl", isReadOnly: true)
    .WithBindMount(Path.Combine(sharedVaultPath, "kyc"), "/vault")
    .WithVolume("vault_credentials", "/vault-credentials", isReadOnly: true)
    .WithEntrypoint("/bin/sh")
    .WithParentRelationship(vaultInit)
    .WithArgs("-c", "mkdir -p /vault/creds && cp /vault-credentials/kyc/role-id /vault/creds/role-id && cp /vault-credentials/kyc/secret-id /vault/creds/secret-id && vault agent -config=/vault-agent.hcl")
    .WaitForCompletion(vaultInit);

var documentVaultAgent = builder.AddContainer("document-vault-agent", "hashicorp/vault", "1.21.1")
    .WithBindMount("../../configs/vault-agent.hcl", "/vault-agent.hcl", isReadOnly: true)
    .WithBindMount(Path.Combine(sharedVaultPath, "document"), "/vault")
    .WithVolume("vault_credentials", "/vault-credentials", isReadOnly: true)
    .WithEntrypoint("/bin/sh")
    .WithParentRelationship(vaultInit)
    .WithArgs("-c", "mkdir -p /vault/creds && cp /vault-credentials/document/role-id /vault/creds/role-id && cp /vault-credentials/document/secret-id /vault/creds/secret-id && vault agent -config=/vault-agent.hcl")
    .WaitForCompletion(vaultInit);


var cache = builder.AddRedis("redis")
    .WithRedisCommander()
    .WithDataVolume();

var postgres = builder.AddPostgres("postgres", password: pgPassword, userName: pgUser)
    .WithImage("postgres:16-alpine")
    .WithArgs(
        "-c", "max_connections=200",
        "-c", "shared_buffers=256MB",
        "-c", "effective_cache_size=1GB",
        "-c", "work_mem=16MB",
        "-c", "maintenance_work_mem=64MB",
        "-c", "wal_buffers=8MB",
        "-c", "checkpoint_completion_target=0.9",
        "-c", "log_statement=all"
    )
    .WithVolume("postgres_data", "/var/lib/postgresql/data")
#if DEBUG
    .WithPgWeb(x => x.WithImageTag("0.17.0"))
    .WithPgAdmin(x => x.WithImageTag("9.12.0"))
#endif
    ;

var identityDb = postgres.AddDatabase("IdentityDb", databaseName: "diaspora_identity_db");
var notificationDb = postgres.AddDatabase("NotificationDb", databaseName: "diaspora_notification_db");
var kycDb = postgres.AddDatabase("KycDb", databaseName: "diaspora_kyc_db");
var profileDb = postgres.AddDatabase("ProfileDb", databaseName: "diaspora_profile_db");
var documentDb = postgres.AddDatabase("DocumentDb", databaseName: "diaspora_document_db");
var procedureDb = postgres.AddDatabase("ProcedureDb", databaseName: "diaspora_procedure_db");

var rabbitmq = builder.AddRabbitMQ("rabbitmq", userName: rabbitUser, password: rabbitPassword)
    .WithImage("rabbitmq:4-management-alpine")
    .WithVolume("rabbitmq_data", "/var/lib/rabbitmq")
    .WithHttpEndpoint(targetPort: 15672, name: "management");

var elasticsearch = builder.AddElasticsearch("elasticsearch", password: elasticPassword)
    .WithImage("elasticsearch:9.3.0")
    .WithVolume("es_data", "/usr/share/elasticsearch/data");

var kibana = builder.AddContainer("kibana", "docker.elastic.co/kibana/kibana:9.3.0")
    .WithHttpEndpoint(targetPort: 5601, name: "http")
    .WithEnvironment("ELASTICSEARCH_HOSTS", elasticsearch.GetEndpoint("http"))
    .WithEnvironment("ELASTICSEARCH_USERNAME", "kibana_system")
    .WithEnvironment("ELASTICSEARCH_PASSWORD", kibanaPassword)
    .WithParentRelationship(elasticsearch)
    .WaitFor(elasticsearch);
void AddSharedConfig(IResourceBuilder<ProjectResource> svc, string serviceName) =>
    svc
        .WithEnvironment("JwtSettings__Secret", jwtSecret)
        .WithEnvironment("JwtSettings__Issuer", "DonationSystem.Identity")
        .WithEnvironment("JwtSettings__Audience", "DonationSystem")
        .WithEnvironment("JwtSettings__ExpirationMinutes", "60")
        // Serilog
        .WithEnvironment("Serilog__MinimumLevel__Default", "Information")
        .WithEnvironment("Serilog__MinimumLevel__Override__Microsoft", "Warning")
        .WithEnvironment("Serilog__MinimumLevel__Override__System", "Warning")
        // Elasticsearch 
        .WithEnvironment("Elasticsearch__IndexPrefix", "diaspora")
        .WithEnvironment("Elasticsearch__AutoRegisterTemplate", "true")
        .WithEnvironment("Elasticsearch__NumberOfShards", "1")
        .WithEnvironment("Elasticsearch__NumberOfReplicas", "0")
        .WithEnvironment("Elasticsearch__Username", "elastic")
        .WithEnvironment("Elasticsearch__Password", elasticPassword)
         // Gateway
        .WithEnvironment("Gateway__Secret", gatewaySecret)
        // Messaging
        .WithEnvironment("Messaging__ServiceName", serviceName);

var identityApi = builder.AddProject<Projects.IdentityService_API>("identityservice")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Docker")
    .WithEnvironment("Vault__TokenPath", Path.Combine(sharedVaultPath, "identity", "token"))
    .WithEnvironment("Vault__Url", vault.GetEndpoint("http"))
    .WithEnvironment("Vault__KeyName", "identity")
    .WithReference(identityDb)
    .WithReference(rabbitmq)
    .WithReference(cache)
    .WithReference(elasticsearch)
    .WaitFor(identityDb)
    .WaitFor(rabbitmq)
    .WaitFor(identityVaultAgent)
    .WaitFor(elasticsearch)
    .WaitFor(cache)
    .WithHttpHealthCheck("/health");
AddSharedConfig(identityApi, "IdentityService");

var notificationApi = builder.AddProject<Projects.Notification_Api>("notificationservice")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Docker")
    .WithEnvironment("Vault__Url", vault.GetEndpoint("http"))
    .WithReference(notificationDb)
    .WithReference(rabbitmq)
    .WithReference(elasticsearch)
    .WithReference(cache)
    .WaitFor(notificationDb)
    .WaitFor(identityApi)
    .WaitFor(cache)
    .WaitFor(rabbitmq)
    .WaitFor(elasticsearch)
    .WithHttpHealthCheck("/health");
AddSharedConfig(notificationApi, "NotificationService");

var profileApi = builder.AddProject<Projects.Profile_Api>("profileservice")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Docker")
    .WithReference(profileDb)
    .WithReference(rabbitmq)
    .WithReference(elasticsearch)
    .WithReference(cache)
    .WaitFor(profileDb)
    .WaitFor(identityApi)
    .WaitFor(rabbitmq)
    .WaitFor(cache)
    .WaitFor(elasticsearch)
    .WithHttpHealthCheck("/health");
AddSharedConfig(profileApi, "ProfileService");

var documentApi = builder.AddProject<Projects.Document_API>("documentservice")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Docker")
    .WithEnvironment("Vault__TokenPath", Path.Combine(sharedVaultPath, "document", "token"))
    .WithEnvironment("Vault__Url", vault.GetEndpoint("http"))
    .WithEnvironment("Vault__KeyName", "document")
    .WithEnvironment("FileStorage__BasePath", "/app/uploads")
    .WithReference(documentDb)
    .WithReference(rabbitmq)
    .WithReference(elasticsearch)
    .WithReference(cache)
    .WaitFor(documentDb)
    .WaitFor(identityApi)
    .WaitFor(rabbitmq)
    .WaitFor(cache)
    .WaitFor(documentVaultAgent)
    .WaitFor(elasticsearch)
    .WithHttpHealthCheck("/health");
AddSharedConfig(documentApi, "DocumentService");

var kycApi = builder.AddProject<Projects.KYC_Api>("kycservice")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Docker")
    .WithEnvironment("Vault__Url", vault.GetEndpoint("http"))
    .WithEnvironment("Vault__KeyName", "kyc")
    .WithEnvironment("Vault__TokenPath", Path.Combine(sharedVaultPath, "kyc", "token"))
    .WithReference(kycDb)
    .WithReference(rabbitmq)
    .WithReference(cache)
    .WithReference(elasticsearch)
    .WaitFor(kycDb)
    .WaitFor(identityApi)
    .WaitFor(rabbitmq)
    .WaitFor(cache)
    .WaitFor(elasticsearch)
    .WaitFor(kycVaultAgent)
    .WithHttpHealthCheck("/health");
AddSharedConfig(kycApi, "KycService");

var procedureApi = builder.AddProject<Projects.Procedure_Api>("procedureservice")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Docker")
    .WithReference(procedureDb)
    .WithReference(rabbitmq)
    .WithReference(elasticsearch)
    .WithReference(cache)
    .WaitFor(procedureDb)
    .WaitFor(identityApi)
    .WaitFor(rabbitmq)
    .WaitFor(cache)
    .WaitFor(elasticsearch)
    .WithHttpHealthCheck("/health");
AddSharedConfig(procedureApi, "ProcedureService");

var gateway = builder.AddProject<Projects.ServcicesGateway>("servcicesgateway")
    .WithExternalHttpEndpoints()
    .WithEnvironment("Gateway__Secret", gatewaySecret)
    .WithEnvironment("JwtSettings__SecretKey", jwtSecret)
    .WithEnvironment("JwtSettings__Issuer", "DonationSystem.Identity")
    .WithEnvironment("JwtSettings__Audience", "DonationSystem")
    .WithEnvironment("JwtSettings__ExpirationMinutes", "60")
    .WithEnvironment("AuthorizationCache__DefaultExpirationMinutes", "5")
    .WithReference(cache)
    .WithReference(identityApi)
    .WithReference(notificationApi)
    .WithReference(profileApi)
    .WithReference(procedureApi)
    .WithReference(documentApi)
    .WithReference(kycApi)
    .WithChildRelationship(identityApi)
    .WithChildRelationship(profileApi)
    .WithChildRelationship(procedureApi)
    .WithChildRelationship(documentApi)
    .WithChildRelationship(notificationApi)
    .WithChildRelationship(kycApi)
    .WaitFor(cache)
    .WaitFor(identityApi)
    .WaitFor(procedureApi)
    .WaitFor(profileApi)
    .WaitFor(documentApi)
    .WaitFor(notificationApi)
    .WaitFor(kycApi);

builder.Build().Run();