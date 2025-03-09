using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace AppInventariCor.Services
{
    public interface INavigationService
    {
        Task NavigateToAsync(string route);
        Task NavigateToAsync(string route, Dictionary<string, object> parameters);
        Task GoBackAsync();
        Task NavigateAndClearBackStackAsync(string route);
    }

    public class NavigationService : INavigationService
    {
        protected readonly IServiceProvider ServiceProvider;

        public NavigationService(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public Task NavigateToAsync(string route)
        {
            return Shell.Current.GoToAsync(route);
        }

        public Task NavigateToAsync(string route, Dictionary<string, object> parameters)
        {
            return Shell.Current.GoToAsync(route, parameters);
        }

        public Task GoBackAsync()
        {
            return Shell.Current.GoToAsync("..");
        }

        public Task NavigateAndClearBackStackAsync(string route)
        {
            return Shell.Current.GoToAsync($"//{route}");
        }
    }
}