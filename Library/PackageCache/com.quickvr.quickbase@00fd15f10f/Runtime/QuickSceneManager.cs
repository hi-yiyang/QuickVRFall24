﻿using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

using System.Collections;
using System.Collections.Generic;

namespace QuickVR
{

    public class QuickSceneManager : MonoBehaviour
    {

        #region PUBLIC ATTRIBUTES

        public enum SceneState
        {
            Loading,    //The scene is loading
            Preloaded,  //The scene is loaded, but remains in background, meaning that the root gameobjects are not active
            Loaded,     //The scene is loaded and the root gameobjects are active
            Unloading,  //The scene is unloading
        }

        #endregion

        #region PROTECTED ATTRIBUTES

        protected static Dictionary<string, SceneData> _loadedScenes = new Dictionary<string, SceneData>();

        protected class SceneData
        {
            public Scene _scene;
            public SceneState _state;
            public List<bool> _rootGOActive = new List<bool>();
        }


        protected Scene _logicScene;

        #endregion

        #region CREATION AND DESTRUCTION

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        protected static void Init()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            string sName = arg0.name;
            Debug.Log("LOADED SCENE = " + sName);

            if (!_loadedScenes.ContainsKey(sName))
            {
                SceneData sData = new SceneData();
                sData._scene = arg0;
                _loadedScenes[sName] = sData;
            }

            _loadedScenes[sName]._state = SceneState.Loaded;
        }

        //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        //protected static void Init()
        //{
        //    for (int i = 0; i < SceneManager.sceneCount; i++)
        //    {
        //        SceneData sData = new SceneData();
        //        sData._scene = SceneManager.GetSceneAt(i);
        //        sData._state = SceneState.Loaded;
        //        _loadedScenes[SceneManager.GetSceneAt(i).name] = sData;
        //    }
        //}

        #endregion

        #region GET AND SET

        public virtual SceneState? GetSceneState(string sceneName)
        {
            if (_loadedScenes.TryGetValue(sceneName, out SceneData sceneData))
            {
                return sceneData._state;
            }

            return null;
        }

        public virtual Scene GetSceneByName(string sceneName)
        {
            Scene result = new Scene();
            if (_loadedScenes.TryGetValue(sceneName, out SceneData sData))
            {
                result = sData._scene;
            }

            return result;
        }

        protected virtual void SetSceneState(string sceneName, SceneState newState)
        {
            if (newState == SceneState.Preloaded)
            {
                Scene scene = GetSceneByName(sceneName);
                foreach (GameObject go in scene.GetRootGameObjects())
                {
                    _loadedScenes[sceneName]._rootGOActive.Add(go.activeSelf);
                    go.SetActive(false);
                }
            }
            else if (newState == SceneState.Loaded)
            {
                Scene scene = GetSceneByName(sceneName);
                GameObject[] rootObjects = scene.GetRootGameObjects();
                List<bool> preloadObjectsState = _loadedScenes[sceneName]._rootGOActive;

                for (int i = 0; i < preloadObjectsState.Count; i++)
                {
                    rootObjects[i].SetActive(preloadObjectsState[i]);
                }
            }

            _loadedScenes[sceneName]._state = newState;
        }

        public virtual bool IsSceneValid(string sceneName)
        {
            return SceneManager.GetSceneByName(sceneName).IsValid();
        }

        public virtual bool IsSceneLoaded(string sceneName)
        {
            SceneState? sState = GetSceneState(sceneName);

            return sState.HasValue ? sState == SceneState.Loaded : false;
        }

        public virtual void PreLoadSceneAdditive(string sceneName)
        {
            LoadSceneAdditive(sceneName, false, false);
        }

        public virtual void PreLoadSceneAdditiveAsync(string sceneName)
        {
            LoadSceneAdditive(sceneName, false, true);
        }

        public virtual void LoadSceneAdditive(string sceneName)
        {
            LoadSceneAdditive(sceneName, true, false);
        }

        public virtual void LoadSceneAdditiveAsync(string sceneName)
        {
            LoadSceneAdditive(sceneName, true, true);
        }

        protected virtual void LoadSceneAdditive(string sceneName, bool allowSceneActivation, bool isAsync)
        {
            //We can only Load a scene if it has not yet been loaded, or it is is preloaded
            if (
                !_loadedScenes.TryGetValue(sceneName, out SceneData sceneData) || 
                allowSceneActivation && (sceneData._state == SceneState.Loading || sceneData._state == SceneState.Preloaded)
                )
            {
                StartCoroutine(CoLoadSceneAdditive(sceneName, allowSceneActivation, isAsync));
            }
            else
            {
                QuickVRManager.LogWarning(sceneName + " is already " + SceneState.Loaded + ". Please, unload it first");
            }
        }

        public virtual void UnloadScenes(string[] sceneNames)
        {
            foreach (string s in sceneNames)
            {
                UnloadScene(s);
            }
        }

        public virtual void UnloadScene(Scene scene)
        {
            if (scene.IsValid())
            {
                StartCoroutine(CoUnloadScene(scene));
            }
        }

        public virtual void UnloadScene(string sceneName)
        {
            //The scene is send back to background
            if (_loadedScenes.TryGetValue(sceneName, out SceneData sceneData) && sceneData._state != SceneState.Unloading)
            {
                StartCoroutine(CoUnloadScene(GetSceneByName(sceneName)));
            }
        }

        public virtual Scene GetActiveScene()
        {
            return SceneManager.GetActiveScene();
        }

        public virtual void ActivateScene(string sceneName, bool disableCameras = true)
        {
            if (!_loadedScenes.TryGetValue(sceneName, out SceneData sceneData) || sceneData._state != SceneState.Unloading)
            {
                StartCoroutine(CoActivateScene(sceneName, disableCameras));
            }
            else
            {
                QuickVRManager.LogWarning(sceneName + " cannot be the Active scene because it is " + SceneState.Unloading);
            }
        }

        public virtual Scene GetLogicScene()
        {
            return _logicScene;
        }

        public virtual void SetLogicScene(string sceneName)
        {
            if (!_loadedScenes.TryGetValue(sceneName, out SceneData sceneData) || sceneData._state != SceneState.Unloading)
            {
                StartCoroutine(CoSetLogicScene(sceneName));
            }
            else
            {
                QuickVRManager.LogWarning(sceneName + " cannot be the Logic scene because it is " + SceneState.Unloading);
            }
        }

        public virtual void SetLogicScene(Scene scene)
        {
            SetLogicScene(scene.name);
        }

        #endregion

        #region UPDATE

        protected virtual IEnumerator CoLoadSceneAdditive(string sceneName, bool allowSceneActivation, bool isAsync)
        {
            if (_loadedScenes.TryGetValue(sceneName, out SceneData sceneData))
            {
                while (sceneData._state == SceneState.Loading)
                {
                    yield return null;
                }
            }
            else
            {
                SceneData sData = new SceneData();
                _loadedScenes[sceneName] = sData;

                if (Application.CanStreamedLevelBeLoaded(sceneName))
                {
                    if (isAsync)
                    {
                        yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                    }
                    else
                    {
                        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
                    }
                }
                else
                {
                    AsyncOperationHandle<IList<IResourceLocation>> test = Addressables.LoadResourceLocationsAsync(sceneName);
                    yield return test;
                    if (test.Result.Count > 0)
                    {
                        AsyncOperationHandle<SceneInstance> opLoadScene = Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                        yield return opLoadScene;
                    }
                    else
                    {
                        //FAIL
                    }
                }

                sData._scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            }

            SetSceneState(sceneName, allowSceneActivation ? SceneState.Loaded : SceneState.Preloaded);
        }

        protected virtual IEnumerator CoWaitForSceneLoaded(string sceneName)
        {
            //Ensure that we are activating a loaded scene
            if (!_loadedScenes.ContainsKey(sceneName))
            {
                LoadSceneAdditiveAsync(sceneName);
            }

            //Wait for the scene to be loaded, in case we are trying to activate a scene 
            //that is not loaded yet. 
            while (_loadedScenes[sceneName]._state == SceneState.Loading)
            {
                yield return null;
            }
            SetSceneState(sceneName, SceneState.Loaded);
        }

        protected virtual IEnumerator CoSetLogicScene(string sceneName)
        {
            yield return StartCoroutine(CoWaitForSceneLoaded(sceneName));

            _logicScene = GetSceneByName(sceneName);
        }

        protected virtual IEnumerator CoActivateScene(string sceneName, bool disableCameras)
        {
            yield return StartCoroutine(CoWaitForSceneLoaded(sceneName));

            //At this point, the scene is loaded. Activate it. 
            Scene scene = GetSceneByName(sceneName);
            SceneManager.SetActiveScene(scene);
            LightProbes.Tetrahedralize();

            if (disableCameras)
            {
                //Deactivate any present camera in the geometry scene 
                foreach (Camera cam in Camera.allCameras)
                {
                    if (cam.tag != "MainCamera")
                    {
                        cam.gameObject.SetActive(false);
                    }
                }
            }
        }

        protected virtual IEnumerator CoUnloadScene(Scene scene)
        {
            //SetSceneState(sceneName, SceneState.Unloading);

            _loadedScenes.Remove(scene.name);
            yield return SceneManager.UnloadSceneAsync(scene);
        }

        #endregion

    }
}
