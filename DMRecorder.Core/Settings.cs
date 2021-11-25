﻿namespace DMRecorder.Core;

using System.Collections.Concurrent;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public abstract class Settings<T>
    where T : Settings<T>, new()
{
    private static readonly IDeserializer deserializer = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .IgnoreFields()
        .Build();
    private static readonly ISerializer serializer = new SerializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .IgnoreFields()
        .Build();
    private static readonly ConcurrentDictionary<string, T> cache = new();

    [YamlIgnore]
    public string Filepath { get; private set; }


    protected Settings()
    {
        Filepath = string.Empty;
    }

    protected abstract void SetDefault();

    public static T Load(string filepath)
    {
        T result;

        if (cache.ContainsKey(filepath) == true)
            return cache[filepath];

        if (File.Exists(filepath) == false)
        {
            result = new T { Filepath = filepath };
            result.SetDefault();
        }
        else
        {
            var yamlText = File.ReadAllText(filepath);
            result = deserializer.Deserialize<T>(yamlText);
            result.Filepath = filepath;
        }

        cache[filepath] = result;

        return result;
    }

    public void Save()
    {
        var directory = Path.GetDirectoryName(Filepath);
        if (directory is not null)
        {
            if (Directory.Exists(directory) == false)
                Directory.CreateDirectory(directory);
        }

        var yamlText = serializer.Serialize(this);
        File.WriteAllText(Filepath, yamlText);
    }
}
