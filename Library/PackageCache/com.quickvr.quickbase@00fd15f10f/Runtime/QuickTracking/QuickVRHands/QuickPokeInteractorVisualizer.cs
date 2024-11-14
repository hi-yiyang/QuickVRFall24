using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR.Interaction.Toolkit;

namespace QuickVR
{

    [RequireComponent(typeof(XRPokeInteractor))]
    public class QuickPokeInteractorVisualizer : MonoBehaviour
    {

        #region PUBLIC ATTRIBUTES

        public bool _debug = false;

        #endregion

        protected XRPokeInteractor _pokeInteractor = null;
        protected GameObject _debugObject = null;

        protected virtual void Awake()
        {
            _pokeInteractor = GetComponent<XRPokeInteractor>();
            _debugObject = transform.CreateChild("__Debug__").gameObject;
            _debugObject.GetOrCreateComponent<MeshRenderer>().SetMesh(QuickUtils.GetUnityPrimitiveMesh(PrimitiveType.Sphere));
        }

        protected virtual void Update()
        {
            _debugObject.gameObject.SetActive(_debug);

            if (_debugObject.gameObject.activeSelf)
            {
                float r = _pokeInteractor.pokeHoverRadius;
                _debugObject.transform.localScale = Vector3.one * r;
            }
        }

    }

}


