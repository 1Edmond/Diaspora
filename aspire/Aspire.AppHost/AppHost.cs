using Aspire.Hosting;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var sharedVaultPath = Path.Combine(Path.GetTempPath(), "vault-aspire");
Directory.CreateDirectory(Path.Combine(sharedVaultPath, "identity"));
Directory.CreateDirectory(Path.Combine(sharedVaultPath, "kyc"));
Directory.CreateDirectory(Path.Combine(sharedVaultPath, "document"));
Directory.CreateDirectory(Path.Combine(sharedVaultPath, "profile"));
Directory.CreateDirectory(Path.Combine(sharedVaultPath, "procedure"));
Directory.CreateDirectory(Path.Combine(sharedVaultPath, "wallet"));
Directory.CreateDirectory(Path.Combine(sharedVaultPath, "chat"));
Directory.CreateDirectory(Path.Combine(sharedVaultPath, "marketplace"));

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
    .WithHttpHealthCheck("/v1/sys/health?standbyok=true&uninitcode=200&sealedcode=200", 200);

var vaultInit = builder.AddContainer("vault-init", "hashicorp/vault", "1.21.1")
    .WithBindMount("../../scripts/init-vault.sh", "/init-vault.sh", isReadOnly: true)
    .WithVolume("vault_credentials", "/vault-credentials")
    .WithEnvironment("VAULT_ADDR", vault.GetEndpoint("http"))
    .WithEntrypoint("/bin/sh")
    .WithParentRelationship(vault)
    .WithArgs("-c", "/init-vault.sh")
    .WaitFor(vault);


IResourceBuilder<ContainerResource> AddVaultAgent(string name, string service) =>
    builder.AddContainer($"{service}-vault-agent", "hashicorp/vault", "1.21.1")
        .WithBindMount("../../configs/vault-agent.hcl", "/vault-agent.hcl", isReadOnly: true)
        .WithBindMount(Path.Combine(sharedVaultPath, service), "/vault")
        .WithVolume("vault_credentials", "/vault-credentials", isReadOnly: true)
        .WithEntrypoint("/bin/sh")
        .WithParentRelationship(vaultInit)
        .WithArgs("-c", $"mkdir -p /vault/creds && cp /vault-credentials/{service}/role-id /vault/creds/role-id && cp /vault-credentials/{service}/secret-id /vault/creds/secret-id && vault agent -config=/vault-agent.hcl")
        .WaitForCompletion(vaultInit);

var identityVaultAgent = AddVaultAgent("identity-vault-agent", "identity");
var kycVaultAgent = AddVaultAgent("kyc-vault-agent", "kyc");
var documentVaultAgent = AddVaultAgent("document-vault-agent", "document");
//var marketplaceVaultAgent = AddVaultAgent("marketplace-vault-agent", "marketplace");


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
var walletDb = postgres.AddDatabase("WalletDb", databaseName: "diaspora_wallet_db");
var chatDb = postgres.AddDatabase("ChatDb", databaseName: "diaspora_chat_db");
var marketPlaceDb = postgres.AddDatabase("MarketplaceDb", databaseName: "diaspora_marketplace_db");

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
        .WithEnvironment("Serilog__MinimumLevel__Default", "Information")
        .WithEnvironment("Serilog__MinimumLevel__Override__Microsoft", "Warning")
        .WithEnvironment("Serilog__MinimumLevel__Override__System", "Warning")
        .WithEnvironment("Elasticsearch__IndexPrefix", "diaspora")
        .WithEnvironment("Elasticsearch__AutoRegisterTemplate", "true")
        .WithEnvironment("Elasticsearch__NumberOfShards", "1")
        .WithEnvironment("Elasticsearch__NumberOfReplicas", "0")
        .WithEnvironment("Elasticsearch__Username", "elastic")
        .WithEnvironment("Elasticsearch__Password", elasticPassword)
        .WithEnvironment("Gateway__Secret", gatewaySecret)
        .WithEnvironment("Messaging__ServiceName", serviceName);


var identityApi = builder.AddProject<IdentityService_API>("identityservice")
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
    .WithEnvironment("Vault__TokenPath", Path.Combine(sharedVaultPath, "identity", "token"))
    .WithEnvironment("Vault__Url", vault.GetEndpoint("http"))
    .WithEnvironment("Vault__KeyName", "identity")
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
    .WithEnvironment("Vault__TokenPath", Path.Combine(sharedVaultPath, "identity", "token"))
    .WithEnvironment("Vault__Url", vault.GetEndpoint("http"))
    .WithEnvironment("Vault__KeyName", "identity")
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

//var kycApi = builder.AddProject<Projects.KYC_Api>("kycservice")
//    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Docker")
//    .WithEnvironment("Vault__Url", vault.GetEndpoint("http"))
//    .WithEnvironment("Vault__KeyName", "kyc")
//    .WithEnvironment("Vault__TokenPath", Path.Combine(sharedVaultPath, "kyc", "token"))
//    .WithReference(kycDb)
//    .WithReference(rabbitmq)
//    .WithReference(cache)
//    .WithReference(elasticsearch)
//    .WaitFor(kycDb)
//    .WaitFor(identityApi)
//    .WaitFor(rabbitmq)
//    .WaitFor(cache)
//    .WaitFor(elasticsearch)
//    .WaitFor(kycVaultAgent)
//    .WithHttpHealthCheck("/health");
//AddSharedConfig(kycApi, "KycService");

var procedureApi = builder.AddProject<Projects.Procedure_Api>("procedureservice")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Docker")
   .WithEnvironment("Vault__TokenPath", Path.Combine(sharedVaultPath, "identity", "token"))
    .WithEnvironment("Vault__Url", vault.GetEndpoint("http"))
    .WithEnvironment("Vault__KeyName", "identity")
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

//var walletApi = builder.AddProject<Projects.Wallet_Api>("walletservice")
//    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Docker")
//.WithEnvironment("Vault__TokenPath", Path.Combine(sharedVaultPath, "identity", "token"))
// .WithEnvironment("Vault__Url", vault.GetEndpoint("http"))
// .WithEnvironment("Vault__KeyName", "identity")
//    .WithReference(walletDb)
//    .WithReference(rabbitmq)
//    .WithReference(elasticsearch)
//    .WithReference(cache)
//    .WaitFor(walletDb)
//    .WaitFor(identityApi)
//    .WaitFor(rabbitmq)
//    .WaitFor(cache)
//    .WaitFor(elasticsearch)
//    .WithHttpHealthCheck("/health");
//AddSharedConfig(walletApi, "WalletService");

//var chatApi = builder.AddProject<Projects.Chat_Api>("chatservice")
//    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Docker")
//.WithEnvironment("Vault__TokenPath", Path.Combine(sharedVaultPath, "identity", "token"))
// .WithEnvironment("Vault__Url", vault.GetEndpoint("http"))
// .WithEnvironment("Vault__KeyName", "identity")
//    .WithReference(chatDb)
//    .WithReference(rabbitmq)
//    .WithReference(elasticsearch)
//    .WithReference(cache)
//    .WaitFor(chatDb)
//    .WaitFor(identityApi)
//    .WaitFor(rabbitmq)
//    .WaitFor(cache)
//    .WaitFor(elasticsearch)
//    .WithHttpHealthCheck("/health");

//AddSharedConfig(chatApi, "ChatService");

//var marketplaceApi = builder.AddProject<Projects.Marketplace_Api>("marketplaceservice")
//    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Docker")
//    .WithEnvironment("Vault__TokenPath", Path.Combine(sharedVaultPath, "marketplace", "token"))
//    .WithEnvironment("Vault__Url", vault.GetEndpoint("http"))
//    .WithEnvironment("Vault__KeyName", "marketplace")
//    .WithReference(marketPlaceDb)
//    .WithReference(rabbitmq)
//    .WithReference(elasticsearch)
//    .WithReference(cache)
//    .WaitFor(marketPlaceDb)
//    .WaitFor(identityApi)
//    .WaitFor(rabbitmq)
//    .WaitFor(cache)
//    .WaitFor(elasticsearch)
//   // .WaitFor(marketplaceVaultAgent)
//    .WithHttpHealthCheck("/health");

//AddSharedConfig(marketplaceApi, "MarketplaceService");


var gateway = builder.AddProject<Projects.ServcicesGateway>("servcicesgateway")
    .WithExternalHttpEndpoints()
    .WithEnvironment("Gateway__Secret", gatewaySecret)
    .WithEnvironment("JwtSettings__SecretKey", jwtSecret)
    .WithEnvironment("JwtSettings__Issuer", "DonationSystem.Identity")
    .WithEnvironment("JwtSettings__Audience", "DonationSystem")
    .WithEnvironment("JwtSettings__ExpirationMinutes", "60")
    .WithEnvironment("AuthorizationCache__DefaultExpirationMinutes", "5")
    .WithReference(identityApi)
    .WithReference(profileApi)
    .WithReference(procedureApi)
    .WithReference(notificationApi)
    .WithReference(documentApi)
    //.WithReference(kycApi)
    //.WithReference(walletApi)
    //.WithReference(chatApi)
    //.WithReference(marketplaceApi)
    .WithReference(cache)
    .WithChildRelationship(identityApi)
    .WithChildRelationship(profileApi)
    .WithChildRelationship(procedureApi)
    .WithChildRelationship(notificationApi)
    .WithChildRelationship(documentApi)
    //.WithChildRelationship(kycApi)
    //.WithChildRelationship(walletApi)
    //.WithChildRelationship(chatApi)
    //.WithChildRelationship(marketplaceApi)
    .WaitFor(cache)
    .WaitFor(identityApi)
    .WaitFor(profileApi)
    .WaitFor(procedureApi)
    .WaitFor(notificationApi)
    .WaitFor(documentApi)
    //.WaitFor(kycApi)
    //.WaitFor(walletApi)
    //.WaitFor(chatApi)
    //.WaitFor(marketplaceApi)
    ;

//var backoffice = builder.AddProject<Projects.Diaspora_Backoffice>("backoffice")
//    .WithReference(gateway)
//    .WaitFor(gateway)
//    ;

builder.Build().Run();