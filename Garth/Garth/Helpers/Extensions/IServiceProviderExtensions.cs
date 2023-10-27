﻿using System.Collections;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Garth.Helpers.Extensions;

public static class IServiceProviderExtensions
{
    /// <summary>
    /// Get all registered <see cref="ServiceDescriptor"/>
    /// </summary>
    /// <param name="provider"></param>
    /// <returns></returns>
    public static Dictionary<Type, ServiceDescriptor> GetAllServiceDescriptors(this ServiceProvider provider)
    {
        if (provider is ServiceProvider serviceProvider)
        {
            var result = new Dictionary<Type, ServiceDescriptor>();

            var engine = serviceProvider.GetFieldValue("_engine");
            var callSiteFactory = engine.GetFieldValue("CallSiteFactory");
            var descriptorLookup = callSiteFactory.GetFieldValue("_descriptorLookup");
            if (descriptorLookup is IDictionary dictionary)
            {
                foreach (DictionaryEntry entry in dictionary)
                {
                    result.Add((Type)entry.Key, (ServiceDescriptor)entry.Value.GetPropertyValue("Last"));
                }
            }

            return result;
        }

        throw new NotSupportedException($"Type '{provider.GetType()}' is not supported!");
    }
}
