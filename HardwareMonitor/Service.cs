using ConsoleService.Hardware;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net.Http;
using System.IO;
using System.Security.Cryptography;

namespace ConsoleService
{
    internal class Service
    {

        private static readonly HttpClient _client = new HttpClient();

        private HardwareInfos _hardware = new HardwareInfos();

        private StringBuilder _ipAdress = new StringBuilder();

        

        public void RunRefreshing(int ms,string port = null)
        {
            new Task(async () => {
                do
                {
                    _hardware.RefreshAll();

                    await Task.Delay(ms);

                } while (true);
            }).Start();
        }

        public void RunDataPost(string datakey)
        {
            new Task(async () =>
            {
                do
                {
                    Dictionary<string, string> _content = new Dictionary<string, string>();

                    JObject jObject = JObject.FromObject(_hardware);

                    jObject.Merge(JObject.FromObject(_hardware.GetHardDisksValues()));

                    string data = CryptingString(datakey, jObject.ToString());

                    _content.Add("datakey", datakey);
                    _content.Add("data", data);

                    var content = new FormUrlEncodedContent(_content);

                    await _client.PostAsync("https://handi-amg.uit.ac.ma/api/posts", content);

                    await Task.Delay(1100);

                } while (true);

            }).Start();

            Console.WriteLine($"To get data: https://handi-amg.uit.ac.ma/api/get?datakey={datakey}");
        }


        public void RunListener(string port ,string salt)
        {
            new Task(() =>
            {
                var listener = new HttpListener();

                listener.Prefixes.Add($"http://+:{port}/");
                listener.Start();              

                while (listener.IsListening)
                {
                    // Wait for a request
                    var context = listener.GetContext();
                    var request = context.Request;
                    var response = context.Response;
                    
                    JObject o = JObject.FromObject(_hardware);
                    o.Merge(JObject.FromObject(_hardware.GetHardDisksValues()));

                    // Serialize the data into a JSON string
                    var json = CryptingString(salt,o.ToString());

                    // Construct the response
                    var buffer = Encoding.UTF8.GetBytes(json);
                    response.ContentLength64 = buffer.Length;
                    var output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);

                    // Close the output stream
                    output.Close();
                }
            }).Start();
            Console.WriteLine(GetIpAdress(port));
        }


        public StringBuilder GetIpAdress(string port)
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            stringBuilder.AppendLine($"My IP Address is: {ip.Address}:{port}");
                        }
                    }
                }
            }
            return stringBuilder;
        }

        private string CryptingString(string key, string text)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] textBytes = Encoding.UTF8.GetBytes(text);

            for (int i = 0; i < textBytes.Length; i++)
            {
                textBytes[i] = (byte)(textBytes[i] ^ keyBytes[i % keyBytes.Length]);
            }

            return StringToBinary(Convert.ToBase64String(textBytes));
        }

        private string DeCryptingString(string key, string text)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] textBytes = Convert.FromBase64String(BinaryToString(text));

            for (int i = 0; i < textBytes.Length; i++)
            {
                textBytes[i] = (byte)(textBytes[i] ^ keyBytes[i % keyBytes.Length]);
            }

            return Encoding.UTF8.GetString(textBytes);
        }

        private string StringToBinary(string input)
        {
            StringBuilder sb = new StringBuilder();

            foreach (byte b in Encoding.UTF8.GetBytes(input))
            {
                string temp = Convert.ToString(b, 2);

                // This ensure that 8 chars represent the 8bits
                temp = "00000000".Substring(temp.Length) + temp;
                sb.Append(temp);
            }
            return sb.ToString();
        }

        private string BinaryToString(string input)
        {
            StringBuilder sb = new StringBuilder();
            while (input.Length > 0)
            {
                string block = input.Substring(0, 8);
                input = input.Substring(8);
                int value = 0;
                foreach (int x in block)
                {
                    int temp = x - 48;
                    value = (value << 1) | temp;
                }
                sb.Append(Convert.ToChar(value));
            }
            return sb.ToString();
        }

    }
}

