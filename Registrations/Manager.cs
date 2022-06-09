using System.Collections.Concurrent;
using System.Text.Json;
using UdpWebApi.Models;

namespace UdpWebApi.Registrations;

public class Manager
{
     private ConcurrentDictionary<string, Register> registers;

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
     

}