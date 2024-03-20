global using static ConsoleApp1.Constants;
using ConsoleApp1;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ServForOracle.NetCore;
using ServForOracle.NetCore.Parameters;
using System.Data;

Console.WriteLine("Start!");

var factory = new LoggerFactory();

var list = new List<PackageCustumType>();
for (int i = 0; i < 10; i++)
{
    list.Add(new PackageCustumType());
}

var array = list.ToArray();
var service = new ServiceForOracle(factory.CreateLogger<ServiceForOracle>(), new MemoryCache(new MemoryCacheOptions()), conString);

//var parameter = Param.Input(list.ToArray());
var parameter = new ParamObject<PackageCustumType[]>(array, ParameterDirection.Input);

service.ExecuteProcedure(producerName, parameter);

Console.WriteLine("Ended!");
Console.ReadLine();
