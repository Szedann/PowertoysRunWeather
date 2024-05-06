// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ManagedCommon;
using Microsoft.PowerToys.Settings.UI.Library;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Wox.Infrastructure;
using Wox.Plugin;
using Wox.Plugin.Logger;
using Weather.NET;

namespace Community.PowerToys.Run.Plugin.Weather
{
    public class Main : IPlugin, IContextMenu, ISettingProvider, IReloadable, IDisposable, IDelayedExecutionPlugin
    {
        public static string PluginID => "69586FC9E5F5479A9F4B2E573860EF4B";

        public string Name => Properties.Resources.plugin_name;
        public string Description => Properties.Resources.plugin_description;
        private string? APIToken { get; set; }
        private string? DefaultCity { get; set; }

        private WeatherClient? _weatherClient;

        private PluginInitContext? _context;

        private string? _iconPath;

        private bool _disposed;



        // TODO: add additional options
        public IEnumerable<PluginAdditionalOption> AdditionalOptions => new List<PluginAdditionalOption>()
        {
            new()
            {
                Key = nameof(APIToken),
                DisplayLabel = "API Token",
                DisplayDescription = "API Token for WeatherCrossing",
                PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Textbox
            },
            new()
            {
                Key = nameof(DefaultCity),
                DisplayLabel = "Default City",
                PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Textbox
            },
        };

        // TODO: return context menus for each Result (optional, remove if not needed)
        public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
        {
            return new List<ContextMenuResult>(0);
        }

        // TODO: return query results
        public List<Result> Query(Query query)
        {
            ArgumentNullException.ThrowIfNull(query);

            var results = new List<Result>();

            if (string.IsNullOrEmpty(APIToken))
            {
                results.Add(new Result
                {
                    Title = "OpenWeatherMap - Missing API Token",
                    SubTitle = Description,
                    QueryTextDisplay = string.Empty,
                    IcoPath = _iconPath,
                    Action = _ => true
                });
                return results;
            }


            return results;
        }

        // TODO: return delayed query results (optional, remove if not needed)
        public List<Result> Query(Query query, bool delayedExecution)
        {
            ArgumentNullException.ThrowIfNull(query);



            var results = new List<Result>();

            if (_weatherClient == null) return results;

            if (string.IsNullOrEmpty(query.Search))
            {
                var searchResponse = _weatherClient.GetCurrentWeather(DefaultCity);
                results.Add(new Result
                {
                    Title = searchResponse.Main.Temperature + " in " + searchResponse.CityName,
                    SubTitle = searchResponse.Weather.First().Title,
                    IcoPath = searchResponse.Weather.First().IconUrl
                });
            }

            return results;
        }

        public void Init(PluginInitContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _context.API.ThemeChanged += OnThemeChanged;
            UpdateIconPath(_context.API.GetCurrentTheme());
        }

        public string GetTranslatedPluginTitle()
        {
            return Properties.Resources.plugin_name;
        }

        public string GetTranslatedPluginDescription()
        {
            return Properties.Resources.plugin_description;
        }

        private void OnThemeChanged(Theme oldtheme, Theme newTheme)
        {
            UpdateIconPath(newTheme);
        }

        private void UpdateIconPath(Theme theme)
        {
            if (theme == Theme.Light || theme == Theme.HighContrastWhite)
            {
                _iconPath = "Images/Community.PowerToys.Run.Plugin.Weather.light.png";
            }
            else
            {
                _iconPath = "Images/Community.PowerToys.Run.Plugin.Weather.dark.png";
            }
        }

        public Control CreateSettingPanel()
        {
            throw new NotImplementedException();
        }

        public void UpdateSettings(PowerLauncherPluginSettings settings)
        {
            Log.Info("UpdateSettings", GetType());
            APIToken = settings?.AdditionalOptions?.FirstOrDefault(x => x.Key == nameof(APIToken))?.TextValue;
            DefaultCity = settings?.AdditionalOptions?.FirstOrDefault(x => x.Key == nameof(DefaultCity))?.TextValue;
        }

        public void ReloadData()
        {
            if (_context is null)
            {
                return;
            }

            UpdateIconPath(_context.API.GetCurrentTheme());
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                if (_context != null && _context.API != null)
                {
                    _context.API.ThemeChanged -= OnThemeChanged;
                }

                _disposed = true;
            }
        }
    }
}
