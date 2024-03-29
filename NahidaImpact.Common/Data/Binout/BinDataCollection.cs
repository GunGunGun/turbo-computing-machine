﻿using System.Collections.Immutable;
using System.Text.Json;
using NahidaImpact.Common.Data.Provider;
using Microsoft.Extensions.Logging;

namespace NahidaImpact.Common.Data.Binout;
public class BinDataCollection
{
    private readonly ImmutableDictionary<uint, AvatarConfig> _avatarConfigs;

    public BinDataCollection(IAssetProvider assetProvider, ILogger<BinDataCollection> logger, DataHelper dataHelper)
    {
        _avatarConfigs = LoadAvatarConfigs(assetProvider, dataHelper);

        logger.LogInformation("Loaded {count} avatar configs", _avatarConfigs.Count);
    }

    public AvatarConfig GetAvatarConfig(uint id)
    {
        return _avatarConfigs[id];
    }

    private static ImmutableDictionary<uint, AvatarConfig> LoadAvatarConfigs(IAssetProvider assetProvider, DataHelper dataHelper)
    {
        ImmutableDictionary<uint, AvatarConfig>.Builder builder = ImmutableDictionary.CreateBuilder<uint, AvatarConfig>();
        IEnumerable<string> avatarConfigFiles = assetProvider.EnumerateAvatarConfigFiles();

        foreach (string avatarConfigFile in avatarConfigFiles)
        {
            string avatarName = avatarConfigFile[(avatarConfigFile.LastIndexOf('_') + 1)..];
            avatarName = avatarName.Remove(avatarName.IndexOf('.'));

            if (dataHelper.TryResolveAvatarIdByName(avatarName, out uint id)) 
            {
                JsonDocument configJson = assetProvider.GetFileAsJsonDocument(avatarConfigFile);
                if (configJson.RootElement.ValueKind != JsonValueKind.Object)
                    throw new JsonException($"BinDataCollection::LoadAvatarConfigs - expected an object, got {configJson.RootElement.ValueKind}");

                AvatarConfig avatarConfig = configJson.RootElement.Deserialize<AvatarConfig>()!;
                builder.Add(id, avatarConfig);
            }
            else
            {
                throw new KeyNotFoundException($"BinDataCollection::LoadAvatarConfigs - failed to resolve avatar id for {avatarName}");
            }
        }

        return builder.ToImmutable();
    }
}
