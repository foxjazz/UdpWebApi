using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Math.EC;
using PgpCore;
using UdpWebApi.Models;
using UdpWebApi.Registrations;

namespace UdpWebApi;

[ApiController]
[Route("[controller]")]
public class UdpController : ControllerBase
{
    public delegate void UpdateInfoTxtDelegate(UdpFormat formattedMessage);

    private static Register local_register;
    private bool _gotTextFlag = false;
    private UdpFormat _rmessage;
    private bool listenInit;
    private static UdpListener udpl;
    private IConfiguration config;

    public class msg
    {
        public string message { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public string emailKey { get; set; }
    }

    private IManager manager;

    public UdpController(IManager __manager, IConfiguration c)
    {
        config = c;
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
        var s = utilities.getIP();
        var r = s.Result;
        var m = new msg();
        string date = DateTime.Now.ToString("mm:ss");
        m.message = "from server" + date;
        m.type = "info";
        m.name = "sys";
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
        if (udpl.serverType != "local")
            return new msg();

        using (UdpClient socket = new UdpClient())
        {
            var m = new msg();
            if (udpl.localIP == null)
            {
                m.message = "local IP not set";
                m.name = "sys";
                return m;
            }
            if (!this.listenInit)
            {
                if (local_register == null)
                {
                    local_register = manager.getLocal();
                    if (local_register.status == "Not Found")
                    {
                        m.message = "no local register";
                        m.type = "info";
                        m.name = "sys";
                        listenInit = true;
                        return m;
                    }
                }

                var utilities = new Utlities2();
                var s = utilities.GetDefaultIPv4Address();

                local_register.IP = udpl.localIP;
                UdpListener.UpdateInfoTxt = new UpdateInfoTxtDelegate(UpdateUdpTxtMethod);
                var startingAtPort = 26000;
                var maxNumberOfPortsToCheck = 550;
                var range = Enumerable.Range(startingAtPort, maxNumberOfPortsToCheck);
                var portsInUse =
                    from p in range
                    join used in System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties()
                            .GetActiveUdpListeners()
                        on p equals used.Port
                    select p;

                var FirstFreeUDPPortInRange = range.Except(portsInUse).FirstOrDefault();
                local_register.port = FirstFreeUDPPortInRange.ToString();


                utilities.PostRegistration(local_register, udpl.centerHost +  "/Udp/register");

                udpl.initUdp(FirstFreeUDPPortInRange);
                this.listenInit = true;
                udpl.Listen();
            }

            while (!_gotTextFlag)
            {
                Thread.Sleep(50);
            }

            m.message = _rmessage.message;
            m.type = "msg";
            m.name = _rmessage.name;
            return m;
        }
        //konverter det til en string
    }

    public void UpdateUdpTxtMethod(UdpFormat rtext)
    {
        _rmessage = rtext;
        _gotTextFlag = true;
    }

    [HttpGet("Close")]
    public msg close()
    {
        if (udpl.serverType != "local")
            return new msg();
        if (udpl != null)
        {
            var oi = new UdpFormat();
            oi.name = "sys";
            oi.message = "closed";
            _rmessage = oi; 
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
        if (udpl.serverType != "local")
            return new msg();
        try
        {
            using (UdpClient socket = new UdpClient())
            {
                IPEndPoint EndPoint = null;
                byte[] data = Encoding.UTF8.GetBytes(message.message);
                var r = manager.search(message.emailKey);
                var port = Int32.Parse(r.port);
                int len = socket.Send(data, data.Length, r.IP, port);
                var m = new msg();
                m.name = "sys";
                m.type = "info";
                m.message = len.ToString();
                return m;
            }
        }
        catch (Exception ex)
        {
            ex.Message.AppendToLog();
            System.Diagnostics.Process.GetCurrentProcess().Close();
        }

        return new msg();
    }

    [HttpGet("Search")]
    public Register search(string email)
    {
        return manager.search(email);
    }

    [HttpPost("register")]
    public msg register([FromBody] Register reg)
    {

        var m = new msg();
       if (reg.IP == null)
        {
            m.message = "registration needs IP";
            m.name = "sys";
            m.type = "error";
            return m;
        }
        try
        {
            local_register = reg;
            udpl.localIP = reg.IP;
            manager.add(reg);

            m.message = "success";
            m.name = "sys";
            m.type = "info";
            return m;
        }
        catch (Exception ex)
        {
            m.message = ex.Message;
            return m;
        }
    }

    [HttpGet("list")]

    public List<Register> list()
    {
        var man = new Manager();
        return man.list();
    }

}