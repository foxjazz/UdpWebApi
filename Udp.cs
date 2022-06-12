using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using PgpCore;
using UdpWebApi.Models;
using UdpWebApi.Registrations;

namespace UdpWebApi;
[ApiController]
[Route("[controller]")]
public class UdpController : ControllerBase
{
    public delegate void UpdateInfoTxtDelegate(string str);

    private Register local_register;
    private bool _gotTextFlag = false;
    private string _rmessage;
    private bool listenInit;
    private static UdpListener udpl;
         
    public class msg
    {
        public string message { get; set; }
        
    }
    private IManager manager;
    public UdpController(IManager __manager)
    {
        manager = __manager;
        udpl = UdpListener.GetInstance;
        this.listenInit = false;
    }
    // GET
    private IPEndPoint _endPoint;
    [HttpGet("Test")]
    public msg Function2()
    {
        var utilities = new Utlities2();
        var  s = utilities.getIP();
        var r = s.Result;
        var m = new msg();
        string date = DateTime.Now.ToString("mm:ss");
        m.message = "from server" + date;
        return m;
    }
    [HttpGet("IsAlive")]
    public string IsAlive()
    {
        return "alive";
    }

    
    [HttpGet("IP")]
    public string IP()
    {
        //var ip = request.HttpContext.Connection.RemoteIpAddress;
        
        // var data = Dns.GetHostEntry(Dns.GetHostName());
        var ip = HttpContext.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress?.ToString();
        // string clientAddress = HttpContext.Current.Request.UserHostAddress;
        var ip2 = HttpContext.Request.Host.Value;
        return ip;
    }

    [HttpGet("Listen")]
    public msg listen()
    {
        /*var c = new UdpClient();
        c.Client.Listen(52626);*/
        using (UdpClient socket = new UdpClient())
        {
            msg m;
            m = new msg();
            if (!this.listenInit)
            {
                if (local_register == null)
                {
                    m.message = "no local register";
                    return m;
                }
                    
                UdpListener.UpdateInfoTxt = new UpdateInfoTxtDelegate(UpdateUdpTxtMethod);
                var startingAtPort = 26000;
                var maxNumberOfPortsToCheck = 550;
                var range = Enumerable.Range(startingAtPort, maxNumberOfPortsToCheck);
                var portsInUse = 
                    from p in range
                    join used in System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners()
                        on p equals used.Port
                    select p;

                var FirstFreeUDPPortInRange = range.Except(portsInUse).FirstOrDefault();

                udpl.initUdp(FirstFreeUDPPortInRange);
                this.listenInit = true;
                udpl.Listen();
            }
            while (!_gotTextFlag)
            {
                Thread.Sleep(50);    
                
            }

            m.message = _rmessage;
            return m;
        }
        //konverter det til en string
    }

    public void UpdateUdpTxtMethod(string rtext)
    {
        _rmessage = rtext;
        _gotTextFlag = true;
    }

    [HttpGet("Close")]
    public msg close()
    {
        if (udpl != null)
        {
            _rmessage = "closed";
            _gotTextFlag = true;
            udpl.close();
        }
        var m = new msg();
        m.message = "closed";
        return m;
    }
    
    [HttpPost("Send")]
    public msg send([FromBody] msg message)
    {
        using (UdpClient socket = new UdpClient())
        {
            IPEndPoint EndPoint = null;
            byte[] data = Encoding.UTF8.GetBytes(message.message);

            //kalder send metoden der har de 4 parametre i sig
            //- Clienten vil gerne sende en besked til serveren
            int len = socket.Send(data, data.Length, "127.0.0.1", 5005);
            var m = new msg();
            m.message = len.ToString();
            return m;
        }
    }

    [HttpGet("Search")]
    public Register search(string email)
    {
        return manager.search(email);
    }

    [HttpPost("register")]
    public msg register([FromBody] Register reg)
    { var m = new msg();
        try
        {
            local_register = reg;
            manager.add(reg);
           
            m.message = "success";
            return m;
        }
        catch (Exception ex)
        {
            m.message = ex.Message;
            return m;
        }
    }
    
}