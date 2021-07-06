﻿using Liquid.Core.Implementations;
using Liquid.Core.Interfaces;
using Liquid.WebApi.Http.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Liquid.WebApi.Http.Middlewares
{
    /// <summary>
    /// Inserts configured context keys in LiquidContext service.
    /// Includes its behavior in netcore pipelines before request execution.
    /// </summary>
    public class LiquidContextMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILiquidConfiguration<ScopedContextSettings> _options;

        /// <summary>
        /// Initialize a instance of <see cref="LiquidContextMiddleware"/>
        /// </summary>
        /// <param name="next">Invoked request.</param>
        /// <param name="options">Context keys configuration.</param>
        public LiquidContextMiddleware(RequestDelegate next, ILiquidConfiguration<ScopedContextSettings> options)
        {
            _next = next;
            _options = options;
        }

        /// <summary>
        /// Obtains the value of configured keys from HttpContext and inserts them in LiquidContext service.
        /// Includes its behavior in netcore pipelines before request execution.
        /// </summary>
        /// <param name="context">HTTP-specific information about an individual HTTP request.</param>
        public async Task InvokeAsync(HttpContext context)
        {

            var liquidContext = context.RequestServices.GetRequiredService<LiquidContext>();

            var value = string.Empty;

            foreach (var key in _options.Settings.Keys)
            {
                value = context.GetValueFromHeader(key.KeyName);

                if (string.IsNullOrEmpty(value))
                    value = context.GetValueFromQuery(key.KeyName);

                if (string.IsNullOrEmpty(value) && key.Required)
                    //TODO: implementar exceção para as chaves do contexto.
                    throw new Exception();

                liquidContext.Upsert(key.KeyName, value);
            }

            if (_options.Settings.Culture)
            {
                liquidContext.Upsert("culture", CultureInfo.CurrentCulture.Name);
            }

            await _next(context);
        }
    }
}