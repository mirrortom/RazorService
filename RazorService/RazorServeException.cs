using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorService;

public class RazorServeException : Exception
{
    public RazorServeException() : base() { }
    public RazorServeException(string message) : base(message) { }
    public RazorServeException(string message, Exception inner) : base(message, inner) { }
}
