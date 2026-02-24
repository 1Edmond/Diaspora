using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Logging.Configurations;

public class ElasticsearchConfiguration
{
    public string Url { get; set; } = "http://elasticsearch";
    public string IndexPrefix { get; set; } = "";
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool AutoRegisterTemplate { get; set; } = true;
    public int NumberOfShards { get; set; } = 1;
    public int NumberOfReplicas { get; set; } = 1;

    public static ElasticsearchConfiguration FromConnectionString(string connectionString)
    {
        var uri = new Uri(connectionString);

        var userInfo = uri.UserInfo.Split(':');

        return new ElasticsearchConfiguration
        {
            Url = $"{uri.Scheme}://{uri.Host}:{uri.Port}",
            Username = userInfo.Length > 0 ? userInfo[0] : "",
            Password = userInfo.Length > 1 ? userInfo[1] : ""
        };

    }
    }