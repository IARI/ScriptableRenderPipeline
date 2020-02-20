using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering.HighDefinition.Attributes;
using UnityEngine.Rendering.HighDefinition.Compositor;

using UnityEditor;
using UnityEditorInternal;

namespace UnityEditor.Rendering.HighDefinition.Compositor
{
    internal class CompositionLayerUI
    {
        static partial class TextUI
        {
            // main layer
            static public readonly GUIContent Resolution = EditorGUIUtility.TrTextContent("Resolution", "Specifies the resolution of this layer's render target. Lower resolution increases the performance at the expense of visual quality.");
            static public readonly GUIContent BufferFormat = EditorGUIUtility.TrTextContent("Format", "Specifies the color buffer format of this layer. ");
            static public readonly GUIContent OutputRenderer = EditorGUIUtility.TrTextContent("Output Renderer", "Redirects the output of this layer to the surface which is drawn by the selected mesh renderer. ");
            static public readonly GUIContent AOVs = EditorGUIUtility.TrTextContent("AOVs", "Specifies the AOVs . ");

            // Sub layer
            static public readonly GUIContent NameContent = EditorGUIUtility.TrTextContent("Layer Name", "Specifies the name of this layer.");
            static public readonly GUIContent Camera = EditorGUIUtility.TrTextContent("Source Camera", "Specifies the camera of the scene that will provide the content for this layer.");
            static public readonly GUIContent Image = EditorGUIUtility.TrTextContent("Source Image", "Specifies the image that will provide the content for this layer.");
            static public readonly GUIContent Video = EditorGUIUtility.TrTextContent("Source Video", "Specifies the video that will provide the content for this layer.");
            static public readonly GUIContent ClearDepth = EditorGUIUtility.TrTextContent("Clear Depth", "If enabled, the depth buffer will be cleared before rendering this layer.");
            static public readonly GUIContent ClearAlpha = EditorGUIUtility.TrTextContent("Clear Alpha", "If enabled, the alpha channel will be cleared before rendering this layer. If enabled, post processing will affect only the objects of this layer");
            static public readonly GUIContent ClearMode = EditorGUIUtility.TrTextContent("Clear Color", "To override the clear mode of this layer, activate the option by clicking on the check-box and then select the desired value.");
            static public readonly GUIContent AAMode = EditorGUIUtility.TrTextContent("Anti Aliasing", "To override the anti-aliasing mode, activate the option by clicking on the check-box and then select the desired value.");
            static public readonly GUIContent CullingMask = EditorGUIUtility.TrTextContent("Culling Mask", "To override the culling mask, activate the option by clicking on the check-box and then select the desired value.");
            static public readonly GUIContent VolumeMask = EditorGUIUtility.TrTextContent("Volume Mask", "Specifies the type of output variable in this layer. This option affects all cameras that re stacked in this layer.");
        }

        public static void DrawItemInList(Rect rect, SerializedCompositionLayer serialized, RenderTexture thumbnail = null, float aspectRatio = 1.0f)
        {
            bool isCameraStack = serialized.OutTarget.intValue == (int)CompositorLayer.OutputTarget.CameraStack;

            // Compute the desired indentation 
            {
                rect.x = isCameraStack ? rect.x + CompositorStyle.k_ListItemStackPading + 2 : rect.x + 2;
                rect.width = isCameraStack ? rect.width - CompositorStyle.k_ListItemStackPading : rect.width;
                rect.y += CompositorStyle.k_ListItemPading;
                rect.height = EditorGUIUtility.singleLineHeight;
            }

            if (thumbnail)
            {
                Rect newRect = rect;
                newRect.width = 20;
                EditorGUI.PropertyField(newRect, serialized.Show, GUIContent.none);
                rect.x += 20;
                Rect previewRect = rect;
                previewRect.width = CompositorStyle.k_ThumbnailSize * aspectRatio;
                previewRect.height = CompositorStyle.k_ThumbnailSize;
                EditorGUI.DrawPreviewTexture(previewRect, thumbnail);
                previewRect.x += previewRect.width + 5;
                EditorGUI.DrawTextureAlpha(previewRect, thumbnail);
                rect.x += previewRect.width * 2 + 12;
                rect.width -= (CompositorStyle.k_ThumbnailSize * 2 + 30);
                rect.y += 6;
                EditorGUI.LabelField(rect, serialized.LayerName.stringValue);
            }
            else
            {
                Rect newRect = rect;
                newRect.width = 20;
                EditorGUI.PropertyField(newRect, serialized.Show, GUIContent.none);
                newRect.x += 20;
                if (isCameraStack)
                {
                    Rect iconRect = newRect;
                    iconRect.width = CompositorStyle.k_IconSize;
                    iconRect.height = CompositorStyle.k_IconSize;
                    iconRect.y -= 5;
                    switch (serialized.InputLayerType.enumValueIndex)
                    {
                        case (int)CompositorLayer.LayerType.Camera:
                            if (CompositorStyle.cameraIcon)
                            {
                                GUI.DrawTexture(iconRect, CompositorStyle.cameraIcon);
                            }
                            break;
                        case (int)CompositorLayer.LayerType.Video:
                            if (CompositorStyle.videoIcon)
                            {
                                GUI.DrawTexture(iconRect, CompositorStyle.videoIcon);
                            }
                            break;
                        case (int)CompositorLayer.LayerType.Image:
                            if (CompositorStyle.imageIcon)
                            {
                                GUI.DrawTexture(iconRect, CompositorStyle.imageIcon);
                            }
                            break;
                        default:
                            // This will happen if someone adds a new layer type and does not update this switch statement
                            Debug.Log("Unknown layer type: Please add code here to draw this type of layer.");
                            break;
                    }
                    newRect.x += CompositorStyle.k_IconSize;
                }

                newRect.width = rect.width - 60 - 20;
                EditorGUI.LabelField(newRect, serialized.LayerName.stringValue);
                rect.y += rect.height;
            }
        }

        public static void DrawOutputLayerProperties(Rect rect, SerializedCompositionLayer serializedProperties, System.Action resetRenderTargetCallback)
        {
            rect.y += CompositorStyle.k_ListItemPading;
            rect.height = CompositorStyle.k_SingleLineHeight;

            EditorGUI.PropertyField(rect, serializedProperties.ColorFormat, TextUI.BufferFormat);
            rect.y += CompositorStyle.k_Spacing;

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(rect, serializedProperties.ResolutionScale, TextUI.Resolution);

            if (EditorGUI.EndChangeCheck())
            {
                // if the resolution changes, reset the RTs
                resetRenderTargetCallback();
            }
            rect.y += CompositorStyle.k_Spacing;

            EditorGUI.PropertyField(rect, serializedProperties.OutputRenderer, TextUI.OutputRenderer);
            rect.y += CompositorStyle.k_Spacing;

            serializedProperties.AOVBitmask.intValue = EditorGUI.MaskField(rect, TextUI.AOVs, serializedProperties.AOVBitmask.intValue, System.Enum.GetNames(typeof(MaterialSharedProperty)));
            rect.y += CompositorStyle.k_Spacing;
        }

        public static void DrawStackedLayerProperties(Rect rect, SerializedCompositionLayer serializedProperties, ReorderableList filterList)
        {
            rect.y += CompositorStyle.k_ListItemPading;
            rect.height = CompositorStyle.k_SingleLineHeight;

            EditorGUI.PropertyField(rect, serializedProperties.LayerName, TextUI.NameContent);
            rect.y += CompositorStyle.k_Spacing;

            switch (serializedProperties.InputLayerType.enumValueIndex)
            {
                case (int)CompositorLayer.LayerType.Camera:
                    EditorGUI.PropertyField(rect, serializedProperties.InputCamera, TextUI.Camera);
                    break;
                case (int)CompositorLayer.LayerType.Video:
                    EditorGUI.PropertyField(rect, serializedProperties.InputVideo, TextUI.Video);
                    break;
                case (int)CompositorLayer.LayerType.Image:
                    EditorGUI.PropertyField(rect, serializedProperties.InputTexture, TextUI.Image);
                    rect.y += CompositorStyle.k_Spacing;
                    EditorGUI.PropertyField(rect, serializedProperties.FitType);
                    break;
                default:
                    // This will happen if someone adds a new layer type and does not update this switch statement
                    Debug.Log("Unknown layer type: Please add code here to handle this type of layer.");
                    break;
            }
            rect.y += 1.1f * CompositorStyle.k_Spacing;

            EditorGUI.PropertyField(rect, serializedProperties.ClearDepth, TextUI.ClearDepth);
            rect.y += CompositorStyle.k_Spacing;

            EditorGUI.PropertyField(rect, serializedProperties.ClearAlpha, TextUI.ClearAlpha);
            rect.y += CompositorStyle.k_Spacing;

            DrawPropertyHelper(rect, TextUI.ClearMode, serializedProperties.OverrideClearMode, serializedProperties.ClearMode);
            rect.y += CompositorStyle.k_Spacing;

            DrawPropertyHelper(rect, TextUI.AAMode, serializedProperties.OverrideAA, serializedProperties.AAMode);
            rect.y += CompositorStyle.k_Spacing;

            DrawPropertyHelper(rect, TextUI.CullingMask, serializedProperties.OverrideCulling, serializedProperties.CullingMaskProperty);
            rect.y += CompositorStyle.k_Spacing;

            DrawPropertyHelper(rect, TextUI.VolumeMask, serializedProperties.OverrideVolume, serializedProperties.VolumeMask);
            rect.y += CompositorStyle.k_Spacing;

            Rect filterRect = rect;
            filterRect.y += 0.5f * CompositorStyle.k_Spacing;
            filterList.DoList(filterRect);
        }

        static void DrawPropertyHelper(Rect rect, GUIContent label, SerializedProperty checkBox, SerializedProperty serializedProperty)
        {
            Rect rectCopy = rect;
            rectCopy.width = 200;
            EditorGUI.PropertyField(rectCopy, checkBox, label);

            using (new EditorGUI.DisabledScope(checkBox.boolValue != true))
            {
                float pad = EditorGUIUtility.labelWidth + EditorGUIUtility.singleLineHeight + 2;
                rect.x += pad;
                rect.width -= rect.x;
                EditorGUI.PropertyField(rect, serializedProperty, GUIContent.none);
                rect.x -= pad;
                rect.width += pad;
            }
        }
    }
}
