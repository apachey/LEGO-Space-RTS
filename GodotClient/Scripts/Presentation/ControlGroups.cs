using System.Text;
using LegoSpaceRTS.SimCore;

namespace LegoSpaceRTS.Presentation;

public sealed class ControlGroups
{
    private readonly List<EntityId>[] _groups = Enumerable.Range(0, 10).Select(_ => new List<EntityId>(128)).ToArray();

    public void Assign(int index, IReadOnlyList<EntityId> ids)
    {
        List<EntityId> group = _groups[index]; group.Clear();
        for (int i = 0; i < ids.Count && group.Count < 128; i++) if (!group.Contains(ids[i])) group.Add(ids[i]);
        group.Sort(static (a, b) => a.Value.CompareTo(b.Value));
    }
    public void Add(int index, IReadOnlyList<EntityId> ids)
    {
        List<EntityId> group = _groups[index];
        for (int i = 0; i < ids.Count && group.Count < 128; i++) if (!group.Contains(ids[i])) group.Add(ids[i]);
        group.Sort(static (a, b) => a.Value.CompareTo(b.Value));
    }
    public void Remove(int index, IReadOnlyList<EntityId> ids)
    {
        List<EntityId> group = _groups[index];
        for (int i = 0; i < ids.Count; i++) group.Remove(ids[i]);
    }
    public IReadOnlyList<EntityId> Recall(int index, SimulationWorld world)
    {
        List<EntityId> group = _groups[index];
        for (int i = group.Count - 1; i >= 0; i--) if (!world.Entities.Selectable.Has(group[i])) group.RemoveAt(i);
        return group;
    }

    public string GetMembershipText(EntityId id)
    {
        StringBuilder? builder = null;
        for (int i = 0; i < _groups.Length; i++)
        {
            if (!_groups[i].Contains(id)) continue;
            builder ??= new StringBuilder(8);
            if (builder.Length > 0) builder.Append(',');
            builder.Append(i);
        }
        return builder?.ToString() ?? string.Empty;
    }
}
