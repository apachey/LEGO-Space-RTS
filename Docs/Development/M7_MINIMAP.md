# M7 T069 Minimap

Status: implemented functional presentation. Visual styling remains non-canon.

## Player-facing behavior

- The map is permanently north-up. The white viewport polygon rotates and
  changes shape with the RTS camera.
- Left-click centers the camera and left-drag continuously scrubs it.
- Right-click issues the existing exact-cell-center Move command. `Shift` +
  right-click uses the existing queued-move modifier. Selected production
  structures set their rally point instead.
- Ground mobiles, true air, structures and resources use different shapes.
- Owned, allied, enemy, neutral and resource colors remain independently
  editable in the HUD Lab.
- Terrain, explored fog, unseen fog, current contacts, remembered static
  contacts, owned/known-enemy Tube links, visible Surge activations and
  actionable alert pulses are separate layers.

## Knowledge boundary

`MinimapPresentationSource` consumes the viewer-filtered
`PresentationSnapshot` and the viewer's fog mask. It does not iterate raw enemy
entities to create current markers.

- Enemy mobile and air contacts disappear when visibility is lost.
- Only previously observed enemy structures and resources may survive as
  remembered markers, and only while their cell is explored but not visible.
- Revisiting a remembered cell removes the contact when the object is confirmed
  absent.
- Owned Tube links originate only from owned local state. Enemy Tube segments
  become known only while adjacent route cells are visible, persist under
  explored fog, and disappear when renewed visibility disproves them.
- Surge pulses come only from owned anchors or enemy anchors present in the
  viewer-filtered snapshot.
- Every frame is bounded and validates marker legality, raster sizes, map bounds
  and capacities before smoke acceptance.

The current local prototype reads the local viewer state. The retained frame
contract is suitable for the already filtered network client adapter without
changing snapshot packet format 3.

## Presentation and cost

- Terrain is a cached 160×160 build-cell raster rebuilt only when map topology
  changes.
- Fog is a 160×160 viewer-knowledge texture refreshed with the HUD at 10 Hz.
- Markers are interpolated between 10-Hz frames and rendered through four
  bounded `MultiMeshInstance2D` channels rather than one node per contact.
- The camera polygon updates every render frame independently of the 10-Hz
  strategic data.
- Markers, Tube lines and pings are capped at 512, 256 and 32 respectively.

## HUD Lab

`F8` → **M7 HUD Lab** exposes every current minimap presentation token and all
eight fixtures include visible, explored, unseen and remembered states. The
Martian fixture adds operational/dashed owned Tube links plus an explored enemy
segment; brownout and critical fixtures add alert pulses. Lab minimap input moves the synthetic camera polygon
or creates move feedback, so interaction can be evaluated without altering the
simulation.

## Deliberately deferred to T073

Attack-move-on-minimap and networked team pings require the complete command
catalog, target-mode state and transport path. T069 does not invent a partial
command or change a public packet format to simulate them.
