using NDisconf.Client;
using System;

namespace NDisconf.TestConsole
{
    class Program
    {
        public static string DemoItem { get; set; } = "I'm DemoItem.";
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to NDisconf Demo.");
            NDisconfManager.Register(act: manager =>
            {
                manager.FileRules.For("configs/appSetting.json").OnChanged(c =>
                {
                    Console.WriteLine("appSetting委托调用1");
                }).OnChanged(c =>
                {
                    Console.WriteLine("appSetting委托调用2");
                });
                manager.ItemRules.For("DemoItem").SetStaticProperty<Program>().OnChanged((c, v) =>
                {
                    Console.WriteLine("键：{1}  值：{1}", c, v);
                });
            });
            Console.ReadLine();
        }
    }
}
