using System;
using System.Linq;
using System.Reflection;
Assembly asm = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll");
var t = asm.GetType("MegaCrit.Sts2.Core.Localization.DynamicVars.IfUpgradedVar")!;
var fld = t.GetField("upgradeDisplay", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance)!;
foreach(var c in t.GetConstructors(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance))
{
 object?[] ctorArgs = c.GetParameters().Select(p => p.ParameterType.IsEnum ? Enum.GetValues(p.ParameterType).GetValue(0) : p.ParameterType==typeof(string)?"Upgrade": p.ParameterType==typeof(decimal)?(object)2m:null).ToArray();
 var obj = c.Invoke(ctorArgs);
 Console.WriteLine(string.Join(",", c.GetParameters().Select(p=>p.ParameterType.Name)) + " -> name=" + t.BaseType!.GetProperty("Name")!.GetValue(obj) + " display=" + fld.GetValue(obj) + " base=" + t.BaseType!.GetProperty("BaseValue")!.GetValue(obj));
}
