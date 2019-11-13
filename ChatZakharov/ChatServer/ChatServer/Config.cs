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
        public int SendTimeoutMs { get; private set; }
        public int ReceiveTimeoutMs { get; private set; }

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

                    string sendTimeout = reader.ReadLine();
                    if (sendTimeout.EndsWith("="))
                        res.SendTimeoutMs = 0;
                    else
                        res.SendTimeoutMs = Convert.ToInt32(sendTimeout.Substring(sendTimeout.IndexOf("=") + 1));

                    string receiveTimeout = reader.ReadLine();
                    if (receiveTimeout.EndsWith("="))
                        res.ReceiveTimeoutMs = 0;
                    else
                        res.ReceiveTimeoutMs = Convert.ToInt32(receiveTimeout.Substring(receiveTimeout.IndexOf("=") + 1));
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
                writer.WriteLine($"internal_ip={res.Ip}");
                writer.WriteLine($"port={res.Port}");
                writer.WriteLine($"send_timeout_ms={res.SendTimeoutMs}");
                writer.WriteLine($"receive_timeout_ms={res.ReceiveTimeoutMs}");
            }
            Console.WriteLine("New config created");
            return res;
        }

        private Config()
        {
            Ip = IPAddress.Parse("127.0.0.1");
            Port = 1815;
            SendTimeoutMs = 10000;
            ReceiveTimeoutMs = 10000;
        }
    }
}
