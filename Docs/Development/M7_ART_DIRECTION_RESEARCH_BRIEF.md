# M7 — ART-DIRECTION RESEARCH BRIEF

## Status

**NON-CANONICAL DEVELOPMENT RESEARCH.**

This brief guides visual experiments. It is not a checklist, does not replace
`Docs/Canon/`, and does not lock a rendering style. A solution may diverge when
it looks better, reads better from the RTS camera, or fits a faction better.

## Desired center

The target is a fully volumetric 3D RTS with strong readable forms and
expressive mechanical construction.

Desired qualities:

- large, immediately understandable silhouettes;
- convincing mass and volume;
- machinery whose construction and function are visually legible;
- large functional components instead of dense small-scale noise;
- moderately exaggerated proportions where they improve character or
  readability;
- decisive color masses;
- strong materials, lighting and VFX without pursuing photorealism;
- attractive but relatively clean environments that do not compete with units;
- enough close-view detail to feel rewarding, with normal RTS-camera
  readability taking priority.

LEGO is primarily a **construction language**. Vehicles should visibly consist
of LEGO-like parts and mechanisms without reading as photographs of physical
sets or toys arranged on a tabletop.

The approximate center is:

> Industrial Annihilation + StarCraft II + Beyond All Reason, translated into
> a LEGO construction language.

This is neither a formula nor a request to copy any of those games.

## Positive references

- Industrial Annihilation — the strongest single reference;
- Beyond All Reason;
- StarCraft II;
- LEGO Battles;
- CrystAlien Conflict;
- Planetary Annihilation: TITANS;
- Grey Goo;
- Warcraft III;
- Battle Aces;
- AirMech / AirMech Strike;
- Zero-K;
- Stormgate;
- Tempest Rising;
- Crossfire: Legion;
- Godsworn;
- ZeroSpace.

Primary visual anchors used for the renderer study include the
[Industrial Annihilation site](https://industrialannihilation.com/), the
[Beyond All Reason screenshot gallery](https://www.beyondallreason.info/screenshots),
the [official StarCraft II site](https://starcraft2.blizzard.com/), and the
[Planetary Annihilation: TITANS store gallery](https://store.steampowered.com/app/386070/Planetary_Annihilation_TITANS/).

## Negative references

- Darwinia;
- Rymdkapsel;
- Eufloria;
- Rusted Warfare;
- Company of Heroes;
- Supreme Commander 2;
- Age of Mythology: Retold;
- Universe at War: Earth Assault;
- Armies of Exigo;
- Rise of Nations: Rise of Legends;
- IMMORTAL: Gates of Pyre;
- 9-Bit Armies: A Bit Too Far;
- The Settlers: New Allies;
- Sins of a Solar Empire II;
- SpellForce 3 Reforced.

The list is a boundary for the combined direction. It does not imply one shared
failure or a per-title diagnosis that the game director did not provide.

## Neutral references

These may contribute individual ideas but must not define the overall style:

- Command & Conquer: Red Alert 3;
- Halo Wars 2;
- Command & Conquer 3: Tiberium Wars;
- Halo Wars;
- Homeworld: Deserts of Kharak;
- Iron Harvest;
- Northgard;
- Tooth and Tail;
- Impossible Creatures;
- Command & Conquer: Generals / Zero Hour;
- Sanctuary: Shattered Sun;
- Dust Front RTS;
- WAR PARTY;
- Battle Realms: Zen Edition;
- Homeworld 3;
- Dune: Spice Wars;
- D.O.R.F. Real-Time Strategic Conflict.

## Renderer translation for round 2

Round 1's broad categories are retired as forward candidates. Round 2 keeps one
volumetric mechanical center and tests four controlled interpretations:

| Key | Treatment | Question under test |
|---|---|---|
| 1 | Industrial Mass | Can restrained painted/metal material response, grounded light and quiet terrain make construction and weight convincing without becoming photoreal? |
| 2 | Heroic RTS | Can stronger warm/cool separation, saturation and shaped highlights improve silhouette and function without turning flat or noisy? |
| 3 | Constructive LEGO | Can molded polymer, exposed tool metal and visible part logic communicate LEGO without tabletop/toy photography? |
| 4 | Graphic Volume | Can controlled light bands and broad terrain values increase RTS readability while preserving full 3D volume? |

The model, camera, geometry, animation and semantic color slots remain invariant.
Only material response, lighting, terrain treatment and VFX language change.
The carrier uses the accepted Rock Raiders palette semantics, including a major
earth-brown structural mass and role-bound neon-orange/neon-lime emission.

Outline is an independent diagnostic parameter, not part of Graphic Volume or
any other treatment. It is off by default so its effect can be compared without
silently changing the selected material and lighting direction.

## Rejection guards

Round 2 should be rejected or revised if it:

- flattens the model into icon-like shapes;
- relies on outlines or filters to manufacture readability;
- makes plastic gloss the defining idea;
- resembles a photographed physical diorama;
- fills the ground or vehicle with high-frequency noise;
- obscures functional construction with material effects;
- looks good only in a close crop and fails at the normal RTS camera;
- makes every faction share one color or material language.
