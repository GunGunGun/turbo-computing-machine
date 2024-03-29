﻿using System.Diagnostics.CodeAnalysis;
using NahidaImpact.Common.Data.Excel;
using NahidaImpact.Gameserver.Game.Avatar;

namespace NahidaImpact.Gameserver.Game;
internal class Player(ExcelTableCollection excelTables)
{
    private static readonly uint[] AvatarBlackList = [10000001, 10000007]; // kate and traveler

    public uint Uid { get; set; }
    public uint GuidSeed { get; set; }
    public string Name { get; set; } = "Traveler";

    public List<GameAvatar> Avatars { get; set; } = [];
    public List<GameAvatarTeam> AvatarTeams { get; set; } = [];
    public uint CurTeamIndex { get; set; }

    public uint CurAvatarEntityId { get; set; }
    public Vector LastMainWorldPos { get; set; } = new();

    private readonly ExcelTableCollection _excelTables = excelTables;

    public void InitDefaultPlayer()
    {
        // We don't have database atm, so let's init default player state for every session.

        Uid = 1337;
        Name = "ReversedRooms";

        UnlockAllAvatars();

        _ = TryGetAvatar(10000073, out GameAvatar? avatar);
        AvatarTeams.Add(new()
        {
            AvatarGuidList = [avatar!.Guid],
            Index = 1
        });
        CurTeamIndex = 1;

        LastMainWorldPos = GetDefaultMainWorldPos();
    }

    private Vector GetDefaultMainWorldPos() => new()
    {
        X = 2336.789f,
        Y = 249.98896f,
        Z = -751.3081f
    };


    public GameAvatarTeam GetCurrentTeam()
            => AvatarTeams.Find(team => team.Index == CurTeamIndex)!;

    public bool TryGetAvatar(uint avatarId, [MaybeNullWhen(false)] out GameAvatar avatar)
        => (avatar = Avatars.Find(a => a.AvatarId == avatarId)) != null;

    private void UnlockAllAvatars()
    {
        ExcelTable avatarTable = _excelTables.GetTable(ExcelType.Avatar);
        for (int i = 0; i < avatarTable.Count; i++)
        {
            AvatarExcel avatarExcel = avatarTable.GetItemAt<AvatarExcel>(i);
            if (AvatarBlackList.Contains(avatarExcel.Id) || avatarExcel.Id >= 11000000) continue;

            uint currentTimestamp = (uint)DateTimeOffset.Now.ToUnixTimeSeconds();
            GameAvatar avatar = new()
            {
                AvatarId = avatarExcel.Id,
                SkillDepotId = avatarExcel.SkillDepotId,
                WeaponId = avatarExcel.InitialWeapon,
                BornTime = currentTimestamp,
                Guid = NextGuid(),
                WearingFlycloakId = 140001,
                WeaponGuid = NextGuid()
            };

            avatar.InitDefaultProps(avatarExcel);
            Avatars.Add(avatar);
        }
    }

    public ulong NextGuid()
    {
        return ((ulong)Uid << 32) + (++GuidSeed);
    }
}
