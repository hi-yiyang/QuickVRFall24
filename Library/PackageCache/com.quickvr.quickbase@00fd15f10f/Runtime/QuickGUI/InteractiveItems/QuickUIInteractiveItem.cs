using System;
using System.Collections.Generic;
using System.Collections;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace QuickVR 
{
	
    // This class should be added to any gameobject in the scene
    // that should react to input based on the user's cursor.
    // It contains events that can be subscribed to by classes that
    // need to know about input specifics to this gameobject.
    
	public class QuickUIInteractiveItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {

		#region PUBLIC PARAMETERS

		public string _description = "Item Description";

		#endregion

		#region PROTECTED PARAMETERS

		protected bool _isOver;	            //Indicates that the cursor is over this interactive item
		protected bool _isDown;	            //Indicates that the Trigger assigned to the cursor has been pressed on this interactive item
        protected float _timeOver = 0.0f;   //Indicates how much time the cursor is over the interactive item

        protected Coroutine _coTimeOver = null;

        #endregion

		#region EVENTS

        public event Action OnOver;	// Called when the cursor moves over this object
        public event Action OnOut;  // Called when the cursor leaves this object
        public event Action OnUp;   // Called when Fire1 is released whilst the cursor is over this object.
        public event Action OnDown; // Called when Fire1 is pressed whilst the cursor is over this object.

        public delegate void ItemAction(QuickUIInteractiveItem item);
        public static event ItemAction OnInteractiveItemSelected;

        public delegate void ButtonAction(Button button);
        public static event ButtonAction OnButtonSelected;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void OnDisable() {
			_isOver = _isDown = false;
            _timeOver = 0.0f;
		}

		#endregion

		#region GET AND SET

		public virtual bool IsOver() {
			return _isOver;
		}

		public virtual bool IsDown() {
			return _isDown;
		}

        public virtual float GetTimeOver()
        {
            return _timeOver;
        }

        public virtual bool IsInteractable()
        {
            return true;
        }

        #endregion

        #region UPDATE

        //Detect if the Cursor starts to pass over the GameObject
        public void OnPointerEnter(PointerEventData pointerEventData)
        {
            if (IsInteractable())
            {
                Over();
            }
        }

        //Detect when Cursor leaves the GameObject
        public void OnPointerExit(PointerEventData pointerEventData)
        {
            if (IsInteractable())
            {
                Out();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (IsInteractable())
            {
                Down();

                if (OnInteractiveItemSelected != null)
                {
                    OnInteractiveItemSelected(this);
                }

                if (OnButtonSelected != null)
                {
                    Button b = GetComponent<Button>();
                    if (b)
                    {
                        OnButtonSelected(b);
                    }
                }
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (IsInteractable())
            {
                Up();
            }
        }

        // The below functions are called by the VRRayCaster when the appropriate input is detected.
        // They in turn call the appropriate events should they have subscribers.
        public virtual void Over() 
        {
            _isOver = true;
            if (_coTimeOver != null)
            {
                StopCoroutine(_coTimeOver);
            }
            _timeOver = 0;
            _coTimeOver = StartCoroutine(CoUpdateTimeOver());

            if (OnOver != null) OnOver();
        }

		public virtual void Out() {
            _isOver = false;

            if (OnOut != null) OnOut();
        }

		public virtual void Down() {
            if (!_isDown)
            {
                _isDown = true;

                if (OnDown != null) OnDown();
            }
		}

		public virtual void Up() {
            if (_isDown)
            {
                _isDown = false;

                if (OnUp != null) OnUp();
            }
        }

        protected virtual IEnumerator CoUpdateTimeOver()
        {
            while (_isOver)
            {
                _timeOver += Time.deltaTime;
                yield return null;
            }
        }

        protected virtual void Update()
        {
            if (IsInteractable())
            {
                //[HACK] This is in order to account for OpenVRLoader, because it does not takes into account the Unity input system. 
                if (InputManager.GetButtonDown(InputManager.DEFAULT_BUTTON_CONTINUE))
                {
                    if (_isOver && !_isDown)
                    {
                        Down();
                    }
                }
                else if (InputManager.GetButtonUp(InputManager.DEFAULT_BUTTON_CONTINUE))
                {
                    if (_isOver && _isDown)
                    {
                        Up();
                    }
                }
            }
        }

        #endregion

    }

}