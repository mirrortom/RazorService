using RazorService;
using RazorServiceTest;
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

Console.WriteLine("enter continue...");
Console.ReadLine();

PersonEntity model2 = new()
{
    Id = 2,
    Name = "tom",
    Description = "rich guy",
    Money = 10_000
};
string razor = $"<div>{nameof(model2.Id)}: @Model.Id</div><div>{nameof(model2.Name)}: @Model.Name</div><div>{nameof(model2.Description)}: @Model.Description</div><div>{nameof(model2.Money)}: @Model.Money</div>";
html = RazorServe.RunTxt(razor, model2);
File.WriteAllText("index1.html", html, Encoding.UTF8);
