using RazorService;
using RazorServiceTest;
using System.Drawing;
using System.Dynamic;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

RazorCfg.AddSearchDirs("razorFiles");
RazorCfg.StartTimer();
string main = "main";
PersonEntity model = new()
{
    Id = 1,
    Name = "mirror",
    Description = "software worker",
    Money = 10
};

string html = RazorServe.Run(main, model);
File.WriteAllText("index.html", html, Encoding.UTF8);
Console.WriteLine("index.html created.");
Console.WriteLine();

// dynamic model
dynamic model2=new ExpandoObject();
model2.Id = 1001;
model2.Name = "anne";
model2.Description = "data engineer";
model2.Money = 50;
html = RazorServe.Run(main, model2);
File.WriteAllText("index2.html", html, Encoding.UTF8);
Console.WriteLine("index2.html created.");


Console.ReadLine();
RazorCfg.StopTimer();