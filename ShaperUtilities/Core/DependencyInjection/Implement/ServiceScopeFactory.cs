﻿using System;
using ShaperUtilities.Core.DependencyInjection.Service;

namespace ShaperUtilities.Core.DependencyInjection.Implement;

/// <summary>
/// Factory for creating new service scopes.
/// </summary>
public class ServiceScopeFactory(ServiceProvider rootServiceProvider) : IServiceScopeFactory
{
    private readonly ServiceProvider _rootServiceProvider =
        rootServiceProvider ?? throw new ArgumentNullException(nameof(rootServiceProvider));

    public IServiceScope CreateScope()
        => new ServiceScope(_rootServiceProvider.CreateScopedServiceProvider());
}