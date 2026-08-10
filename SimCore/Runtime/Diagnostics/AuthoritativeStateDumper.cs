using System.Text;

namespace LegoSpaceRTS.SimCore
{
/// <summary>Development-only ordered state text used to localize deterministic hash divergence.</summary>
public static class AuthoritativeStateDumper
{
    public static string Dump(SimulationWorld world)
    {
        StringBuilder b=new StringBuilder(8192);
        b.Append("tick=").Append(world.Tick.Value).Append(" nextEntity=").Append(world.Entities.NextEntityValue)
            .Append(" map=").Append(world.Map.Id.Value).Append(" topology=").Append(world.Map.TopologyVersion).AppendLine();
        for(int player=0;player<world.PlayerCount;player++){OperationsCapacityState capacity=world.GetOperationsCapacity((byte)player);b.Append("player ").Append(player).Append(" oc=").Append(capacity.Active).Append('+').Append(capacity.Reserved).Append('/').Append(capacity.Maximum);for(int ei=0;ei<world.Entities.Alive.Count;ei++){EntityId root=world.Entities.Alive[ei];if(world.Entities.EnergyDomain.TryGet(root,out EnergyDomain energy)&&world.Entities.Ownership.TryGet(root,out Ownership owner)&&owner.PlayerSlot==player)b.Append(" energy=").Append(energy.Reserve.Raw).Append('/').Append(energy.ReserveCapacity.Raw).Append(" flow=").Append(energy.GenerationPerSecond).Append('-').Append(energy.ContinuousDemandPerSecond);}b.AppendLine();}
        var alive=world.Entities.Alive;
        for(int i=0;i<alive.Count;i++)
        {
            EntityId id=alive[i]; b.Append("entity ").Append(id.Value);
            if(world.Entities.Ownership.TryGet(id,out Ownership o))b.Append(" owner=").Append(o.PlayerSlot);
            if(world.Entities.Transform.TryGet(id,out SimTransform t))b.Append(" pos=").Append(t.Position.X.Raw).Append(',').Append(t.Position.Y.Raw).Append(" angle=").Append(t.Orientation.Raw);
            if(world.Entities.Movement.TryGet(id,out Movement m))b.Append(" speed=").Append(m.CurrentSpeed.Raw).Append(" vel=").Append(m.CurrentVelocity.X.Raw).Append(',').Append(m.CurrentVelocity.Y.Raw).Append(" desired=").Append(m.DesiredMovement.X.Raw).Append(',').Append(m.DesiredMovement.Y.Raw).Append(" pathIndex=").Append(m.PathIndex).Append(" state=").Append((byte)m.State).Append(" stuck=").Append(m.StuckTicks).Append(" compress=").Append(m.CompressionTicks);
            if(world.Entities.Navigation.TryGet(id,out NavigationAgent n))b.Append(" fp=").Append((byte)n.Footprint).Append(" layer=").Append((byte)n.Layer).Append(" target=").Append(n.Target.X.Raw).Append(',').Append(n.Target.Y.Raw).Append(" hasTarget=").Append(n.HasTarget?1:0).Append(" dirty=").Append(n.PathDirty?1:0).Append(" topo=").Append(n.PathTopologyVersion).Append(" age=").Append(n.RequestAge).Append(" cohort=").Append(n.Formation.CohortId).Append(" slot=").Append(n.Formation.SlotIndex).Append('/').Append(n.Formation.MemberCount).Append(" cols=").Append(n.Formation.Columns).Append(" reflowTick=").Append(n.Formation.LastReflowTick);
            if(world.Entities.Selectable.TryGet(id,out Selectable s))b.Append(" content=").Append(s.ContentType.Value).Append(" kind=").Append((byte)s.Kind);
            if(world.Entities.Vision.TryGet(id,out Vision v))b.Append(" vision=").Append(v.RadiusBuildCells);
            if(world.Entities.ResourceNode.TryGet(id,out ResourceNode resource))b.Append(" resourceType=").Append((byte)resource.Type).Append(" depositSize=").Append((byte)resource.DepositSize).Append(" remaining=").Append(resource.Remaining).Append('/').Append(resource.Capacity).Append(" visualState=").Append((byte)resource.VisualState);
            if(world.Entities.Worker.TryGet(id,out Worker worker))b.Append(" workerState=").Append((byte)worker.TaskState).Append(" resourceTarget=").Append(worker.ResourceTarget.Value).Append(" receiverTarget=").Append(worker.ReceiverTarget.Value).Append(" extractionTicks=").Append(worker.ExtractionTicks).Append('/').Append(worker.TicksPerOre);
            if(world.Entities.Builder.TryGet(id,out Builder builder))b.Append(" builderState=").Append((byte)builder.JobState).Append(" constructionTarget=").Append(builder.ConstructionTarget.Value);
            if(world.Entities.ResourceCarrier.TryGet(id,out ResourceCarrier carrier))b.Append(" cargoType=").Append((byte)carrier.Type).Append(" cargo=").Append(carrier.Amount).Append('/').Append(carrier.Capacity);
            if(world.Entities.ResourceReceiver.TryGet(id,out ResourceReceiver receiver))b.Append(" receiverType=").Append((byte)receiver.AcceptedType).Append(" pendingHauled=").Append(receiver.PendingHauledAmount).Append(" hqEmergency=").Append(receiver.IsHqEmergencyReceiver);
            if(world.Entities.ResourceBank.TryGet(id,out ResourceBank bank))b.Append(" bankType=").Append((byte)bank.Type).Append(" processed=").Append(bank.ProcessedAmount);
            if(world.Entities.Building.TryGet(id,out Building building))b.Append(" buildingType=").Append(building.Type.Value).Append(" anchor=").Append(building.AnchorX).Append(',').Append(building.AnchorY).Append(" orientation=").Append(building.Orientation).Append(" footprint=").Append(building.FootprintWidth).Append('x').Append(building.FootprintHeight).Append(" buildingState=").Append((byte)building.State);
            if(world.Entities.EnergyDomainMember.TryGet(id,out EnergyDomainMember energyMember))b.Append(" energyDomain=").Append(energyMember.DomainRoot.Value);
            if(world.Entities.ConstructionSite.TryGet(id,out ConstructionSite site))b.Append(" builder=").Append(site.AssignedBuilder.Value).Append(" fundingBank=").Append(site.FundingBank.Value).Append(" reservedOre=").Append(site.ReservedOre).Append(" consumedOre=").Append(site.ConsumedOre).Append(" requiredEnergy=").Append(site.RequiredEnergy).Append(" reservedEnergy=").Append(site.ReservedEnergy).Append(" consumedEnergy=").Append(site.ConsumedEnergy).Append(" constructionTicks=").Append(site.ProgressTicks).Append('/').Append(site.RequiredTicks);
            if(world.Entities.Production.TryGet(id,out Production production)){b.Append(" productionCount=").Append(production.Count).Append(" spawnBlocked=").Append(production.SpawnBlocked?1:0).Append(" rally=").Append(production.HasRallyPoint?1:0).Append('@').Append(production.RallyPoint.X.Raw).Append(',').Append(production.RallyPoint.Y.Raw);for(int pi=0;pi<production.Count;pi++){ProductionQueueItem item=production.Get(pi);b.Append(" item").Append(pi).Append('=').Append(item.UnitType.Value).Append(':').Append(item.RemainingTicks).Append('/').Append(item.TotalTicks).Append(':').Append(item.ReservedOre);}}
            if(world.TryGetQueue(id,out UnitCommandQueue q))b.Append(" queue=").Append(q.Count);
            RouteCorridor? corridor=world.GetCorridor(id);if(corridor!=null)b.Append(" corridorCells=").Append(corridor.Cells.Count).Append(" corridorTopo=").Append(corridor.TopologyVersion);
            b.AppendLine();
        }
        b.Append("pendingCommands=").Append(world.Commands.Count).Append(" stateHash=").Append(StateHasher.HashHex(world)).AppendLine();
        return b.ToString();
    }

    public static string FirstDifference(string expected,string actual)
    {
        string[] a=expected.Replace("\r",string.Empty).Split('\n');string[] b=actual.Replace("\r",string.Empty).Split('\n');
        int n=a.Length<b.Length?a.Length:b.Length;
        for(int i=0;i<n;i++)if(!string.Equals(a[i],b[i],System.StringComparison.Ordinal))return "line="+(i+1)+" expected=["+a[i]+"] actual=["+b[i]+"]";
        return a.Length==b.Length?"no textual state difference":"line-count expected="+a.Length+" actual="+b.Length;
    }
}
}
