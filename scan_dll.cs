using System;
using System.Reflection;
using System.Linq;

class Program {
    static void Main(string[] args) {
        try {
            var asm = Assembly.LoadFrom(args[0]);
            var types = asm.GetTypes().Where(t => t.Namespace == "GorillaLocomotion");
            foreach (var t in types) {
                Console.WriteLine($"{t.Name} (Public: {t.IsPublic})");
            }
        } catch (ReflectionTypeLoadException ex) {
            foreach (var t in ex.Types.Where(t => t != null && t.Namespace == "GorillaLocomotion")) {
                Console.WriteLine($"{t.Name} (Public: {t.IsPublic})");
            }
        } catch (Exception ex) {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
