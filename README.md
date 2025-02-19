# Bullet Shoot System (ECS)
ECS Version of [Bullet Pattern System done using Script Defined Sequential Action](https://github.com/GDAsim/UnityBulletPatternSystem)

I have to change up the architecture a little in order to keep similar structure as the normal version

Implementation Milestones:
- [x] Basic Bullet Movement Action
- [x] Homing Bullet Movement Action
- [x] Multi Gun Controller System
- [X] Gun Preshoot Action
- [X] Bullet Sync Move (Delay Action)
- [ ] Teleport Bullet Movement (Instant Action, Delta Movement Action)
- [ ] Bullet Spawn Bullets (Split Action)


---

### Info

Implementation Milestones:
Level 1: Spawn & Move
- Basic Bullet Spawning
- Editor/Inspector Refrencing
- Static Definition
- Scriptable Object Definition
- Shoot & Reload System
- Basic Forward Move

Level 2: Seek & Destroy
- Enum & Interface 
- Upgrade Bullet Movement to Func Setup 
- Update Basic Move to Generic TransformAction
- Bullet Action System : State Management lite
- Different Homing Types

Level 3: Multi Shoot System
- Upgrade to SystemController to setup Multiple Guns
- Different Gun Setup for Bullet Patterns
- Normal,Cycle,Helix

Level 4: Sync Shoot System
- Add Gun Preshoot Action System
- Add DelayAction
- Spawn All Then Move

Level 5: Teleport Shoot System
- Add StartTimer for Action
- Upgrade to support Instant Action (0 Duration Action)
- Upgrade to support Non Delta Transform Action
- Different Multiple Teleport Setup

Level 6: Split Shoot System
- Add Bullets to Spawn new/child Bullets as SplitAction
- Add TransformDelta to perform and apply to new spawned bullets
- Add destroy after split
- Add Copy feature to copy current bullet state over to new/child bullets (+Gives Sync Feature) 
