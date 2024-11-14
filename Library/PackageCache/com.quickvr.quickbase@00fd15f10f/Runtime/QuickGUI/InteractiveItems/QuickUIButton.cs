using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace QuickVR
{

    public class QuickUIButton : QuickUIInteractiveItem
    {

        #region PUBLIC ATTRIBUTES

        public Material _buttonBGMaterial = null;
        
        public Color _colorNormal = Color.white;
        public Color _colorSelected = Color.cyan;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected Selectable _button = null;
        //protected Image _buttonBG = null;
        protected TextMeshProUGUI _label = null;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            _button = GetComponent<Selectable>();
            _label = _button.GetComponentInChildren<TextMeshProUGUI>();

            ColorBlock cBlock = _button.colors;
            cBlock.highlightedColor = _colorSelected;

            _button.colors = cBlock;

            //_buttonBG = gameObject.GetOrCreateComponent<Image>();
            
            //if (!_buttonBGMaterial)
            //{
            //    _buttonBGMaterial = Resources.Load<Material>("GUIText");
            //}
            //_buttonBG.material = new Material(_buttonBGMaterial);
            //_buttonBG.color = _colorNormal;
        }

        protected override void OnDisable()
        {
            //_buttonBG.color = _colorNormal;

            base.OnDisable();
        }

        #endregion

        #region GET AND SET

        public override bool IsInteractable()
        {
            return _button.interactable;
        }

        #endregion

        #region UPDATE

        public override void Over()
        {
            base.Over();

            //_buttonBG.color = _colorSelected;
        }

        public override void Out()
        {
            base.Out();

            //_buttonBG.color = _colorNormal;
        }

        protected override void Update()
        {
            float tAlpha = IsInteractable() ? _button.colors.normalColor.a : _button.colors.disabledColor.a;
            _label.color = new Color(_label.color.r, _label.color.g, _label.color.b, tAlpha);
            
            base.Update();

            //if (_buttonBG.material)
            //{
            //    _buttonBG.material.color = _buttonBG.color;
            //}
        }

        #endregion

    }

}


