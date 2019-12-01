using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    class Config
    {
        public IPAddress Ip { get; private set; }
        public int Port { get; private set; }
        public int ConnectionTimeoutMs { get; private set; }
        public string ConnectionString { get; private set; }
        public bool FirstRun { get; private set; }

        private static Config config;

        public static Config GetConfig()
        {
            if (config == null)
            {
                config = new Config();
                try
                {
                    using (StreamReader reader = new StreamReader(File.Open("config.ini", FileMode.Open)))
                    {
                        string ip = reader.ReadLine();
                        config.Ip = IPAddress.Parse(ip.Substring(ip.IndexOf("=") + 1));

                        string port = reader.ReadLine();
                        config.Port = Convert.ToInt32(port.Substring(port.IndexOf("=") + 1));

                        string connectionTimeout = reader.ReadLine();
                        if (connectionTimeout.EndsWith("="))
                            config.ConnectionTimeoutMs = 0;
                        else
                            config.ConnectionTimeoutMs = Convert.ToInt32(connectionTimeout.Substring(connectionTimeout.IndexOf("=") + 1));

                        string first_run = reader.ReadLine();
                        if (first_run.EndsWith("="))
                            config.FirstRun = false;
                        else
                            config.FirstRun = Convert.ToBoolean(first_run.Substring(first_run.IndexOf("=") + 1));

                        string connectionString = reader.ReadLine();
                        if (connectionString.EndsWith("="))
                            throw new Exception("DB connection string isn't set");
                        else
                        {
                            string connectionSubstring = connectionString.Substring(connectionString.IndexOf("Server"));
                            connectionSubstring = connectionSubstring.Remove(connectionSubstring.Length - 1);
                            if (config.FirstRun && connectionSubstring.Contains("Database"))
                            {
                                connectionSubstring = connectionSubstring.Substring(0, connectionSubstring.IndexOf("Database"));

                            }
                            config.ConnectionString = connectionSubstring;
                        }
                    }
                }
                catch (FileNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                    config = CreateConfig();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine("Invalid config file, fix it, or delete it to create new on the next launch");
                }
            }
            return config;
        }

        public void SetConnectionString()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(File.Open("config.ini", FileMode.Open)))
                {
                    writer.WriteLine($"internal_ip={config.Ip}");
                    writer.WriteLine($"port={config.Port}");
                    writer.WriteLine($"connection_timeout_ms={config.ConnectionTimeoutMs}");
                    writer.WriteLine($"first_run=False");
                    writer.WriteLine($"db_connectionString=({config.ConnectionString}{(config.ConnectionString.EndsWith("Database=SimpleChat;") ? "" : "Database=SimpleChat;")})");
                    writer.WriteLine("");
                    writer.WriteLine("// Configure db_connectionString before first run;");
                    writer.WriteLine("");
                    writer.WriteLine("// IMPORTANT first_run will drop and recreate chat database if it sets to true.");
                    writer.WriteLine("// After success db creation it will automatically switch to false;");
                }
                config.ConnectionString = $"{config.ConnectionString}Database=Chat;";
                config.FirstRun = false;
            }
            catch (Exception e)
            {
                Console.WriteLine("SetConnectionString exception");
                Console.WriteLine(e.Message);
            }
        }

        private static Config CreateConfig()
        {
            Config res = new Config();
            using (StreamWriter writer = new StreamWriter(File.Open("config.ini", FileMode.Create)))
            {
                writer.WriteLine($"internal_ip={res.Ip}");
                writer.WriteLine($"port={res.Port}");
                writer.WriteLine($"connection_timeout_ms={res.ConnectionTimeoutMs}");
                writer.WriteLine($"first_run={res.FirstRun}");
                writer.WriteLine($"db_connectionString=({res.ConnectionString})");
                writer.WriteLine("");
                writer.WriteLine("// Configure db_connectionString before first run;");
                writer.WriteLine("");
                writer.WriteLine("// IMPORTANT first_run will drop and recreate chat database if it sets to true.");
                writer.WriteLine("// After success db creation it will automatically switch to false;");
            }
            Console.WriteLine("New config created");
            return res;
        }

        private Config()
        {
            Ip = IPAddress.Parse("127.0.0.1");
            Port = 1815;
            ConnectionTimeoutMs = 5000;
            ConnectionString = "Server=127.0.0.1;Port=9999;User Id=postgres;Password=password;";
            FirstRun = false;
        }
    }
}
