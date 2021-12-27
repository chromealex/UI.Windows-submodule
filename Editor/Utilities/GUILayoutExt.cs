using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.CodeDom.Compiler;

namespace UnityEditor.UI.Windows {
    
	
	public static class SplitterGUI {
	
		public static readonly GUIStyle splitter;
	
		static SplitterGUI() {
			//GUISkin skin = GUI.skin;
		
			SplitterGUI.splitter = new GUIStyle();
			SplitterGUI.splitter.normal.background = EditorGUIUtility.whiteTexture;
			SplitterGUI.splitter.stretchWidth = true;
			SplitterGUI.splitter.margin = new RectOffset(0, 0, 7, 7);
		}
	
		private static readonly Color splitterColor = EditorGUIUtility.isProSkin ? new Color(0.3f, 0.3f, 0.3f) : new Color(0.5f, 0.5f, 0.5f);
	
		// GUILayout Style
		public static void Splitter(Color rgb, float thickness = 1) {
			Rect position = GUILayoutUtility.GetRect(GUIContent.none, SplitterGUI.splitter, GUILayout.Height(thickness));
		
			if (Event.current.type == EventType.Repaint) {
				Color restoreColor = GUI.color;
				GUI.color = rgb;
				SplitterGUI.splitter.Draw(position, false, false, false, false);
				GUI.color = restoreColor;
			}
		}
	
		public static void Splitter(float thickness, GUIStyle splitterStyle) {
			Rect position = GUILayoutUtility.GetRect(GUIContent.none, splitterStyle, GUILayout.Height(thickness));
		
			if (Event.current.type == EventType.Repaint) {
				Color restoreColor = GUI.color;
				GUI.color = SplitterGUI.splitterColor;
				splitterStyle.Draw(position, false, false, false, false);
				GUI.color = restoreColor;
			}
		}
	
		public static void Splitter(float thickness = 1) {
			SplitterGUI.Splitter(thickness, SplitterGUI.splitter);
		}
	
		// GUI Style
		public static void Splitter(Rect position) {
			if (Event.current.type == EventType.Repaint) {
				Color restoreColor = GUI.color;
				GUI.color = SplitterGUI.splitterColor;
				SplitterGUI.splitter.Draw(position, false, false, false, false);
				GUI.color = restoreColor;
			}
		}
	
	}

    public abstract class CustomEditorAttribute : System.Attribute {

        public System.Type type;
        public int order;

        protected CustomEditorAttribute(System.Type type, int order = 0) {

            this.type = type;
            this.order = order;

        }

    }

    public class ComponentCustomEditorAttribute : CustomEditorAttribute {

	    public ComponentCustomEditorAttribute(System.Type type, int order = 0) : base(type, order) {}

    }

    public struct GUITab {

	    public string caption;
	    public System.Action onDraw;
	    public float customWidth;

	    public static GUITab none {
		    get { return new GUITab(null, null); }
	    }

	    public GUITab(string caption, System.Action onDraw, float customWidth = 0f) {

		    this.caption = caption;
		    this.onDraw = onDraw;
		    this.customWidth = customWidth;

	    }

    }
    
    public static class GUILayoutExt {

	    public struct GUIBackgroundColorUsing : IDisposable {

		    private Color oldColor;

		    public GUIBackgroundColorUsing(Color color) {

			    this.oldColor = GUI.backgroundColor;
			    GUI.backgroundColor = color;

		    }
		    
		    public void Dispose() {

			    GUI.backgroundColor = this.oldColor;

		    }

	    }

	    public struct GUIAlphaUsing : IDisposable {

		    private Color oldColor;

		    public GUIAlphaUsing(float alpha) {

			    this.oldColor = GUI.color;
			    GUI.color = new Color(this.oldColor.r, this.oldColor.g, this.oldColor.b, alpha);

		    }
		    
		    public void Dispose() {

			    GUI.color = this.oldColor;

		    }

	    }

	    public struct GUIColorUsing : IDisposable {

		    private Color oldColor;

		    public GUIColorUsing(Color color) {

			    this.oldColor = GUI.color;
			    GUI.color = color;

		    }
		    
		    public void Dispose() {

			    GUI.color = this.oldColor;

		    }

	    }
	    
	    public static GUIBackgroundColorUsing GUIBackgroundColor(Color color) {
		    
		    return new GUIBackgroundColorUsing(color);
		    
	    }

	    public static GUIColorUsing GUIColor(Color color) {
		    
		    return new GUIColorUsing(color);
		    
	    }

	    public static void DrawImages(Texture2D preview, System.Collections.Generic.List<ImageCollectionItem> images) {
		    
		    if (images != null) {

			    GUILayoutExt.Box(4f, 4f, () => {

				    if (preview != null) {
                                    
					    var labelStyle = new GUIStyle(EditorStyles.label);
					    labelStyle.fontStyle = FontStyle.Bold;
					    labelStyle.alignment = TextAnchor.LowerCenter;
					    var w = EditorGUIUtility.currentViewWidth - 80f;
					    var h = w / preview.width * preview.height;
					    GUILayout.Label(string.Empty, GUILayout.MinWidth(w), GUILayout.MinHeight(h), GUILayout.Width(w), GUILayout.Height(h));
					    var lastRect = GUILayoutUtility.GetLastRect();
					    lastRect.width = w;
					    lastRect.height = h;
					    EditorGUI.DrawTextureTransparent(lastRect, preview);
					    EditorGUI.DropShadowLabel(lastRect, preview.width + "x" + preview.height, labelStyle);
                                    
				    }

				    foreach (var img in images) {

					    EditorGUI.BeginDisabledGroup(true);
					    EditorGUILayout.ObjectField(img.holder, typeof(Component), allowSceneObjects: true);
					    EditorGUILayout.ObjectField(img.obj, typeof(UnityEngine.Object), allowSceneObjects: true);
					    GUILayoutExt.Separator();
					    EditorGUI.EndDisabledGroup();

				    }

			    });

		    }

	    }

	    public static void DrawFieldsBeneath(SerializedObject serializedObject, System.Type baseType) {
		    
		    var iter = serializedObject.GetIterator();
		    iter.NextVisible(true);
		    System.Type baseClassType = null;
		    do {

			    if (EditorHelpers.IsFieldOfTypeBeneath(serializedObject.targetObject.GetType(), baseType, iter.propertyPath) == true) {

				    var newBaseClassType = EditorHelpers.GetFieldViaPath(serializedObject.targetObject.GetType(), iter.propertyPath).DeclaringType;
				    if (newBaseClassType != null && newBaseClassType != baseClassType) {
                        
					    GUILayoutExt.DrawSplitter(newBaseClassType.Name);
					    baseClassType = newBaseClassType;
                        
				    }
				    EditorGUILayout.PropertyField(iter);

			    }

		    } while (iter.NextVisible(false) == true);
		    
	    }
	    
	    public static void DrawSplitter(string label) {

		    var splitted = label.Split('`');
		    if (splitted.Length > 1) {

			    label = splitted[0];

		    }

		    var labelStyle = EditorStyles.centeredGreyMiniLabel;
		    var size = labelStyle.CalcSize(new GUIContent(label.ToSentenceCase()));
		    GUILayout.Label(label.ToSentenceCase().UppercaseWords(), labelStyle);
		    var lastRect = GUILayoutUtility.GetLastRect();
		    SplitterGUI.Splitter(new Rect(lastRect.x, lastRect.y + lastRect.height * 0.5f, lastRect.width * 0.5f - size.x * 0.5f, 1f));
		    SplitterGUI.Splitter(new Rect(lastRect.x + lastRect.width * 0.5f + size.x * 0.5f, lastRect.y + lastRect.height * 0.5f, lastRect.width * 0.5f - size.x * 0.5f, 1f));

	    }
	    
	    public static void DrawSafeAreaFields(UnityEngine.Object target, SerializedProperty useSafeZone, SerializedProperty safeZone) {
		    
		    EditorGUILayout.PropertyField(useSafeZone);
		    if (useSafeZone.boolValue == true) {
                
			    GUILayoutExt.Box(6f, 2f, () => {
                    
				    EditorGUILayout.PropertyField(safeZone);
				    if (safeZone.objectReferenceValue == null) {

					    GUILayoutExt.Box(2f, 2f, () => {
                            
						    GUILayout.BeginHorizontal();
						    GUILayout.FlexibleSpace();
						    if (GUILayout.Button("Generate", GUILayout.Width(80f), GUILayout.Height(30f)) == true) {

							    var obj = (Component)target;
							    if (PrefabUtility.IsPartOfAnyPrefab(obj) == true) {

								    var path = AssetDatabase.GetAssetPath(obj.gameObject);
								    using (var edit = new EditPrefabAssetScope(path)) {

									    EditorHelpers.AddSafeZone(edit.prefabRoot.transform);
                                
								    }
                            
							    } else {

								    var root = obj.gameObject;
								    EditorHelpers.AddSafeZone(root.transform);
                            
							    }
                        
						    }
						    GUILayout.FlexibleSpace();
						    GUILayout.EndHorizontal();
                            
					    }, GUIStyle.none);
                        
				    }

			    });
                
		    }
            
	    }

	    public static string GetPropertyToString(SerializedProperty property) {

		    if (property.hasMultipleDifferentValues == true) {

			    return "?";

		    }

		    string str = string.Empty;
		    switch (property.propertyType) {
			    
			    case SerializedPropertyType.Enum:
				    str = property.enumDisplayNames[property.enumValueIndex];
				    break;

			    case SerializedPropertyType.Boolean:
				    str = property.boolValue == true ? "True" : "False";
				    break;

			    case SerializedPropertyType.Integer:
				    str = property.intValue.ToString();
				    break;

			    case SerializedPropertyType.String:
				    str = property.stringValue;
				    break;

		    }
		    
		    return str;

	    }
	    
	    public static void DrawProperty(SerializedProperty property) {
		    
		    var prop = property.serializedObject.FindProperty(property.propertyPath);
		    prop.NextVisible(true);
		    do {

			    if (prop.propertyPath.StartsWith(property.propertyPath + ".") == false) {

				    break;

			    }
			    EditorGUILayout.PropertyField(prop, true);

		    } while (prop.NextVisible(false) == true);

	    }

	    public static void DrawProperty(Rect rect, SerializedProperty property, float elementHeight) {

		    var prop = property.serializedObject.FindProperty(property.propertyPath);
		    prop.NextVisible(true);
		    do {

			    if (prop.propertyPath.StartsWith(property.propertyPath + ".") == false) {

				    break;

			    }
			    EditorGUI.PropertyField(rect, prop, true);

			    rect.y += elementHeight;

		    } while (prop.NextVisible(false) == true);

	    }

	    public static void DrawStateButtons(UnityEngine.Object[] targets) {
		    
		    GUILayoutExt.Padding(10f, 10f, () => {
			    
			    GUILayout.BeginHorizontal();
			    if (GUILayout.Button("Show") == true) {

				    for (int i = 0; i < targets.Length; ++i) {

					    var wo = targets[i] as UnityEngine.UI.Windows.WindowObject;
					    if (wo != null) wo.Show();

				    }
				    
			    }

			    if (GUILayout.Button("Hide") == true) {
				    
				    for (int i = 0; i < targets.Length; ++i) {

					    var wo = targets[i] as UnityEngine.UI.Windows.WindowObject;
					    if (wo != null) wo.Hide();

				    }

			    }
			    GUILayout.EndHorizontal();

		    });
		    
	    }
	    
	    public static void DrawComponentHeaderItem(string caption, string value) {
	        
		    GUILayoutExt.Padding(16f, 4f, () => {

			    using (new GUIColorUsing(new Color(1f, 1f, 1f, 0.5f))) {
				    
				    GUILayout.Label(caption, EditorStyles.miniBoldLabel, GUILayout.Height(16f));
				    
			    }

			    GUILayout.Space(-6f);
			    using (new GUIColorUsing(new Color(1f, 1f, 1f, 1f))) {

				    GUILayoutExt.Padding(5f, 0f, () => {
					    
					    GUILayout.Label(EditorHelpers.StringToCaption(value), EditorStyles.label);
					    
				    }, GUIStyle.none);

			    }

		    }, GUIStyle.none);

	    }

	    public static void DrawComponentHeader(SerializedObject serializedObject, string caption, System.Action onDraw) {
		    
		    GUILayoutExt.DrawComponentHeader(serializedObject, caption, onDraw, new Color(0f, 0.6f, 1f, 0.4f));

	    }

	    public static void DrawComponentHeader(SerializedObject serializedObject, string caption, System.Action onDraw, Color color) {

		    var colorCaption = new Color(0f, 0f, 0f, 0.1f);

		    var width = 40f;
		    
		    GUILayout.BeginVertical();
		    {
			    GUILayoutExt.Separator(color);
			    GUILayout.BeginHorizontal();
			    {
				    var rect = EditorGUILayout.GetControlRect(GUILayout.Width(width), GUILayout.ExpandHeight(true));
				    rect.y -= 2f;
				    rect.x -= 3f;
				    rect.height += 4f;
				    EditorGUI.DrawRect(rect, color);
				    {
					    var style = new GUIStyle(EditorStyles.whiteLargeLabel);
					    style.alignment = TextAnchor.MiddleCenter;
					    style.normal.textColor = colorCaption;
					    style.fontStyle = FontStyle.Bold;
					    style.fontSize = 30;
					    rect.height = 40f;
					    GUI.Label(rect, caption, style);
				    }
				    
				    GUILayout.BeginHorizontal();
				    {
					    onDraw.Invoke();

					    if (EditorApplication.isPlaying == true) {

						    var isValid = true;
						    for (int i = 0; i < serializedObject.targetObjects.Length; ++i) {

							    if ((serializedObject.targetObjects[i] is UnityEngine.UI.Windows.WindowObject) == false ||
							        PrefabUtility.IsPartOfPrefabAsset(serializedObject.targetObjects[i]) == true) {

								    isValid = false;
								    break;

							    }
							    
						    }

						    if (isValid == true) {

							    GUILayoutExt.DrawStateButtons(serializedObject.targetObjects);

						    }

					    }
				    }
				    GUILayout.EndHorizontal();
			    }
			    GUILayout.EndHorizontal();
			    GUILayoutExt.Separator(color);
		    }
		    GUILayout.EndVertical();
		    
	    }

	    public static int DrawTabs(int selectedIndex, ref Vector2 scrollPosition, params GUITab[] tabs) {

		    var color = new Color(0f, 0.6f, 1f, 0.4f);
		    var selectedColor = new Color(0f, 0f, 0f, 0.2f);

		    var hasFlex = false;
		    scrollPosition = GUILayout.BeginScrollView(scrollPosition);
		    GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
		    {
			    var normalStyle = new GUIStyle(EditorStyles.toolbarButton);
			    var selectedStyle = new GUIStyle(EditorStyles.toolbarButton);
			    selectedStyle.normal = normalStyle.active;
			    selectedStyle.onNormal = normalStyle.onActive;
			    selectedStyle.normal.background = Texture2D.whiteTexture;
			    selectedStyle.normal.textColor = Color.white;
			    
			    var attrs = new System.Collections.Generic.List<GUILayoutOption>();
			    GUILayoutOption[] attrsArr = null;
			    for (int i = 0; i < tabs.Length; ++i) {

				    var tab = tabs[i];
				    if (string.IsNullOrEmpty(tab.caption) == true) {

					    if (selectedIndex == i) --selectedIndex;
					    continue;
					    
				    }
				    
				    attrs.Clear();
				    if (tab.customWidth > 0f) {
    
					    GUILayout.FlexibleSpace();
					    hasFlex = true;
					    
					    attrs.Add(GUILayout.Width(tab.customWidth));
					    attrs.Add(GUILayout.ExpandWidth(false));

				    } else {
					    
					    attrs.Add(GUILayout.ExpandWidth(false));
					    
				    }

				    attrsArr = attrs.ToArray();

				    if (selectedIndex == i) {

					    GUILayout.BeginVertical();
					    {
						    GUILayoutExt.Separator(color, 2f);
						    var bc = GUI.backgroundColor;
						    GUI.backgroundColor = selectedColor;
						    GUILayout.Label(" " + tab.caption + " ", selectedStyle, attrsArr);
						    GUI.backgroundColor = bc;
					    }
					    GUILayout.EndVertical();

				    } else {
					
					    GUILayout.BeginVertical();
					    {
						    GUILayoutExt.Separator();
						    if (GUILayout.Button(" " + tab.caption + " ", normalStyle, attrsArr) == true) {

							    selectedIndex = i;

						    }
					    }
					    GUILayout.EndVertical();

				    }
				    
			    }
		    }

		    if (hasFlex == false) {
			    
			    GUILayout.FlexibleSpace();

		    }
		    
		    GUILayout.EndHorizontal();
		    GUILayout.EndScrollView();

		    if (tabs[selectedIndex].onDraw != null) {

			    GUILayout.BeginVertical();
			    {
				    GUILayoutExt.Separator(selectedColor);
				    //++EditorGUI.indentLevel;
				    if (selectedIndex >= 0 && selectedIndex < tabs.Length) {

					    GUILayoutExt.Box(8f, 0f, tabs[selectedIndex].onDraw);

				    }

				    //--EditorGUI.indentLevel;
			    }
			    GUILayout.EndVertical();

		    }
		    
		    return selectedIndex;

	    }
	    
	    public static void DrawGradient(float height, Color from, Color to, string labelFrom, string labelTo) {
	        
		    var tex = new Texture2D(2, 1, TextureFormat.RGBA32, false);
		    tex.filterMode = FilterMode.Bilinear;
		    tex.wrapMode = TextureWrapMode.Clamp;
		    tex.SetPixel(0, 0, from);
		    tex.SetPixel(1, 0, to);
		    tex.Apply();
		    
		    Rect rect = EditorGUILayout.GetControlRect(false, height);
		    rect.height = height;
		    EditorGUI.DrawTextureTransparent(rect, tex, ScaleMode.StretchToFill);
		    
		    GUILayout.BeginHorizontal();
		    {
			    GUILayout.Label(labelFrom);
			    GUILayout.FlexibleSpace();
			    GUILayout.Label(labelTo);
		    }
		    GUILayout.EndHorizontal();
		    
	    }

	    public static Rect ProgressBar(float value, float max, bool drawLabel = false, float height = 4f) {
		    
		    return GUILayoutExt.ProgressBar(value, max, new Color(0f, 0f, 0f, 0.3f), new Color32(104, 148, 192, 255), drawLabel, height);
		    
	    }

	    public static Rect ProgressBar(float value, float max, Color back, Color fill, bool drawLabel = false, float height = 4f) {

		    var progress = value / max;
		    var lineHeight = (drawLabel == true ? height * 2f : height);
		    Rect rect = EditorGUILayout.GetControlRect(false, lineHeight);
		    rect.height = lineHeight;
		    var fillRect = rect;
		    fillRect.width = progress * rect.width;
		    EditorGUI.DrawRect(rect, back);
		    EditorGUI.DrawRect(fillRect, fill);

		    if (drawLabel == true) {
			    
			    EditorGUI.LabelField(rect, string.Format("{0}/{1}", value, max), EditorStyles.centeredGreyMiniLabel);
			    
		    }

		    return rect;

	    }

        public static bool ToggleLeft(ref bool state, ref bool isDirty, string caption, string text) {

            var labelRich = new GUIStyle(EditorStyles.label);
            labelRich.richText = true;

            var isLocalDirty = false;
            var flag = EditorGUILayout.ToggleLeft(caption, state, labelRich);
            if (flag != state) {

                isLocalDirty = true;
                isDirty = true;
                state = flag;
                        
            }
            if (string.IsNullOrEmpty(text) == false) GUILayoutExt.SmallLabel(text);
            EditorGUILayout.Space();

            return isLocalDirty;

        }

        public static LayerMask DrawLayerMaskField(string label, LayerMask layerMask) {

	        System.Collections.Generic.List<string> layers = new System.Collections.Generic.List<string>();
	        System.Collections.Generic.List<int> layerNumbers = new System.Collections.Generic.List<int>();

	        for (int i = 0; i < 32; i++) {
		        string layerName = LayerMask.LayerToName(i);
		        if (layerName != "") {
			        layers.Add(layerName);
			        layerNumbers.Add(i);
		        }
	        }
	        int maskWithoutEmpty = 0;
	        for (int i = 0; i < layerNumbers.Count; i++) {
		        if (((1 << layerNumbers[i]) & layerMask.value) > 0)
			        maskWithoutEmpty |= (1 << i);
	        }
	        maskWithoutEmpty = EditorGUILayout.MaskField( label, maskWithoutEmpty, layers.ToArray());
	        int mask = 0;
	        for (int i = 0; i < layerNumbers.Count; i++) {
		        if ((maskWithoutEmpty & (1 << i)) > 0)
			        mask |= (1 << layerNumbers[i]);
	        }
	        layerMask.value = mask;
	        return layerMask;

        }

        public static void PropertyField(SerializedProperty property, System.Func<UnityEngine.UI.Windows.WindowObject.EditorParametersRegistry, bool> regCheck = null) {

	        var hasAnyReg = false;
	        var description = string.Empty;
	        var holders = new System.Collections.Generic.List<string>();
	        var holdersObjs = new System.Collections.Generic.List<UnityEngine.UI.Windows.IHolder>();
	        if (regCheck != null) {

		        description = $"Value is hold by "; //{string.Join(", ", holders)}";

		        holders.Add(description);
		        foreach (var target in property.serializedObject.targetObjects) {

			        if (target is UnityEngine.UI.Windows.WindowObject windowObject) {

				        if (windowObject.registry != null) {

					        foreach (var reg in windowObject.registry) {

						        if (reg.GetHolder() == null) continue;
						        if (regCheck.Invoke(reg) == false) continue;
						        
						        holders.Add(reg.GetHolderName());
						        holdersObjs.Add(reg.GetHolder());
						        hasAnyReg = true;

					        }

				        }

			        }

		        }

	        }

	        if (hasAnyReg == false) {

		        EditorGUILayout.PropertyField(property);

	        } else {
		        
		        EditorGUI.BeginDisabledGroup(true);
		        EditorGUILayout.PropertyField(property);
		        EditorGUI.EndDisabledGroup();
		        
		        GUILayoutExt.Box(4f, 2f, () => {

			        var rect = new Rect(0f, 0f, EditorGUIUtility.currentViewWidth, 1000f);
			        var style = new GUIStyle(EditorStyles.miniLabel);
			        style.normal.textColor = new Color(0x00 / 255f, 0x98 / 255f, 0xDA / 255f, 1f);
			        style.onNormal.textColor = style.active.textColor = style.onActive.textColor = style.hover.textColor = style.onHover.textColor = style.focused.textColor = style.onFocused.textColor = new Color(0x00 / 255f, 0xAA / 255f, 0xDA / 255f, 1f);
			        var buttonRects = EditorGUIUtility.GetFlowLayoutedRects(rect, style, 1f, 1f, holders);
			        
			        GUILayout.BeginHorizontal();
			        GUILayout.EndHorizontal();
			        var areaRect = GUILayoutUtility.GetLastRect();
			        for (int i = 0; i < buttonRects.Count; ++i) areaRect.height = Mathf.Max(0f, buttonRects[i].yMax);
			        
			        GUILayoutUtility.GetRect(areaRect.width, areaRect.height);
			        GUI.BeginGroup(areaRect);
			        {
				        for (int i = 0; i < buttonRects.Count; ++i) {

					        if (i == 0) {
						        
						        GUI.Label(buttonRects[i], holders[i], EditorStyles.miniLabel);
						        
					        } else {

						        var position = buttonRects[i];

						        Handles.BeginGUI();
						        Handles.color = style.normal.textColor;
						        Handles.DrawLine(new Vector3(position.xMin, position.yMax), new Vector3(position.xMax, position.yMax));
						        Handles.color = Color.white;
						        Handles.EndGUI();
						        if (GUI.Button(position, holders[i], style) == true) {
							        
							        EditorGUIUtility.PingObject((Component)holdersObjs[i - 1]);
							        
						        }

					        }

				        }
			        }
			        GUI.EndGroup();
			        
		        }, EditorStyles.helpBox);
		        
	        }

        }
        
        public static void DrawHeader(string caption) {

	        var style = GUIStyle.none;//new GUIStyle("In BigTitle");
	        //new Editor().DrawHeader();
            
	        GUILayout.Space(4f);
	        GUILayoutExt.Separator();
	        GUILayoutExt.Padding(
		        16f, 4f,
		        () => {
                    
			        GUILayout.Label(caption, EditorStyles.boldLabel);
                    
		        }, style);
	        GUILayoutExt.Separator(new Color(0.2f, 0.2f, 0.2f, 1f));
            
        }

        public static void SmallLabel(string text) {

            var labelRich = new GUIStyle(EditorStyles.miniLabel);
            labelRich.richText = true;
            labelRich.wordWrap = true;

            var oldColor = GUI.color;
            var c = oldColor;
            c.a = 0.5f;
            GUI.color = c;
            
            EditorGUILayout.LabelField(text, labelRich);

            GUI.color = oldColor;

        }

        public static int Pages(int count, int page, int elementsOnPage, System.Action<int, int> onDraw, System.Action<int> onPageElementsChanged, System.Action onDrawHeader = null) {

            var from = page * elementsOnPage;
            var to = from + elementsOnPage;
            if (from < 0) from = 0;
            if (to > count) to = count;
            var pages = Mathf.CeilToInt(count / (float)elementsOnPage) - 1;
            
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                if (onDrawHeader != null) onDrawHeader.Invoke();
                
                GUILayout.FlexibleSpace();
                
                GUILayout.BeginHorizontal();
                {

                    GUILayout.Label("On page:", EditorStyles.toolbarButton);
                    if (GUILayout.Button(elementsOnPage.ToString(), EditorStyles.toolbarDropDown, GUILayout.MinWidth(30f)) == true) {

                        var items = new[] { 10, 20, 30, 40, 50, 100 };
                        var menu = new GenericMenu();
                        for (int i = 0; i < items.Length; ++i) {

                            var idx = i;
                            menu.AddItem(new GUIContent(items[i].ToString()), items[i] == elementsOnPage, () => { onPageElementsChanged.Invoke(items[idx]); });

                        }

                        //menu.DropDown(GUILayoutUtility.GetLastRect());
                        menu.ShowAsContext();

                    }

                    EditorGUI.BeginDisabledGroup(page <= 0);
                    if (GUILayout.Button("◄", EditorStyles.toolbarButton) == true) {

                        --page;

                    }

                    EditorGUI.EndDisabledGroup();

                    var pageStr = GUILayout.TextField((page + 1).ToString(), EditorStyles.toolbarTextField, GUILayout.MinWidth(20f));
                    if (int.TryParse(pageStr, out var res) == true) {

                        page = res - 1;

                    }
                    GUILayout.Label("/", EditorStyles.toolbarButton);
                    GUILayout.Label(string.Format("{0}", pages + 1), EditorStyles.toolbarButton, GUILayout.MinWidth(20f));

                    EditorGUI.BeginDisabledGroup(page >= pages);
                    if (GUILayout.Button("►", EditorStyles.toolbarButton) == true) {

                        ++page;

                    }

                    EditorGUI.EndDisabledGroup();

                }
                GUILayout.EndHorizontal();
                
                if (page < 0) page = 0;
                if (page > pages) page = pages;

            }
            GUILayout.EndHorizontal();
            
            onDraw.Invoke(from, to);

            return page;

        }

        public static int GetFieldsCount(object instance) {
            
            var fields = instance.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            return fields.Length;

        }

        public static void Icon(string path, float width = 32f, float height = 32f) {

	        var icon = new GUIStyle();
	        icon.normal.background = UnityEditor.Experimental.EditorResources.Load<Texture2D>(path);
	        EditorGUILayout.LabelField(string.Empty, icon, GUILayout.Width(width), GUILayout.Height(height));

        }

        private static bool HasBaseType(this System.Type type, System.Type baseType) {

	        return baseType.IsAssignableFrom(type);

        }

        private static bool HasInterface(this System.Type type, System.Type interfaceType) {

	        return interfaceType.IsAssignableFrom(type);
	        
        }

        public static void DataLabel(string content, params GUILayoutOption[] options) {

            var style = new GUIStyle(EditorStyles.label);
            var rect = GUILayoutUtility.GetRect(new GUIContent(content), style, options);
            style.richText = true;
            style.stretchHeight = false;
            style.fixedHeight = 0f;
            EditorGUI.SelectableLabel(rect, content, style);

        }

        public static string GetTypeLabel(System.Type type) {

            var output = type.Name;
            var sOutput = output.Split('`');
            if (sOutput.Length > 0) {

                output = sOutput[0];

            }

            var genericTypes = type.GenericTypeArguments;
            if (genericTypes != null && genericTypes.Length > 0) {

                var sTypes = string.Empty;
                for (int i = 0; i < genericTypes.Length; ++i) {

                    sTypes += (i > 0 ? ", " : string.Empty) + genericTypes[i].Name;

                }

                output += "<" + sTypes + ">";

            }

            return output;

        }

        public static void TypeLabel(System.Type type, params GUILayoutOption[] options) {

            GUILayoutExt.DataLabel(GUILayoutExt.GetTypeLabel(type), options);

        }

        public static void Separator() {
            
            GUILayoutExt.Separator(new Color(0.1f, 0.1f, 0.1f, 0.2f));
            
        }

        public static void Separator(Color color, params GUILayoutOption[] options) {
	        
	        GUILayoutExt.Separator(color, 1f, options);
	        
        }

        public static void Separator(Color color, float height, params GUILayoutOption[] options) {

	        GUIStyle horizontalLine;
	        horizontalLine = new GUIStyle();
	        horizontalLine.normal.background = EditorGUIUtility.whiteTexture;
	        horizontalLine.margin = new RectOffset(0, 0, 0, 0);
	        horizontalLine.fixedHeight = height;
	        
	        var c = GUI.color;
	        GUI.color = color;
	        GUILayout.Box(GUIContent.none, horizontalLine);
	        GUI.color = c;

        }

        public static void DrawBoxNotFilled(Rect rect, float size, Color color, float padding = 0f) {
	        
	        var s1 = new Rect(rect);
	        s1.height = size;
	        s1.y += padding;
	        s1.x += padding + 1f;
	        s1.width -= padding * 2f + 2f;
	        
	        var s2 = new Rect(rect);
	        s2.y += rect.height - size - padding;
	        s2.height = size;
	        s2.x += padding + 1f;
	        s2.width -= padding * 2f + 2f;
	        
	        var s3 = new Rect(rect);
	        s3.width = size;
	        s3.x += padding;
	        s3.y += padding;
	        s3.height -= padding * 2f;
	        
	        var s4 = new Rect(rect);
	        s4.width = size;
	        s4.x += rect.width - size - padding;
	        s4.y += padding;
	        s4.height -= padding * 2f;

	        DrawRect(s1, color);
	        DrawRect(s2, color);
	        DrawRect(s3, color);
	        DrawRect(s4, color);

        }
        
        public static void DrawRect(Rect rect, Color color) {

	        GUIStyle horizontalLine;
	        horizontalLine = new GUIStyle();
	        horizontalLine.normal.background = EditorGUIUtility.whiteTexture;
	        horizontalLine.margin = new RectOffset(0, 0, 0, 0);
	        
	        var c = GUI.color;
	        GUI.color = color;
	        GUI.Box(rect, GUIContent.none, horizontalLine);
	        GUI.color = c;

        }

        public static void TableCaption(string content, GUIStyle style) {

            style = new GUIStyle(style);
            style.alignment = TextAnchor.MiddleCenter;
            style.stretchWidth = true;
            style.stretchHeight = true;

            GUILayout.Label(content, style);

        }

        private static int foldOutLevel;

        public static void FoldOut(ref bool state, string content, System.Action onContent, GUIStyle style = null, System.Action<Rect> onHeader = null) {

            if (style == null) {

                style = new GUIStyle(EditorStyles.foldoutHeader);
                style.fixedWidth = 0f;
                style.stretchWidth = true;

                if (GUILayoutExt.foldOutLevel == 0) {

                    style.fixedHeight = 24f;
                    style.richText = true;
                    content = "<b>" + content + "</b>";

                } else {

                    style.fixedHeight = 16f;
                    style.richText = true;

                }

            }

            ++GUILayoutExt.foldOutLevel;
            state = GUILayoutExt.BeginFoldoutHeaderGroup(state, new GUIContent(content), style, menuAction: onHeader);
            if (state == true) {

	            GUILayout.BeginHorizontal();
	            {
		            GUILayout.Space(10f);
		            GUILayout.BeginVertical();
		            onContent.Invoke();
		            GUILayout.EndVertical();
	            }
	            GUILayout.EndHorizontal();

            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            --GUILayoutExt.foldOutLevel;

        }

        public static bool BeginFoldoutHeaderGroup(
            bool foldout,
            GUIContent content,
            GUIStyle style = null,
            System.Action<Rect> menuAction = null,
            GUIStyle menuIcon = null) {

            return GUILayoutExt.BeginFoldoutHeaderGroup(GUILayoutUtility.GetRect(content, style), foldout, content, style, menuAction, menuIcon);

        }

        public static bool BeginFoldoutHeaderGroup(
            Rect position,
            bool foldout,
            GUIContent content,
            GUIStyle style = null,
            System.Action<Rect> menuAction = null,
            GUIStyle menuIcon = null) {
            //if (EditorGUIUtility.hierarchyMode) position.xMin -= (float)(EditorStyles.inspectorDefaultMargins.padding.left - EditorStyles.inspectorDefaultMargins.padding.right);
            if (style == null) style = EditorStyles.foldoutHeader;
            Rect position1 = new Rect() {
                x = (float)((double)position.xMax - (double)style.padding.right - 16.0),
                y = position.y + (float)style.padding.top,
                size = Vector2.one * 16f
            };
            bool isHover = position1.Contains(Event.current.mousePosition);
            bool isActive = isHover && Event.current.type == EventType.MouseDown && Event.current.button == 0;
            if (menuAction != null && isActive) {
                menuAction(position1);
                Event.current.Use();
            }

            foldout = GUI.Toggle(position, foldout, content, style);
            if (menuAction != null && Event.current.type == EventType.Repaint) {
                if (menuIcon == null) menuIcon = EditorStyles.foldoutHeaderIcon;
                menuIcon.Draw(position1, isHover, isActive, false, false);
            }

            return foldout;
        }

        public static void Box(float padding, float margin, System.Action onContent, GUIStyle style = null, params GUILayoutOption[] options) {

            GUILayoutExt.Padding(margin, () => {

                if (style == null) {

                    style = "GroupBox";

                } else {

                    style = new GUIStyle(style);

                }

                style.padding = new RectOffset();
                style.margin = new RectOffset();

                GUILayout.BeginVertical(style, options);
                {

                    GUILayoutExt.Padding(padding, onContent);

                }
                GUILayout.EndVertical();

            }, options);

        }

        public static void Padding(float padding, System.Action onContent, params GUILayoutOption[] options) {

            GUILayoutExt.Padding(padding, padding, onContent, options);

        }

        public static void Padding(float paddingX, float paddingY, System.Action onContent, params GUILayoutOption[] options) {

            GUILayoutExt.Padding(paddingX, paddingY, onContent, GUIStyle.none, options);

        }

        public static void Padding(float paddingX, float paddingY, System.Action onContent, GUIStyle style, params GUILayoutOption[] options) {

            GUILayout.BeginVertical(style, options);
            {
                GUILayout.Space(paddingY);
                GUILayout.BeginHorizontal(options);
                {
                    GUILayout.Space(paddingX);
                    {
                        GUILayout.BeginVertical(options);
                        onContent.Invoke();
                        GUILayout.EndVertical();
                    }
                    GUILayout.Space(paddingX);
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(paddingY);
            }
            GUILayout.EndVertical();

        }

        public static bool DrawDropdown(Rect position, GUIContent content, FocusType focusType, UnityEngine.Object selectButton = null) {

	        if (selectButton != null) {

		        var selectButtonWidth = 80f;
		        var space = 4f;
		        var rect = new Rect(position.x, position.y, position.width - selectButtonWidth - space, position.height);
		        var result = EditorGUI.DropdownButton(rect, content, focusType);
		        if (GUI.Button(new Rect(position.x + rect.width + space, position.y, selectButtonWidth, position.height), "Select") == true) {

			        Selection.activeObject = selectButton;

		        }
				
		        return result;
				
	        }
			
	        return EditorGUI.DropdownButton(position, content, focusType);

        }

    }

    public class PopupWindowAnim : EditorWindow {

		private const float defaultWidth = 150;
		private const float defaultHeight = 250;
		private const float elementHeight = 20;
		
		/// <summary> Прямоугольник, в котором будет отображен попап </summary>
		public Rect screenRect;
		
		/// <summary> Указывает, что является разделителем в пути </summary>
		public char separator = '/';
		
		/// <summary> Позволяет использовать/убирать поиск </summary>
		public bool useSearch = true;
		
		/// <summary> Название рута </summary>
		public new string title = "Menu";

		public new string name { get { return title; } set { title = value; } }
		
		/// <summary> Стили, используемые для визуализации попапа </summary>
		private static Styles styles;
		
		//Поиск
		/// <summary> Строка поиска </summary>
		public string searchText = "";

		/// <summary> Активен ли поиск? </summary>
		private bool hasSearch { get { return useSearch && !string.IsNullOrEmpty(searchText); } }
		
		//Анимация
		private float _anim;
		private int _animTarget = 1;
		private long _lastTime;
		
		//Элементы
		/// <summary> Список конечных элементов (до вызова Show) </summary>
		private System.Collections.Generic.List<PopupItem> submenu = new System.Collections.Generic.List<PopupItem>();
		/// <summary> Хранит контекст элементов (нужно при заполнении попапа) </summary>
		private System.Collections.Generic.List<string> folderStack = new System.Collections.Generic.List<string>();
		/// <summary> Список элементов (после вызова Show) </summary>
		private Element[] _tree;
		/// <summary> Список элементов, подходящих под условия поиска </summary>
		private Element[] _treeSearch;
		/// <summary> Хранит контексты элементов (после вызова Show) </summary>
		private System.Collections.Generic.List<GroupElement> _stack = new System.Collections.Generic.List<GroupElement>();
		/// <summary> Указывает, нуждается ли выбор нового элемента в прокрутке </summary>
		private bool scrollToSelected;
		
		private Element[] activeTree { get { return (!hasSearch ? _tree : _treeSearch); } }

		private GroupElement activeParent { get { return _stack[(_stack.Count - 2) + _animTarget]; } }

		private Element activeElement {
			get {
				if (activeTree == null)
					return null;
				var childs = GetChildren(activeTree, activeParent);
				if (childs.Count == 0)
					return null;
				return childs[activeParent.selectedIndex];
			}
		}
		
		/// <summary> Создание окна </summary>
		public static PopupWindowAnim Create(Rect screenRect, bool useSearch = true) {
			var popup = CreateInstance<PopupWindowAnim>();
			popup.screenRect = screenRect;
			popup.useSearch = useSearch;
			return popup;
		}
		
		/// <summary> Создание окна </summary>
		public static PopupWindowAnim CreateByPos(Vector2 pos, bool useSearch = true) {
			return Create(new Rect(pos.x, pos.y, defaultWidth, defaultHeight), useSearch);
		}
		
		/// <summary> Создание окна </summary>
		public static PopupWindowAnim CreateByPos(Vector2 pos, float width, bool useSearch = true) {
			return Create(new Rect(pos.x, pos.y, width, defaultHeight), useSearch);
		}
		
		/// <summary> Создание окна. Вызывается из OnGUI()! </summary>
		public static PopupWindowAnim CreateBySize(Vector2 size, bool useSearch = true) {
			var screenPos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
			return Create(new Rect(screenPos.x, screenPos.y, size.x, size.y), useSearch);
		}
		
		/// <summary> Создание окна. Вызывается из OnGUI()! </summary>
		public static PopupWindowAnim Create(float width, bool useSearch = true) {
			return CreateBySize(new Vector2(width, defaultHeight), useSearch);
		}
		
		/// <summary> Создание окна. Вызывается из OnGUI()! </summary>
		public static PopupWindowAnim Create(bool useSearch = true) {
			return CreateBySize(new Vector2(defaultWidth, defaultHeight), useSearch);
		}
		
		/// <summary> Отображает попап </summary>
		public new void Show() {
			if (submenu.Count == 0)
				DestroyImmediate(this);
			else
				Init();
		}
		
		/// <summary> Отображает попап </summary>
		public void ShowAsDropDown() {
			Show();
		}
		
		public void SetHeightByElementCount(int elementCount) {
			screenRect.height = elementCount * elementHeight + (useSearch ? 30f : 0f) + 26f;
		}
		
		public void SetHeightByElementCount() {
			SetHeightByElementCount(maxElementCount);
		}
		
		public bool autoHeight;
		public bool autoClose;
		
		public void BeginRoot(string folderName) {
			var previous = folderStack.Count != 0 ? folderStack[folderStack.Count - 1] : "";
			if (string.IsNullOrEmpty(folderName))
				folderName = "<Noname>";
			if (!string.IsNullOrEmpty(previous))
				folderStack.Add(previous + separator + folderName);
			else
				folderStack.Add(folderName);
		}
		
		public void EndRoot() {
			if (folderStack.Count > 0)
				folderStack.RemoveAt(folderStack.Count - 1);
			else
				throw new Exception("Excess call EndFolder()");
		}
		
		public void EndRootAll() {
			while (folderStack.Count > 0)
				folderStack.RemoveAt(folderStack.Count - 1);
		}
		
		public void Item(string title, Texture2D image, Action action, int order) {
			var folder = "";
			if (folderStack.Count > 0)
				folder = folderStack[folderStack.Count - 1] ?? "";
			submenu.Add(string.IsNullOrEmpty(folder)
				            ? new PopupItem(this.title + separator + title, action) { image = image, order = order }
				            : new PopupItem(this.title + separator + folder + separator + title, action) { image = image, order = order });
		}

		public void Item(string title, Texture2D image, Action<PopupItem> action, bool searchable) {
			var folder = "";
			if (folderStack.Count > 0)
				folder = folderStack[folderStack.Count - 1] ?? "";
			submenu.Add(string.IsNullOrEmpty(folder)
				            ? new PopupItem(this.title + separator + title, action) { image = image, searchable = searchable }
				            : new PopupItem(this.title + separator + folder + separator + title, action) { image = image, searchable = searchable });
		}

		public void Item(string title, Texture2D image, Action<PopupItem> action, bool searchable, int order) {
			var folder = "";
			if (folderStack.Count > 0)
				folder = folderStack[folderStack.Count - 1] ?? "";
			submenu.Add(string.IsNullOrEmpty(folder)
				            ? new PopupItem(this.title + separator + title, action) { image = image, searchable = searchable, order = order }
				            : new PopupItem(this.title + separator + folder + separator + title, action) { image = image, searchable = searchable, order = order });
		}

		public void Item(string title, Action action) {
			var folder = "";
			if (folderStack.Count > 0)
				folder = folderStack[folderStack.Count - 1] ?? "";
			submenu.Add(string.IsNullOrEmpty(folder)
			            ? new PopupItem(this.title + separator + title, action)
			            : new PopupItem(this.title + separator + folder + separator + title, action));
		}
		
		public void Item(string title) {
			var folder = "";
			if (folderStack.Count > 0)
				folder = folderStack[folderStack.Count - 1] ?? "";
			submenu.Add(string.IsNullOrEmpty(folder)
			            ? new PopupItem(this.title + separator + title, () => { })
			            : new PopupItem(this.title + separator + folder + separator + title, () => { }));
		}
		
		public void ItemByPath(string path, Texture2D image, Action action) {
			if (string.IsNullOrEmpty(path))
				path = "<Noname>";
			submenu.Add(new PopupItem(title + separator + path, action) { image = image });
		}
		
		public void ItemByPath(string path, Action action) {
			if (string.IsNullOrEmpty(path))
				path = "<Noname>";
			submenu.Add(new PopupItem(title + separator + path, action));
		}
		
		public void ItemByPath(string path) {
			if (string.IsNullOrEmpty(path))
				path = "<Noname>";
			submenu.Add(new PopupItem(title + separator + path, () => { }));
		}
		
		private void Init() {
			CreateComponentTree();
			if (autoHeight)
				SetHeightByElementCount();
			ShowAsDropDown(new Rect(screenRect.x, screenRect.y, 1, 1), new Vector2(screenRect.width, screenRect.height));
			Focus();
			wantsMouseMove = true;
		}
		
		private void CreateComponentTree() {

			var list = new System.Collections.Generic.List<string>();
			var elements = new System.Collections.Generic.List<Element>();

			this.submenu = this.submenu.OrderBy(x => x.order).ThenBy(x => x.path).ToList();
			
			for (int i = 0; i < submenu.Count; i++) {

				var submenuItem = submenu[i];
				string menuPath = submenuItem.path;
				var separators = new[] { separator };
				var pathParts = menuPath.Split(separators);

				while (pathParts.Length - 1 < list.Count) {

					list.RemoveAt(list.Count - 1);

				}

				while (list.Count > 0 && pathParts[list.Count - 1] != list[list.Count - 1]) {

					list.RemoveAt(list.Count - 1);

				}

				while (pathParts.Length - 1 > list.Count) {

					elements.Add(new GroupElement(list.Count, pathParts[list.Count]));
					list.Add(pathParts[list.Count]);

				}

				elements.Add(new CallElement(list.Count, pathParts[pathParts.Length - 1], submenuItem));

			}

			_tree = elements.ToArray();
			for (int i = 0; i < _tree.Length; i++) {
				var elChilds = GetChildren(_tree, _tree[i]);
				if (elChilds.Count > maxElementCount)
					maxElementCount = elChilds.Count;
			}
			if (_stack.Count == 0) {
				_stack.Add(_tree[0] as GroupElement);
				goto to_research;
			}
			var parent = _tree[0] as GroupElement;
			var level = 0;
			to_startCycle:
			var stackElement = _stack[level];
			_stack[level] = parent;
			if (_stack[level] != null) {
				_stack[level].selectedIndex = stackElement.selectedIndex;
				_stack[level].scroll = stackElement.scroll;
			}
			level++;
			if (level != _stack.Count) {
				var childs = GetChildren(activeTree, parent);
				var child = childs.FirstOrDefault(x => _stack[level].name == x.name);
				if (child is GroupElement)
					parent = child as GroupElement;
				else
					while (_stack.Count > level)
						_stack.RemoveAt(level);
				goto to_startCycle;
			}
			to_research:
			s_DirtyList = false;
			RebuildSearch();
		}
		
		private int maxElementCount = 1;
		private static bool s_DirtyList = true;

		private void RebuildSearch() {
			if (!hasSearch) {
				_treeSearch = null;
				if (_stack[_stack.Count - 1].name == "Search") {
					_stack.Clear();
					_stack.Add(_tree[0] as GroupElement);
				}
				_animTarget = 1;
				_lastTime = DateTime.Now.Ticks;
			}
			else {
				var separatorSearch = new[] { ' ', separator };
				var searchLowerWords = searchText.ToLower().Split(separatorSearch);
				var firstElements = new System.Collections.Generic.List<Element>();
				var otherElements = new System.Collections.Generic.List<Element>();
				foreach (var element in _tree) {
					if (!(element is CallElement))
						continue;
					if (element.searchable == false) continue;
					var elementNameShortLower = element.name.ToLower().Replace(" ", string.Empty);
					var itsSearchableItem = true;
					var firstContainsFlag = false;
					for (int i = 0; i < searchLowerWords.Length; i++) {
						var searchLowerWord = searchLowerWords[i];
						if (elementNameShortLower.Contains(searchLowerWord)) {
							if (i == 0 && elementNameShortLower.StartsWith(searchLowerWord))
								firstContainsFlag = true;
						}
						else {
							itsSearchableItem = false;
							break;
						}
					}
					if (itsSearchableItem) {
						if (firstContainsFlag)
							firstElements.Add(element);
						else
							otherElements.Add(element);
					}
				}
				firstElements.Sort();
				otherElements.Sort();
				
				var searchElements = new System.Collections.Generic.List<Element>
				{ new GroupElement(0, "Search") };
				searchElements.AddRange(firstElements);
				searchElements.AddRange(otherElements);
				//            searchElements.Add(_tree[_tree.Length - 1]);
				_treeSearch = searchElements.ToArray();
				_stack.Clear();
				_stack.Add(_treeSearch[0] as GroupElement);
				if (GetChildren(activeTree, activeParent).Count >= 1)
					activeParent.selectedIndex = 0;
				else
					activeParent.selectedIndex = -1;
			}
		}
		
		public void OnGUI() {
			if (_tree == null) {
				Close();
				return; 
			}
			//Создание стиля
			if (styles == null)
				styles = new Styles();
			//Фон
			if (s_DirtyList)
				CreateComponentTree();
			HandleKeyboard();
			GUI.Label(new Rect(0, 0, position.width, position.height), GUIContent.none, styles.background);
			
			//Поиск
			if (useSearch) {
				GUILayout.Space(7f);
				var rectSearch = GUILayoutUtility.GetRect(10f, 20f);
				rectSearch.x += 8f;
				rectSearch.width -= 16f;
				EditorGUI.FocusTextInControl("ComponentSearch");
				GUI.SetNextControlName("ComponentSearch");
				if (SearchField(rectSearch, ref searchText))
					RebuildSearch();
			}
			
			//Элементы
			ListGUI(activeTree, _anim, GetElementRelative(0), GetElementRelative(-1));
			if (_anim < 1f && _stack.Count > 1)
				ListGUI(activeTree, _anim + 1f, GetElementRelative(-1), GetElementRelative(-2));
			if (_anim != _animTarget && Event.current.type == EventType.Repaint) {
				var ticks = DateTime.Now.Ticks;
				var coef = (ticks - _lastTime) / 1E+07f;
				_lastTime = ticks;
				_anim = Mathf.MoveTowards(_anim, _animTarget, coef * 4f);
				if (_animTarget == 0 && _anim == 0f) {
					_anim = 1f;
					_animTarget = 1;
					_stack.RemoveAt(_stack.Count - 1);
				}
				Repaint();
			}
		}
		
		private void HandleKeyboard() {
			Event current = Event.current;
			if (current.type == EventType.KeyDown) {
				if (current.keyCode == KeyCode.DownArrow) {
					activeParent.selectedIndex++;
					activeParent.selectedIndex = Mathf.Min(activeParent.selectedIndex,
					                                       GetChildren(activeTree, activeParent).Count - 1);
					scrollToSelected = true;
					current.Use();
				}
				if (current.keyCode == KeyCode.UpArrow) {
					GroupElement element2 = activeParent;
					element2.selectedIndex--;
					activeParent.selectedIndex = Mathf.Max(activeParent.selectedIndex, 0);
					scrollToSelected = true;
					current.Use();
				}
				if (current.keyCode == KeyCode.Return || current.keyCode == KeyCode.KeypadEnter) {
					GoToChild(activeElement, true);
					current.Use();
				}
				if (!hasSearch) {
					if (current.keyCode == KeyCode.LeftArrow || current.keyCode == KeyCode.Backspace) {
						GoToParent();
						current.Use();
					}
					if (current.keyCode == KeyCode.RightArrow) {
						GoToChild(activeElement, false);
						current.Use();
					}
					if (current.keyCode == KeyCode.Escape) {
						Close();
						current.Use();
					}
				}
			}
		}
		
		private static bool SearchField(Rect position, ref string text) {
			var rectField = position;
			rectField.width -= 15f;
			var startText = text;
			text = GUI.TextField(rectField, startText ?? "", styles.searchTextField);
			
			var rectCancel = position;
			rectCancel.x += position.width - 15f;
			rectCancel.width = 15f;
			var styleCancel = text == "" ? styles.searchCancelButtonEmpty : styles.searchCancelButton;
			if (GUI.Button(rectCancel, GUIContent.none, styleCancel) && text != "") {
				text = "";
				GUIUtility.keyboardControl = 0;
			}
			return startText != text;
		}
		
		private void ListGUI(Element[] tree, float anim, GroupElement parent, GroupElement grandParent) {
			anim = Mathf.Floor(anim) + Mathf.SmoothStep(0f, 1f, Mathf.Repeat(anim, 1f));
			Rect rectArea = position;
			rectArea.x = position.width * (1f - anim) + 1f;
			rectArea.y = useSearch ? 30f : 0;
			rectArea.height -= useSearch ? 30f : 0;
			rectArea.width -= 2f;
			GUILayout.BeginArea(rectArea);
			{
				var rectHeader = GUILayoutUtility.GetRect(10f, 25f);
				var nameHeader = parent.name;
				GUI.Label(rectHeader, nameHeader, styles.header);
				if (grandParent != null) {
					var rectHeaderBackArrow = new Rect(rectHeader.x + 4f, rectHeader.y + 7f, 13f, 13f);
					if (Event.current.type == EventType.Repaint)
						styles.leftArrow.Draw(rectHeaderBackArrow, false, false, false, false);
					if (Event.current.type == EventType.MouseDown && rectHeader.Contains(Event.current.mousePosition)) {
						GoToParent();
						Event.current.Use();
					}
				}
				ListGUI(tree, parent);
			}
			GUILayout.EndArea();
		}
		
		private void ListGUI(Element[] tree, GroupElement parent) {
			parent.scroll = GUILayout.BeginScrollView(parent.scroll, new GUILayoutOption[0]);
			EditorGUIUtility.SetIconSize(new Vector2(16f, 16f));
			var children = GetChildren(tree, parent);
			var rect = new Rect();
			for (int i = 0; i < children.Count; i++) {
				var e = children[i];
				var options = new[] { GUILayout.ExpandWidth(true) };
				var rectElement = GUILayoutUtility.GetRect(16f, elementHeight, options);
				if ((Event.current.type == EventType.MouseMove || Event.current.type == EventType.MouseDown) 
					&& parent.selectedIndex != i && rectElement.Contains(Event.current.mousePosition)) {
					parent.selectedIndex = i;
					Repaint();
				}
				bool on = false;
				if (i == parent.selectedIndex) {
					on = true;
					rect = rectElement;
				}
				if (Event.current.type == EventType.Repaint) {
					(e.content.image != null ? styles.componentItem : styles.groupItem).Draw(rectElement, e.content, false, false, on, on);
					if (!(e is CallElement)) {
						var rectElementForwardArrow = new Rect(rectElement.x + rectElement.width - 13f, rectElement.y + 4f, 13f, 13f);
						styles.rightArrow.Draw(rectElementForwardArrow, false, false, false, false);
					}
				}
				if (Event.current.type == EventType.MouseDown && rectElement.Contains(Event.current.mousePosition)) {
					Event.current.Use();
					parent.selectedIndex = i;
					GoToChild(e, true);
				}
			}
			EditorGUIUtility.SetIconSize(Vector2.zero);
			GUILayout.EndScrollView();
			if (scrollToSelected && Event.current.type == EventType.Repaint) {
				scrollToSelected = false;
				var lastRect = GUILayoutUtility.GetLastRect();
				if ((rect.yMax - lastRect.height) > parent.scroll.y) {
					parent.scroll.y = rect.yMax - lastRect.height;
					Repaint();
				}
				if (rect.y < parent.scroll.y) {
					parent.scroll.y = rect.y;
					Repaint();
				}
			}
		}
		
		private void GoToParent() {
			if (_stack.Count <= 1) 
				return;
			_animTarget = 0;
			_lastTime = DateTime.Now.Ticks;
		}
		
		private void GoToChild(Element e, bool addIfComponent) {
			var element = e as CallElement;
			if (element != null) {
				if (!addIfComponent) 
					return;
				element.action();
				if (this.autoClose == true) Close();
			}
			else if (!hasSearch) {
					_lastTime = DateTime.Now.Ticks;
					if (_animTarget == 0)
						_animTarget = 1;
					else if (_anim == 1f) {
							_anim = 0f;
							_stack.Add(e as GroupElement);
						}
				}
		}
		
		private System.Collections.Generic.List<Element> GetChildren(Element[] tree, Element parent) {
			var list = new System.Collections.Generic.List<Element>();
			var num = -1;
			var index = 0;
			while (index < tree.Length) {
				if (tree[index] == parent) {
					num = parent.level + 1;
					index++;
					break;
				}
				index++;
			}
			if (num == -1) 
				return list;
			while (index < tree.Length) {
				var item = tree[index];
				if (item.level < num)
					return list;
				if (item.level <= num || hasSearch)
					list.Add(item);
				index++;
			}
			return list;
		}
		
		private GroupElement GetElementRelative(int rel) {
			int num = (_stack.Count + rel) - 1;
			return num < 0 ? null : _stack[num];
		}
		
		
		private class CallElement : Element {
			public Action action;
			
			public CallElement(int level, string name, PopupItem item) {
				base.level = level;
				content = new GUIContent(name, item.image);
				action = () => {
					item.action();
					content = new GUIContent(name, item.image);
				};
				this.searchable = item.searchable;
			}
		}
		
		[Serializable]
		private class GroupElement : Element {
			public Vector2 scroll;
			public int selectedIndex;
			
			public GroupElement(int level, string name) {
				this.level = level;
				content = new GUIContent(name);
				this.searchable = true;
			}
		}
		
		private class Element : IComparable {
			public GUIContent content;
			public int level;
			public bool searchable;
			
			public string name { get { return content.text; } }
			
			public int CompareTo(object o) {
				return String.Compare(name, ((Element)o).name, StringComparison.Ordinal);
			}
		}
		
		private class Styles {
			public GUIStyle searchTextField = "SearchTextField";
			public GUIStyle searchCancelButton = "SearchCancelButton";
			public GUIStyle searchCancelButtonEmpty = "SearchCancelButtonEmpty";
			public GUIStyle background = "grey_border";
			public GUIStyle componentItem = new GUIStyle("PR Label");
			public GUIStyle groupItem;
			public GUIStyle header = new GUIStyle("In BigTitle");
			public GUIStyle leftArrow = "AC LeftArrow";
			public GUIStyle rightArrow = "AC RightArrow";
			
			public Styles() {
				header.font = EditorStyles.boldLabel.font;
				header.richText = true;
				componentItem.alignment = TextAnchor.MiddleLeft;
				componentItem.padding.left -= 15;
				componentItem.fixedHeight = 20f;
				componentItem.richText = true;
				groupItem = new GUIStyle(componentItem);
				groupItem.padding.left += 0x11;
				groupItem.richText = true;
			}
		}
		
		public class PopupItem {
			public PopupItem(string path, Action action) {
				this.path = path;
				this.action = action;
				this.searchable = true;
			}
			
			public PopupItem(string path, Action<PopupItem> action) {
				this.path = path;
				this.action = () => { action(this); };
				this.searchable = true;
			}

			public int order;
			public string path;
			public Texture2D image;
			public Action action;
			public bool searchable;

		}
	}
    
    public class Popup {
		/// <summary> Окно, которое связано с попапом </summary>
		internal PopupWindowAnim window;
		/// <summary> Прямоугольник, в котором будет отображен попап </summary>
		public Rect screenRect { get { return window.screenRect; } set { window.screenRect = value; } }
		
		/// <summary> Указывает, что является разделителем в пути </summary>
		public char separator { get { return window.separator; } set { window.separator = value; } }
		
		/// <summary> Позволяет использовать/убирать поиск </summary>
		public bool useSearch { get { return window.useSearch; } set { window.useSearch = value; } }

		/// <summary> Название рута </summary>
		public string title { get { return window.title; } set { window.title = value; } }

		/// <summary> Название рута </summary>
		public string searchText { get { return window.searchText; } set { window.searchText = value; } }

		/// <summary> Автоматически установить размер по высоте, узнав максимальное количество видимых элементов </summary>
		public bool autoHeight { get { return window.autoHeight; } set { window.autoHeight = value; } }
		public bool autoClose { get { return window.autoClose; } set { window.autoClose = value; } }
		
		/// <summary> Создание окна </summary>
		public Popup(Rect screenRect, bool useSearch = true, string title = "Menu", char separator = '/') {
			window = PopupWindowAnim.Create(screenRect, useSearch);
			this.title = title;
			this.separator = separator;
		}
		
		/// <summary> Создание окна </summary>
		public Popup(Vector2 size, bool useSearch = true, string title = "Menu", char separator = '/') {
			window = PopupWindowAnim.CreateBySize(size, useSearch);
			this.title = title;
			this.separator = separator;
		}
		
		/// <summary> Создание окна </summary>
		public Popup(float width, bool useSearch = true, string title = "Menu", char separator = '/', bool autoHeight = true) {
			window = PopupWindowAnim.Create(width, useSearch);
			this.title = title;
			this.separator = separator;
			this.autoHeight = autoHeight;
		}
		
		/// <summary> Создание окна </summary>
		public Popup(bool useSearch = true, string title = "Menu", char separator = '/', bool autoHeight = true) {
			window = PopupWindowAnim.Create(useSearch);
			this.title = title;
			this.separator = separator;
			this.autoHeight = autoHeight;
		}
		
		public void BeginFolder(string folderName) {
			window.BeginRoot(folderName);
		}
		
		public void EndFolder() {
			window.EndRoot();
		}
		
		public void EndFolderAll() {
			window.EndRootAll();
		}
		
		public void Item(string name) {
			window.Item(name);
		}
		
		public void Item(string name, Action action, int order = 0) {
			window.Item(name, null, action, order);
		}
		
		public void Item(string name, Texture2D image, Action action, int order = 0) {
			window.Item(name, image, action, order);
		}

		public void Item(string name, Texture2D image, Action<PopupWindowAnim.PopupItem> action, bool searchable = true) {
			window.Item(name, image, action, searchable);
		}

		public void Item(string name, Texture2D image, Action<PopupWindowAnim.PopupItem> action, bool searchable, int order) {
			window.Item(name, image, action, searchable, order);
		}

		public void ItemByPath(string path) {
			window.ItemByPath(path);
		}
		
		public void ItemByPath(string path, Action action) {
			window.ItemByPath(path, action);
		}
		
		public void ItemByPath(string path, Texture2D image, Action action) {
			window.ItemByPath(path, image, action);
		}
		
		public void Show() {
			window.Show();
		}
		
		public static void DrawInt(GUIContent label, string selected, System.Action<int> onResult, GUIContent[] options, int[] keys) {
			
			DrawInt_INTERNAL(new Rect(), selected, label, onResult, options, keys, true);
			
		}

		public static void DrawInt(Rect rect, string selected, GUIContent label, System.Action<int> onResult, GUIContent[] options, int[] keys) {

			DrawInt_INTERNAL(rect, selected, label, onResult, options, keys, false);

		}

		private static void DrawInt_INTERNAL(Rect rect, string selected, GUIContent label, System.Action<int> onResult, GUIContent[] options, int[] keys, bool layout) {

			var state = false;
			if (layout == true) {

				GUILayout.BeginHorizontal();
				if (label != null) GUILayout.Label(label);
				if (GUILayout.Button(selected, EditorStyles.popup) == true) {
					
					state = true;
					
				}
				GUILayout.EndHorizontal();

			} else {
				
				if (label != null) rect = EditorGUI.PrefixLabel(rect, label);
				if (GUI.Button(rect, selected, EditorStyles.popup) == true) {
					
					state = true;
					
				}
				
			}
			
			if (state == true) {

				Popup popup = null;
				if (layout == true) {

					popup = new Popup() { title = (label == null ? string.Empty : label.text), screenRect = new Rect(rect.x, rect.y + rect.height, rect.width, 200f) };
					
				} else {
					
					Vector2 vector = GUIUtility.GUIToScreenPoint(new Vector2(rect.x, rect.y));
					rect.x = vector.x;
					rect.y = vector.y;
					
					popup = new Popup() { title = (label == null ? string.Empty : label.text), screenRect = new Rect(rect.x, rect.y + rect.height, rect.width, 200f) };
					
				}
				
				for (int i = 0; i < options.Length; ++i) {
					
					var option = options[i];
					var result = keys[i];
					popup.ItemByPath(option.text, () => {
						
						onResult(result);
						
					});
					
				}
				
				popup.Show();

			}

		}

	}
    
}