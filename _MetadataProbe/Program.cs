using System.Reflection;

Assembly asm = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
Type t = asm.GetType("MegaCrit.Sts2.Core.Models.Monsters.TheInsatiable")!;
object instance = Activator.CreateInstance(t)!;

Console.WriteLine($"TYPE {t.FullName}");
Console.WriteLine("STRING FIELDS");
foreach (FieldInfo field in t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
{
    if (field.FieldType == typeof(string))
    {
        Console.WriteLine($"{field.Name} = {field.GetValue(instance)}");
    }
}

Console.WriteLine("PROPERTIES");
foreach (PropertyInfo property in t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
{
    if (property.PropertyType == typeof(string) || property.PropertyType == typeof(bool) || property.PropertyType == typeof(int) || property.PropertyType == typeof(float))
    {
        try
        {
            Console.WriteLine($"{property.Name} ({property.PropertyType.Name}) = {property.GetValue(instance)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{property.Name} ({property.PropertyType.Name}) threw {ex.GetType().Name}");
        }
    }
}

Console.WriteLine("METHODS");
foreach (MethodInfo method in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
{
    Console.WriteLine($"{method.ReturnType.Name} {method.Name}({string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name))})");
}
