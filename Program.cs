using System.Linq;
using TTRL;
namespace TTRL
{
    static class App
    {
        public static void Main(string[] args)
        {
            if (Interpreter.devmode)
            {
                Interpreter.Start("../../../main.ttrl");
            }
            else
            {
                Interpreter.Start(args[0]);
            }
            Console.ReadLine();
        }
    }
}
