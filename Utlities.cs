namespace UdpWebApi;
using System.Net.Http;
public class Utlities2
{
    public async Task<string> getIP()
    {
        var hc = new HttpClient();
        string ipResult="";
        var result = hc.GetStringAsync("http://checkip.dyndns.org");
        var res = await result;
        var ct = res;
        var ipa = res.Split(":");
        if (ipa.Length == 2)
        {
            ipResult = ipa[1];
            ipa = ipResult.Split("<");
            ipResult = ipa[0];
        }
        else
        {
            ipResult = "ip not found";
        }
        return ipResult;
        // http://checkip.dyndns.org
    }
}