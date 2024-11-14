using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using System.IO;

using AltProg.CleanEmptyDir;

namespace QuickVR
{

    public static class QuickVRMenu
    {
        
        private static void AddUniqueComponent<T>() where T : Component
        {
            if (Selection.activeTransform == null) return;

            if (!Selection.activeGameObject.GetComponent<T>())
            {
                Selection.activeGameObject.AddComponent<T>();
            }
        }

        #region TRACKING COMPONENTS

        private const string MENU_QUICK_UNITY_VR = "QuickVR/Tracking/QuickUnityVR";

        [MenuItem(MENU_QUICK_UNITY_VR)]
        private static void AddQuickUnityVR()
        {
            AddUniqueComponent<QuickUnityVR>();
        }

        [MenuItem(MENU_QUICK_UNITY_VR, true)]
        private static bool ValidateAddQuickUnityVR()
        {
            GameObject go = Selection.activeGameObject;
            return go ? go.GetComponent<Animator>() && !go.GetComponent<QuickUnityVR>() : false;
        }

        #endregion

        #region TOOLS

        private const string MENU_ENFORCE_TPOSE = "QuickVR/EnforceTPose";
        private const string MENU_SAVE_MESH_POSE = "QuickVR/SaveMeshPose";

        [MenuItem(MENU_ENFORCE_TPOSE)]
        private static void EnforceTPose()
        {
            Selection.activeGameObject.GetComponent<Animator>().EnforceTPose();
        }

        [MenuItem(MENU_ENFORCE_TPOSE, true)]
        private static bool ValidateEnforceTPose()
        {
            GameObject go = Selection.activeGameObject;
            return go ? go.GetComponent<Animator>() : false;
        }

        [MenuItem(MENU_SAVE_MESH_POSE)]
        private static void SaveMeshPose()
        {
            GameObject go = Selection.activeGameObject.GetComponentInParent<Animator>().gameObject;

            Vector3 tmpPos = go.transform.position;
            Quaternion tmpRot = go.transform.rotation;
            Vector3 tmpScale = go.transform.localScale;

            go.transform.ResetTransformation();

            SkinnedMeshRenderer[] smRenderers = go.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer smRenderer in smRenderers)
            {
                // Apply bone changes to mesh
                SaveMeshPose(smRenderer);
            }

            //Restore bones bindPoses
            foreach (SkinnedMeshRenderer smRenderer in smRenderers)
            {
                RestoreBindPose(smRenderer, smRenderer.rootBone);
            }

            go.transform.position = tmpPos;
            go.transform.rotation = tmpRot;
            go.transform.localScale = tmpScale;

            if (PrefabUtility.GetPrefabAssetType(go) == PrefabAssetType.Regular)
            {
                PrefabUtility.ApplyPrefabInstance(go, InteractionMode.AutomatedAction);
            }

            AssetDatabase.SaveAssets();
        }

        [MenuItem(MENU_SAVE_MESH_POSE, true)]
        private static bool ValidateSaveMeshPose()
        {
            GameObject go = Selection.activeGameObject;
            return go ? go.GetComponentInParent<Animator>() : false;
        }

        private static void SaveMeshPose(SkinnedMeshRenderer smr)
        {

            //Mesh m = GameObject.Instantiate<Mesh>(smr.sharedMesh);

            //Vector3[] vertices = m.vertices;
            //BoneWeight[] boneWeights = m.boneWeights;

            //for (int i = 0; i < vertices.Length; i++)
            //{
            //    Vector3 vPosLS = vertices[i];   //The vertex position in local space. 
            //    Vector3 weightedPosition = Vector3.zero;

            //    // Calculate new vertex position based on updated bone rotations
            //    for (int j = 0; j < 4; j++)
            //    {
            //        int boneIndex;
            //        if (j == 0) boneIndex = boneWeights[i].boneIndex0;
            //        else if (j == 1) boneIndex = boneWeights[i].boneIndex1;
            //        else if (j == 2) boneIndex = boneWeights[i].boneIndex2;
            //        else boneIndex = boneWeights[i].boneIndex3;

            //        float boneWeight;
            //        if (j == 0) boneWeight = boneWeights[i].weight0;
            //        else if (j == 1) boneWeight = boneWeights[i].weight1;
            //        else if (j == 2) boneWeight = boneWeights[i].weight2;
            //        else boneWeight = boneWeights[i].weight3;

            //        if (boneWeight > 0)
            //        {
            //            Transform bone = smr.bones[boneIndex];
            //            Matrix4x4 bindPose = m.bindposes[boneIndex];
            //            Matrix4x4 bonePose = Matrix4x4.TRS(bone.position, bone.rotation, bone.localScale);
            //            Matrix4x4 diffPose = bonePose * bindPose.inverse;
            //            weightedPosition += bone.TransformPoint(bone.InverseTransformPoint(diffPose * vPosLS)) * boneWeight;
            //        }
            //    }

            //    vertices[i] = weightedPosition;
            //}

            //m.vertices = vertices;

            Mesh m = new Mesh();
            smr.BakeMesh(m);

            m.bindposes = smr.sharedMesh.bindposes;
            m.boneWeights = smr.sharedMesh.boneWeights;

            string path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(smr.sharedMesh)) + "/" + smr.sharedMesh.name + ".asset";
            while (AssetDatabase.LoadAssetAtPath<Object>(path) != null)
            {
                path = AssetDatabase.GenerateUniqueAssetPath(path);
            }

            AssetDatabase.CreateAsset(m, path);
            AssetDatabase.SaveAssets();

            smr.sharedMesh = m;

            smr.sharedMesh.RecalculateNormals();
            smr.sharedMesh.RecalculateBounds();
        }

        private static void RestoreBindPose(SkinnedMeshRenderer smRenderer, Transform tBone)
        {
            int boneIndex = GetBoneIndex(smRenderer, tBone);
            if (boneIndex != -1)
            {
                Matrix4x4 bindPose = smRenderer.sharedMesh.bindposes[boneIndex].inverse;

                tBone.position = bindPose.GetColumn(3);
                tBone.rotation = Quaternion.LookRotation(bindPose.GetColumn(2), bindPose.GetColumn(1));
                tBone.localScale = new Vector3(
                    bindPose.GetColumn(0).magnitude,
                    bindPose.GetColumn(1).magnitude,
                    bindPose.GetColumn(2).magnitude
                );

                for (int i = 0; i < tBone.childCount; i++)
                {
                    RestoreBindPose(smRenderer, tBone.GetChild(i));
                }
            }
        }

        private static int GetBoneIndex(SkinnedMeshRenderer smRenderer, Transform tBone)
        {
            int result = -1;

            for (int i = 0; (i < smRenderer.bones.Length) && (result == -1); i++)
            {
                if (tBone == smRenderer.bones[i])
                {
                    result = i;
                }
            }

            return result;
        }

        [MenuItem("QuickVR/PlayerPrefs")]
        private static void GetWindowPlayerPrefs()
        {
            EditorWindow.GetWindow<QuickPlayerPrefsWindowEditor>();

            string path = "Assets/QuickVRCfg/Resources/QuickSettingsCustom.asset";
            QuickSettingsAsset settings = AssetDatabase.LoadAssetAtPath<QuickSettingsAsset>(path);
            if (!settings)
            {
                settings = ScriptableObject.CreateInstance<QuickSettingsAsset>();
                QuickUtilsEditor.CreateDataFolder("QuickVRCfg/Resources");
                AssetDatabase.CreateAsset(settings, path);
                AssetDatabase.SaveAssets();
            }

            QuickPlayerPrefs.Init();

            //Check if the base settings are defined
            SettingsBase.SetSubjectID(SettingsBase.GetSubjectID());
            SettingsBase.SetGender(SettingsBase.GetGender());
            SettingsBase.SetLanguage(SettingsBase.GetLanguage());
        }

        [MenuItem("QuickVR/MetallicMapFixer")]
        private static void GetWindowMetallicMapFixer()
        {
            EditorWindow.GetWindow<QuickMetallicMapFixer>();
        }

        [MenuItem("QuickVR/ReferenceFixer")]
        private static void GetWindowReferenceFixer()
        {
            EditorWindow.GetWindow<QuickReferenceFixer>();
        }

        [MenuItem("QuickVR/SkeletonFixer")]
        private static void GetWindowSkeletonFixer()
        {
            EditorWindow.GetWindow<QuickSkeletonFixer>();
        }

        [MenuItem("QuickVR/CleanEmptyDir")]
        private static void ShowWindow()
        {
            MainWindow w = EditorWindow.GetWindow<MainWindow>();
            w.titleContent.text = "Clean";
        }

        #endregion

        //#region BODY TRACKING COMPONENTS

        //[MenuItem(MENU_QUICKVR_ROOT + "/" + MENU_QUICKVR_BODYTRACKING + "/" + "QuickMotive")]
        //static void AddQuickMotive()
        //{
        //    AddUniqueComponent<QuickMotiveBodyTracking>();
        //}

        //[MenuItem(MENU_QUICKVR_ROOT + "/" + MENU_QUICKVR_BODYTRACKING + "/" + "QuickKinect")]
        //static void AddQuickKinnect()
        //{
        //    AddUniqueComponent<QuickKinectBodyTracking>();
        //}

        //[MenuItem(MENU_QUICKVR_ROOT + "/" + MENU_QUICKVR_BODYTRACKING + "/" + "QuickNeuron")]
        //static void AddQuickNeuron()
        //{
        //    AddUniqueComponent<QuickNeuronBodyTracking>();
        //}

        //[MenuItem(MENU_QUICKVR_ROOT + "/" + MENU_QUICKVR_BODYTRACKING + "/" + "QuickDummy")]
        //static void AddQuickDummy()
        //{
        //    AddUniqueComponent<QuickDummyBodyTracking>();
        //}

        //#endregion

    }

}

public static class MatrixExtensions
{
    public static Quaternion ExtractRotation(this Matrix4x4 matrix)
    {
        Vector3 forward;
        forward.x = matrix.m02;
        forward.y = matrix.m12;
        forward.z = matrix.m22;
        Vector3 upwards;
        upwards.x = matrix.m01;
        upwards.y = matrix.m11;
        upwards.z = matrix.m21;
        return Quaternion.LookRotation(forward, upwards);
    }
    public static Vector3 ExtractPosition(this Matrix4x4 matrix)
    {
        Vector3 position;
        position.x = matrix.m03;
        position.y = matrix.m13;
        position.z = matrix.m23;
        return position;
    }
    public static Vector3 ExtractScale(this Matrix4x4 matrix)
    {
        Vector3 scale;
        scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
        scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
        scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
        return scale;
    }
}

