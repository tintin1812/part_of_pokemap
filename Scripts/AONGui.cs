using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UnityEngine{

    public class AComponent{
        public delegate void Draw(AComponent self);
        public Draw draw;
    }

    public class AComponent_Button : AComponent{
        public delegate void OnClick();
    }

    public class AComponent_SelectionGrid : AComponent{
        public delegate void OnSelect(int next);
    }

    public class AComponent_Toggle : AComponent{
        public delegate void OnToggle(bool b);
    }

    public class AComponent_TextField : AComponent{
        public delegate void OnText(string text);
    }

    public class AComponent_BeginScrollView : AComponent{
        public delegate void OnScroll(Vector2 position);
    }

	public class AONGUI{

        public static AONGUIBehaviour Target{ get; set;}
		
		public static bool enabled { 
            get{
                return GUI.enabled;
            }
            set{
                GUI.enabled = value;
            }
        }

		public static GUISkin skin {
            get{
                return GUI.skin;
            }
            set{
                GUI.skin = value;
            }
        }

        public static bool changed {
            get{
                return GUI.changed;
            }
            set{
                GUI.changed = value;
            }
        }

		public static void BeginGroup(Rect position){
            if(Target == null){
                GUI.BeginGroup(position);
            }else
            {
                Target.AComponents.Add( new AComponent(){
                    draw = (AComponent self) =>{
                        GUI.BeginGroup(position);
                    }
                });
            }
		}

		public static void EndGroup(){
            if(Target == null){
                GUI.EndGroup();
            }else
            {
                Target.AComponents.Add( new AComponent(){
                    draw = (AComponent self) =>{
                        GUI.EndGroup();
                    }
                });
            }
		}
		
		// public static float HorizontalSlider(Rect position, float value, float leftValue, float rightValue){
        //     return GUI.HorizontalSlider(position, value, leftValue, rightValue);
		// }

		public static void SelectionGrid(Rect position, int selected, GUIContent[] contents, int xCount, GUIStyle style, AComponent_SelectionGrid.OnSelect onSelect){
            Target.AComponents.Add( new AComponent_SelectionGrid(){
                draw = (AComponent self) =>{
                    int next = GUI.SelectionGrid(position, selected, contents, xCount, style);
                    if(selected != next){
                        // onSelect(next);
                        Target.Actions += ()=>{
                            onSelect(next);
                        };
                    }
                }
            });
		}
        public static void SelectionGrid(Rect position, int selected, string[] texts, int xCount, GUIStyle style, AComponent_SelectionGrid.OnSelect onSelect){
            Target.AComponents.Add( new AComponent_SelectionGrid(){
                draw = (AComponent self) =>{
                    int next = GUI.SelectionGrid(position, selected, texts, xCount, style);
                    if(selected != next){
                        // onSelect(next);
                        Target.Actions += ()=>{
                            onSelect(next);
                        };
                    }
                }
            });
		}
        // public static int SelectionGrid(Rect position, int selected, GUIContent[] content, int xCount){
		// 	return GUI.SelectionGrid(position, selected, content, xCount);
		// }
        // public static int SelectionGrid(Rect position, int selected, Texture[] images, int xCount){
		// 	return GUI.SelectionGrid(position, selected, images, xCount);
		// }

		// public static bool Button(Rect position, GUIContent content, GUIStyle style){
		// 	return GUI.Button(position, content, style);
		// }
		// public static bool Button(Rect position, Texture image){
		// 	return GUI.Button(position, image);
		// }
      	// public static bool Button(Rect position, Texture image, GUIStyle style){
		// 	return GUI.Button(position, image, style);
		// }
       	public static void Button(Rect position, string text, GUIStyle style, AComponent_Button.OnClick onClick){
			var bt = new AComponent_Button(){
                draw = (AComponent self) =>{
                    if(GUI.Button(position, text, style)){
                        // onClick();
                        Target.Actions += ()=>{
                            onClick();
                        };
                    }
                }
            };
            Target.AComponents.Add(bt);
		}
       	public static void Button(Rect position, GUIContent content, AComponent_Button.OnClick onClick){
            var bt = new AComponent_Button(){
                draw = (AComponent self) =>{
                    if(GUI.Button(position, content)){
                        Target.Actions += ()=>{
                            onClick();
                        };
                        // onClick();
                    }
                }
            };
            Target.AComponents.Add(bt);
		}
        // public static bool Button(Rect position, string text){
        //     return GUI.Button(position, text);
        // }

        public static void Button(Rect position, string text, AComponent_Button.OnClick onClick){
            var bt = new AComponent_Button(){
                draw = (AComponent self) =>{
                    if(GUI.Button(position, text)){
                        // onClick();
                        Target.Actions += ()=>{
                            onClick();
                        };
                    }
                }
            };
            Target.AComponents.Add(bt);
		}

        public static void Button(Rect position, string text, KeyCode keycode, AComponent_Button.OnClick onClick){
            var bt = new AComponent_Button(){
                draw = (AComponent self) =>{
                    if(Input.GetKeyDown(keycode) || GUI.Button(position, text)){
                        // onClick();
                        Target.Actions += ()=>{
                            onClick();
                        };
                    }
                }
            };
            Target.AComponents.Add(bt);
		}

		public static void Box(Rect position, string text){
            Target.AComponents.Add( new AComponent(){
                draw = (AComponent self) =>{
                    GUI.Box(position, text);
                }
            });
		}
      	public static void Box(Rect position, Texture image){
            Target.AComponents.Add( new AComponent(){
                draw = (AComponent self) =>{
                    GUI.Box(position, image);
                }
            });
		}
        public static void Box(Rect position, GUIContent content){
            Target.AComponents.Add( new AComponent(){
                draw = (AComponent self) =>{
                    GUI.Box(position, content);
                }
            });
		}
        public static void Box(Rect position, string text, GUIStyle style){
            Target.AComponents.Add( new AComponent(){
                draw = (AComponent self) =>{
                    GUI.Box(position, text, style);
                }
            });
		}
        public static void Box(Rect position, Texture image, GUIStyle style){
            Target.AComponents.Add( new AComponent(){
                draw = (AComponent self) =>{
                    GUI.Box(position, image, style);
                }
            });
		}
        public static void Box(Rect position, GUIContent content, GUIStyle style){
            Target.AComponents.Add( new AComponent(){
                draw = (AComponent self) =>{
                    GUI.Box(position, content, style);
                }
            });
		}

		// public static string TextField(Rect position, string text, int maxLength, GUIStyle style){
		// 	return GUI.TextField(position, text, maxLength, style);
		// }
        // public static string TextField(Rect position, string text, GUIStyle style){
		// 	return GUI.TextField(position, text, style);
		// }
        public static void TextField(Rect position, string text, int maxLength, AComponent_TextField.OnText onText){
            Target.AComponents.Add(new AComponent_TextField(){
                draw = (AComponent self) =>{
                    string text_next = GUI.TextField(position, text, maxLength);
                    if(GUI.changed && text_next != text){
                        // onText(text_next);
                        Target.Actions += ()=>{
                            onText(text_next);
                        };
                    }
                }
            });
		}
        public static void TextField(Rect position, string text, AComponent_TextField.OnText onText){
            Target.AComponents.Add(new AComponent_TextField(){
                draw = (AComponent self) =>{
                    string text_next = GUI.TextField(position, text);
                    if(GUI.changed && text_next != text){
                        // onText(text_next);
                        Target.Actions += ()=>{
                            onText(text_next);
                        };
                    }
                }
            });
		}

		// public static bool Toggle(Rect position, bool value, Texture image, GUIStyle style){
		// 	return GUI.Toggle(position, value, image, style);
		// }
        // public static bool Toggle(Rect position, bool value, GUIContent content, GUIStyle style){
		// 	return GUI.Toggle(position, value, content, style);
		// }
       	// public static bool Toggle(Rect position, bool value, string text, GUIStyle style){
		// 	return GUI.Toggle(position, value, text, style);
		// }
        // public static bool Toggle(Rect position, bool value, GUIContent content){
		// 	return GUI.Toggle(position, value, content);
		// }
        // public static bool Toggle(Rect position, bool value, Texture image){
		// 	return GUI.Toggle(position, value, image);
		// }
        // public static bool Toggle(Rect position, int id, bool value, GUIContent content, GUIStyle style){
		// 	return GUI.Toggle(position, id, value, content, style);
		// }
        public static void Toggle(Rect position, bool value, string text, AComponent_Toggle.OnToggle onToggle){
            Target.AComponents.Add(new AComponent_Toggle(){
                draw = (AComponent self) =>{
                    bool b = GUI.Toggle(position, value, text);
                    if(b != value){
                        // onToggle(b);
                        Target.Actions += ()=>{
                            onToggle(b);
                        };
                    }
                }
            });
		}

		// public static Vector2 BeginScrollView(Rect position, Vector2 scrollPosition, Rect viewRect, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar){
		// 	return GUI.BeginScrollView( position, scrollPosition, viewRect, horizontalScrollbar, verticalScrollbar);
		// }
        // public static Vector2 BeginScrollView(Rect position, Vector2 scrollPosition, Rect viewRect){
		// 	return GUI.BeginScrollView( position, scrollPosition, viewRect);
		// }
        public static void BeginScrollView(Rect position, Vector2 scrollPosition, Rect viewRect, bool alwaysShowHorizontal, bool alwaysShowVertical, AComponent_BeginScrollView.OnScroll onScroll){
            Target.AComponents.Add(new AComponent_BeginScrollView(){
                draw = (AComponent self) =>{
                    Vector2 next = GUI.BeginScrollView( position, scrollPosition, viewRect, alwaysShowHorizontal, alwaysShowVertical);
                    if(GUI.changed && next != scrollPosition){
                        // onScroll(next);
                        Target.Actions += ()=>{
                            onScroll(next);
                        };
                    }
                }
            });
		}
        // public static Vector2 BeginScrollView(Rect position, Vector2 scrollPosition, Rect viewRect, bool alwaysShowHorizontal, bool alwaysShowVertical, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar){
        //     return GUI.BeginScrollView( position, scrollPosition, viewRect, alwaysShowHorizontal, alwaysShowVertical, horizontalScrollbar, verticalScrollbar);
		// }

		public static void EndScrollView(){
            
            Target.AComponents.Add(new AComponent(){
                draw = (AComponent self) =>{
                    GUI.EndScrollView();
                }
            });
		}
        // public static void EndScrollView(bool handleScrollWheel){
        //     GUI.EndScrollView(handleScrollWheel);
		// }

		public static void Label(Rect position, string text, GUIStyle style){
            Target.AComponents.Add( new AComponent(){
                draw = (AComponent self) =>{
                    GUI.Label(position, text, style);
                }
            });
		}
        public static void Label(Rect position, GUIContent content){
            Target.AComponents.Add( new AComponent(){
                draw = (AComponent self) =>{
                    GUI.Label(position, content);
                }
            });
		}
        public static void Label(Rect position, Texture image){
            Target.AComponents.Add( new AComponent(){
                draw = (AComponent self) =>{
                    GUI.Label(position, image);
                }
            });
		}
        public static void Label(Rect position, string text){
            Target.AComponents.Add( new AComponent(){
                draw = (AComponent self) =>{
                    GUI.Label(position, text);
                }
            });
		}
        public static void Label(Rect position, GUIContent content, GUIStyle style){
			Target.AComponents.Add( new AComponent(){
                draw = (AComponent self) =>{
                    GUI.Label(position, content, style);
                }
            });
		}
        public static void Label(Rect position, Texture image, GUIStyle style){
            Target.AComponents.Add( new AComponent(){
                draw = (AComponent self) =>{
                    GUI.Label(position, image, style);
                }
            });
		}

		// public static void DrawTexture(Rect position, Texture image, ScaleMode scaleMode, bool alphaBlend, float imageAspect, Color color, Vector4 borderWidths, Vector4 borderRadiuses){
        //     GUI.DrawTexture( position, image, scaleMode, alphaBlend, imageAspect, color, borderWidths, borderRadiuses);
		// }
        // public static void DrawTexture(Rect position, Texture image, ScaleMode scaleMode, bool alphaBlend, float imageAspect){
        //     GUI.DrawTexture( position, image, scaleMode, alphaBlend, imageAspect);
		// }
        // public static void DrawTexture(Rect position, Texture image, ScaleMode scaleMode, bool alphaBlend){
        //     GUI.DrawTexture( position, image, scaleMode, alphaBlend);
		// }
        // public static void DrawTexture(Rect position, Texture image, ScaleMode scaleMode){
        //     GUI.DrawTexture( position, image, scaleMode);
		// }
       	public static void DrawTexture(Rect position, Texture image){
            Target.AComponents.Add( new AComponent(){
                draw = (AComponent self) =>{
                    GUI.DrawTexture( position, image);
                }
            });
		}
        // public static void DrawTexture(Rect position, Texture image, ScaleMode scaleMode, bool alphaBlend, float imageAspect, Color color, Vector4 borderWidths, float borderRadius){
		// 	GUI.DrawTexture( position, image, scaleMode, alphaBlend, imageAspect, color, borderWidths, borderRadius);
		// }
        // public static void DrawTexture(Rect position, Texture image, ScaleMode scaleMode, bool alphaBlend, float imageAspect, Color color, float borderWidth, float borderRadius){
		// 	GUI.DrawTexture( position, image, scaleMode, alphaBlend, imageAspect, color, borderWidth, borderRadius);
		// }

        // public static void DrawTextureWithTexCoords(Rect position, Texture image, Rect texCoords){
        //     Target.AComponents.Add( new AComponent(){
        //         draw = (AComponent self) =>{
        //             GUI.DrawTextureWithTexCoords( position, image, texCoords);
        //         }
        //     });
        // }

        // public static string GetNameOfFocusedControl(){
        //     return GUI.GetNameOfFocusedControl();
        // }
		public static void FocusControl(string name){
            Target.AComponents.Add( new AComponent(){
                draw = (AComponent self) =>{
                    GUI.FocusControl(name);
                }
            });
		}
		public static void SetNextControlName(string name){
            Target.AComponents.Add( new AComponent(){
                draw = (AComponent self) =>{
                    GUI.SetNextControlName(name);
                }
            });
		}

        public static void AddOnGui(AComponent.Draw draw){
            Target.AComponents.Add( new AComponent(){
                draw = draw
            });
		}
		/*
        //
        // Summary:
        //     Tinting color for all text rendered by the GUI.
        public static Color contentColor { get; set; }
        //
        // Summary:
        //     Global tinting color for all background elements rendered by the GUI.
        public static Color backgroundColor { get; set; }
        //
        // Summary:
        //     Returns true if any controls changed the value of the input data.
        public static bool changed { get; set; }
        //
        // Summary:
        //     Is the GUI enabled?
        public static bool enabled { get; set; }
        //
        // Summary:
        //     Global tinting color for the GUI.
        public static Color color { get; set; }
        //
        // Summary:
        //     The sorting depth of the currently executing GUI behaviour.
        public static int depth { get; set; }
        //
        // Summary:
        //     The tooltip of the control the mouse is currently over, or which has keyboard
        //     focus. (Read Only).
        public static string tooltip { get; set; }
        //
        // Summary:
        //     The global skin to use.
        public static GUISkin skin { get; set; }
        //
        // Summary:
        //     The GUI transform matrix.
        public static Matrix4x4 matrix { get; set; }
        protected static string mouseTooltip { get; }
        protected static Rect tooltipRect { get; set; }

        public static void BeginClip(Rect position);
        public static void BeginClip(Rect position, Vector2 scrollOffset, Vector2 renderOffset, bool resetOffset);
        //
        // Summary:
        //     Begin a group. Must be matched with a call to EndGroup.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the group.
        //
        //   text:
        //     Text to display on the group.
        //
        //   image:
        //     Texture to display on the group.
        //
        //   content:
        //     Text, image and tooltip for this group. If supplied, any mouse clicks are "captured"
        //     by the group and not If left out, no background is rendered, and mouse clicks
        //     are passed.
        //
        //   style:
        //     The style to use for the background.
        public static void BeginGroup(Rect position, GUIContent content, GUIStyle style);
        //
        // Summary:
        //     Begin a group. Must be matched with a call to EndGroup.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the group.
        //
        //   text:
        //     Text to display on the group.
        //
        //   image:
        //     Texture to display on the group.
        //
        //   content:
        //     Text, image and tooltip for this group. If supplied, any mouse clicks are "captured"
        //     by the group and not If left out, no background is rendered, and mouse clicks
        //     are passed.
        //
        //   style:
        //     The style to use for the background.
        public static void BeginGroup(Rect position, Texture image, GUIStyle style);
        //
        // Summary:
        //     Begin a group. Must be matched with a call to EndGroup.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the group.
        //
        //   text:
        //     Text to display on the group.
        //
        //   image:
        //     Texture to display on the group.
        //
        //   content:
        //     Text, image and tooltip for this group. If supplied, any mouse clicks are "captured"
        //     by the group and not If left out, no background is rendered, and mouse clicks
        //     are passed.
        //
        //   style:
        //     The style to use for the background.
        public static void BeginGroup(Rect position, string text, GUIStyle style);
        //
        // Summary:
        //     Begin a group. Must be matched with a call to EndGroup.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the group.
        //
        //   text:
        //     Text to display on the group.
        //
        //   image:
        //     Texture to display on the group.
        //
        //   content:
        //     Text, image and tooltip for this group. If supplied, any mouse clicks are "captured"
        //     by the group and not If left out, no background is rendered, and mouse clicks
        //     are passed.
        //
        //   style:
        //     The style to use for the background.
        public static void BeginGroup(Rect position, GUIStyle style);
        //
        // Summary:
        //     Begin a group. Must be matched with a call to EndGroup.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the group.
        //
        //   text:
        //     Text to display on the group.
        //
        //   image:
        //     Texture to display on the group.
        //
        //   content:
        //     Text, image and tooltip for this group. If supplied, any mouse clicks are "captured"
        //     by the group and not If left out, no background is rendered, and mouse clicks
        //     are passed.
        //
        //   style:
        //     The style to use for the background.
        public static void BeginGroup(Rect position, string text);
        //
        // Summary:
        //     Begin a group. Must be matched with a call to EndGroup.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the group.
        //
        //   text:
        //     Text to display on the group.
        //
        //   image:
        //     Texture to display on the group.
        //
        //   content:
        //     Text, image and tooltip for this group. If supplied, any mouse clicks are "captured"
        //     by the group and not If left out, no background is rendered, and mouse clicks
        //     are passed.
        //
        //   style:
        //     The style to use for the background.
        public static void BeginGroup(Rect position, Texture image);
        //
        // Summary:
        //     Begin a group. Must be matched with a call to EndGroup.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the group.
        //
        //   text:
        //     Text to display on the group.
        //
        //   image:
        //     Texture to display on the group.
        //
        //   content:
        //     Text, image and tooltip for this group. If supplied, any mouse clicks are "captured"
        //     by the group and not If left out, no background is rendered, and mouse clicks
        //     are passed.
        //
        //   style:
        //     The style to use for the background.
        public static void BeginGroup(Rect position);
        //
        // Summary:
        //     Begin a group. Must be matched with a call to EndGroup.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the group.
        //
        //   text:
        //     Text to display on the group.
        //
        //   image:
        //     Texture to display on the group.
        //
        //   content:
        //     Text, image and tooltip for this group. If supplied, any mouse clicks are "captured"
        //     by the group and not If left out, no background is rendered, and mouse clicks
        //     are passed.
        //
        //   style:
        //     The style to use for the background.
        public static void BeginGroup(Rect position, GUIContent content);
        //
        // Summary:
        //     Begin a scrolling view inside your GUI.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the ScrollView.
        //
        //   scrollPosition:
        //     The pixel distance that the view is scrolled in the X and Y directions.
        //
        //   viewRect:
        //     The rectangle used inside the scrollview.
        //
        //   horizontalScrollbar:
        //     Optional GUIStyle to use for the horizontal scrollbar. If left out, the horizontalScrollbar
        //     style from the current GUISkin is used.
        //
        //   verticalScrollbar:
        //     Optional GUIStyle to use for the vertical scrollbar. If left out, the verticalScrollbar
        //     style from the current GUISkin is used.
        //
        //   alwaysShowHorizontal:
        //     Optional parameter to always show the horizontal scrollbar. If false or left
        //     out, it is only shown when viewRect is wider than position.
        //
        //   alwaysShowVertical:
        //     Optional parameter to always show the vertical scrollbar. If false or left out,
        //     it is only shown when viewRect is taller than position.
        //
        // Returns:
        //     The modified scrollPosition. Feed this back into the variable you pass in, as
        //     shown in the example.
        public static Vector2 BeginScrollView(Rect position, Vector2 scrollPosition, Rect viewRect, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar);
        //
        // Summary:
        //     Begin a scrolling view inside your GUI.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the ScrollView.
        //
        //   scrollPosition:
        //     The pixel distance that the view is scrolled in the X and Y directions.
        //
        //   viewRect:
        //     The rectangle used inside the scrollview.
        //
        //   horizontalScrollbar:
        //     Optional GUIStyle to use for the horizontal scrollbar. If left out, the horizontalScrollbar
        //     style from the current GUISkin is used.
        //
        //   verticalScrollbar:
        //     Optional GUIStyle to use for the vertical scrollbar. If left out, the verticalScrollbar
        //     style from the current GUISkin is used.
        //
        //   alwaysShowHorizontal:
        //     Optional parameter to always show the horizontal scrollbar. If false or left
        //     out, it is only shown when viewRect is wider than position.
        //
        //   alwaysShowVertical:
        //     Optional parameter to always show the vertical scrollbar. If false or left out,
        //     it is only shown when viewRect is taller than position.
        //
        // Returns:
        //     The modified scrollPosition. Feed this back into the variable you pass in, as
        //     shown in the example.
        public static Vector2 BeginScrollView(Rect position, Vector2 scrollPosition, Rect viewRect);
        //
        // Summary:
        //     Begin a scrolling view inside your GUI.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the ScrollView.
        //
        //   scrollPosition:
        //     The pixel distance that the view is scrolled in the X and Y directions.
        //
        //   viewRect:
        //     The rectangle used inside the scrollview.
        //
        //   horizontalScrollbar:
        //     Optional GUIStyle to use for the horizontal scrollbar. If left out, the horizontalScrollbar
        //     style from the current GUISkin is used.
        //
        //   verticalScrollbar:
        //     Optional GUIStyle to use for the vertical scrollbar. If left out, the verticalScrollbar
        //     style from the current GUISkin is used.
        //
        //   alwaysShowHorizontal:
        //     Optional parameter to always show the horizontal scrollbar. If false or left
        //     out, it is only shown when viewRect is wider than position.
        //
        //   alwaysShowVertical:
        //     Optional parameter to always show the vertical scrollbar. If false or left out,
        //     it is only shown when viewRect is taller than position.
        //
        // Returns:
        //     The modified scrollPosition. Feed this back into the variable you pass in, as
        //     shown in the example.
        public static Vector2 BeginScrollView(Rect position, Vector2 scrollPosition, Rect viewRect, bool alwaysShowHorizontal, bool alwaysShowVertical);
        //
        // Summary:
        //     Begin a scrolling view inside your GUI.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the ScrollView.
        //
        //   scrollPosition:
        //     The pixel distance that the view is scrolled in the X and Y directions.
        //
        //   viewRect:
        //     The rectangle used inside the scrollview.
        //
        //   horizontalScrollbar:
        //     Optional GUIStyle to use for the horizontal scrollbar. If left out, the horizontalScrollbar
        //     style from the current GUISkin is used.
        //
        //   verticalScrollbar:
        //     Optional GUIStyle to use for the vertical scrollbar. If left out, the verticalScrollbar
        //     style from the current GUISkin is used.
        //
        //   alwaysShowHorizontal:
        //     Optional parameter to always show the horizontal scrollbar. If false or left
        //     out, it is only shown when viewRect is wider than position.
        //
        //   alwaysShowVertical:
        //     Optional parameter to always show the vertical scrollbar. If false or left out,
        //     it is only shown when viewRect is taller than position.
        //
        // Returns:
        //     The modified scrollPosition. Feed this back into the variable you pass in, as
        //     shown in the example.
        public static Vector2 BeginScrollView(Rect position, Vector2 scrollPosition, Rect viewRect, bool alwaysShowHorizontal, bool alwaysShowVertical, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar);
        //
        // Summary:
        //     Create a Box on the GUI Layer.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the box.
        //
        //   text:
        //     Text to display on the box.
        //
        //   image:
        //     Texture to display on the box.
        //
        //   content:
        //     Text, image and tooltip for this box.
        //
        //   style:
        //     The style to use. If left out, the box style from the current GUISkin is used.
        public static void Box(Rect position, string text);
        //
        // Summary:
        //     Create a Box on the GUI Layer.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the box.
        //
        //   text:
        //     Text to display on the box.
        //
        //   image:
        //     Texture to display on the box.
        //
        //   content:
        //     Text, image and tooltip for this box.
        //
        //   style:
        //     The style to use. If left out, the box style from the current GUISkin is used.
        public static void Box(Rect position, Texture image);
        //
        // Summary:
        //     Create a Box on the GUI Layer.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the box.
        //
        //   text:
        //     Text to display on the box.
        //
        //   image:
        //     Texture to display on the box.
        //
        //   content:
        //     Text, image and tooltip for this box.
        //
        //   style:
        //     The style to use. If left out, the box style from the current GUISkin is used.
        public static void Box(Rect position, GUIContent content);
        //
        // Summary:
        //     Create a Box on the GUI Layer.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the box.
        //
        //   text:
        //     Text to display on the box.
        //
        //   image:
        //     Texture to display on the box.
        //
        //   content:
        //     Text, image and tooltip for this box.
        //
        //   style:
        //     The style to use. If left out, the box style from the current GUISkin is used.
        public static void Box(Rect position, string text, GUIStyle style);
        //
        // Summary:
        //     Create a Box on the GUI Layer.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the box.
        //
        //   text:
        //     Text to display on the box.
        //
        //   image:
        //     Texture to display on the box.
        //
        //   content:
        //     Text, image and tooltip for this box.
        //
        //   style:
        //     The style to use. If left out, the box style from the current GUISkin is used.
        public static void Box(Rect position, Texture image, GUIStyle style);
        //
        // Summary:
        //     Create a Box on the GUI Layer.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the box.
        //
        //   text:
        //     Text to display on the box.
        //
        //   image:
        //     Texture to display on the box.
        //
        //   content:
        //     Text, image and tooltip for this box.
        //
        //   style:
        //     The style to use. If left out, the box style from the current GUISkin is used.
        public static void Box(Rect position, GUIContent content, GUIStyle style);
        //
        // Summary:
        //     Bring a specific window to back of the floating windows.
        //
        // Parameters:
        //   windowID:
        //     The identifier used when you created the window in the Window call.
        public static void BringWindowToBack(int windowID);
        //
        // Summary:
        //     Bring a specific window to front of the floating windows.
        //
        // Parameters:
        //   windowID:
        //     The identifier used when you created the window in the Window call.
        public static void BringWindowToFront(int windowID);
        //
        // Summary:
        //     Make a single press button. The user clicks them and something happens immediately.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the button.
        //
        //   text:
        //     Text to display on the button.
        //
        //   image:
        //     Texture to display on the button.
        //
        //   content:
        //     Text, image and tooltip for this button.
        //
        //   style:
        //     The style to use. If left out, the button style from the current GUISkin is used.
        //
        // Returns:
        //     true when the users clicks the button.
        public static bool Button(Rect position, GUIContent content, GUIStyle style);
        //
        // Summary:
        //     Make a single press button. The user clicks them and something happens immediately.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the button.
        //
        //   text:
        //     Text to display on the button.
        //
        //   image:
        //     Texture to display on the button.
        //
        //   content:
        //     Text, image and tooltip for this button.
        //
        //   style:
        //     The style to use. If left out, the button style from the current GUISkin is used.
        //
        // Returns:
        //     true when the users clicks the button.
        public static bool Button(Rect position, Texture image);
        //
        // Summary:
        //     Make a single press button. The user clicks them and something happens immediately.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the button.
        //
        //   text:
        //     Text to display on the button.
        //
        //   image:
        //     Texture to display on the button.
        //
        //   content:
        //     Text, image and tooltip for this button.
        //
        //   style:
        //     The style to use. If left out, the button style from the current GUISkin is used.
        //
        // Returns:
        //     true when the users clicks the button.
        public static bool Button(Rect position, Texture image, GUIStyle style);
        //
        // Summary:
        //     Make a single press button. The user clicks them and something happens immediately.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the button.
        //
        //   text:
        //     Text to display on the button.
        //
        //   image:
        //     Texture to display on the button.
        //
        //   content:
        //     Text, image and tooltip for this button.
        //
        //   style:
        //     The style to use. If left out, the button style from the current GUISkin is used.
        //
        // Returns:
        //     true when the users clicks the button.
        public static bool Button(Rect position, string text, GUIStyle style);
        //
        // Summary:
        //     Make a single press button. The user clicks them and something happens immediately.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the button.
        //
        //   text:
        //     Text to display on the button.
        //
        //   image:
        //     Texture to display on the button.
        //
        //   content:
        //     Text, image and tooltip for this button.
        //
        //   style:
        //     The style to use. If left out, the button style from the current GUISkin is used.
        //
        // Returns:
        //     true when the users clicks the button.
        public static bool Button(Rect position, GUIContent content);
        //
        // Summary:
        //     Make a single press button. The user clicks them and something happens immediately.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the button.
        //
        //   text:
        //     Text to display on the button.
        //
        //   image:
        //     Texture to display on the button.
        //
        //   content:
        //     Text, image and tooltip for this button.
        //
        //   style:
        //     The style to use. If left out, the button style from the current GUISkin is used.
        //
        // Returns:
        //     true when the users clicks the button.
        public static bool Button(Rect position, string text);
        //
        // Summary:
        //     If you want to have the entire window background to act as a drag area, use the
        //     version of DragWindow that takes no parameters and put it at the end of the window
        //     function.
        public static void DragWindow();
        //
        // Summary:
        //     Make a window draggable.
        //
        // Parameters:
        //   position:
        //     The part of the window that can be dragged. This is clipped to the actual window.
        public static void DragWindow(Rect position);
        public static void DrawTexture(Rect position, Texture image, ScaleMode scaleMode, bool alphaBlend, float imageAspect, Color color, Vector4 borderWidths, Vector4 borderRadiuses);
        //
        // Summary:
        //     Draw a texture within a rectangle.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to draw the texture within.
        //
        //   image:
        //     Texture to display.
        //
        //   scaleMode:
        //     How to scale the image when the aspect ratio of it doesn't fit the aspect ratio
        //     to be drawn within.
        //
        //   alphaBlend:
        //     Whether to apply alpha blending when drawing the image (enabled by default).
        //
        //   imageAspect:
        //     Aspect ratio to use for the source image. If 0 (the default), the aspect ratio
        //     from the image is used. Pass in w/h for the desired aspect ratio. This allows
        //     the aspect ratio of the source image to be adjusted without changing the pixel
        //     width and height.
        public static void DrawTexture(Rect position, Texture image, ScaleMode scaleMode, bool alphaBlend, float imageAspect);
        //
        // Summary:
        //     Draw a texture within a rectangle.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to draw the texture within.
        //
        //   image:
        //     Texture to display.
        //
        //   scaleMode:
        //     How to scale the image when the aspect ratio of it doesn't fit the aspect ratio
        //     to be drawn within.
        //
        //   alphaBlend:
        //     Whether to apply alpha blending when drawing the image (enabled by default).
        //
        //   imageAspect:
        //     Aspect ratio to use for the source image. If 0 (the default), the aspect ratio
        //     from the image is used. Pass in w/h for the desired aspect ratio. This allows
        //     the aspect ratio of the source image to be adjusted without changing the pixel
        //     width and height.
        public static void DrawTexture(Rect position, Texture image, ScaleMode scaleMode, bool alphaBlend);
        //
        // Summary:
        //     Draw a texture within a rectangle.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to draw the texture within.
        //
        //   image:
        //     Texture to display.
        //
        //   scaleMode:
        //     How to scale the image when the aspect ratio of it doesn't fit the aspect ratio
        //     to be drawn within.
        //
        //   alphaBlend:
        //     Whether to apply alpha blending when drawing the image (enabled by default).
        //
        //   imageAspect:
        //     Aspect ratio to use for the source image. If 0 (the default), the aspect ratio
        //     from the image is used. Pass in w/h for the desired aspect ratio. This allows
        //     the aspect ratio of the source image to be adjusted without changing the pixel
        //     width and height.
        public static void DrawTexture(Rect position, Texture image, ScaleMode scaleMode);
        //
        // Summary:
        //     Draw a texture within a rectangle.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to draw the texture within.
        //
        //   image:
        //     Texture to display.
        //
        //   scaleMode:
        //     How to scale the image when the aspect ratio of it doesn't fit the aspect ratio
        //     to be drawn within.
        //
        //   alphaBlend:
        //     Whether to apply alpha blending when drawing the image (enabled by default).
        //
        //   imageAspect:
        //     Aspect ratio to use for the source image. If 0 (the default), the aspect ratio
        //     from the image is used. Pass in w/h for the desired aspect ratio. This allows
        //     the aspect ratio of the source image to be adjusted without changing the pixel
        //     width and height.
        public static void DrawTexture(Rect position, Texture image);
        //
        // Summary:
        //     Draws a border with rounded corners within a rectangle. The texture is used to
        //     pattern the border. Note that this method only works on shader model 2.5 and
        //     above.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to draw the texture within.
        //
        //   image:
        //     Texture to display.
        //
        //   scaleMode:
        //     How to scale the image when the aspect ratio of it doesn't fit the aspect ratio
        //     to be drawn within.
        //
        //   alphaBlend:
        //     Whether to apply alpha blending when drawing the image (enabled by default).
        //
        //   imageAspect:
        //     Aspect ratio to use for the source image. If 0 (the default), the aspect ratio
        //     from the image is used. Pass in w/h for the desired aspect ratio. This allows
        //     the aspect ratio of the source image to be adjusted without changing the pixel
        //     width and height.
        //
        //   color:
        //     A tint color to apply on the texture.
        //
        //   borderWidth:
        //     The width of the border. If 0, the full texture is drawn.
        //
        //   borderWidths:
        //     The width of the borders (left, top, right and bottom). If Vector4.zero, the
        //     full texture is drawn.
        //
        //   borderRadius:
        //     The radius for rounded corners. If 0, corners will not be rounded.
        //
        //   borderRadiuses:
        //     The radiuses for rounded corners (top-left, top-right, bottom-right and bottom-left).
        //     If Vector4.zero, corners will not be rounded.
        public static void DrawTexture(Rect position, Texture image, ScaleMode scaleMode, bool alphaBlend, float imageAspect, Color color, Vector4 borderWidths, float borderRadius);
        //
        // Summary:
        //     Draws a border with rounded corners within a rectangle. The texture is used to
        //     pattern the border. Note that this method only works on shader model 2.5 and
        //     above.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to draw the texture within.
        //
        //   image:
        //     Texture to display.
        //
        //   scaleMode:
        //     How to scale the image when the aspect ratio of it doesn't fit the aspect ratio
        //     to be drawn within.
        //
        //   alphaBlend:
        //     Whether to apply alpha blending when drawing the image (enabled by default).
        //
        //   imageAspect:
        //     Aspect ratio to use for the source image. If 0 (the default), the aspect ratio
        //     from the image is used. Pass in w/h for the desired aspect ratio. This allows
        //     the aspect ratio of the source image to be adjusted without changing the pixel
        //     width and height.
        //
        //   color:
        //     A tint color to apply on the texture.
        //
        //   borderWidth:
        //     The width of the border. If 0, the full texture is drawn.
        //
        //   borderWidths:
        //     The width of the borders (left, top, right and bottom). If Vector4.zero, the
        //     full texture is drawn.
        //
        //   borderRadius:
        //     The radius for rounded corners. If 0, corners will not be rounded.
        //
        //   borderRadiuses:
        //     The radiuses for rounded corners (top-left, top-right, bottom-right and bottom-left).
        //     If Vector4.zero, corners will not be rounded.
        public static void DrawTexture(Rect position, Texture image, ScaleMode scaleMode, bool alphaBlend, float imageAspect, Color color, float borderWidth, float borderRadius);
        //
        // Summary:
        //     Draw a texture within a rectangle with the given texture coordinates.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to draw the texture within.
        //
        //   image:
        //     Texture to display.
        //
        //   texCoords:
        //     How to scale the image when the aspect ratio of it doesn't fit the aspect ratio
        //     to be drawn within.
        //
        //   alphaBlend:
        //     Whether to alpha blend the image on to the display (the default). If false, the
        //     picture is drawn on to the display.
        public static void DrawTextureWithTexCoords(Rect position, Texture image, Rect texCoords, bool alphaBlend);
        //
        // Summary:
        //     Draw a texture within a rectangle with the given texture coordinates.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to draw the texture within.
        //
        //   image:
        //     Texture to display.
        //
        //   texCoords:
        //     How to scale the image when the aspect ratio of it doesn't fit the aspect ratio
        //     to be drawn within.
        //
        //   alphaBlend:
        //     Whether to alpha blend the image on to the display (the default). If false, the
        //     picture is drawn on to the display.
        public static void DrawTextureWithTexCoords(Rect position, Texture image, Rect texCoords);
        public static void EndClip();
        //
        // Summary:
        //     End a group.
        public static void EndGroup();
        //
        // Summary:
        //     Ends a scrollview started with a call to BeginScrollView.
        //
        // Parameters:
        //   handleScrollWheel:
        public static void EndScrollView();
        //
        // Summary:
        //     Ends a scrollview started with a call to BeginScrollView.
        //
        // Parameters:
        //   handleScrollWheel:
        public static void EndScrollView(bool handleScrollWheel);
        //
        // Summary:
        //     Move keyboard focus to a named control.
        //
        // Parameters:
        //   name:
        //     Name set using SetNextControlName.
        [FreeFunctionAttribute("GetGUIState().FocusKeyboardControl")]
        public static void FocusControl(string name);
        //
        // Summary:
        //     Make a window become the active window.
        //
        // Parameters:
        //   windowID:
        //     The identifier used when you created the window in the Window call.
        public static void FocusWindow(int windowID);
        //
        // Summary:
        //     Get the name of named control that has focus.
        [FreeFunctionAttribute("GetGUIState().GetNameOfFocusedControl")]
        public static string GetNameOfFocusedControl();
        //
        // Summary:
        //     Make a horizontal scrollbar. Scrollbars are what you use to scroll through a
        //     document. Most likely, you want to use scrollViews instead.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the scrollbar.
        //
        //   value:
        //     The position between min and max.
        //
        //   size:
        //     How much can we see?
        //
        //   leftValue:
        //     The value at the left end of the scrollbar.
        //
        //   rightValue:
        //     The value at the right end of the scrollbar.
        //
        //   style:
        //     The style to use for the scrollbar background. If left out, the horizontalScrollbar
        //     style from the current GUISkin is used.
        //
        // Returns:
        //     The modified value. This can be changed by the user by dragging the scrollbar,
        //     or clicking the arrows at the end.
        public static float HorizontalScrollbar(Rect position, float value, float size, float leftValue, float rightValue);
        //
        // Summary:
        //     Make a horizontal scrollbar. Scrollbars are what you use to scroll through a
        //     document. Most likely, you want to use scrollViews instead.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the scrollbar.
        //
        //   value:
        //     The position between min and max.
        //
        //   size:
        //     How much can we see?
        //
        //   leftValue:
        //     The value at the left end of the scrollbar.
        //
        //   rightValue:
        //     The value at the right end of the scrollbar.
        //
        //   style:
        //     The style to use for the scrollbar background. If left out, the horizontalScrollbar
        //     style from the current GUISkin is used.
        //
        // Returns:
        //     The modified value. This can be changed by the user by dragging the scrollbar,
        //     or clicking the arrows at the end.
        public static float HorizontalScrollbar(Rect position, float value, float size, float leftValue, float rightValue, GUIStyle style);
        //
        // Summary:
        //     A horizontal slider the user can drag to change a value between a min and a max.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the slider.
        //
        //   value:
        //     The value the slider shows. This determines the position of the draggable thumb.
        //
        //   leftValue:
        //     The value at the left end of the slider.
        //
        //   rightValue:
        //     The value at the right end of the slider.
        //
        //   slider:
        //     The GUIStyle to use for displaying the dragging area. If left out, the horizontalSlider
        //     style from the current GUISkin is used.
        //
        //   thumb:
        //     The GUIStyle to use for displaying draggable thumb. If left out, the horizontalSliderThumb
        //     style from the current GUISkin is used.
        //
        // Returns:
        //     The value that has been set by the user.
        public static float HorizontalSlider(Rect position, float value, float leftValue, float rightValue, GUIStyle slider, GUIStyle thumb);
        //
        // Summary:
        //     A horizontal slider the user can drag to change a value between a min and a max.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the slider.
        //
        //   value:
        //     The value the slider shows. This determines the position of the draggable thumb.
        //
        //   leftValue:
        //     The value at the left end of the slider.
        //
        //   rightValue:
        //     The value at the right end of the slider.
        //
        //   slider:
        //     The GUIStyle to use for displaying the dragging area. If left out, the horizontalSlider
        //     style from the current GUISkin is used.
        //
        //   thumb:
        //     The GUIStyle to use for displaying draggable thumb. If left out, the horizontalSliderThumb
        //     style from the current GUISkin is used.
        //
        // Returns:
        //     The value that has been set by the user.
        public static float HorizontalSlider(Rect position, float value, float leftValue, float rightValue);
        //
        // Summary:
        //     Make a text or texture label on screen.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the label.
        //
        //   text:
        //     Text to display on the label.
        //
        //   image:
        //     Texture to display on the label.
        //
        //   content:
        //     Text, image and tooltip for this label.
        //
        //   style:
        //     The style to use. If left out, the label style from the current GUISkin is used.
        public static void Label(Rect position, string text, GUIStyle style);
        //
        // Summary:
        //     Make a text or texture label on screen.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the label.
        //
        //   text:
        //     Text to display on the label.
        //
        //   image:
        //     Texture to display on the label.
        //
        //   content:
        //     Text, image and tooltip for this label.
        //
        //   style:
        //     The style to use. If left out, the label style from the current GUISkin is used.
        public static void Label(Rect position, GUIContent content);
        //
        // Summary:
        //     Make a text or texture label on screen.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the label.
        //
        //   text:
        //     Text to display on the label.
        //
        //   image:
        //     Texture to display on the label.
        //
        //   content:
        //     Text, image and tooltip for this label.
        //
        //   style:
        //     The style to use. If left out, the label style from the current GUISkin is used.
        public static void Label(Rect position, Texture image);
        //
        // Summary:
        //     Make a text or texture label on screen.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the label.
        //
        //   text:
        //     Text to display on the label.
        //
        //   image:
        //     Texture to display on the label.
        //
        //   content:
        //     Text, image and tooltip for this label.
        //
        //   style:
        //     The style to use. If left out, the label style from the current GUISkin is used.
        public static void Label(Rect position, string text);
        //
        // Summary:
        //     Make a text or texture label on screen.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the label.
        //
        //   text:
        //     Text to display on the label.
        //
        //   image:
        //     Texture to display on the label.
        //
        //   content:
        //     Text, image and tooltip for this label.
        //
        //   style:
        //     The style to use. If left out, the label style from the current GUISkin is used.
        public static void Label(Rect position, GUIContent content, GUIStyle style);
        //
        // Summary:
        //     Make a text or texture label on screen.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the label.
        //
        //   text:
        //     Text to display on the label.
        //
        //   image:
        //     Texture to display on the label.
        //
        //   content:
        //     Text, image and tooltip for this label.
        //
        //   style:
        //     The style to use. If left out, the label style from the current GUISkin is used.
        public static void Label(Rect position, Texture image, GUIStyle style);
        public static Rect ModalWindow(int id, Rect clientRect, WindowFunction func, GUIContent content, GUIStyle style);
        public static Rect ModalWindow(int id, Rect clientRect, WindowFunction func, Texture image, GUIStyle style);
        public static Rect ModalWindow(int id, Rect clientRect, WindowFunction func, string text, GUIStyle style);
        public static Rect ModalWindow(int id, Rect clientRect, WindowFunction func, GUIContent content);
        public static Rect ModalWindow(int id, Rect clientRect, WindowFunction func, string text);
        public static Rect ModalWindow(int id, Rect clientRect, WindowFunction func, Texture image);
        //
        // Summary:
        //     Make a text field where the user can enter a password.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the text field.
        //
        //   password:
        //     Password to edit. The return value of this function should be assigned back to
        //     the string as shown in the example.
        //
        //   maskChar:
        //     Character to mask the password with.
        //
        //   maxLength:
        //     The maximum length of the string. If left out, the user can type for ever and
        //     ever.
        //
        //   style:
        //     The style to use. If left out, the textField style from the current GUISkin is
        //     used.
        //
        // Returns:
        //     The edited password.
        public static string PasswordField(Rect position, string password, char maskChar, int maxLength, GUIStyle style);
        //
        // Summary:
        //     Make a text field where the user can enter a password.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the text field.
        //
        //   password:
        //     Password to edit. The return value of this function should be assigned back to
        //     the string as shown in the example.
        //
        //   maskChar:
        //     Character to mask the password with.
        //
        //   maxLength:
        //     The maximum length of the string. If left out, the user can type for ever and
        //     ever.
        //
        //   style:
        //     The style to use. If left out, the textField style from the current GUISkin is
        //     used.
        //
        // Returns:
        //     The edited password.
        public static string PasswordField(Rect position, string password, char maskChar);
        //
        // Summary:
        //     Make a text field where the user can enter a password.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the text field.
        //
        //   password:
        //     Password to edit. The return value of this function should be assigned back to
        //     the string as shown in the example.
        //
        //   maskChar:
        //     Character to mask the password with.
        //
        //   maxLength:
        //     The maximum length of the string. If left out, the user can type for ever and
        //     ever.
        //
        //   style:
        //     The style to use. If left out, the textField style from the current GUISkin is
        //     used.
        //
        // Returns:
        //     The edited password.
        public static string PasswordField(Rect position, string password, char maskChar, int maxLength);
        //
        // Summary:
        //     Make a text field where the user can enter a password.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the text field.
        //
        //   password:
        //     Password to edit. The return value of this function should be assigned back to
        //     the string as shown in the example.
        //
        //   maskChar:
        //     Character to mask the password with.
        //
        //   maxLength:
        //     The maximum length of the string. If left out, the user can type for ever and
        //     ever.
        //
        //   style:
        //     The style to use. If left out, the textField style from the current GUISkin is
        //     used.
        //
        // Returns:
        //     The edited password.
        public static string PasswordField(Rect position, string password, char maskChar, GUIStyle style);
        //
        // Summary:
        //     Make a button that is active as long as the user holds it down.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the button.
        //
        //   text:
        //     Text to display on the button.
        //
        //   image:
        //     Texture to display on the button.
        //
        //   content:
        //     Text, image and tooltip for this button.
        //
        //   style:
        //     The style to use. If left out, the button style from the current GUISkin is used.
        //
        // Returns:
        //     True when the users clicks the button.
        public static bool RepeatButton(Rect position, string text);
        //
        // Summary:
        //     Make a button that is active as long as the user holds it down.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the button.
        //
        //   text:
        //     Text to display on the button.
        //
        //   image:
        //     Texture to display on the button.
        //
        //   content:
        //     Text, image and tooltip for this button.
        //
        //   style:
        //     The style to use. If left out, the button style from the current GUISkin is used.
        //
        // Returns:
        //     True when the users clicks the button.
        public static bool RepeatButton(Rect position, Texture image);
        //
        // Summary:
        //     Make a button that is active as long as the user holds it down.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the button.
        //
        //   text:
        //     Text to display on the button.
        //
        //   image:
        //     Texture to display on the button.
        //
        //   content:
        //     Text, image and tooltip for this button.
        //
        //   style:
        //     The style to use. If left out, the button style from the current GUISkin is used.
        //
        // Returns:
        //     True when the users clicks the button.
        public static bool RepeatButton(Rect position, GUIContent content);
        //
        // Summary:
        //     Make a button that is active as long as the user holds it down.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the button.
        //
        //   text:
        //     Text to display on the button.
        //
        //   image:
        //     Texture to display on the button.
        //
        //   content:
        //     Text, image and tooltip for this button.
        //
        //   style:
        //     The style to use. If left out, the button style from the current GUISkin is used.
        //
        // Returns:
        //     True when the users clicks the button.
        public static bool RepeatButton(Rect position, string text, GUIStyle style);
        //
        // Summary:
        //     Make a button that is active as long as the user holds it down.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the button.
        //
        //   text:
        //     Text to display on the button.
        //
        //   image:
        //     Texture to display on the button.
        //
        //   content:
        //     Text, image and tooltip for this button.
        //
        //   style:
        //     The style to use. If left out, the button style from the current GUISkin is used.
        //
        // Returns:
        //     True when the users clicks the button.
        public static bool RepeatButton(Rect position, Texture image, GUIStyle style);
        //
        // Summary:
        //     Make a button that is active as long as the user holds it down.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the button.
        //
        //   text:
        //     Text to display on the button.
        //
        //   image:
        //     Texture to display on the button.
        //
        //   content:
        //     Text, image and tooltip for this button.
        //
        //   style:
        //     The style to use. If left out, the button style from the current GUISkin is used.
        //
        // Returns:
        //     True when the users clicks the button.
        public static bool RepeatButton(Rect position, GUIContent content, GUIStyle style);
        //
        // Summary:
        //     Scrolls all enclosing scrollviews so they try to make position visible.
        //
        // Parameters:
        //   position:
        public static void ScrollTo(Rect position);
        public static bool ScrollTowards(Rect position, float maxDelta);
        //
        // Summary:
        //     Make a grid of buttons.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the grid.
        //
        //   selected:
        //     The index of the selected grid button.
        //
        //   texts:
        //     An array of strings to show on the grid buttons.
        //
        //   images:
        //     An array of textures on the grid buttons.
        //
        //   contents:
        //     An array of text, image and tooltips for the grid button.
        //
        //   xCount:
        //     How many elements to fit in the horizontal direction. The controls will be scaled
        //     to fit unless the style defines a fixedWidth to use.
        //
        //   style:
        //     The style to use. If left out, the button style from the current GUISkin is used.
        //
        //   content:
        //
        // Returns:
        //     The index of the selected button.
        public static int SelectionGrid(Rect position, int selected, Texture[] images, int xCount, GUIStyle style);
        //
        // Summary:
        //     Make a grid of buttons.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the grid.
        //
        //   selected:
        //     The index of the selected grid button.
        //
        //   texts:
        //     An array of strings to show on the grid buttons.
        //
        //   images:
        //     An array of textures on the grid buttons.
        //
        //   contents:
        //     An array of text, image and tooltips for the grid button.
        //
        //   xCount:
        //     How many elements to fit in the horizontal direction. The controls will be scaled
        //     to fit unless the style defines a fixedWidth to use.
        //
        //   style:
        //     The style to use. If left out, the button style from the current GUISkin is used.
        //
        //   content:
        //
        // Returns:
        //     The index of the selected button.
        public static int SelectionGrid(Rect position, int selected, GUIContent[] contents, int xCount, GUIStyle style);
        //
        // Summary:
        //     Make a grid of buttons.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the grid.
        //
        //   selected:
        //     The index of the selected grid button.
        //
        //   texts:
        //     An array of strings to show on the grid buttons.
        //
        //   images:
        //     An array of textures on the grid buttons.
        //
        //   contents:
        //     An array of text, image and tooltips for the grid button.
        //
        //   xCount:
        //     How many elements to fit in the horizontal direction. The controls will be scaled
        //     to fit unless the style defines a fixedWidth to use.
        //
        //   style:
        //     The style to use. If left out, the button style from the current GUISkin is used.
        //
        //   content:
        //
        // Returns:
        //     The index of the selected button.
        public static int SelectionGrid(Rect position, int selected, string[] texts, int xCount, GUIStyle style);
        //
        // Summary:
        //     Make a grid of buttons.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the grid.
        //
        //   selected:
        //     The index of the selected grid button.
        //
        //   texts:
        //     An array of strings to show on the grid buttons.
        //
        //   images:
        //     An array of textures on the grid buttons.
        //
        //   contents:
        //     An array of text, image and tooltips for the grid button.
        //
        //   xCount:
        //     How many elements to fit in the horizontal direction. The controls will be scaled
        //     to fit unless the style defines a fixedWidth to use.
        //
        //   style:
        //     The style to use. If left out, the button style from the current GUISkin is used.
        //
        //   content:
        //
        // Returns:
        //     The index of the selected button.
        public static int SelectionGrid(Rect position, int selected, GUIContent[] content, int xCount);
        //
        // Summary:
        //     Make a grid of buttons.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the grid.
        //
        //   selected:
        //     The index of the selected grid button.
        //
        //   texts:
        //     An array of strings to show on the grid buttons.
        //
        //   images:
        //     An array of textures on the grid buttons.
        //
        //   contents:
        //     An array of text, image and tooltips for the grid button.
        //
        //   xCount:
        //     How many elements to fit in the horizontal direction. The controls will be scaled
        //     to fit unless the style defines a fixedWidth to use.
        //
        //   style:
        //     The style to use. If left out, the button style from the current GUISkin is used.
        //
        //   content:
        //
        // Returns:
        //     The index of the selected button.
        public static int SelectionGrid(Rect position, int selected, Texture[] images, int xCount);
        //
        // Summary:
        //     Make a grid of buttons.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the grid.
        //
        //   selected:
        //     The index of the selected grid button.
        //
        //   texts:
        //     An array of strings to show on the grid buttons.
        //
        //   images:
        //     An array of textures on the grid buttons.
        //
        //   contents:
        //     An array of text, image and tooltips for the grid button.
        //
        //   xCount:
        //     How many elements to fit in the horizontal direction. The controls will be scaled
        //     to fit unless the style defines a fixedWidth to use.
        //
        //   style:
        //     The style to use. If left out, the button style from the current GUISkin is used.
        //
        //   content:
        //
        // Returns:
        //     The index of the selected button.
        public static int SelectionGrid(Rect position, int selected, string[] texts, int xCount);
        //
        // Summary:
        //     Set the name of the next control.
        //
        // Parameters:
        //   name:
        [FreeFunctionAttribute("GetGUIState().SetNameOfNextControl")]
        public static void SetNextControlName(string name);
        public static float Slider(Rect position, float value, float size, float start, float end, GUIStyle slider, GUIStyle thumb, bool horiz, int id);
        //
        // Summary:
        //     Make a Multi-line text area where the user can edit a string.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the text field.
        //
        //   text:
        //     Text to edit. The return value of this function should be assigned back to the
        //     string as shown in the example.
        //
        //   maxLength:
        //     The maximum length of the string. If left out, the user can type for ever and
        //     ever.
        //
        //   style:
        //     The style to use. If left out, the textArea style from the current GUISkin is
        //     used.
        //
        // Returns:
        //     The edited string.
        public static string TextArea(Rect position, string text, int maxLength);
        //
        // Summary:
        //     Make a Multi-line text area where the user can edit a string.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the text field.
        //
        //   text:
        //     Text to edit. The return value of this function should be assigned back to the
        //     string as shown in the example.
        //
        //   maxLength:
        //     The maximum length of the string. If left out, the user can type for ever and
        //     ever.
        //
        //   style:
        //     The style to use. If left out, the textArea style from the current GUISkin is
        //     used.
        //
        // Returns:
        //     The edited string.
        public static string TextArea(Rect position, string text);
        //
        // Summary:
        //     Make a Multi-line text area where the user can edit a string.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the text field.
        //
        //   text:
        //     Text to edit. The return value of this function should be assigned back to the
        //     string as shown in the example.
        //
        //   maxLength:
        //     The maximum length of the string. If left out, the user can type for ever and
        //     ever.
        //
        //   style:
        //     The style to use. If left out, the textArea style from the current GUISkin is
        //     used.
        //
        // Returns:
        //     The edited string.
        public static string TextArea(Rect position, string text, int maxLength, GUIStyle style);
        //
        // Summary:
        //     Make a Multi-line text area where the user can edit a string.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the text field.
        //
        //   text:
        //     Text to edit. The return value of this function should be assigned back to the
        //     string as shown in the example.
        //
        //   maxLength:
        //     The maximum length of the string. If left out, the user can type for ever and
        //     ever.
        //
        //   style:
        //     The style to use. If left out, the textArea style from the current GUISkin is
        //     used.
        //
        // Returns:
        //     The edited string.
        public static string TextArea(Rect position, string text, GUIStyle style);
        //
        // Summary:
        //     Make a single-line text field where the user can edit a string.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the text field.
        //
        //   text:
        //     Text to edit. The return value of this function should be assigned back to the
        //     string as shown in the example.
        //
        //   maxLength:
        //     The maximum length of the string. If left out, the user can type for ever and
        //     ever.
        //
        //   style:
        //     The style to use. If left out, the textField style from the current GUISkin is
        //     used.
        //
        // Returns:
        //     The edited string.
        public static string TextField(Rect position, string text, int maxLength, GUIStyle style);
        //
        // Summary:
        //     Make a single-line text field where the user can edit a string.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the text field.
        //
        //   text:
        //     Text to edit. The return value of this function should be assigned back to the
        //     string as shown in the example.
        //
        //   maxLength:
        //     The maximum length of the string. If left out, the user can type for ever and
        //     ever.
        //
        //   style:
        //     The style to use. If left out, the textField style from the current GUISkin is
        //     used.
        //
        // Returns:
        //     The edited string.
        public static string TextField(Rect position, string text, GUIStyle style);
        //
        // Summary:
        //     Make a single-line text field where the user can edit a string.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the text field.
        //
        //   text:
        //     Text to edit. The return value of this function should be assigned back to the
        //     string as shown in the example.
        //
        //   maxLength:
        //     The maximum length of the string. If left out, the user can type for ever and
        //     ever.
        //
        //   style:
        //     The style to use. If left out, the textField style from the current GUISkin is
        //     used.
        //
        // Returns:
        //     The edited string.
        public static string TextField(Rect position, string text, int maxLength);
        //
        // Summary:
        //     Make a single-line text field where the user can edit a string.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the text field.
        //
        //   text:
        //     Text to edit. The return value of this function should be assigned back to the
        //     string as shown in the example.
        //
        //   maxLength:
        //     The maximum length of the string. If left out, the user can type for ever and
        //     ever.
        //
        //   style:
        //     The style to use. If left out, the textField style from the current GUISkin is
        //     used.
        //
        // Returns:
        //     The edited string.
        public static string TextField(Rect position, string text);
        //
        // Summary:
        //     Make an on/off toggle button.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the button.
        //
        //   value:
        //     Is this button on or off?
        //
        //   text:
        //     Text to display on the button.
        //
        //   image:
        //     Texture to display on the button.
        //
        //   content:
        //     Text, image and tooltip for this button.
        //
        //   style:
        //     The style to use. If left out, the toggle style from the current GUISkin is used.
        //
        // Returns:
        //     The new value of the button.
        public static bool Toggle(Rect position, bool value, Texture image, GUIStyle style);
        //
        // Summary:
        //     Make an on/off toggle button.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the button.
        //
        //   value:
        //     Is this button on or off?
        //
        //   text:
        //     Text to display on the button.
        //
        //   image:
        //     Texture to display on the button.
        //
        //   content:
        //     Text, image and tooltip for this button.
        //
        //   style:
        //     The style to use. If left out, the toggle style from the current GUISkin is used.
        //
        // Returns:
        //     The new value of the button.
        public static bool Toggle(Rect position, bool value, GUIContent content, GUIStyle style);
        //
        // Summary:
        //     Make an on/off toggle button.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the button.
        //
        //   value:
        //     Is this button on or off?
        //
        //   text:
        //     Text to display on the button.
        //
        //   image:
        //     Texture to display on the button.
        //
        //   content:
        //     Text, image and tooltip for this button.
        //
        //   style:
        //     The style to use. If left out, the toggle style from the current GUISkin is used.
        //
        // Returns:
        //     The new value of the button.
        public static bool Toggle(Rect position, bool value, string text, GUIStyle style);
        //
        // Summary:
        //     Make an on/off toggle button.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the button.
        //
        //   value:
        //     Is this button on or off?
        //
        //   text:
        //     Text to display on the button.
        //
        //   image:
        //     Texture to display on the button.
        //
        //   content:
        //     Text, image and tooltip for this button.
        //
        //   style:
        //     The style to use. If left out, the toggle style from the current GUISkin is used.
        //
        // Returns:
        //     The new value of the button.
        public static bool Toggle(Rect position, bool value, GUIContent content);
        //
        // Summary:
        //     Make an on/off toggle button.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the button.
        //
        //   value:
        //     Is this button on or off?
        //
        //   text:
        //     Text to display on the button.
        //
        //   image:
        //     Texture to display on the button.
        //
        //   content:
        //     Text, image and tooltip for this button.
        //
        //   style:
        //     The style to use. If left out, the toggle style from the current GUISkin is used.
        //
        // Returns:
        //     The new value of the button.
        public static bool Toggle(Rect position, bool value, Texture image);
        public static bool Toggle(Rect position, int id, bool value, GUIContent content, GUIStyle style);
        //
        // Summary:
        //     Make an on/off toggle button.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the button.
        //
        //   value:
        //     Is this button on or off?
        //
        //   text:
        //     Text to display on the button.
        //
        //   image:
        //     Texture to display on the button.
        //
        //   content:
        //     Text, image and tooltip for this button.
        //
        //   style:
        //     The style to use. If left out, the toggle style from the current GUISkin is used.
        //
        // Returns:
        //     The new value of the button.
        public static bool Toggle(Rect position, bool value, string text);
        //
        // Summary:
        //     Make a toolbar.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the toolbar.
        //
        //   selected:
        //     The index of the selected button.
        //
        //   texts:
        //     An array of strings to show on the toolbar buttons.
        //
        //   images:
        //     An array of textures on the toolbar buttons.
        //
        //   contents:
        //     An array of text, image and tooltips for the toolbar buttons.
        //
        //   style:
        //     The style to use. If left out, the button style from the current GUISkin is used.
        //
        //   buttonSize:
        //     Determines how toolbar button size is calculated.
        //
        // Returns:
        //     The index of the selected button.
        public static int Toolbar(Rect position, int selected, GUIContent[] contents, GUIStyle style);
        //
        // Summary:
        //     Make a toolbar.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the toolbar.
        //
        //   selected:
        //     The index of the selected button.
        //
        //   texts:
        //     An array of strings to show on the toolbar buttons.
        //
        //   images:
        //     An array of textures on the toolbar buttons.
        //
        //   contents:
        //     An array of text, image and tooltips for the toolbar buttons.
        //
        //   style:
        //     The style to use. If left out, the button style from the current GUISkin is used.
        //
        //   buttonSize:
        //     Determines how toolbar button size is calculated.
        //
        // Returns:
        //     The index of the selected button.
        public static int Toolbar(Rect position, int selected, string[] texts);
        //
        // Summary:
        //     Make a toolbar.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the toolbar.
        //
        //   selected:
        //     The index of the selected button.
        //
        //   texts:
        //     An array of strings to show on the toolbar buttons.
        //
        //   images:
        //     An array of textures on the toolbar buttons.
        //
        //   contents:
        //     An array of text, image and tooltips for the toolbar buttons.
        //
        //   style:
        //     The style to use. If left out, the button style from the current GUISkin is used.
        //
        //   buttonSize:
        //     Determines how toolbar button size is calculated.
        //
        // Returns:
        //     The index of the selected button.
        public static int Toolbar(Rect position, int selected, Texture[] images);
        //
        // Summary:
        //     Make a toolbar.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the toolbar.
        //
        //   selected:
        //     The index of the selected button.
        //
        //   texts:
        //     An array of strings to show on the toolbar buttons.
        //
        //   images:
        //     An array of textures on the toolbar buttons.
        //
        //   contents:
        //     An array of text, image and tooltips for the toolbar buttons.
        //
        //   style:
        //     The style to use. If left out, the button style from the current GUISkin is used.
        //
        //   buttonSize:
        //     Determines how toolbar button size is calculated.
        //
        // Returns:
        //     The index of the selected button.
        public static int Toolbar(Rect position, int selected, GUIContent[] contents);
        //
        // Summary:
        //     Make a toolbar.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the toolbar.
        //
        //   selected:
        //     The index of the selected button.
        //
        //   texts:
        //     An array of strings to show on the toolbar buttons.
        //
        //   images:
        //     An array of textures on the toolbar buttons.
        //
        //   contents:
        //     An array of text, image and tooltips for the toolbar buttons.
        //
        //   style:
        //     The style to use. If left out, the button style from the current GUISkin is used.
        //
        //   buttonSize:
        //     Determines how toolbar button size is calculated.
        //
        // Returns:
        //     The index of the selected button.
        public static int Toolbar(Rect position, int selected, string[] texts, GUIStyle style);
        //
        // Summary:
        //     Make a toolbar.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the toolbar.
        //
        //   selected:
        //     The index of the selected button.
        //
        //   texts:
        //     An array of strings to show on the toolbar buttons.
        //
        //   images:
        //     An array of textures on the toolbar buttons.
        //
        //   contents:
        //     An array of text, image and tooltips for the toolbar buttons.
        //
        //   style:
        //     The style to use. If left out, the button style from the current GUISkin is used.
        //
        //   buttonSize:
        //     Determines how toolbar button size is calculated.
        //
        // Returns:
        //     The index of the selected button.
        public static int Toolbar(Rect position, int selected, Texture[] images, GUIStyle style);
        public static int Toolbar(Rect position, int selected, GUIContent[] contents, GUIStyle style, ToolbarButtonSize buttonSize);
        //
        // Summary:
        //     Remove focus from all windows.
        public static void UnfocusWindow();
        //
        // Summary:
        //     Make a vertical scrollbar. Scrollbars are what you use to scroll through a document.
        //     Most likely, you want to use scrollViews instead.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the scrollbar.
        //
        //   value:
        //     The position between min and max.
        //
        //   size:
        //     How much can we see?
        //
        //   topValue:
        //     The value at the top of the scrollbar.
        //
        //   bottomValue:
        //     The value at the bottom of the scrollbar.
        //
        //   style:
        //     The style to use for the scrollbar background. If left out, the horizontalScrollbar
        //     style from the current GUISkin is used.
        //
        // Returns:
        //     The modified value. This can be changed by the user by dragging the scrollbar,
        //     or clicking the arrows at the end.
        public static float VerticalScrollbar(Rect position, float value, float size, float topValue, float bottomValue, GUIStyle style);
        //
        // Summary:
        //     Make a vertical scrollbar. Scrollbars are what you use to scroll through a document.
        //     Most likely, you want to use scrollViews instead.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the scrollbar.
        //
        //   value:
        //     The position between min and max.
        //
        //   size:
        //     How much can we see?
        //
        //   topValue:
        //     The value at the top of the scrollbar.
        //
        //   bottomValue:
        //     The value at the bottom of the scrollbar.
        //
        //   style:
        //     The style to use for the scrollbar background. If left out, the horizontalScrollbar
        //     style from the current GUISkin is used.
        //
        // Returns:
        //     The modified value. This can be changed by the user by dragging the scrollbar,
        //     or clicking the arrows at the end.
        public static float VerticalScrollbar(Rect position, float value, float size, float topValue, float bottomValue);
        //
        // Summary:
        //     A vertical slider the user can drag to change a value between a min and a max.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the slider.
        //
        //   value:
        //     The value the slider shows. This determines the position of the draggable thumb.
        //
        //   topValue:
        //     The value at the top end of the slider.
        //
        //   bottomValue:
        //     The value at the bottom end of the slider.
        //
        //   slider:
        //     The GUIStyle to use for displaying the dragging area. If left out, the horizontalSlider
        //     style from the current GUISkin is used.
        //
        //   thumb:
        //     The GUIStyle to use for displaying draggable thumb. If left out, the horizontalSliderThumb
        //     style from the current GUISkin is used.
        //
        // Returns:
        //     The value that has been set by the user.
        public static float VerticalSlider(Rect position, float value, float topValue, float bottomValue);
        //
        // Summary:
        //     A vertical slider the user can drag to change a value between a min and a max.
        //
        // Parameters:
        //   position:
        //     Rectangle on the screen to use for the slider.
        //
        //   value:
        //     The value the slider shows. This determines the position of the draggable thumb.
        //
        //   topValue:
        //     The value at the top end of the slider.
        //
        //   bottomValue:
        //     The value at the bottom end of the slider.
        //
        //   slider:
        //     The GUIStyle to use for displaying the dragging area. If left out, the horizontalSlider
        //     style from the current GUISkin is used.
        //
        //   thumb:
        //     The GUIStyle to use for displaying draggable thumb. If left out, the horizontalSliderThumb
        //     style from the current GUISkin is used.
        //
        // Returns:
        //     The value that has been set by the user.
        public static float VerticalSlider(Rect position, float value, float topValue, float bottomValue, GUIStyle slider, GUIStyle thumb);
        public static Rect Window(int id, Rect clientRect, WindowFunction func, Texture image, GUIStyle style);
        public static Rect Window(int id, Rect clientRect, WindowFunction func, string text);
        public static Rect Window(int id, Rect clientRect, WindowFunction func, Texture image);
        public static Rect Window(int id, Rect clientRect, WindowFunction func, GUIContent content);
        public static Rect Window(int id, Rect clientRect, WindowFunction func, string text, GUIStyle style);
        public static Rect Window(int id, Rect clientRect, WindowFunction func, GUIContent title, GUIStyle style);
        protected static Vector2 DoBeginScrollView(Rect position, Vector2 scrollPosition, Rect viewRect, bool alwaysShowHorizontal, bool alwaysShowVertical, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar, GUIStyle background);

        //
        // Summary:
        //     Determines how toolbar button size is calculated.
        public enum ToolbarButtonSize
        {
            //
            // Summary:
            //     Calculates the button size by dividing the available width by the number of buttons.
            //     The minimum size is the maximum content width.
            Fixed = 0,
            //
            // Summary:
            //     The width of each toolbar button is calculated based on the width of its content.
            FitToContents = 1
        }

        //
        // Summary:
        //     Disposable helper class for managing BeginGroup / EndGroup.
        public class GroupScope : Scope
        {
            //
            // Summary:
            //     Create a new GroupScope and begin the corresponding group.
            //
            // Parameters:
            //   position:
            //     Rectangle on the screen to use for the group.
            //
            //   text:
            //     Text to display on the group.
            //
            //   image:
            //     Texture to display on the group.
            //
            //   content:
            //     Text, image and tooltip for this group. If supplied, any mouse clicks are "captured"
            //     by the group and not If left out, no background is rendered, and mouse clicks
            //     are passed.
            //
            //   style:
            //     The style to use for the background.
            public GroupScope(Rect position);
            //
            // Summary:
            //     Create a new GroupScope and begin the corresponding group.
            //
            // Parameters:
            //   position:
            //     Rectangle on the screen to use for the group.
            //
            //   text:
            //     Text to display on the group.
            //
            //   image:
            //     Texture to display on the group.
            //
            //   content:
            //     Text, image and tooltip for this group. If supplied, any mouse clicks are "captured"
            //     by the group and not If left out, no background is rendered, and mouse clicks
            //     are passed.
            //
            //   style:
            //     The style to use for the background.
            public GroupScope(Rect position, string text);
            //
            // Summary:
            //     Create a new GroupScope and begin the corresponding group.
            //
            // Parameters:
            //   position:
            //     Rectangle on the screen to use for the group.
            //
            //   text:
            //     Text to display on the group.
            //
            //   image:
            //     Texture to display on the group.
            //
            //   content:
            //     Text, image and tooltip for this group. If supplied, any mouse clicks are "captured"
            //     by the group and not If left out, no background is rendered, and mouse clicks
            //     are passed.
            //
            //   style:
            //     The style to use for the background.
            public GroupScope(Rect position, Texture image);
            //
            // Summary:
            //     Create a new GroupScope and begin the corresponding group.
            //
            // Parameters:
            //   position:
            //     Rectangle on the screen to use for the group.
            //
            //   text:
            //     Text to display on the group.
            //
            //   image:
            //     Texture to display on the group.
            //
            //   content:
            //     Text, image and tooltip for this group. If supplied, any mouse clicks are "captured"
            //     by the group and not If left out, no background is rendered, and mouse clicks
            //     are passed.
            //
            //   style:
            //     The style to use for the background.
            public GroupScope(Rect position, GUIContent content);
            //
            // Summary:
            //     Create a new GroupScope and begin the corresponding group.
            //
            // Parameters:
            //   position:
            //     Rectangle on the screen to use for the group.
            //
            //   text:
            //     Text to display on the group.
            //
            //   image:
            //     Texture to display on the group.
            //
            //   content:
            //     Text, image and tooltip for this group. If supplied, any mouse clicks are "captured"
            //     by the group and not If left out, no background is rendered, and mouse clicks
            //     are passed.
            //
            //   style:
            //     The style to use for the background.
            public GroupScope(Rect position, GUIStyle style);
            //
            // Summary:
            //     Create a new GroupScope and begin the corresponding group.
            //
            // Parameters:
            //   position:
            //     Rectangle on the screen to use for the group.
            //
            //   text:
            //     Text to display on the group.
            //
            //   image:
            //     Texture to display on the group.
            //
            //   content:
            //     Text, image and tooltip for this group. If supplied, any mouse clicks are "captured"
            //     by the group and not If left out, no background is rendered, and mouse clicks
            //     are passed.
            //
            //   style:
            //     The style to use for the background.
            public GroupScope(Rect position, string text, GUIStyle style);
            //
            // Summary:
            //     Create a new GroupScope and begin the corresponding group.
            //
            // Parameters:
            //   position:
            //     Rectangle on the screen to use for the group.
            //
            //   text:
            //     Text to display on the group.
            //
            //   image:
            //     Texture to display on the group.
            //
            //   content:
            //     Text, image and tooltip for this group. If supplied, any mouse clicks are "captured"
            //     by the group and not If left out, no background is rendered, and mouse clicks
            //     are passed.
            //
            //   style:
            //     The style to use for the background.
            public GroupScope(Rect position, Texture image, GUIStyle style);

            protected override void CloseScope();
        }
        //
        // Summary:
        //     Disposable helper class for managing BeginScrollView / EndScrollView.
        public class ScrollViewScope : Scope
        {
            //
            // Summary:
            //     Create a new ScrollViewScope and begin the corresponding ScrollView.
            //
            // Parameters:
            //   position:
            //     Rectangle on the screen to use for the ScrollView.
            //
            //   scrollPosition:
            //     The pixel distance that the view is scrolled in the X and Y directions.
            //
            //   viewRect:
            //     The rectangle used inside the scrollview.
            //
            //   alwaysShowHorizontal:
            //     Optional parameter to always show the horizontal scrollbar. If false or left
            //     out, it is only shown when clientRect is wider than position.
            //
            //   alwaysShowVertical:
            //     Optional parameter to always show the vertical scrollbar. If false or left out,
            //     it is only shown when clientRect is taller than position.
            //
            //   horizontalScrollbar:
            //     Optional GUIStyle to use for the horizontal scrollbar. If left out, the horizontalScrollbar
            //     style from the current GUISkin is used.
            //
            //   verticalScrollbar:
            //     Optional GUIStyle to use for the vertical scrollbar. If left out, the verticalScrollbar
            //     style from the current GUISkin is used.
            public ScrollViewScope(Rect position, Vector2 scrollPosition, Rect viewRect);
            //
            // Summary:
            //     Create a new ScrollViewScope and begin the corresponding ScrollView.
            //
            // Parameters:
            //   position:
            //     Rectangle on the screen to use for the ScrollView.
            //
            //   scrollPosition:
            //     The pixel distance that the view is scrolled in the X and Y directions.
            //
            //   viewRect:
            //     The rectangle used inside the scrollview.
            //
            //   alwaysShowHorizontal:
            //     Optional parameter to always show the horizontal scrollbar. If false or left
            //     out, it is only shown when clientRect is wider than position.
            //
            //   alwaysShowVertical:
            //     Optional parameter to always show the vertical scrollbar. If false or left out,
            //     it is only shown when clientRect is taller than position.
            //
            //   horizontalScrollbar:
            //     Optional GUIStyle to use for the horizontal scrollbar. If left out, the horizontalScrollbar
            //     style from the current GUISkin is used.
            //
            //   verticalScrollbar:
            //     Optional GUIStyle to use for the vertical scrollbar. If left out, the verticalScrollbar
            //     style from the current GUISkin is used.
            public ScrollViewScope(Rect position, Vector2 scrollPosition, Rect viewRect, bool alwaysShowHorizontal, bool alwaysShowVertical);
            //
            // Summary:
            //     Create a new ScrollViewScope and begin the corresponding ScrollView.
            //
            // Parameters:
            //   position:
            //     Rectangle on the screen to use for the ScrollView.
            //
            //   scrollPosition:
            //     The pixel distance that the view is scrolled in the X and Y directions.
            //
            //   viewRect:
            //     The rectangle used inside the scrollview.
            //
            //   alwaysShowHorizontal:
            //     Optional parameter to always show the horizontal scrollbar. If false or left
            //     out, it is only shown when clientRect is wider than position.
            //
            //   alwaysShowVertical:
            //     Optional parameter to always show the vertical scrollbar. If false or left out,
            //     it is only shown when clientRect is taller than position.
            //
            //   horizontalScrollbar:
            //     Optional GUIStyle to use for the horizontal scrollbar. If left out, the horizontalScrollbar
            //     style from the current GUISkin is used.
            //
            //   verticalScrollbar:
            //     Optional GUIStyle to use for the vertical scrollbar. If left out, the verticalScrollbar
            //     style from the current GUISkin is used.
            public ScrollViewScope(Rect position, Vector2 scrollPosition, Rect viewRect, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar);
            //
            // Summary:
            //     Create a new ScrollViewScope and begin the corresponding ScrollView.
            //
            // Parameters:
            //   position:
            //     Rectangle on the screen to use for the ScrollView.
            //
            //   scrollPosition:
            //     The pixel distance that the view is scrolled in the X and Y directions.
            //
            //   viewRect:
            //     The rectangle used inside the scrollview.
            //
            //   alwaysShowHorizontal:
            //     Optional parameter to always show the horizontal scrollbar. If false or left
            //     out, it is only shown when clientRect is wider than position.
            //
            //   alwaysShowVertical:
            //     Optional parameter to always show the vertical scrollbar. If false or left out,
            //     it is only shown when clientRect is taller than position.
            //
            //   horizontalScrollbar:
            //     Optional GUIStyle to use for the horizontal scrollbar. If left out, the horizontalScrollbar
            //     style from the current GUISkin is used.
            //
            //   verticalScrollbar:
            //     Optional GUIStyle to use for the vertical scrollbar. If left out, the verticalScrollbar
            //     style from the current GUISkin is used.
            public ScrollViewScope(Rect position, Vector2 scrollPosition, Rect viewRect, bool alwaysShowHorizontal, bool alwaysShowVertical, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar);

            //
            // Summary:
            //     The modified scrollPosition. Feed this back into the variable you pass in, as
            //     shown in the example.
            public Vector2 scrollPosition { get; }
            //
            // Summary:
            //     Whether this ScrollView should handle scroll wheel events. (default: true).
            public bool handleScrollWheel { get; set; }

            protected override void CloseScope();
        }
		*/
	}
}

