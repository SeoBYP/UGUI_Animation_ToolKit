#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using UGUIAnimationToolkit.Editor;
using UGUIAnimationToolkit.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace UGUIAnimationToolkit.Editor // 네임스페이스를 Text.Editor로 명확히 하는 것을 추천합니다.
{
    [CustomEditor(typeof(UITextAnimator))]
    public class UITextAnimatorEditor : UnityEditor.Editor
    {
        private ReorderableList _onPlayList; // 변수 이름 변경
        private SerializedProperty _onPlaySequenceProp;

        private void OnEnable()
        {
            // 사용하신 코드에 맞춰 "onPlaySequence"를 찾도록 유지했습니다.
            // 만약 UITextAnimator의 필드 이름이 onEnableSequence라면 이 부분을 수정해야 합니다.
            _onPlaySequenceProp = serializedObject.FindProperty("onPlaySequence");
            if (_onPlaySequenceProp != null)
            {
                _onPlayList = CreateList(_onPlaySequenceProp.FindPropertyRelative("modules"));
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // 버튼 에디터처럼 전체를 Box로 감싸 통일성을 줍니다.
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("On Play Sequence", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            if (_onPlayList != null)
            {
                _onPlayList.DoLayoutList();
            }

            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }

        // UGUIButtonEditor의 CreateList 로직을 그대로 가져와서 수정합니다.
        private ReorderableList CreateList(SerializedProperty modulesProp)
        {
            if (modulesProp == null)
            {
                // 해당 프로퍼티를 찾지 못했을 때 경고를 표시합니다.
                // UGUIAnimator.cs에 onPlaySequence 필드가 있는지 확인하세요.
                return null;
            }

            var list = new ReorderableList(serializedObject, modulesProp, true, true, true, true);

            list.drawHeaderCallback =
                rect => EditorGUI.LabelField(rect, "Text Animation Modules", EditorStyles.boldLabel);
            list.elementHeightCallback = index =>
            {
                if (index < 0 || index >= modulesProp.arraySize) return EditorGUIUtility.singleLineHeight;
                var element = modulesProp.GetArrayElementAtIndex(index);
                if (element.managedReferenceValue == null) return EditorGUIUtility.singleLineHeight * 2;
                if (!element.isExpanded) return EditorGUIUtility.singleLineHeight + 4;
                return EditorGUI.GetPropertyHeight(element, true) + 10;
            };

            list.onAddDropdownCallback = (rect, l) =>
            {
                var menu = new GenericMenu();

                // 분리된 ModuleClipboard 클래스를 사용합니다.
                if (ModuleClipboard.ClipboardJson != null && ModuleClipboard.ClipboardType != null)
                {
                    var pasteName = ModuleClipboard.ClipboardType.Name.Replace("Module", "");
                    menu.AddItem(new GUIContent($"Paste As New ({pasteName})"), false, () =>
                    {
                        var newModule = Activator.CreateInstance(ModuleClipboard.ClipboardType);
                        JsonUtility.FromJsonOverwrite(ModuleClipboard.ClipboardJson, newModule);
                        modulesProp.arraySize++;
                        var element = modulesProp.GetArrayElementAtIndex(modulesProp.arraySize - 1);
                        element.managedReferenceValue = newModule;
                        element.isExpanded = true;
                        serializedObject.ApplyModifiedProperties();
                    });
                    menu.AddSeparator("");
                }

                var menuItems = new System.Collections.Generic.List<MenuItem>();

                // [핵심 수정] ButtonAnimationModule -> TextAnimationModule로 변경
                foreach (var type in TypeCache.GetTypesDerivedFrom<TextAnimationModule>())
                {
                    if (type.IsAbstract) continue;
                    var typeName = type.Name.Replace("Module", "");
                    var categoryAttribute = type.GetCustomAttribute<ModuleCategoryAttribute>();
                    var path = categoryAttribute?.Path != null ? $"{categoryAttribute.Path}/{typeName}" : typeName;
                    var order = categoryAttribute?.Order ?? int.MaxValue;
                    menuItems.Add(new MenuItem { Path = path, Type = type, Order = order });
                }

                var sortedItems = menuItems.OrderBy(item => item.Order).ThenBy(item => item.Path);
                foreach (var item in sortedItems)
                {
                    menu.AddItem(new GUIContent(item.Path), false, () =>
                    {
                        modulesProp.arraySize++;
                        var element = modulesProp.GetArrayElementAtIndex(modulesProp.arraySize - 1);
                        var moduleType = item.Type;
                        element.managedReferenceValue = Activator.CreateInstance(moduleType);
                        element.isExpanded = true;
                        serializedObject.ApplyModifiedProperties();
                    });
                }

                menu.ShowAsContext();
            };

            list.drawElementCallback = (rect, index, active, focused) =>
            {
                var element = modulesProp.GetArrayElementAtIndex(index);
                rect.y += 2;

                if (element.managedReferenceValue == null)
                {
                    EditorGUI.HelpBox(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight * 2 - 4),
                        "Module is null or missing. Please remove it.", MessageType.Warning);
                    return;
                }

                var headerRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.DrawRect(headerRect, new Color(0.18f, 0.18f, 0.18f));

                var enableProp = element.FindPropertyRelative("Enable");
                var enableRect = new Rect(headerRect.x + 5, headerRect.y, 15, headerRect.height);
                enableProp.boolValue = EditorGUI.Toggle(enableRect, enableProp.boolValue);

                string typeName = element.managedReferenceFullTypename.Split('.').Last().Replace("Module", "");
                var labelRect = new Rect(enableRect.xMax, headerRect.y, headerRect.width - 60, headerRect.height);
                if (GUI.Button(labelRect, typeName, EditorStyles.boldLabel))
                {
                    element.isExpanded = !element.isExpanded;
                }

                var menuRect = new Rect(headerRect.xMax - 25, headerRect.y, 25, headerRect.height);
                if (EditorGUI.DropdownButton(menuRect, new GUIContent("..."), FocusType.Passive, GUI.skin.label))
                {
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Copy Values"), false, () =>
                    {
                        var module = element.managedReferenceValue;
                        ModuleClipboard.ClipboardType = module.GetType();
                        ModuleClipboard.ClipboardJson = JsonUtility.ToJson(module);
                    });

                    if (ModuleClipboard.ClipboardJson != null &&
                        ModuleClipboard.ClipboardType == element.managedReferenceValue.GetType())
                    {
                        menu.AddItem(new GUIContent("Paste Values"), false, () =>
                        {
                            JsonUtility.FromJsonOverwrite(ModuleClipboard.ClipboardJson, element.managedReferenceValue);
                            serializedObject.ApplyModifiedProperties();
                        });
                    }
                    else
                    {
                        menu.AddDisabledItem(new GUIContent("Paste Values"));
                    }

                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent("Remove"), false, () => modulesProp.DeleteArrayElementAtIndex(index));
                    menu.ShowAsContext();
                }

                if (element.isExpanded)
                {
                    var contentRect = new Rect(rect.x, headerRect.yMax + 4, rect.width, 0);
                    var prop = element.Copy();
                    var endProp = prop.GetEndProperty();
                    prop.NextVisible(true);
                    EditorGUI.indentLevel++;
                    while (prop.NextVisible(false) && !SerializedProperty.EqualContents(prop, endProp))
                    {
                        contentRect.height = EditorGUI.GetPropertyHeight(prop, true);
                        EditorGUI.PropertyField(contentRect, prop, true);
                        contentRect.y += contentRect.height + EditorGUIUtility.standardVerticalSpacing;
                    }

                    EditorGUI.indentLevel--;
                }
            };
            return list;
        }
    }
}
#endif