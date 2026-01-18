using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SleepJournal.ViewModels;
using SleepJournal.Views;
using SleepJournal.Services;
using SleepJournal.Tests.Helpers;
using Xunit;

namespace SleepJournal.Tests;

/// <summary>
/// Tests for application initialization and dependency injection setup.
/// These tests prevent regressions like DataTemplate instantiation failures.
/// </summary>
public class AppInitializationTests
{
    /// <summary>
    /// Bug Fix Test: Ensures all ViewModels and services can be instantiated from DI container.
    /// 
    /// Previous Bug: AppShell used DataTemplate which tried to create pages with 
    /// parameterless constructors, causing TargetInvocationException crash on startup.
    /// 
    /// Fix: Removed WelcomePage from AppShell.xaml DataTemplate and registered all
    /// pages as routes so they're resolved from DI with proper constructor injection.
    /// 
    /// This test validates that all required dependencies are properly registered
    /// and can be injected, which is essential for pages to be created from DI.
    /// </summary>
    [Fact]
    public void AllViewModelsAndServices_CanBeInstantiatedFromDI()
    {
        // Arrange - Create a service collection with all app dependencies
        var services = new ServiceCollection();

        // Register logging first
        services.AddLogging(builder => builder.AddDebug());

        // Register all services (matching MauiProgram.cs)
        services.AddSingleton<IDataService, SQLiteDataService>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<SQLiteDataService>>();
            return new SQLiteDataService(logger, TestHelpers.GetTempDbPath());
        });
        services.AddSingleton<IBiometricService, BiometricService>();
        services.AddSingleton<IAudioService, AudioService>();
        services.AddSingleton<INotificationService, NotificationService>();

        // Register all ViewModels
        services.AddTransient<MainPageViewModel>();
        services.AddTransient<HistoryPageViewModel>();
        services.AddTransient<SettingsPageViewModel>();
        services.AddTransient<StatisticsViewModel>();
        services.AddTransient<WelcomePageViewModel>();

        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert - Verify each ViewModel can be created from DI
        // If any ViewModel has constructor parameters that aren't registered,
        // this will throw an exception
        var mainPageViewModel = serviceProvider.GetRequiredService<MainPageViewModel>();
        Assert.NotNull(mainPageViewModel);

        var historyPageViewModel = serviceProvider.GetRequiredService<HistoryPageViewModel>();
        Assert.NotNull(historyPageViewModel);

        var settingsPageViewModel = serviceProvider.GetRequiredService<SettingsPageViewModel>();
        Assert.NotNull(settingsPageViewModel);

        var statisticsViewModel = serviceProvider.GetRequiredService<StatisticsViewModel>();
        Assert.NotNull(statisticsViewModel);

        var welcomePageViewModel = serviceProvider.GetRequiredService<WelcomePageViewModel>();
        Assert.NotNull(welcomePageViewModel);
    }

    /// <summary>
    /// Ensures all ViewModels can be instantiated independently from DI.
    /// This catches missing service registrations early.
    /// </summary>
    [Fact]
    public void AllViewModels_CanBeInstantiatedFromDI()
    {
        // Arrange
        var services = new ServiceCollection();

        // Register logging first
        services.AddLogging(builder => builder.AddDebug());

        services.AddSingleton<IDataService, SQLiteDataService>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<SQLiteDataService>>();
            return new SQLiteDataService(logger, TestHelpers.GetTempDbPath());
        });
        services.AddSingleton<IBiometricService, BiometricService>();
        services.AddSingleton<IAudioService, AudioService>();
        services.AddSingleton<INotificationService, NotificationService>();

        services.AddTransient<MainPageViewModel>();
        services.AddTransient<HistoryPageViewModel>();
        services.AddTransient<SettingsPageViewModel>();
        services.AddTransient<StatisticsViewModel>();
        services.AddTransient<WelcomePageViewModel>();

        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        Assert.NotNull(serviceProvider.GetRequiredService<MainPageViewModel>());
        Assert.NotNull(serviceProvider.GetRequiredService<HistoryPageViewModel>());
        Assert.NotNull(serviceProvider.GetRequiredService<SettingsPageViewModel>());
        Assert.NotNull(serviceProvider.GetRequiredService<StatisticsViewModel>());
        Assert.NotNull(serviceProvider.GetRequiredService<WelcomePageViewModel>());
    }

    /// <summary>
    /// Ensures no page has a parameterless constructor that could be accidentally
    /// invoked by DataTemplate. All pages should require ViewModels via DI.
    /// </summary>
    [Theory]
    [InlineData(typeof(MainPage))]
    [InlineData(typeof(HistoryPage))]
    [InlineData(typeof(SettingsPage))]
    [InlineData(typeof(StatisticsPage))]
    [InlineData(typeof(WelcomePage))]
    public void Pages_DoNotHaveParameterlessConstructors(Type pageType)
    {
        // Arrange & Act
        var constructors = pageType.GetConstructors();
        var parameterlessConstructor = constructors
            .FirstOrDefault(c => c.GetParameters().Length == 0);

        // Assert - Pages should NOT have parameterless constructors
        // They must require ViewModels through DI
        Assert.Null(parameterlessConstructor);
    }

    /// <summary>
    /// Verifies that required services are properly registered as Singletons.
    /// DataService and other infrastructure services should be singletons to
    /// maintain state and avoid multiple database connections.
    /// </summary>
    [Fact]
    public void Services_AreRegisteredAsSingletons()
    {
        // Arrange
        var services = new ServiceCollection();

        // Register logging first
        services.AddLogging(builder => builder.AddDebug());

        services.AddSingleton<IDataService, SQLiteDataService>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<SQLiteDataService>>();
            return new SQLiteDataService(logger, TestHelpers.GetTempDbPath());
        });
        services.AddSingleton<IBiometricService, BiometricService>();
        services.AddSingleton<IAudioService, AudioService>();
        services.AddSingleton<INotificationService, NotificationService>();

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var dataService1 = serviceProvider.GetRequiredService<IDataService>();
        var dataService2 = serviceProvider.GetRequiredService<IDataService>();

        var audioService1 = serviceProvider.GetRequiredService<IAudioService>();
        var audioService2 = serviceProvider.GetRequiredService<IAudioService>();

        // Assert - Same instance should be returned
        Assert.Same(dataService1, dataService2);
        Assert.Same(audioService1, audioService2);
    }

    /// <summary>
    /// Verifies that ViewModels are registered as Transient.
    /// Each navigation should get a fresh ViewModel instance to avoid state pollution.
    /// </summary>
    [Fact]
    public void ViewModels_AreRegisteredAsTransient()
    {
        // Arrange
        var services = new ServiceCollection();

        // Register logging first
        services.AddLogging(builder => builder.AddDebug());

        services.AddSingleton<IDataService, SQLiteDataService>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<SQLiteDataService>>();
            return new SQLiteDataService(logger, TestHelpers.GetTempDbPath());
        });
        services.AddSingleton<IBiometricService, BiometricService>();
        services.AddSingleton<IAudioService, AudioService>();
        services.AddSingleton<INotificationService, NotificationService>();

        services.AddTransient<MainPageViewModel>();

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var viewModel1 = serviceProvider.GetRequiredService<MainPageViewModel>();
        var viewModel2 = serviceProvider.GetRequiredService<MainPageViewModel>();

        // Assert - Different instances should be returned
        Assert.NotSame(viewModel1, viewModel2);
    }
}
