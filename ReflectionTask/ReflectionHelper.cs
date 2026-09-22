using System.Reflection;
using BankAccountSystem.Attributes;

namespace BankAccountSystem.Helpers;

public static class ReflectionHelper
{
    public static void InspectAndInvoke(object obj)
    {
        Type type = obj.GetType();

        Console.WriteLine();
        Console.WriteLine("===== PRIVATE FIELDS =====");

        FieldInfo[] fields = type.GetFields(
            BindingFlags.Instance |
            BindingFlags.NonPublic
        );

        foreach (FieldInfo field in fields)
        {
            object? value = field.GetValue(obj);

            Console.WriteLine(
                $"{field.Name} = {value}"
            );
        }

        Console.WriteLine();
        Console.WriteLine("===== AUDIT LOGGABLE METHODS =====");

        MethodInfo[] methods = type.GetMethods(
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic
        );

        foreach (MethodInfo method in methods)
        {
            bool hasAttribute =
                method.GetCustomAttribute<AuditLoggableAttribute>() != null;

            if (!hasAttribute)
                continue;

            Console.WriteLine($"Method: {method.Name}");

            if (method.GetParameters().Length == 0)
            {
                method.Invoke(obj, null);
            }
        }
    }
}