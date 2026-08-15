using System;
using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
public sealed class EntityStore
{
    private readonly List<EntityId> _alive = new();
    private bool[] _exists = new bool[128];
    public uint NextEntityValue { get; private set; } = 1;

    public readonly ComponentStore<Ownership> Ownership = new();
    public readonly ComponentStore<SimTransform> Transform = new();
    public readonly ComponentStore<Movement> Movement = new();
    public readonly ComponentStore<NavigationAgent> Navigation = new();
    public readonly ComponentStore<Selectable> Selectable = new();
    public readonly ComponentStore<Vision> Vision = new();
    public readonly ComponentStore<Excavatable> Excavatable = new();
    public readonly ComponentStore<ResourceNode> ResourceNode = new();
    public readonly ComponentStore<Worker> Worker = new();
    public readonly ComponentStore<Builder> Builder = new();
    public readonly ComponentStore<ResourceCarrier> ResourceCarrier = new();
    public readonly ComponentStore<ResourceReceiver> ResourceReceiver = new();
    public readonly ComponentStore<ResourceBank> ResourceBank = new();
    public readonly ComponentStore<Building> Building = new();
    public readonly ComponentStore<EnergyDomain> EnergyDomain = new();
    public readonly ComponentStore<EnergyDomainMember> EnergyDomainMember = new();
    public readonly ComponentStore<WorksiteNode> WorksiteNode = new();
    public readonly ComponentStore<WorksiteMember> WorksiteMember = new();
    public readonly ComponentStore<WorksiteComponent> WorksiteComponent = new();
    public readonly ComponentStore<Deployment> Deployment = new();
    public readonly ComponentStore<ForwardServiceProvider> ForwardServiceProvider = new();
    public readonly ComponentStore<ForwardServiceMember> ForwardServiceMember = new();
    public readonly ComponentStore<MissionRefitState> MissionRefitState = new();
    public readonly ComponentStore<MissionRefitJob> MissionRefitJob = new();
    public readonly ComponentStore<ResonanceCore> ResonanceCore = new();
    public readonly ComponentStore<SurgeZone> SurgeZone = new();
    public readonly ComponentStore<SurgeReceiver> SurgeReceiver = new();
    public readonly ComponentStore<TubeStation> TubeStation = new();
    public readonly ComponentStore<TubeLink> TubeLink = new();
    public readonly ComponentStore<TubeComponent> TubeComponent = new();
    public readonly ComponentStore<PowerState> PowerState = new();
    public readonly ComponentStore<ConstructionSite> ConstructionSite = new();
    public readonly ComponentStore<Production> Production = new();

    public IReadOnlyList<EntityId> Alive => _alive;

    public EntityId Create()
    {
        EntityId id = new(NextEntityValue++);
        Ensure(id.Value);
        _exists[id.Value] = true;
        _alive.Add(id);
        return id;
    }

    internal EntityId CreateRestored(uint value)
    {
        if (value == 0) throw new ArgumentOutOfRangeException(nameof(value));
        Ensure(value);
        if (_exists[value]) throw new InvalidOperationException($"Entity {value} already exists.");
        EntityId id = new(value);
        _exists[value] = true;
        _alive.Add(id);
        if (value >= NextEntityValue) NextEntityValue = checked(value + 1);
        return id;
    }

    internal void RestoreNextEntityValue(uint value)
    {
        if (value == 0) throw new ArgumentOutOfRangeException(nameof(value));
        NextEntityValue = value;
    }

    public bool Exists(EntityId id) => id.Value < _exists.Length && _exists[id.Value];

    public bool Destroy(EntityId id)
    {
        if (!Exists(id)) return false;
        _exists[id.Value] = false;
        int index = _alive.BinarySearch(id, EntityIdComparer.Instance);
        if (index >= 0) _alive.RemoveAt(index);
        Ownership.Remove(id);
        Transform.Remove(id);
        Movement.Remove(id);
        Navigation.Remove(id);
        Selectable.Remove(id);
        Vision.Remove(id);
        Excavatable.Remove(id);
        ResourceNode.Remove(id);
        Worker.Remove(id);
        Builder.Remove(id);
        ResourceCarrier.Remove(id);
        ResourceReceiver.Remove(id);
        ResourceBank.Remove(id);
        Building.Remove(id);
        EnergyDomain.Remove(id);
        EnergyDomainMember.Remove(id);
        WorksiteNode.Remove(id);
        WorksiteMember.Remove(id);
        WorksiteComponent.Remove(id);
        Deployment.Remove(id);
        ForwardServiceProvider.Remove(id);
        ForwardServiceMember.Remove(id);
        MissionRefitState.Remove(id);
        MissionRefitJob.Remove(id);
        ResonanceCore.Remove(id);
        SurgeZone.Remove(id);
        SurgeReceiver.Remove(id);
        TubeStation.Remove(id);
        TubeLink.Remove(id);
        TubeComponent.Remove(id);
        PowerState.Remove(id);
        ConstructionSite.Remove(id);
        Production.Remove(id);
        return true;
    }

    private void Ensure(uint index)
    {
        if (index < _exists.Length) return;
        int size = _exists.Length;
        while (index >= size) size = checked(size * 2);
        Array.Resize(ref _exists, size);
    }

    private sealed class EntityIdComparer : IComparer<EntityId>
    {
        public static readonly EntityIdComparer Instance = new();
        public int Compare(EntityId x, EntityId y) => x.Value.CompareTo(y.Value);
    }
}
}
