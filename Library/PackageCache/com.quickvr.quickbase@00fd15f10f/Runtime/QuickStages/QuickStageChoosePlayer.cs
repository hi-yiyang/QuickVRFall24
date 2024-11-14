using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;

namespace QuickVR
{
    
    public class QuickStageChoosePlayer : QuickStageBase
    {

        #region PUBLIC ATTRIBUTES

        public float _rotationSpeed = 25.0f;
        public List<GameObject> _players = new List<GameObject>();

        public QuickUserGUI _pfGUI = null;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected QuickUserGUI _gui = null;

        protected bool _isPlayerSelected = false;

        public static GameObject _selectedPlayer
        {
            get; protected set;
        }
        public static int _selectedPlayerID
        {
            get; protected set;
        }

        protected Transform _playerExhibitor = null;

        #endregion

        #region CONSTANTS

        protected const string BUTTON_ARROW_BACK = "ButtonArrowBack";
        protected const string BUTTON_ARROW_NEXT = "ButtonArrowNext";
        protected const string BUTTON_SELECT = "ButtonSelect";

        protected const string PLAYER_EXHIBITOR_NAME = "__PlayerExhibitor__";

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Start()
        {
            _gui = GetComponentInChildren<QuickUserGUI>(true);

            if (!_gui)
            {
                _gui = Instantiate(_pfGUI);
                _gui.transform.SetParent(transform, false);
                _gui.transform.ResetTransformation();
            }

            _gui.gameObject.SetActive(false);
            
            _playerExhibitor = _gui.transform.Find(PLAYER_EXHIBITOR_NAME);

            base.Start();
        }

        #endregion

        #region GET AND SET

        protected virtual void AddGUIActions()
        {
            _gui.GetButton(BUTTON_ARROW_BACK).OnDown += ActionArrowBack;
            _gui.GetButton(BUTTON_ARROW_NEXT).OnDown += ActionArrowNext;
            _gui.GetButton(BUTTON_SELECT).OnDown += ActionSelect;
        }

        protected virtual void RemoveGUIActions()
        {
            _gui.GetButton(BUTTON_ARROW_BACK).OnDown -= ActionArrowBack;
            _gui.GetButton(BUTTON_ARROW_NEXT).OnDown -= ActionArrowNext;
            _gui.GetButton(BUTTON_SELECT).OnDown -= ActionSelect;
        }

        protected virtual void LoadSelectedPlayer()
        {
            if (_selectedPlayer)
            {
                _selectedPlayer.SetActive(false);
            }

            _selectedPlayer = _playerExhibitor.GetChild(_selectedPlayerID).gameObject;
            _selectedPlayer.SetActive(true);
        }

        protected virtual int GetInitialSelectedPlayerID()
        {
            return 0;
        }

        #endregion

        #region ACTIONS

        protected virtual void ActionArrowBack()
        {
            _selectedPlayerID--;
            if (_selectedPlayerID < 0)
            {
                _selectedPlayerID = _playerExhibitor.childCount - 1;
            }

            LoadSelectedPlayer();
        }

        protected virtual void ActionArrowNext()
        {
            _selectedPlayerID = (_selectedPlayerID + 1) % _playerExhibitor.childCount;

            LoadSelectedPlayer();
        }

        protected virtual void ActionSelect()
        {
            _isPlayerSelected = true;
            //SettingsChessMe.SetPlayerSelectedPrefabName(_selectedPlayerID);
        }

        protected virtual void SetGUIPosition()
        {
            Animator animator = QuickSingletonManager.GetInstance<QuickVRManager>().GetAnimatorTarget();
            Vector3 fwd = animator.transform.forward;
            //_gui.transform.position = animator.GetBoneTransform(HumanBodyBones.Head).position + fwd * 2;
            _gui.transform.position = animator.transform.position + fwd * 2;
            _gui.transform.forward = fwd;
        }

        #endregion

        #region UPDATE

        protected override IEnumerator CoUpdate()
        {
            _interactionManager._interactorHandRight.SetInteractorEnabled(InteractorType.UI, true);
            _gui._followCamera = false;
            _gui.gameObject.SetActive(true);

            SetGUIPosition();

            InstantiatePlayers();

            //Try to load the selected player from the last session
            //_selectedPlayerID = SettingsChessMe.GetPlayerSelectedPrefabName();
            _selectedPlayerID = GetInitialSelectedPlayerID();
            LoadSelectedPlayer();

            AddGUIActions();

            while (!_isPlayerSelected)
            {
                _playerExhibitor.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime);
                yield return null;
            }

            _interactionManager._interactorHandRight.SetInteractorEnabled(InteractorType.UI, false);

            _gui.gameObject.SetActive(false);

            RemoveGUIActions();
        }

        protected virtual void InstantiatePlayers()
        {
            foreach (GameObject p in _players)
            {
                GameObject go = Instantiate(p);
                go.transform.parent = _playerExhibitor;
                go.transform.ResetTransformation();
                go.SetActive(false);
                go.name = p.name;
            }
        }

        #endregion

    }

}


