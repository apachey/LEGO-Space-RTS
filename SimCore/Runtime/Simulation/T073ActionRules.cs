using System;
using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
/// <summary>Shared deterministic commitment/refund arithmetic approved for T073 jobs.</summary>
public static class CancellationAccounting
{
    public static int Consumed(int total, int progressTicks, int totalTicks)
    {
        if (total < 0 || progressTicks < 0 || totalTicks <= 0 || progressTicks > totalTicks) throw new ArgumentOutOfRangeException();
        if (progressTicks == 0) return 0;
        int initialCommit = checked((total + 4) / 5);
        return checked(initialCommit + (int)((long)(total - initialCommit) * progressTicks / totalTicks));
    }

    public static int Refund(int total, int progressTicks, int totalTicks)
    {
        int consumed = Consumed(total, progressTicks, totalTicks);
        return checked(total - consumed + consumed / 2);
    }

    public static bool CrystalsCommitted(int progressTicks, int totalTicks)
        => progressTicks > 0 && checked(progressTicks * 2) >= totalTicks;
}

public static class ActionPrerequisites
{
    public static bool AreMet(SimulationWorld world, byte playerSlot, ContentActionPrerequisiteGroup[] groups)
    {
        for (int i = 0; i < groups.Length; i++)
        {
            bool groupMet = false;
            ContentActionPrerequisite[] alternatives = groups[i].Alternatives;
            for (int j = 0; j < alternatives.Length && !groupMet; j++)
                groupMet = alternatives[j].Kind == ContentActionPrerequisiteKind.Building
                    ? HasCompletedBuilding(world, playerSlot, alternatives[j].TargetId)
                    : ResearchSystem.HasCompleted(world, playerSlot, alternatives[j].TargetId);
            if (!groupMet) return false;
        }
        return true;
    }

    public static bool HasCompletedBuilding(SimulationWorld world, byte playerSlot, ContentId type)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
            if (world.Entities.Ownership.TryGet(alive[i], out Ownership owner) && owner.PlayerSlot == playerSlot &&
                world.Entities.Building.TryGet(alive[i], out Building building) && building.Type == type && building.State == BuildingState.Completed) return true;
        return false;
    }
}
}
