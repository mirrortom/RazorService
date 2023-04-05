using RazorService;
using RazorServiceTest;
using System.Drawing;
using System.Dynamic;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;

RazorCfg.SetSearchDirs("razorFiles");
RazorCfg.StartTimer();

// razor
string main = "main";
// models
PersonEntity model = new()
{
    Id = 1,
    Name = "mirror",
    Description = "software worker",
    Money = 10
};
dynamic model2 = new ExpandoObject();
model2.Id = 1001;
model2.Name = "anne";
model2.Description = "data engineer";
model2.Money = 50;

dynamic[] models = { model, model2 };

// run
try
{
    foreach (var item in models)
    {
        string html = RazorServe.Run(main, item);
        File.WriteAllText($"index_{item.Name}.html", html, Encoding.UTF8);
        Console.WriteLine($"index_{item.Name}.html created.");
        Console.WriteLine();
    }
}
catch (RazorServeException e)
{
    Console.WriteLine($"RazorServeException: {Environment.NewLine}{e.Message}");
}
catch (Exception e)
{
    Console.WriteLine("RazorService异常: " + e.Message);
}


// exit
Console.ReadLine();
RazorCfg.StopTimer();