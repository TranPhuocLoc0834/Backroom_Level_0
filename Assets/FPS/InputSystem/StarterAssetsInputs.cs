using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public partial class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
		public bool interact;
		public bool quick1;
		public bool quick2;
		public bool quick3;
		public bool quick4;
		public bool quick5;
		public bool turnOnFlashlight;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM
		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}
		 public void OnInteract(InputValue value)
		{
			interact = value.isPressed;
		}
		// >>> QuickSlot (chuẩn theo Equipment: 1,2,3,4,5)
        public void OnQuickSlot1(InputValue value) => quick1 = value.isPressed;
		public void OnQuickSlot2(InputValue value) => quick2 = value.isPressed;
		public void OnQuickSlot3(InputValue value) => quick3 = value.isPressed;
		public void OnQuickSlot4(InputValue value) => quick4 = value.isPressed;
		public void OnQuickSlot5(InputValue value) => quick5 = value.isPressed;
		public void OnTurnFlashlightOn(InputValue value) => turnOnFlashlight = value.isPressed;

#endif


		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
	
}