# M8.5 T082 — Rock Raiders material and texture-needs matrix

The accepted M7 role-authored material family remains authoritative. These are production requirements, not generated texture assets and not permission to bake structural detail into maps.

## Shared rules

- Black mechanical hulls and bright lime conduits define the faction; purple, organic tissue, nests and insect anatomy are forbidden.
- Every curved shell, hinge, docking bay, containment claw and energy path must have a mechanical parent and function.
- Translucent lime is not automatically emissive; emission marks active Charge, weapon, scan, docking or construction state only.
- Silhouette, transformation seams, open channels, planted contacts and crystal forms remain geometry; textures may not fake them.

## Reusable texture families

| ID | Purpose | Channels | Resolution | Texel density | Tiling | LOD fallback | Provenance | State |
|---|---|---|---|---|---|---|---|---|
| `ali_black_hull_surface` | Restrained molded black-shell roughness variation across craft-derived hulls without inventing biological skin or panel structure. | Tangent-space normal and linear roughness; black body color remains parametric. | 2048x2048 | 256 px/m at Close | Shared model-space 4 m repeat across connected hull modules. | Half strength at Combat; master roughness only at Strategic. | Project-authored procedural source informed by verified Mars Mission Alien hulls; human review required. | `SPECIFIED_NOT_AUTHORED` |
| `ali_lime_conduit_surface` | Controlled variation on lime conduits, energy rails and translucent housings while preserving their physical path. | Linear roughness and restrained emissive mask; geometry defines every conduit. | 1024x1024 | 512 px/m on localized conduit UVs | Short trim regions aligned to conduit direction; no phase reset at joints. | Collapse to one bounded lime Signal strip at Strategic. | Project-authored procedural/trim source; human review required. | `SPECIFIED_NOT_AUTHORED` |
| `ali_crystal_containment_mask` | Facet-localized Charge intensity and containment contact masks without baked glow or fake crystal depth. | Linear roughness, transmission control and separate emission mask. | 1024x1024 | Object-local crystal atlas; not world-density bound. | Non-tiling per approved crystal form. | One faceted Glass mass and bounded emission at Strategic. | Project-authored procedural crystal source; human review required. | `SPECIFIED_NOT_AUTHORED` |
| `ali_bay_state_signal_atlas` | Launch, docking, transformation, Charge and configuration state indicators on mechanical interfaces. | sRGB color/alpha with separate linear emission mask. | 512x512 | Screen-space and trim atlas; not world-density bound. | Non-tiling stable interface IDs. | One directional Signal block per active interface at Strategic. | Project-authored vector/procedural source; human review required. | `SPECIFIED_NOT_AUTHORED` |

## Per-asset needs

| Asset | Geometry must carry | Master-material roles | Reusable texture families | Bespoke textures |
|---|---|---|---|---|
| ETX Servitor | low crescent hover shell<br>central crystal cradle<br>single folding manipulator | `Body`, `Accent`, `Tool`, `Glass`, `Signal`, `Neutral` | `ali_black_hull_surface`, `ali_lime_conduit_surface`, `ali_crystal_containment_mask`, `ali_bay_state_signal_atlas` | None required in this faction draft |
| Alien Jet | broad swept plate<br>open pilot cavity<br>paired tall lime arches | `Body`, `Accent`, `Tool`, `Glass`, `Signal`, `Neutral` | `ali_black_hull_surface`, `ali_lime_conduit_surface`, `ali_bay_state_signal_atlas` | None required in this faction draft |
| Razor Skimmer | flat swept plan<br>twin forward prongs<br>central exposed energy core | `Body`, `Accent`, `Tool`, `Glass`, `Signal`, `Neutral` | `ali_black_hull_surface`, `ali_lime_conduit_surface`, `ali_crystal_containment_mask`, `ali_bay_state_signal_atlas` | None required in this faction draft |
| ETX Alien Strike | long keel and tail blades<br>paired huge crescents<br>visible planted siege braces | `Body`, `Accent`, `Tool`, `Glass`, `Signal`, `Neutral` | `ali_black_hull_surface`, `ali_lime_conduit_surface`, `ali_bay_state_signal_atlas` | None required in this faction draft |
| ETX Alien Infiltrator | long split nose<br>paired curved side modules<br>visible lime conduit links | `Body`, `Accent`, `Tool`, `Glass`, `Signal`, `Neutral` | `ali_black_hull_surface`, `ali_lime_conduit_surface`, `ali_bay_state_signal_atlas` | None required in this faction draft |
| Alien Mothership | interrupted circular carrier hull<br>open central channel and long tail<br>integrated unfolding craft bays | `Body`, `Accent`, `Tool`, `Glass`, `Signal`, `Lamp`, `Neutral` | `ali_black_hull_surface`, `ali_lime_conduit_surface`, `ali_crystal_containment_mask`, `ali_bay_state_signal_atlas` | None required in this faction draft |
| ETX Command Core | broad landed hull<br>raised central core<br>open seam entrances | `Body`, `Accent`, `Tool`, `Glass`, `Signal`, `Lamp`, `Neutral` | `ali_black_hull_surface`, `ali_lime_conduit_surface`, `ali_crystal_containment_mask`, `ali_bay_state_signal_atlas` | None required in this faction draft |
| Resonance Core | large faceted crystal<br>radial restraint claws<br>concentric Charge rings | `Body`, `Accent`, `Tool`, `Glass`, `Signal`, `Neutral` | `ali_black_hull_surface`, `ali_lime_conduit_surface`, `ali_crystal_containment_mask`, `ali_bay_state_signal_atlas` | None required in this faction draft |
| ETX Fabricator | low curved shell<br>open forward channel<br>paired overhead feed arms | `Body`, `Accent`, `Tool`, `Glass`, `Signal`, `Lamp`, `Neutral` | `ali_black_hull_surface`, `ali_lime_conduit_surface`, `ali_crystal_containment_mask`, `ali_bay_state_signal_atlas` | None required in this faction draft |
| Reconfiguration Dock | paired tall split shells<br>central suspended cradle<br>visible hinge and conduit network | `Body`, `Accent`, `Tool`, `Glass`, `Signal`, `Lamp`, `Neutral` | `ali_black_hull_surface`, `ali_lime_conduit_surface`, `ali_bay_state_signal_atlas` | None required in this faction draft |
| Power Coupler | opposed black arcs<br>thin central conductor<br>low three-point base | `Body`, `Accent`, `Tool`, `Glass`, `Signal`, `Neutral` | `ali_black_hull_surface`, `ali_lime_conduit_surface`, `ali_bay_state_signal_atlas` | None required in this faction draft |
| ETX Defense Node | landed swept craft shell<br>central hinged hardpoint<br>planted low cradle and Charge path | `Body`, `Accent`, `Tool`, `Glass`, `Signal`, `Lamp`, `Neutral` | `ali_black_hull_surface`, `ali_lime_conduit_surface`, `ali_bay_state_signal_atlas` | None required in this faction draft |
