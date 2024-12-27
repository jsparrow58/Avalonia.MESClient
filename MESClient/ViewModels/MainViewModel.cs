using System;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MESClient.Helpers;

namespace MESClient.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        private const string buttonActiveClass = "active";
        
        [ObservableProperty]
        private string _greeting = "Welcome to Avalonia!";
        
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SideMenuImage))]
        private bool _sideMenuExpanded = true;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HomeButtonIsActive))]
        [NotifyPropertyChangedFor(nameof(ProcessButtonIsActive))]
        private ViewModelBase _currentPage;

        private readonly HomePageViewModel _homePage = new();
        private readonly ProcessPageViewModel _processPage = new();

        public MainViewModel()
        {
            CurrentPage = _homePage;
        }
        
        public IImage SideMenuImage => ImageHelper.LoadFromAssets(SideMenuExpanded ? "logo" : "icon");
        
        public bool HomeButtonIsActive => CurrentPage == _homePage;
        public bool ProcessButtonIsActive => CurrentPage == _processPage;
        
        [RelayCommand]
        private void SideMenuResize()
        {
            SideMenuExpanded = !SideMenuExpanded;
        }
        
        [RelayCommand]
        private void GoToHomePage()
        {
            CurrentPage = _homePage;
        }

        [RelayCommand]
        private void GoToProcessPage()
        {
            CurrentPage = _processPage;
        }
    }
}
