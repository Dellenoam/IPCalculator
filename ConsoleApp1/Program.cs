using System;
using System.IO;
using System.Text.RegularExpressions;

namespace ConsoleApp1
{
    internal class Program
    {
        struct IPData
        {
            public string ip { get; set; }
            public string[] ip_mass { get; set; }
            public string[] ip_min { get; set; }
            public string ip_min_str { get; set; }
            public string ip_max_str { get; set; }
            public string[] ip_max { get; set; }
            public string[] mask_mass { get; set; }
            public string[] mask_mass_inverse { get; set; }
            public string mask { get; set; }
            public string[] mask_long { get; set; }
            public string num_allowed_mask { get; set; }
            public MatchCollection matchedIP { get; set; }
            public MatchCollection matched_mask_short { get; set; }
            public MatchCollection matched_mask_long { get; set; }
        }

        static void IPCalculator(IPData ipdata, Logging logging)
        {
            Regex is_ip_correct = new Regex(@"\b(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b");
            ipdata.matchedIP = is_ip_correct.Matches(ipdata.ip);

            if (ipdata.matchedIP.Count == 0)
            {
                Console.WriteLine("Вы ввели некорректные данные.");
                return;
            }

            ipdata.num_allowed_mask = @"(255|254|252|248|240|224|192|128|0)";
            var is_mask_correct_short = new Regex("/[0-32]");
            ipdata.matched_mask_short = is_mask_correct_short.Matches(ipdata.mask);
            var is_mask_correct_long = new Regex("^" + ipdata.num_allowed_mask + @"\." + ipdata.num_allowed_mask + @"\." + ipdata.num_allowed_mask + @"\." + ipdata.num_allowed_mask + "$");
            ipdata.matched_mask_long = is_mask_correct_long.Matches(ipdata.mask);

            if (ipdata.matched_mask_long.Count == 0 && ipdata.matched_mask_short.Count == 0)
            {
                Console.WriteLine("Вы ввели некорректные данные.");
                return;
            }
            else
            {
                if (ipdata.matched_mask_short.Count != 0)
                {
                    ipdata.ip_mass = ipdata.ip.Split('.');
                    ipdata.mask = ipdata.mask.Replace("/", "");
                    ipdata.mask_long = new[] { "0.0.0.0", "128.0.0.0", "192.0.0.0", "224.0.0.0", "240.0.0.0", "248.0.0.0", "252.0.0.0", "254.0.0.0", "255.0.0.0",
                    "255.128.0.0", "255.192.0.0", "255.224.0.0", "255.240.0.0", "255.248.0.0", "255.252.0.0", "255.254.0.0", "255.255.0.0",
                    "255.255.128.0", "255.255.192.0", "255.255.224.0", "255.255.240.0", "255.255.248.0", "255.255.252.0", "255.255.254.0", "255.255.255.0",
                    "255.255.255.128", "255.255.255.192", "255.255.255.224", "255.255.255.240", "255.255.255.248", "255.255.255.252", "255.255.255.254", "255.255.255.255"};
                    ipdata.mask_mass = ipdata.mask_long[Convert.ToInt32(ipdata.mask)].Split('.');
                    ipdata.mask_mass_inverse = new string[4];
                    Array.Copy(ipdata.mask_mass, ipdata.mask_mass_inverse, 4);
                    ipdata.ip_min = new string[4];
                    ipdata.ip_max = new string[4];
                }
                else
                {
                    ipdata.ip_mass = ipdata.ip.Split('.');
                    ipdata.mask_mass = ipdata.mask.Split('.');
                    ipdata.mask_mass_inverse = new string[4];
                    Array.Copy(ipdata.mask_mass, ipdata.mask_mass_inverse, 4);
                    ipdata.ip_min = new string[4];
                    ipdata.ip_max = new string[4];

                    for (int i = 0; i <= 3; i++)
                    {
                        if (i > 0)
                        {
                            var previous_digit = Convert.ToInt32(ipdata.mask_mass[i - 1]);
                            if (previous_digit != 255 && Convert.ToInt32(ipdata.mask_mass[i]) != 0)
                            {
                                Console.WriteLine("Вы ввели некорректные данные.");
                                return;
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                }

                for (int i = 0; i <= 3; i++)
                {
                    ipdata.ip_mass[i] = Convert.ToString(Convert.ToInt32(ipdata.ip_mass[i]), 2).PadLeft(8, '0');
                    ipdata.mask_mass[i] = Convert.ToString(Convert.ToInt32(ipdata.mask_mass[i]), 2).PadLeft(8, '0');
                    ipdata.mask_mass_inverse[i] = Convert.ToString(Convert.ToInt32(ipdata.mask_mass_inverse[i]), 2).PadLeft(8, '0');
                }

                for (int i = 0; i <= 3; i++)
                {
                    ipdata.ip_min_str = ""; 
                    ipdata.ip_max_str = "";

                    for (int j = 0; j <= 7; j++)
                    {

                        if (ipdata.ip_mass[i][j] == '1' && ipdata.mask_mass[i][j] == '1')
                        {
                            ipdata.ip_min_str += '1';
                        }
                        else
                        {
                            ipdata.ip_min_str += '0';
                        }

                        if (ipdata.ip_mass[i][j] == '1' || ipdata.mask_mass_inverse[i][j] == '0')
                        {
                            ipdata.ip_max_str += '1';
                        }
                        else
                        {
                            ipdata.ip_max_str += '0';
                        }
                    }

                    ipdata.ip_min[i] = Convert.ToString(Convert.ToInt32(ipdata.ip_min_str, 2));
                    ipdata.ip_max[i] = Convert.ToString(Convert.ToInt32(ipdata.ip_max_str, 2));
                }

                logging.logs("IP: " + ipdata.ip + " | " + "Mask: " + ipdata.mask + " | " + "Network: " + string.Join(".", ipdata.ip_min) + " | " + " Broadcast: " + string.Join(".", ipdata.ip_max) + " | ");

                Console.WriteLine($"\nАдрес сети: {ipdata.ip_min[0] + '.' + ipdata.ip_min[1] + '.' + ipdata.ip_min[2] + '.' + ipdata.ip_min[3]}");
                Console.WriteLine($"Широковещательный адрес: {ipdata.ip_max[0] + '.' + ipdata.ip_max[1] + '.' + ipdata.ip_max[2] + '.' + ipdata.ip_max[3]}");
                
                if (string.Join(".", ipdata.ip_min) == string.Join(".", ipdata.ip_max))
                {
                    Console.WriteLine($"Минимальный ip: - ");
                    Console.WriteLine($"Максимальный ip: - ");
                    logging.logs("IP Min: - | ");
                    logging.logs("IP Max: - ");
                }
                else
                {
                    Console.WriteLine($"Минимальный ip: {ipdata.ip_min[0] + '.' + ipdata.ip_min[1] + '.' + ipdata.ip_min[2] + '.' + Convert.ToString(Convert.ToInt32(ipdata.ip_min[3]) + 1)}");
                    Console.WriteLine($"Максимальный ip: {ipdata.ip_max[0] + '.' + ipdata.ip_max[1] + '.' + ipdata.ip_max[2] + '.' + Convert.ToString(Convert.ToInt32(ipdata.ip_max[3]) - 1)}\n");
                    logging.logs("IP Min: " + ipdata.ip_min[0] + '.' + ipdata.ip_min[1] + '.' + ipdata.ip_min[2] + '.' + Convert.ToString(Convert.ToInt32(ipdata.ip_min[3]) + 1) + " | ");
                    logging.logs("IP Max: " + ipdata.ip_max[0] + '.' + ipdata.ip_max[1] + '.' + ipdata.ip_max[2] + '.' + Convert.ToString(Convert.ToInt32(ipdata.ip_max[3]) - 1) + "\n");
                }
            }
        }

        static void Main(string[] args)
        {
            string ip, mask;
            if (args.Length == 0)
            {
                Logging logging = new Logging();
                logging.logs("Data and time: " + DateTime.Now);

                Console.Write("Введите ip: ");
                ip = Console.ReadLine();
                Console.Write("Введите маску: ");
                mask = Console.ReadLine();
                IPData ipdata = new IPData();
                ipdata.ip = ip;
                ipdata.mask = mask;
                IPCalculator(ipdata, logging);
            }
            else if (args.Length == 2)
            {
                Logging logging = new Logging();
                logging.logs("Data and time: " + DateTime.Now);

                ip = args[0];
                mask = args[1];
                IPData ipdata = new IPData();
                ipdata.ip = ip;
                ipdata.mask = mask;
                IPCalculator(ipdata, logging);
            }
            else
            {
                Logging logging = new Logging();
                logging.logs("Data and time: " + DateTime.Now + " | " + "Without arguments or missing one argument\n");

                Console.WriteLine("Вы должны ввести два аргумента. Первым аргументом принимается ip, вторым маска");
            }
        }
    }
}
