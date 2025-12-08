using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartCarSharing.Core.Services;
using System;

namespace SmartCarSharingApp.UI.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IAuthenticationService _authService;
        private readonly LoginViewModel _loginViewModel;
        private readonly RegisterViewModel _registerViewModel;

        [ObservableProperty]
        private object _currentViewModel;

        public MainViewModel(IAuthenticationService authService)
        {
            _authService = authService;

            // Initialize child ViewModels
            _loginViewModel = new LoginViewModel(_authService);
            _registerViewModel = new RegisterViewModel(_authService);

            // Setup Navigation Logic
            _loginViewModel.RequestNavigateToRegister += () => CurrentViewModel = _registerViewModel;
            _registerViewModel.RequestNavigateToLogin += () => CurrentViewModel = _loginViewModel;

            // Start with Login
            CurrentViewModel = _loginViewModel;
        }
    }
}