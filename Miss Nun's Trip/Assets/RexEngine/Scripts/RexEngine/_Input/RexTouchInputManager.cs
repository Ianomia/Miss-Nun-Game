/* Copyright Sky Tyrannosaur */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RexEngine
{
    public class RexTouchInputManager:MonoBehaviour, ITouchInputManager
    {
		[System.Serializable]
		public class ButtonNames
		{
			public DPad dPad = new DPad();
			public ActionButtons actionButtons = new ActionButtons();
		}

		[System.Serializable]
		public class DPad
		{
			public string up = "UpButton";
			public string down = "DownButton";
			public string left = "LeftButton";
			public string right = "RightButton";
			public string upLeft = "Up_LeftButton";
			public string upRight = "Up_RightButton";
			public string downLeft = "Down_LeftButton";
			public string downRight = "Down_RightButton";
		}

		[System.Serializable]
		public class ActionButtons
		{
			public string jump = "JumpButton";
			public string attack = "AttackButton";
			public string subAttack = "SubAttackButton";
			public string subAttack_2 = "SubAttack_2Button";
			public string subAttack_3 = "SubAttack_3Button";
			public string dash = "DashButton";
			public string run = "RunButton";
			public string misc_1 = "Misc_1Button";
			public string misc_2 = "Misc_2Button";
		}

		public bool allowCreationOfTouchControls = true;

		protected ButtonNames buttonNames = new ButtonNames();

		protected GameObject touchInterfaceObject;

        protected bool isInitialTouchSet;
        protected Vector2 initialTouchPosition;
        protected Camera uiCamera;
        protected int collisionLayerMask;

        private InputState[] inputStates;

        protected bool isTouchInterfaceEnabled = false;

        public virtual bool Enabled
        {
            get
            {
                return this.isActiveAndEnabled;
            }
            set
            {
                if(!value) ToggleTouchInterface(false); // disable touch ui when disabled
                this.enabled = value;
            }
        }

        protected virtual void Awake()
        {
            int[] values = (int[])System.Enum.GetValues(typeof(InputAction));
            inputStates = new InputState[values.Length];
            for(int i = 0; i < values.Length; i++)
            {
                inputStates[values[i]] = new InputState();
            }

			ToggleTouchInterface(false);
        }

        protected virtual void Start()
        {
			if(!allowCreationOfTouchControls)
			{
				return;
			}

            collisionLayerMask = 1 << LayerMask.NameToLayer("UI");

			if(GameObject.Find("UICamera"))
			{
				uiCamera = GameObject.Find("UICamera").GetComponent<Camera>();
			}

			isTouchInterfaceEnabled = ProjectSettingsAsset.Instance.rexSettingsData.touchControlSettings.touchControlDisplay == RexTouchInput.TouchControlDisplay.AlwaysDisplay ? true : false;

			#if UNITY_ANDROID || UNITY_IPHONE
			if(ProjectSettingsAsset.Instance.rexSettingsData.touchControlSettings.touchControlDisplay == RexTouchInput.TouchControlDisplay.DisplayForMobile)
			{
				isTouchInterfaceEnabled = true;
			}
			#endif 

			RexTouchInput rexTouchInput = GameManager.Instance.player.GetComponent<RexTouchInput>();
			if(rexTouchInput && isTouchInterfaceEnabled)
			{
				if(touchInterfaceObject == null)
				{
					CreateTouchControls();
				}

				ToggleTouchInterface(isTouchInterfaceEnabled);
			}
        }

		public virtual void CreateTouchControls()
		{
			GameObject touchInterfacePrefab = ProjectSettingsAsset.Instance.rexSettingsData.touchControlSettings.touchControlPrefab;
			if(touchInterfacePrefab != null)
			{
				touchInterfaceObject = Instantiate(touchInterfacePrefab, transform.parent);
				touchInterfaceObject.transform.localPosition = Vector3.zero;
				touchInterfaceObject.name = "TouchInterface";
			}
			else
			{
				Debug.Log("TouchInputManager :: Cannot create touch controls; Touch Interface Prefab is not slotted.");
			}
		}

        protected virtual void OnDisable()
        {
            ClearInputs();
        }

        protected virtual void Update()
        {
            ClearInputs();

            Vector3 mousePosition = Vector3.zero;
            bool isTouchStartingThisFrame = true;

            //#if UNITY_IPHONE || UNITY_ANDROID
            if(Input.touchCount > 0 || Input.GetMouseButton(0))
            {
                if(Input.touchCount > 0)
                {
                    for(int i = 0; i < Input.touchCount; i++)
                    {
                        mousePosition = Input.touches[i].position;
                        isTouchStartingThisFrame = (Input.touches[i].phase == TouchPhase.Began) ? true : false;
                        CheckTouchAtPosition(mousePosition, isTouchStartingThisFrame);
                    }
                }
                else
                {
                    mousePosition = Input.mousePosition;
                    isTouchStartingThisFrame = (Input.GetMouseButtonDown(0)) ? true : false;
                    CheckTouchAtPosition(mousePosition, isTouchStartingThisFrame);
                }
            }
            //#endif
        }

        public virtual void ToggleTouchInterface(bool willShow)
        {
			if(!isTouchInterfaceEnabled && willShow)
			{
				return;
			}

            if(touchInterfaceObject == null)
            {
                touchInterfaceObject = GameObject.Find("TouchInterface");
            }

			if(touchInterfaceObject == null && willShow)
			{
				CreateTouchControls();
			}

            if(touchInterfaceObject != null)
            {
                touchInterfaceObject.SetActive(willShow);
            }
        }

        protected virtual void CheckTouchAtPosition(Vector3 position, bool isTouchStartingThisFrame = false)
        {
			if(uiCamera == null)
			{
				return;
			}

			RaycastHit hit;
			Ray ray = uiCamera.ScreenPointToRay(position - new Vector3(0.0f, 0.0f, 0.1f));
			Physics.Raycast(ray, out hit, 10.0f, collisionLayerMask);

            InputState input;

			if((hit.collider && hit.collider.name == buttonNames.actionButtons.jump))
            {
                input = GetInput(InputAction.Jump);
            }
			else if((hit.collider && hit.collider.name == buttonNames.actionButtons.attack))
            {
                input = GetInput(InputAction.Attack);
            }
			else if((hit.collider && hit.collider.name == buttonNames.actionButtons.subAttack))
            {
                input = GetInput(InputAction.SubAttack);
            }
			else if((hit.collider && hit.collider.name == buttonNames.actionButtons.subAttack_2))
			{
				input = GetInput(InputAction.SubAttack_2);
			}
			else if((hit.collider && hit.collider.name == buttonNames.actionButtons.subAttack_3))
			{
				input = GetInput(InputAction.SubAttack_3);
			}
			else if((hit.collider && hit.collider.name == buttonNames.actionButtons.dash))
			{
				input = GetInput(InputAction.Dash);
			}
			else if((hit.collider && hit.collider.name == buttonNames.actionButtons.run))
			{
				input = GetInput(InputAction.Run);
			}
			else if((hit.collider && hit.collider.name == buttonNames.actionButtons.misc_1))
			{
				input = GetInput(InputAction.Misc_1);
			}
			else if((hit.collider && hit.collider.name == buttonNames.actionButtons.misc_2))
			{
				input = GetInput(InputAction.Misc_2);
			}
            else input = null;

            if(input != null)
            {
                input.SetButton(ButtonState.On | (isTouchStartingThisFrame ? ButtonState.Down : ButtonState.Off));
            }

			if((hit.collider && hit.collider.name == buttonNames.dPad.left))
            {
                GetInput(InputAction.MoveHorizontal).SetAxis(-1.0f);
            }
			else if((hit.collider && hit.collider.name == buttonNames.dPad.right))
            {
                GetInput(InputAction.MoveHorizontal).SetAxis(1.0f);
            }
			else if((hit.collider && hit.collider.name == buttonNames.dPad.up))
            {
                GetInput(InputAction.MoveVertical).SetAxis(1.0f);
            }
			else if((hit.collider && hit.collider.name == buttonNames.dPad.down))
            {
                GetInput(InputAction.MoveVertical).SetAxis(-1.0f);
            }
			else if((hit.collider && hit.collider.name == buttonNames.dPad.upLeft))
			{
				GetInput(InputAction.MoveHorizontal).SetAxis(-1.0f);
				GetInput(InputAction.MoveVertical).SetAxis(1.0f);
			}
			else if((hit.collider && hit.collider.name == buttonNames.dPad.upRight))
			{
				GetInput(InputAction.MoveHorizontal).SetAxis(1.0f);
				GetInput(InputAction.MoveVertical).SetAxis(1.0f);
			}
			else if((hit.collider && hit.collider.name == buttonNames.dPad.downLeft))
			{
				GetInput(InputAction.MoveHorizontal).SetAxis(-1.0f);
				GetInput(InputAction.MoveVertical).SetAxis(-1.0f);
			}
			else if((hit.collider && hit.collider.name == buttonNames.dPad.downRight))
			{
				GetInput(InputAction.MoveHorizontal).SetAxis(1.0f);
				GetInput(InputAction.MoveVertical).SetAxis(-1.0f);
			}
        }

        public virtual bool GetButton(int playerId, InputAction action)
        {
            return (inputStates[(int)action].buttonState & ButtonState.On) != 0;
        }

        public virtual bool GetButtonDown(int playerId, InputAction action)
        {
            return (inputStates[(int)action].buttonState & ButtonState.Down) != 0;
        }

        public virtual bool GetButtonUp(int playerId, InputAction action)
        {
            return (inputStates[(int)action].buttonState & ButtonState.Up) != 0;
        }

        public virtual float GetAxis(int playerId, InputAction action)
        {
            return inputStates[(int)action].axisValue;
        }

        private InputState GetInput(InputAction action)
        {
            return inputStates[(int)action];
        }

        private void ClearInputs()
        {
            for(int i = 0; i < inputStates.Length; i++)
            {
                inputStates[i].Clear();
            }
        }

        private class InputState
        {
            public float axisValue;
            public ButtonState buttonState;

            public void SetAxis(float value)
            {
                axisValue = value;
                buttonState = value >= 1f || value <= -1f ? ButtonState.On : ButtonState.Off;
            }

            public void SetButton(ButtonState state)
            {
                this.buttonState = state;
                if((state & ButtonState.On) == 0) axisValue = 0f;
            }

            public void Clear()
            {
                axisValue = 0f;
                buttonState = ButtonState.Off;
            }
        }

        [System.Flags]
        private enum ButtonState
        {
            Off = 0,
            On = 1,
            Down = 2,
            Up = 3
        }
    }
}