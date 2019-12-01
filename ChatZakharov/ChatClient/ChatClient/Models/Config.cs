using System;
using System.IO;
using System.Net;

namespace ChatClient.Models
{
    public class Config
    {
        public IPAddress Ip { get; private set; }
        public int Port { get; private set; }
        public int ConnectionTimeoutMs { get; private set; }

        public static Config GetConfig()
        {
            Config res = new Config();
            try
            {
                using (StreamReader reader = new StreamReader(File.Open("config.ini", FileMode.Open)))
                {
                    string ip = reader.ReadLine();
                    res.Ip = IPAddress.Parse(ip.Substring(ip.IndexOf("=") + 1));

                    string port = reader.ReadLine();
                    res.Port = Convert.ToInt32(port.Substring(port.IndexOf("=") + 1));

                    string connectionTimeout = reader.ReadLine();
                    if (connectionTimeout.EndsWith("="))
                        res.ConnectionTimeoutMs = 0;
                    else
                        res.ConnectionTimeoutMs = Convert.ToInt32(connectionTimeout.Substring(connectionTimeout.IndexOf("=") + 1));
                }
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
                res = CreateConfig();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Invalid config file, fix it, or delete it to create new on the next launch");
            }
            return res;
        }

        private static Config CreateConfig()
        {
            Config res = new Config();
            using (StreamWriter writer = new StreamWriter(File.Open("config.ini", FileMode.Create)))
            {
                writer.WriteLine($"server_ip={res.Ip}");
                writer.WriteLine($"port={res.Port}");
                writer.WriteLine($"connection_timeout_ms={res.ConnectionTimeoutMs}");
            }
            Console.WriteLine("New config created");
            return res;
        }

        private Config()
        {
            Ip = IPAddress.Parse("127.0.0.1");
            Port = 1815;
            ConnectionTimeoutMs = 5000;
        }
    }
}