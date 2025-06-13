using System;
using System.Reflection;
using ShaperUtilities.Core.Attribute;
using IServiceProvider = ShaperUtilities.Core.DependencyInjection.Service.IServiceProvider;
using Service_IServiceProvider = ShaperUtilities.Core.DependencyInjection.Service.IServiceProvider;

namespace ShaperUtilities.Utility
{
    public static class PropertyInjector
    {
        /// <summary>
        /// Attempts to inject services into properties/fields marked with [TryInject] on the given instance.
        /// </summary>
        /// <param name="instance">The object instance on which to perform injection.</param>
        /// <param name="serviceProvider">The IServiceProvider to resolve dependencies from.</param>
        public static void PerformInjection(object instance, IServiceProvider serviceProvider)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            
            if (serviceProvider == null)
            {
                // This means the service provider itself wasn't available, so we can't inject.
                // In a "TryInject" scenario, we might just log this and return.
                Console.WriteLine("PropertyInjector: IServiceProvider is null. Cannot perform injection.");
                return;
            }

            var type = instance.GetType();

            // 1. Inject into Public Properties with a public or private setter
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (property.GetCustomAttribute<TryInjectAttribute>() == null) continue;
                
                // Check if the property has a setter and if it's accessible
                var setter = property.GetSetMethod(true); // true to get private setter
               
                // Check if setter is accessible
                if (setter != null && (setter.IsPublic || setter.IsFamily || setter.IsAssembly || setter.IsPrivate))
                {
                    var service = serviceProvider.GetService(property.PropertyType);
                    
                    if (service != null)
                    {
                        try
                        {
                            property.SetValue(instance, service);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"PropertyInjector ERROR: Failed to set property '{property.Name}' on '{type.Name}': {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"PropertyInjector: Service for property '{property.Name}' of type '{property.PropertyType.Name}' not found or could not be resolved (TryInject).");
                    }
                }
                else
                {
                    Console.WriteLine($"PropertyInjector WARNING: Property '{property.Name}' on '{type.Name}' has [TryInject] but no accessible setter.");
                }
            }

            // Inject into Public or Private Fields (if you also want to support field injection)
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (field.GetCustomAttribute<TryInjectAttribute>() == null) continue;
                
                var service = serviceProvider.GetService(field.FieldType);
                
                if (service != null)
                {
                    try
                    {
                        field.SetValue(instance, service);
                        Console.WriteLine($"PropertyInjector: Injected service into field '{field.Name}' of type '{type.Name}'.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"PropertyInjector ERROR: Failed to set field '{field.Name}' on '{type.Name}': {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"PropertyInjector: Service for field '{field.Name}' of type '{field.FieldType.Name}' not found or could not be resolved (TryInject).");
                }
            }
        }
    }
}