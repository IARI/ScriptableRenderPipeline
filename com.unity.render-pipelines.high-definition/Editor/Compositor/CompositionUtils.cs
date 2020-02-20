using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering.HighDefinition.Compositor;

using UnityEditor;

namespace UnityEditor.Rendering.HighDefinition.Compositor
{
    internal class CompositionUtils
    {
        static public void LoadDefaultCompositionGraph(CompositionManager compositor)
        {
            compositor.shader = UnityEditor.AssetDatabase.LoadAssetAtPath<Shader>(HDUtils.GetHDRenderPipelinePath() + "Runtime/Compositor/ShaderGraphs/DefaultCompositionGraph.shadergraph");
        }

        static public void RemoveAudioListeners(Camera camera)
        {
            var listener = camera.GetComponent<AudioListener>();
            if (listener)
            {
                CoreUtils.Destroy(listener);
            }
        }

        static public void SetDefaultCamera(CompositionManager compositor)
        {
            compositor.shader = UnityEditor.AssetDatabase.LoadAssetAtPath<Shader>(HDUtils.GetHDRenderPipelinePath() + "Runtime/Compositor/ShaderGraphs/DefaultCompositionGraph.shadergraph");
            var camera = CompositionManager.GetSceceCamera();
            if (camera != null)
            {
                var outputCamera = Object.Instantiate(camera);
                RemoveAudioListeners(outputCamera);
                outputCamera.name = "MainCompositorCamera";
                outputCamera.cullingMask = 0; // we don't want to render any 3D objects on the compositor camera
                compositor.outputCamera = outputCamera;
            }
        }

        static public void LoadOrCreateCompositionProfileAsset(CompositionManager compositor)
        {
            var shader = compositor.shader;
            var fullpath = AssetDatabase.GetAssetPath(shader);
            var path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(fullpath), System.IO.Path.GetFileNameWithoutExtension(shader.name)) + ".asset";

            CompositionProfile newProfile = AssetDatabase.LoadAssetAtPath<CompositionProfile>(path);

            if (newProfile == null)
            {
                Debug.Log($"Creating new composition profile asset at path: {path}");

                newProfile = ScriptableObject.CreateInstance<CompositionProfile>();

                // path = AssetDatabase.GenerateUniqueAssetPath(path);
                AssetDatabase.CreateAsset(newProfile, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.Log($"Loading composition profile from {path}");
            }
            compositor.profile = newProfile;
        }
    }
}
