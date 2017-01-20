using UnityEngine;

public class HashIDs
{
	//=====================================================
	// A list of Mecanim state / variable hash IDs.

	// Player / Character
	private static readonly int _stateIdling = Animator.StringToHash( "Base Layer.Idle01" );
	private static readonly int _stateLocomotion = Animator.StringToHash( "Base Layer.Locomotion" );
	private static readonly int _stateRunning = Animator.StringToHash( "Base Layer.Running" );
	private static readonly int _stateIdlePivotLeft = Animator.StringToHash( "Base Layer.IdlePivotLeft" );
	private static readonly int _stateIdlePivotRight = Animator.StringToHash( "Base Layer.IdlePivotRight" );
	private static readonly int _stateRunningPivotLeft = Animator.StringToHash( "Base Layer.RunningPivotLeft" );
	private static readonly int _stateRunningPivotRight = Animator.StringToHash( "Base Layer.RunningPivotRight" );
	private static readonly int _stateDamaged = Animator.StringToHash( "Base Layer.Damaged" );
	private static readonly int _stateDead = Animator.StringToHash( "Base Layer.DEAD" );
	private static readonly int _stateRespawning = Animator.StringToHash( "Base Layer.Respawn" );
	private static readonly int _stateJumping = Animator.StringToHash( "Base Layer.Jump" );
	private static readonly int _stateLanding = Animator.StringToHash( "Base Layer.Land" );
	private static readonly int _stateClimbingUp = Animator.StringToHash( "Base Layer.ClimbUp" );
	private static readonly int _statePushingStart = Animator.StringToHash( "PushObject.PushObject_Start" );
	private static readonly int _statePushingObject = Animator.StringToHash( "Base Layer.PushObject.PushObject_Loop" );
	private static readonly int _statePushingObjectBuffer = Animator.StringToHash( "Base Layer.PushObject.PushBuffer" );
	private static readonly int _stateOpenDoor = Animator.StringToHash( "ObjectInteraction.OpenDoor" );
	private static readonly int _stateUseFloorLever = Animator.StringToHash( "ObjectInteraction.UseFloorLever" );
	private static readonly int _stateUseWallLever = Animator.StringToHash( "ObjectInteraction.UseWallLever" );
	private static readonly int _stateCastSpell = Animator.StringToHash( "Base Layer.CastSpell" );
	private static readonly int _stateOpenChest = Animator.StringToHash( "ObjectInteraction.OpenChest" );
	private static readonly int _stateIsTrapped = Animator.StringToHash( "Trapped.MagicalTrapLoop" );

	private static readonly int _speed = Animator.StringToHash( "Speed" );
	private static readonly int _direction = Animator.StringToHash( "Direction" );
	private static readonly int _angle = Animator.StringToHash( "Angle" );
	private static readonly int _colliderHeight = Animator.StringToHash( "ColliderHeight" );
	private static readonly int _colliderY = Animator.StringToHash( "ColliderY" );
	private static readonly int _isJumping = Animator.StringToHash( "IsJumping" );
	private static readonly int _isLanding = Animator.StringToHash( "IsLanding" );
	private static readonly int _isClimbingUp = Animator.StringToHash( "IsClimbingUp" );
	private static readonly int _isClimbingDown = Animator.StringToHash( "IsClimbingDown" );
	private static readonly int _isPushing = Animator.StringToHash( "IsPushing" );
	private static readonly int _isPushFail = Animator.StringToHash( "IsPushFail" );
	private static readonly int _isDamaged = Animator.StringToHash( "IsDamaged" );
	private static readonly int _isDead = Animator.StringToHash( "IsDead" );
	private static readonly int _isRespawning = Animator.StringToHash( "IsRespawning" );
	private static readonly int _isOpeningDoor = Animator.StringToHash( "IsOpeningDoor" );
	private static readonly int _isUsingFloorLever = Animator.StringToHash( "IsUsingFloorLever" );
	private static readonly int _isUsingWallLever = Animator.StringToHash( "IsUsingWallLever" );
	private static readonly int _isCastingSpell = Animator.StringToHash( "IsCastingSpell" );
	private static readonly int _isOpeningChest = Animator.StringToHash( "IsOpeningChest" );
	private static readonly int _isInteractFail = Animator.StringToHash( "IsInteractFail" );
	private static readonly int _isTrapped = Animator.StringToHash( "IsTrapped" );
	private static readonly int _isEscapingTrap = Animator.StringToHash( "IsEscapingTrap" );
	private static readonly int _isCutsceneWalk = Animator.StringToHash( "IsCutsceneWalk" );
	private static readonly int _isCutsceneCrawl = Animator.StringToHash( "IsCutsceneCrawl" );
	private static readonly int _isCutscenePortal = Animator.StringToHash( "IsCutscenePortal" );
	private static readonly int _isCutsceneCelebrate = Animator.StringToHash( "IsCutsceneCelebrate" );
	private static readonly int _isIdling02 = Animator.StringToHash( "IsIdling02" );
	private static readonly int _isIdling03 = Animator.StringToHash( "IsIdling03" );

	// Switch
	private static readonly int _stateSwitchOnHash = Animator.StringToHash( "Base Layer.SwitchOn" );
	private static readonly int _isSwitchOn = Animator.StringToHash( "IsSwitchOn" );

	// Chest
	private static readonly int _stateChestOpenHash = Animator.StringToHash( "Base Layer.ChestOpen" );
	private static readonly int _isChestOpen = Animator.StringToHash( "IsChestOpen" );

	// Shadow Creature
	private static readonly int _stateAttacking = Animator.StringToHash( "Base Layer.EXPLODE" );
	private static readonly int _isAttacking = Animator.StringToHash( "IsAttacking" );

	// Common
	private static readonly int _isOpen = Animator.StringToHash( "IsOpen" );
	private static readonly int _isEnabled = Animator.StringToHash( "IsEnabled" );
	private static readonly int _enable = Animator.StringToHash( "Enable" );
	private static readonly int _disable = Animator.StringToHash( "Disable" );

	// Boss
	private static readonly int _idle = Animator.StringToHash( "Idle" );
	private static readonly int _idleTaunt = Animator.StringToHash( "IdleTaunt" );
	private static readonly int _teleportIn = Animator.StringToHash( "TeleportIn" );
	private static readonly int _teleportOut = Animator.StringToHash( "TeleportOut" );
	private static readonly int _summon = Animator.StringToHash( "Summon" );
	private static readonly int _attack01 = Animator.StringToHash( "Attack01" );
	private static readonly int _attack02 = Animator.StringToHash( "Attack02" );
	private static readonly int _shock = Animator.StringToHash( "Shock" );
	private static readonly int _stun = Animator.StringToHash( "Stun" );
	private static readonly int _stunHit = Animator.StringToHash( "StunHit" );
	private static readonly int _recover = Animator.StringToHash( "Recover" );
	private static readonly int _dead = Animator.StringToHash( "Dead" );
	private static readonly int _celebrate = Animator.StringToHash( "Celebrate" );

	// NPC
	private static readonly int _walk = Animator.StringToHash( "Walk" );
	private static readonly int _stop = Animator.StringToHash( "Stop" );
	private static readonly int _interact = Animator.StringToHash( "Interact" );
	private static readonly int _attract = Animator.StringToHash( "Attract" );

	// Tutorial
	private static readonly int _tutorial01 = Animator.StringToHash( "Tutorial01" );
	private static readonly int _tutorial02 = Animator.StringToHash( "Tutorial02" );
	private static readonly int _tutorial03 = Animator.StringToHash( "Tutorial03" );
	private static readonly int _tutorial04 = Animator.StringToHash( "Tutorial04" );

	//=====================================================

	// Player / Character
	public static int StateIdling { get { return _stateIdling; } }
	public static int StateLocomotion { get { return _stateLocomotion; } }
	public static int StateRunning { get { return _stateRunning; } }
	public static int StateIdlePivotLeft { get { return _stateIdlePivotLeft; } }
	public static int StateIdlePivotRight { get { return _stateIdlePivotRight; } }
	public static int StateRunningPivotLeft { get { return _stateRunningPivotLeft; } }
	public static int StateRunningPivotRight { get { return _stateRunningPivotRight; } }
	public static int StateDamaged { get { return _stateDamaged; } }
	public static int StateDead { get { return _stateDead; } }
	public static int StateRespawning { get { return _stateRespawning; } }
	public static int StateJumping { get { return _stateJumping; } }
	public static int StateLanding { get { return _stateLanding; } }
	public static int StateClimbingUp { get { return _stateClimbingUp; } }
	public static int StatePushingStart { get { return _statePushingStart; } }
	public static int StatePushingObject { get { return _statePushingObject; } }
	public static int StatePushingObjectBuffer { get { return _statePushingObjectBuffer; } }
	public static int StateOpenDoor { get { return _stateOpenDoor; } }
	public static int StateUseFloorLever { get { return _stateUseFloorLever; } }
	public static int StateUseWallLever { get { return _stateUseWallLever; } }
	public static int StateCastSpell { get { return _stateCastSpell; } }
	public static int StateOpenChest { get { return _stateOpenChest; } }
	public static int StateIsTrapped { get { return _stateIsTrapped; } }

	public static int Speed { get { return _speed; } }
	public static int Direction { get { return _direction; } }
	public static int Angle { get { return _angle; } }
	public static int ColliderHeight { get { return _colliderHeight; } }
	public static int ColliderY { get { return _colliderY; } }
	public static int IsJumping { get { return _isJumping; } }
	public static int IsLanding { get { return _isLanding; } }
	public static int IsClimbingUp { get { return _isClimbingUp; } }
	public static int IsClimbingDown { get { return _isClimbingDown; } }
	public static int IsPushing { get { return _isPushing; } }
	public static int IsPushFail { get { return _isPushFail; } }
	public static int IsOpeningDoor { get { return _isOpeningDoor; } }
	public static int IsUsingFloorLever { get { return _isUsingFloorLever; } }
	public static int IsUsingWallLever { get { return _isUsingWallLever; } }
	public static int IsDamaged { get { return _isDamaged; } }
	public static int IsDead { get { return _isDead; } }
	public static int IsRespawning { get { return _isRespawning; } }
	public static int IsCastingSpell { get { return _isCastingSpell; } }
	public static int IsOpeningChest { get { return _isOpeningChest; } }
	public static int IsInteractFail { get { return _isInteractFail; } }
	public static int IsTrapped { get { return _isTrapped; } }
	public static int IsEscapingTrap { get { return _isEscapingTrap; } }
	public static int IsCutsceneWalk { get { return _isCutsceneWalk; } }
	public static int IsCutsceneCrawl { get { return _isCutsceneCrawl; } }
	public static int IsCutscenePortal { get { return _isCutscenePortal; } }
	public static int IsCutsceneCelebrate { get { return _isCutsceneCelebrate; } }
	public static int IsIdling02 { get { return _isIdling02; } }
	public static int IsIdling03 { get { return _isIdling03; } }

	// Switch
	public static int StateSwitchOnHash { get { return _stateSwitchOnHash; } }
	public static int IsSwitchOn { get { return _isSwitchOn; } }

	// Chest
	public static int StateChestOpenHash { get { return _stateChestOpenHash; } }
	public static int IsChestOpen { get { return _isChestOpen; } }

	// Shadow Creature
	public static int StateAttacking { get { return _stateAttacking; } }
	public static int IsAttacking { get { return _isAttacking; } }

	// Common
	public static int IsOpen { get { return _isOpen; } }
	public static int IsEnabled { get { return _isEnabled; } }
	public static int Enable { get { return _enable; } }
	public static int Disable { get { return _disable; } }

	// Boss
	public static int Idle { get { return _idle; } }
	public static int IdleTaunt { get { return _idleTaunt; } }
	public static int TeleportIn { get { return _teleportIn; } }
	public static int TeleportOut { get { return _teleportOut; } }
	public static int Summon { get { return _summon; } }
	public static int Attack01 { get { return _attack01; } }
	public static int Attack02 { get { return _attack02; } }
	public static int Shock { get { return _shock; } }
	public static int Stun { get { return _stun; } }
	public static int StunHit { get { return _stunHit; } }
	public static int Recover { get { return _recover; } }
	public static int Dead { get { return _dead; } }
	public static int Celebrate { get { return _celebrate; } }

	// NPC
	public static int Walk { get { return _walk; } }
	public static int Stop { get { return _stop; } }
	public static int Interact { get { return _interact; } }
	public static int Attract { get { return _attract; } }

	// Tutorial
	public static int Tutorial01 { get { return _tutorial01; } }
	public static int Tutorial02 { get { return _tutorial02; } }
	public static int Tutorial03 { get { return _tutorial03; } }
	public static int Tutorial04 { get { return _tutorial04; } }

	//=====================================================
}
