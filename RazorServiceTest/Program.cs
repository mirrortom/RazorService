using RazorService;
using RazorServiceTest;
using System.Drawing;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Text;

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