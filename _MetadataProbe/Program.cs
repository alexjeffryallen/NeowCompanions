using System.Reflection;

Assembly asm = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
if (args.FirstOrDefault() == "--members")
{
    foreach (string fullName in args.Skip(1))
    {
        Type? type = asm.GetType(fullName);
        Console.WriteLine($"TYPE {type?.FullName ?? "MISSING"}");
        if (type == null) continue;
        foreach (ConstructorInfo ctor in type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            Console.WriteLine($"  CTOR({string.Join(", ", ctor.GetParameters().Select(p => p.ParameterType.FullName + " " + p.Name))})");
        foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            Console.WriteLine($"  METHOD {method.ReturnType.FullName} {method.Name}({string.Join(", ", method.GetParameters().Select(p => p.ParameterType.FullName + " " + p.Name))})");
        foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            Console.WriteLine($"  PROP {property.PropertyType.FullName} {property.Name}");
    }
    return;
}
if (args.FirstOrDefault() == "--strings")
{
    foreach (string typeName in args.Skip(1))
    {
        Type? type = asm.GetType($"MegaCrit.Sts2.Core.Models.Monsters.{typeName}", false, true);
        Console.WriteLine($"TYPE {type?.FullName ?? $"MISSING {typeName}"}");
        if (type == null)
            continue;

        HashSet<string> strings = [];
        List<Type> relatedTypes = [type, .. type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)];
        for (Type? baseType = type.BaseType;
             baseType != null && baseType.Namespace == "MegaCrit.Sts2.Core.Models.Monsters";
             baseType = baseType.BaseType)
        {
            relatedTypes.Add(baseType);
            relatedTypes.AddRange(baseType.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic));
        }
        IEnumerable<MethodBase> methods = relatedTypes.SelectMany(relatedType => relatedType
            .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Cast<MethodBase>()
            .Concat(relatedType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)));
        foreach (MethodBase method in methods)
        {
            byte[]? il = method.GetMethodBody()?.GetILAsByteArray();
            if (il == null)
                continue;
            for (int i = 0; i + 4 < il.Length; i++)
            {
                if (il[i] != 0x72)
                    continue;
                try
                {
                    string value = method.Module.ResolveString(BitConverter.ToInt32(il, i + 1));
                    if (!string.IsNullOrWhiteSpace(value))
                        strings.Add(value);
                }
                catch { }
            }
        }
        foreach (string value in strings.Order())
            Console.WriteLine($"  {value}");
    }
    return;
}
if (args.FirstOrDefault() == "--ancients")
{
    Type ancientBase = asm.GetTypes().Single(type => type.Name == "AncientEventModel");
    foreach (Type type in asm.GetTypes().Where(type => !type.IsAbstract && ancientBase.IsAssignableFrom(type)).OrderBy(type => type.FullName))
        Console.WriteLine(type.FullName);
    return;
}
if (args.FirstOrDefault() == "--monster-types")
{
    Type monsterBase = asm.GetTypes().Single(type => type.Name == "MonsterModel");
    foreach (Type type in asm.GetTypes().Where(type => !type.IsAbstract && monsterBase.IsAssignableFrom(type)).OrderBy(type => type.Name))
        Console.WriteLine(type.FullName);
    return;
}
if (args.FirstOrDefault() == "--monster-roster")
{
    Type monsterBase = asm.GetTypes().Single(type => type.Name == "MonsterModel");
    foreach (Type type in asm.GetTypes().Where(type => !type.IsAbstract && monsterBase.IsAssignableFrom(type)).OrderBy(type => type.Name))
    {
        try
        {
            object? instance = Activator.CreateInstance(type);
            bool shown = (bool)(type.GetProperty("ShouldShowInCompendium")?.GetValue(instance) ?? false);
            string visuals = (string?)(type.GetProperty("VisualsPath")?.GetValue(instance)) ?? "";
            Console.WriteLine($"{type.Name}\t{shown}\t{visuals}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{type.Name}\tERROR\t{ex.GetType().Name}");
        }
    }
    return;
}
if (args.FirstOrDefault() == "--ui")
{
    Type type = asm.GetType("MegaCrit.Sts2.Core.Nodes.Events.NAncientEventLayout")!;
    Console.WriteLine($"TYPE {type.FullName} BASE {type.BaseType?.FullName}");
    foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
        Console.WriteLine($"  FIELD {field.FieldType.FullName} {field.Name}");
    foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
        Console.WriteLine($"  PROP {property.PropertyType.FullName} {property.Name}");
    foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
        Console.WriteLine($"  METHOD {method.ReturnType.FullName} {method.Name}({string.Join(", ", method.GetParameters().Select(p => p.ParameterType.FullName + " " + p.Name))})");
    return;
}
if (args.FirstOrDefault() == "--type" && args.Length > 1)
{
    Type type = asm.GetType(args[1])!;
    Console.WriteLine($"TYPE {type.FullName} BASE {type.BaseType?.FullName}");
    foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        Console.WriteLine($"  FIELD {field.FieldType.FullName} {field.Name}");
    foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        Console.WriteLine($"  PROP {property.PropertyType.FullName} {property.Name}");
    return;
}
if (args.FirstOrDefault() == "--commands")
{
    foreach (string fullName in new[]
    {
        "MegaCrit.Sts2.Core.Commands.CreatureCmd",
        "MegaCrit.Sts2.Core.Commands.PowerCmd",
        "MegaCrit.Sts2.Core.Commands.AttackCommand"
    })
    {
        Type type = asm.GetType(fullName)!;
        Console.WriteLine($"TYPE {type.FullName}");
        foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
        {
            if (method.Name is "Damage" or "Apply" or "FromCard")
            {
                Console.WriteLine($"  {method.ReturnType.Name} {method.Name}({string.Join(", ", method.GetParameters().Select(p => p.ParameterType.FullName + " " + p.Name + (p.HasDefaultValue ? $" = {p.DefaultValue ?? "null"}" : "")))})");
            }
        }
    }
    return;
}

string[] typeNames = args.Length == 0
    ? ["CorpseSlug", "PhrogParasite", "SkulkingColony", "SlitheringStrangler", "Parafright", "TerrorEel"]
    : args;

foreach (string typeName in typeNames)
{
    Type? t = asm.GetType($"MegaCrit.Sts2.Core.Models.Monsters.{typeName}", false, true);
    if (t == null)
    {
        Console.WriteLine($"MISSING {typeName}");
        continue;
    }

    object? instance = Activator.CreateInstance(t);
    Console.WriteLine($"TYPE {t.FullName}");
    foreach (FieldInfo field in t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
    {
        if (field.FieldType == typeof(string))
        {
            try { Console.WriteLine($"  FIELD {field.Name} = {field.GetValue(instance)}"); }
            catch { }
        }
    }

    foreach (PropertyInfo property in t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
    {
        if (property.PropertyType == typeof(string))
        {
            try { Console.WriteLine($"  PROP {property.Name} = {property.GetValue(instance)}"); }
            catch { }
        }
    }

    foreach (MethodInfo method in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
    {
        Console.WriteLine($"  METHOD {method.ReturnType.Name} {method.Name}({string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name))})");
    }
}
