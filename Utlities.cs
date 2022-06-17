using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.Unicode;
using Extensions;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace UdpWebApi;
using System.Net.Http;
using Models;

public class Utlities2
{
    public async Task<string> getIP()
    {
        var hc = new HttpClient();
        string ipResult="";
        var result = hc.GetStringAsync("http://checkip.dyndns.org");
        var res = await result;
        
        var ipa = res.Split(":");
        if (ipa.Length == 2)
        {
            ipResult = ipa[1];
            ipa = ipResult.Split("<");
            ipResult = ipa[0];
        }
        else
        {
            ipResult = "error, ip not found";
        }
        hc.Dispose();
        return ipResult;
        // http://checkip.dyndns.org
    }
    
    public async Task<string> PostRegistration(Register registration, string uri)
    {
        var hc = new HttpClient();
        
        var serialize = JsonSerializer.Serialize(registration);
        var content = new StringContent(serialize, Encoding.UTF8, "application/json");
        var result = hc.PostAsync(uri, content);
        var res = await result;
        var nr = res.Content.ReadAsStringAsync().Result;
        hc.Dispose();
        var m = JsonConvert.DeserializeObject<UdpController.msg>(nr);
        return m.message;
    }
    public IPAddress GetDefaultIPv4Address()
    {
        var adapters = from adapter in NetworkInterface.GetAllNetworkInterfaces()
            where adapter.OperationalStatus == OperationalStatus.Up &&
                  adapter.Supports(NetworkInterfaceComponent.IPv4)
                  && adapter.GetIPProperties().GatewayAddresses.Count > 0 &&
                  adapter.GetIPProperties().GatewayAddresses[0].Address.ToString() != "0.0.0.0"
            select adapter;

        if (adapters.Count() > 1)
        {
            throw new ApplicationException("The default IPv4 address could not be determined as there are two interfaces with gateways.");
        }
        else
        {
            UnicastIPAddressInformationCollection localIPs = adapters.First().GetIPProperties().UnicastAddresses;
            foreach (UnicastIPAddressInformation localIP in localIPs)
            {
                if (localIP.Address.AddressFamily == AddressFamily.InterNetwork &&
                    !localIP.Address.ToString().StartsWith("10") &&
                    !IPAddress.IsLoopback(localIP.Address))
                {
                    return localIP.Address;
                }
            }
        }

        return null;
    }
}