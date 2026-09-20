using SpecMind.Modules.AI.Configuration;
using SpecMind.Modules.AI.Models;
using SpecMind.Modules.AI.Services;
using Xunit;

namespace SpecMind.Tests;

public class ConfigurationTests
{
    [Fact]
    public void EnvironmentInitializesAllDirectoriesIdempotently()
    {
        using var temp = new TemporaryDirectory();
        var env = new AIEnvironment(temp.Path);
        env.Initialize();
        env.Initialize();
        foreach (var path in new[] { env.ModelsDirectory, env.ChatsDirectory, env.CacheDirectory, env.DownloadsDirectory, env.LogsDirectory })
            Assert.True(Directory.Exists(path));
    }

    [Fact]
    public void ConfigurationPersistsAndResets()
    {
        using var temp = new TemporaryDirectory();
        var env = new AIEnvironment(temp.Path);
        var service = new AIConfigurationService(env);
        Assert.True(File.Exists(env.ConfigurationFile));
        service.Save(new AIConfiguration { Threads = 3, MaxTokens = 128, ModelsDirectory = "CustomModels" });
        var loaded = new AIConfigurationService(env);
        Assert.Equal(3, loaded.Configuration.Threads);
        Assert.Equal(128, loaded.Configuration.MaxTokens);
        Assert.Equal("CustomModels", loaded.GetConfiguration().ModelsDirectory);
        loaded.Reset();
        Assert.Equal(new AIConfiguration().MaxTokens, new AIConfigurationService(env).Configuration.MaxTokens);
    }

    [Theory]
    [InlineData("{broken")]
    [InlineData("null")]
    [InlineData("[]")]
    public void InvalidConfigurationFallsBackWithoutDestroyingFile(string json)
    {
        using var temp = new TemporaryDirectory();
        var env = new AIEnvironment(temp.Path);
        File.WriteAllText(env.ConfigurationFile, json);
        Assert.Equal(new AIConfiguration().ContextSize, new AIConfigurationService(env).Configuration.ContextSize);
        Assert.Equal(json, File.ReadAllText(env.ConfigurationFile));
    }

    [Fact]
    public void UnwritableConfigurationDoesNotBlockStartup()
    {
        using var temp = new TemporaryDirectory();
        var env = new AIEnvironment(temp.Path);
        Directory.CreateDirectory(env.ConfigurationFile);
        Assert.NotNull(new AIConfigurationService(env).Configuration);
    }

    [Fact]
    public void ModelRegistrySwitchesActiveModelAndDeduplicates()
    {
        var manager = new ModelManager();
        Assert.False(manager.HasModels());
        Assert.Null(manager.GetActiveModel());
        var first = new AIModel { Path = "one.gguf" };
        var second = new AIModel { Path = "two.gguf" };
        manager.Register(first);
        manager.Register(new AIModel { Path = first.Path });
        manager.Register(second);
        manager.SetActive(first);
        manager.SetActive(second);
        Assert.Equal(2, manager.Models.Count);
        Assert.True(manager.HasModels());
        Assert.Same(second, manager.GetActiveModel());
        Assert.False(first.IsActive);
    }

    [Fact]
    public void RegisterLocalModelReadsFileMetadata()
    {
        using var temp = new TemporaryDirectory();
        var file = temp.File("sample.gguf");
        File.WriteAllBytes(file, [1, 2, 3]);
        var model = new ModelDownloader(new AIEnvironment(temp.Path)).RegisterModel(file);
        Assert.Equal(3, model.Size);
        Assert.Equal("sample", model.Name);
        Assert.True(model.IsInstalled);
        Assert.False(model.IsLoaded);
        Assert.Equal(file, model.Path);
    }
}
