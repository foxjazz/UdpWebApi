using System.Collections.Concurrent;
using Newtonsoft.Json;
using UdpWebApi.Models;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace UdpWebApi.Registrations;

public class Manager : IManager
{
     private static ConcurrentDictionary<string, Register> registers;

     public Manager()
     {
          if (registers == null) 
          {
               var path = Directory.GetCurrentDirectory();
               
               if (File.Exists(Path.Combine(path, "registered.json")))
               {
                    var txt = File.ReadAllText(Path.Combine(path, "registered.json"));
                    registers = JsonConvert.DeserializeObject<ConcurrentDictionary<string, Register>>(txt);
               }
               else
               {
                    registers = new ConcurrentDictionary<string, Register>();
               }
          }
     }

     public Register getLocal()
     {
          var k = registers.Keys.First();
          Register valu;
          if (registers.TryGetValue(k, out valu) == true)
          {
               return valu;
          };
          valu = new Register();
          valu.status = "Not Found";
          return valu;
     }
     public bool add(Register reg)
     {
          var result =  registers.TryAdd(reg.email, reg);
          save();
          return result;

     }

     public void save()
     {
          string jsonString = JsonSerializer.Serialize(registers);
          var path = Directory.GetCurrentDirectory();
          File.WriteAllText(Path.Combine(path, "registered.json"), jsonString);
     }

     public Register search(string email)
     {
          Register valu;
          if (registers.TryGetValue(email, out valu) == true)
          {
               return valu;
          };
          valu = new Register();
          valu.status = "Not Found";
          return valu;
     }
    public List<Register> list()
    {
        return registers.Values.ToList();
    }
     

}