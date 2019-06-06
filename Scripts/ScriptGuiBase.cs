using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using PygmyMonkey.FileBrowser;
using UnityEngine;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace AON.RpgMapEditor
{
    public class ScriptGuiBase
    {
        [System.Serializable]
        public class ScriptYaml
        {
            public ScriptYaml Init()
            {
                Main = new List<ActionData>();
                HasUsingRef = false;
                return this;
            }
            // [YamlMember(Alias = "input", ApplyNamingConventions = false)]
            // public Input Input { get; set; }

            [YamlMember(Alias = "main", ApplyNamingConventions = false)]
            public List<ActionData> Main { get; set; }

            public static string Key_ShowInteractions = "ShowInteractions";
            public static string[] LockFlag = {
                "ShowInteractions", // 0
			};
            public static bool IsShowInteractions( Flags flag)
            {
                if (flag != null && flag.ContainsKey("ShowInteractions") && flag["ShowInteractions"] >= 1)
                {
                    return true;
                }
                return false;
            }

            [YamlMember(Alias = "has_using_ref", ApplyNamingConventions = false)]
            public bool HasUsingRef { get; set; }

            [YamlMember(Alias = "ref_begin", ApplyNamingConventions = false)]
            public int RefBegin { get; set; }

            [YamlMember(Alias = "begin", ApplyNamingConventions = false)]
            public ActionData Begin { get; set; }

            [YamlMember(Alias = "flag", ApplyNamingConventions = false)]
            public Flags FlagsYaml { get; set; }

            [YamlMember(Alias = "flag_reset", ApplyNamingConventions = false)]
            public List<string> FlagReset { get; set; }

            [YamlMember(Alias = "flag_action", ApplyNamingConventions = false)]
            public List<FlagAction> FlagActions { get; set; }

            public ActionData ActionDataAt(int idx)
            {
                //Check
                if (idx < 0 || idx >= Main.Count)
                {
                    return null;
                }
                return Main[idx];
            }

            public void SetActionData(int idx, ActionData a)
            {
                if (idx < 0 || idx >= Main.Count)
                {
                    return;
                }
                a.id = idx;
                Main[idx] = a;
            }

            public FlagAction ActionFlagAt(int idx)
            {
                //Check
                if (idx < 0 || idx >= FlagActions.Count)
                {
                    return null;
                }
                return FlagActions[idx];
            }

            public List<ActionData> ListActions()
            {
                return Main;
            }

            public void ActionDataAdd(ActionData a)
            {
                Main.Add(a);
            }

            public void ActionDataInsert(int index, ActionData a)
            {
                Main.Insert(index, a);
            }

            public void ActionRemoveAt(int i)
            {
                Main.RemoveAt(i);
            }

            // [YamlMember(Alias = "msgBox", ApplyNamingConventions = false)]
            // public List<MsgBox> MsgBox { get; set; }
            public void ObjToID()
            {
                if(HasUsingRef){
                    return;
                }
                HasUsingRef = true;
                for (int i = 0; i < Main.Count; i++)
                {
                    Main[i].id = i;
                }
                for (int i = 0; i < Main.Count; i++)
                {
                    var act = Main[i];
                    if (act.Check != null)
                    {
                        if (act.Check.Right != null && Main.IndexOf(act.Check.Right) >= 0)
                        {
                            act.Check.RefRight = act.Check.Right.id;
                        }
                        else
                        {
                            act.Check.RefRight = -1;
                        }
                        act.Check.Right = null;
                        if (act.Check.Wrong != null && Main.IndexOf(act.Check.Wrong) >= 0)
                        {
                            act.Check.RefWrong = act.Check.Wrong.id;
                        }
                        else
                        {
                            act.Check.RefWrong = -1;
                        }
                        act.Check.Wrong = null;
                        if (act.Check.SubCheck != null)
                        {
                            for (int j = 0; j < act.Check.SubCheck.Count; j++)
                            {
                                var subCheck = act.Check.SubCheck[j];
                                if (subCheck.Right != null && Main.IndexOf(subCheck.Right) >= 0)
                                {
                                    subCheck.RefRight = subCheck.Right.id;
                                }
                                else
                                {
                                    subCheck.RefRight = -1;
                                }
                                subCheck.Right = null;
                            }
                        }
                    }
                    if (act.Set != null)
                    {
                        if (act.Set.Next != null && Main.IndexOf(act.Set.Next) >= 0)
                        {
                            act.Set.RefNext = act.Set.Next.id;
                        }
                        else
                        {
                            act.Set.RefNext = -1;
                        }
                        act.Set.Next = null;
                    }
                    if (act.MsgboxChat != null)
                    {
                        foreach (var chat in act.MsgboxChat)
                        {
                            if (chat.MsgboxChoise != null)
                            {
                                foreach (var choise in chat.MsgboxChoise)
                                {
                                    if (choise.Next != null && Main.IndexOf(choise.Next) >= 0)
                                    {
                                        choise.RefNext = choise.Next.id;
                                    }
                                    else
                                    {
                                        choise.RefNext = -1;
                                    }
                                    choise.Next = null;

                                    if (choise.Action != null && FlagActions.IndexOf(choise.Action) >= 0)
                                    {
                                        choise.RefAction = FlagActions.IndexOf(choise.Action);
                                    }
                                    else
                                    {
                                        choise.RefAction = -1;
                                    }
                                    choise.Action = null;
                                }
                            }
                        }
                    }
                    if (act.NPCAction != null)
                    {
                        if (act.NPCAction.Next != null && Main.IndexOf(act.NPCAction.Next) >= 0)
                        {
                            act.NPCAction.RefNext = act.NPCAction.Next.id;
                        }
                        else
                        {
                            act.NPCAction.RefNext = -1;
                        }
                        act.NPCAction.Next = null;
                    }

                    if (act.MainCharAction != null)
                    {
                        if (act.MainCharAction.Next != null && Main.IndexOf(act.MainCharAction.Next) >= 0)
                        {
                            act.MainCharAction.RefNext = act.MainCharAction.Next.id;
                        }
                        else
                        {
                            act.MainCharAction.RefNext = -1;
                        }
                        act.MainCharAction.Next = null;

                        if (act.MainCharAction.Wrong != null && Main.IndexOf(act.MainCharAction.Wrong) >= 0)
                        {
                            act.MainCharAction.RefWrong = act.MainCharAction.Wrong.id;
                        }
                        else
                        {
                            act.MainCharAction.RefWrong = -1;
                        }
                        act.MainCharAction.Wrong = null;
                    }
                }
                {
                    if (Begin != null && Main.IndexOf(Begin) >= 0)
                    {
                        RefBegin = Begin.id;
                    }
                    else
                    {
                        RefBegin = -1;
                    }
                    Begin = null;
                }
            }

            public void IdToObj()
            {
                HasUsingRef = false;
                for (int i = 0; i < Main.Count; i++)
                {
                    var act = Main[i];
                    if (act.Check != null)
                    {
                        if (act.Check.Right == null && act.Check.RefRight != -1)
                        {
                            act.Check.Right = ActionDataAt(act.Check.RefRight);
                        }
                        if (act.Check.Wrong == null && act.Check.RefWrong != -1)
                        {
                            act.Check.Wrong = ActionDataAt(act.Check.RefWrong);
                        }
                        if (act.Check.SubCheck != null)
                        {
                            for (int j = 0; j < act.Check.SubCheck.Count; j++)
                            {
                                var subCheck = act.Check.SubCheck[j];
                                if (subCheck.Right == null && subCheck.RefRight != -1)
                                {
                                    subCheck.Right = ActionDataAt(subCheck.RefRight);
                                }
                            }
                        }
                    }
                    if (act.Set != null)
                    {
                        if (act.Set.Next == null && act.Set.RefNext != -1)
                        {
                            act.Set.Next = ActionDataAt(act.Set.RefNext);
                        }
                    }
                    if (act.MsgboxChat != null)
                    {
                        foreach (var chat in act.MsgboxChat)
                        {
                            if (chat.MsgboxChoise != null)
                            {
                                foreach (var choise in chat.MsgboxChoise)
                                {
                                    if (choise.Next == null && choise.RefNext != -1)
                                    {
                                        choise.Next = ActionDataAt(choise.RefNext);
                                    }
                                    if (choise.Action == null && choise.RefAction != -1)
                                    {
                                        choise.Action = ActionFlagAt(choise.RefAction);
                                    }
                                }
                            }
                        }
                    }
                    if (act.NPCAction != null)
                    {
                        if (act.NPCAction.Next == null && act.NPCAction.RefNext != -1)
                        {
                            act.NPCAction.Next = ActionDataAt(act.NPCAction.RefNext);
                        }
                    }
                    if (act.MainCharAction != null)
                    {
                        if (act.MainCharAction.Next == null && act.MainCharAction.RefNext != -1)
                        {
                            act.MainCharAction.Next = ActionDataAt(act.MainCharAction.RefNext);
                        }
                        if (act.MainCharAction.Wrong == null && act.MainCharAction.RefWrong != -1)
                        {
                            act.MainCharAction.Wrong = ActionDataAt(act.MainCharAction.RefWrong);
                        }
                    }
                }
                {
                    if (Begin == null && RefBegin != -1)
                    {
                        Begin = ActionDataAt(RefBegin);
                    }
                }
            }

            public void UpdateRefActionFlagInMsgChat()
            {
                for (int i = 0; i < Main.Count; i++)
                {
                    var act = Main[i];
                    if (act.MsgboxChat != null)
                    {
                        foreach (var chat in act.MsgboxChat)
                        {
                            if (chat.MsgboxChoise != null)
                            {
                                foreach (var choise in chat.MsgboxChoise)
                                {
                                    if (choise.Action != null && FlagActions.IndexOf(choise.Action) < 0)
                                    {
                                        var v = FlagAction.FindFlagAction(FlagActions, choise.Action.Name);
                                        if (v != null)
                                        {
                                            choise.Action = v;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public enum EFormatScope : int
        {
            End = 0,
            Check,
            Set,
            MsgboxChat,
            NPC,
            MainChar,
            // Switch,
        };
        public static string[] StrCountSwitch = { "1", "2", "3", "4", "5", "6", "7", "8" };
        public static string[] StrFlagTarget = Enum.GetNames(typeof(FlagTarget));
        public enum FlagTarget : int
        {
            Script = 0,
            Map = 1,
            World = 2,
        };

        public static string[] StrEOperation = {
            " = ",
            " + ",
            " - ",
            " x ",
            " / ",
            " a + b ",
            " a - b",
            " a x b",
            " a / b",
            " %(a/b)",
            " %a/(a+b)",
        };

        public enum EOperation : int
        {
            Set = 0,
            Add,
            Sub,
            Multi,
            Divi,
            Add_A_B,
            Sub_A_B,
            Multi_A_B,
            Divi_A_B,
            Divi_A_B_1,
            Divi_A_B_2,
        };

        public static string[] StrECompare = {
            " = ",
            " != ",
            " > ",
            " < ",
            " >= ",
            " <= ",
        };

        public enum ECompare : int
        {
            Equal = 0,
            NotEqual,
            Greater,
            Less,
            Greater_Equal,
            Less_Equal,
        };

        public static bool Compare_A_B(int a, int b, ECompare c)
        {
            switch (c)
            {
                case ECompare.Equal:
                    {
                        return (a == b);
                    }
                case ECompare.NotEqual:
                    {
                        return a != b;
                    }
                case ECompare.Greater:
                    {
                        return a > b;
                    }
                case ECompare.Less:
                    {
                        return a < b;
                    }
                case ECompare.Greater_Equal:
                    {
                        return a >= b;
                    }
                case ECompare.Less_Equal:
                    {
                        return a <= b;
                    }
            }
            return false;
        }

        [System.Serializable]
        public class SubCheck
        {
            public SubCheck Init(){
                return this;
            }

            [YamlMember(Alias = "compare", ApplyNamingConventions = false)]
            public int Compare { get; set; }
            [YamlMember(Alias = "value", ApplyNamingConventions = false)]
            public int Value { get; set; }

            [YamlMember(Alias = "ref_right", ApplyNamingConventions = false)]
            public int RefRight { get; set; }

            [YamlMember(Alias = "right", ApplyNamingConventions = false)]
            public ActionData Right { get; set; }
        }

        [System.Serializable]
        public class Check
        {
            public Check Init(){
                Target = (int)FlagTarget.Script;
                Flag = "";
                Compare = 0;
                Value = 0;
                RefWrong = -1;
                RefRight = -1;
                return this;
            }

            [YamlMember(Alias = "target", ApplyNamingConventions = false)]
            public int Target { get; set; }

            [YamlMember(Alias = "flag", ApplyNamingConventions = false)]
            public string Flag { get; set; }

            [YamlMember(Alias = "compare", ApplyNamingConventions = false)]
            public int Compare { get; set; }

            [YamlMember(Alias = "value", ApplyNamingConventions = false)]
            public int Value { get; set; }

            [YamlMember(Alias = "ref_wrong", ApplyNamingConventions = false)]
            public int RefWrong { get; set; }

            [YamlMember(Alias = "wrong", ApplyNamingConventions = false)]
            public ActionData Wrong { get; set; }

            [YamlMember(Alias = "ref_right", ApplyNamingConventions = false)]
            public int RefRight { get; set; }

            [YamlMember(Alias = "right", ApplyNamingConventions = false)]
            public ActionData Right { get; set; }

            [YamlMember(Alias = "sub", ApplyNamingConventions = false)]
            public List<SubCheck> SubCheck { get; set; }
        }

        [System.Serializable]
        public class Set
        {
            public Set Init(){
                Target = (int)FlagTarget.Script;
                Action = "";
                // Flag = "";
                // Operation = 0;
                // Value = 0;
                RefNext = -1;
                return this;
            }

            [YamlMember(Alias = "target", ApplyNamingConventions = false)]
            public int Target { get; set; }

            [YamlMember(Alias = "action", ApplyNamingConventions = false)]
            public string Action { get; set; }

            // [YamlMember(Alias = "flag", ApplyNamingConventions = false)]
            // public string Flag { get; set; }

            // [YamlMember(Alias = "o", ApplyNamingConventions = false)]
            // public int Operation { get; set; }

            // [YamlMember(Alias = "value", ApplyNamingConventions = false)]
            // public int Value { get; set; }

            [YamlMember(Alias = "ref_next", ApplyNamingConventions = false)]
            public int RefNext { get; set; }

            [YamlMember(Alias = "next", ApplyNamingConventions = false)]
            public ActionData Next { get; set; }
        }

        [System.Serializable]
        public class MsgboxChat
        {
            public MsgboxChat Init()
            {
                Type = EType.Chat;
                // Value = "";
                Random = false;
                Group = 1;
                BreakLine = false;
                return this;
            }

            public enum EType : int
            {
                Chat = 0,
                Choise,
                Group2,
                Group3,
                Group4,
            };

            // public static string[] StrEType = Enum.GetNames (typeof(EType));
            public static string[] StrEType = {
                "Chat",
                "Choice",
                "Group2",
                "Group3",
                "Group4"
            };

            [YamlMember(Alias = "type", ApplyNamingConventions = false)]
            public EType Type { get; set; }

            [YamlMember(Alias = "random", ApplyNamingConventions = false)]
            public bool Random { get; set; }

            [YamlMember(Alias = "group", ApplyNamingConventions = false)]
            public int Group { get; set; }

            [YamlMember(Alias = "msgboxChoise", ApplyNamingConventions = false)]
            public List<MsgboxChoise> MsgboxChoise { get; set; }

            [YamlMember(Alias = "break", ApplyNamingConventions = false)]
            public bool BreakLine { get; set; }

            [YamlMember(Alias = "randomOnce", ApplyNamingConventions = false)]
            public bool RandomOnce { get; set; }

            [YamlMember(Alias = "highlight", ApplyNamingConventions = false)]
            public bool Highlight { get; set; }

            [YamlMember(Alias = "shuffle", ApplyNamingConventions = false)]
            public bool Shuffle { get; set; }
        }

        [System.Serializable]
        public class MsgboxChoise
        {
            public MsgboxChoise Init()
            {
                Value = "";
                RefNext = -1;
                RefAction = -1;
                return this;
            }

            [YamlMember(Alias = "rate", ApplyNamingConventions = false)]
            public int Rate { get; set; }

            [YamlMember(Alias = "value", ApplyNamingConventions = false)]
            public string Value { get; set; }

            [YamlMember(Alias = "ref_next", ApplyNamingConventions = false)]
            public int RefNext { get; set; }

            [YamlMember(Alias = "next", ApplyNamingConventions = false)]
            public ActionData Next { get; set; }

            [YamlMember(Alias = "ref_action", ApplyNamingConventions = false)]
            public int RefAction { get; set; }
            [YamlMember(Alias = "action", ApplyNamingConventions = false)]
            public FlagAction Action { get; set; }

            [YamlMember(Alias = "hc", ApplyNamingConventions = false)]
            public string ContentHighlight { get; set; }

            [YamlMember(Alias = "ht", ApplyNamingConventions = false)]
            public int TypeHighlight { get; set; }

            public static string[] StrTypeHighlight = {
                "Non",
                "Full green",
                "Filter red",
            };

            public enum ETypeHighlight : int
            {
                Non = 0,
                Full_Green = 1,
                Words_Red,
            };
        }

        [System.Serializable]
        public class NPCAction
        {
            public NPCAction Init()
            {
                UsingNPCTarget = true;
                // IdNpc = -1;
                // Action = EAction.NULL;
                // x = 0;
                // y = 0;
                RefNext = -1;
                return this;
            }

            [YamlMember(Alias = "usingnpctarget", ApplyNamingConventions = false)]
            public bool UsingNPCTarget { get; set; }
        
            [YamlMember(Alias = "id", ApplyNamingConventions = false)]
            public int IdNpc { get; set; }

            [YamlMember(Alias = "action", ApplyNamingConventions = false)]
            public EAction Action { get; set; }

            public enum EAction : int
            {
                NULL = 0,
                Show = 1,
                Hide = 2,
                Move = 3,
                LookAtCharacter = 4,
                Animation_Talk,
                Animation_Face_Up,
                Animation_Face_Down,
                Animation_Face_Left,
                Animation_Face_Right,
            };

            [YamlMember(Alias = "x", ApplyNamingConventions = false)]
            public int x { get; set; }

            [YamlMember(Alias = "y", ApplyNamingConventions = false)]
            public int y { get; set; }

            [YamlMember(Alias = "ref_next", ApplyNamingConventions = false)]
            public int RefNext { get; set; }
            [YamlMember(Alias = "next", ApplyNamingConventions = false)]
            public ActionData Next { get; set; }
        }

        [System.Serializable]
        public class MainCharAction
        {
            public MainCharAction Init()
            {
                RefNext = -1;
                RefWrong = -1;
                return this;
            }

            [YamlMember(Alias = "ref_next", ApplyNamingConventions = false)]
            public int RefNext { get; set; }
            [YamlMember(Alias = "next", ApplyNamingConventions = false)]
            public ActionData Next { get; set; }

            [YamlMember(Alias = "ref_wrong", ApplyNamingConventions = false)]
            public int RefWrong { get; set; }
            [YamlMember(Alias = "wrong", ApplyNamingConventions = false)]
            public ActionData Wrong { get; set; }

            [YamlMember(Alias = "action", ApplyNamingConventions = false)]
            public EAction Action { get; set; }

            public enum EAction : int
            {
                NULL = 0,
                WaitMoveToPos = 1,
                WaitMoveToHouse = 2,
                WaitInteractionsNPC = 3,
                RewardItem = 4,
                CheckItem = 5,
                BuyItem = 6,
                WarpTo = 7,
            };

            // WaitMoveToPos
            [YamlMember(Alias = "x", ApplyNamingConventions = false)]
            public int x { get; set; }

            [YamlMember(Alias = "y", ApplyNamingConventions = false)]
            public int y { get; set; }

            // WaitInteractionsNPC
            [YamlMember(Alias = "idHouse", ApplyNamingConventions = false)]
            public int IdHouse { get; set; }

            // WaitInteractionsNPC
            [YamlMember(Alias = "idNpc", ApplyNamingConventions = false)]
            public int IdNpc { get; set; }

            [YamlMember(Alias = "idWarp", ApplyNamingConventions = false)]
            public int IdWarp { get; set; }

            //RewardItem
            [YamlMember(Alias = "slugItem", ApplyNamingConventions = false)]
            public string SlugItem { get; set; }

            //RewardItem
            [YamlMember(Alias = "slugPackage", ApplyNamingConventions = false)]
            public string SlugPackage { get; set; }

            //Tip
            [YamlMember(Alias = "tip", ApplyNamingConventions = false)]
            public string Tip { get; set; }
        }

        [System.Serializable]
        public class ActionData
        {
            public ActionData Init()
            {
                format = EFormatScope.End;
                return this;
            }

            public void Reset()
            {
                Check = null;
                Set = null;
                MsgboxChat = null;
                NPCAction = null;
                MainCharAction = null;
            }


            [YamlMember(Alias = "id", ApplyNamingConventions = false)]
            public int id { get; set; }

            [YamlMember(Alias = "randomOnce", ApplyNamingConventions = false)]
            public bool RandomOnce { get; set; }

            private EFormatScope format;
            [YamlMember(Alias = "format", ApplyNamingConventions = false)]
            public EFormatScope Format
            {
                get
                {
                    return format;
                }
                set
                {
                    Reset();
                    format = value;
                    switch (format)
                    {
                        case EFormatScope.End:
                            {
                            }
                            break;
                        case EFormatScope.Check:
                            {
                                Check = new Check().Init();
                            }
                            break;
                        case EFormatScope.Set:
                            {
                                Set = new Set().Init();
                            }
                            break;
                        case EFormatScope.MsgboxChat:
                            {
                                MsgboxChat = new List<MsgboxChat>();
                            }
                            break;
                        case EFormatScope.NPC:
                            {
                                NPCAction = new NPCAction().Init();
                            }
                            break;
                        case EFormatScope.MainChar:
                            {
                                MainCharAction = new MainCharAction().Init();
                            }
                            break;
                    }
                }
            }

            [YamlMember(Alias = "name", ApplyNamingConventions = false)]
            public string Name { get; set; }

            [YamlMember(Alias = "check", ApplyNamingConventions = false)]
            public Check Check { get; set; }

            [YamlMember(Alias = "set", ApplyNamingConventions = false)]
            public Set Set { get; set; }

            [YamlMember(Alias = "msgboxChat", ApplyNamingConventions = false)]
            public List<MsgboxChat> MsgboxChat { get; set; }

            [YamlMember(Alias = "npc", ApplyNamingConventions = false)]
            public NPCAction NPCAction { get; set; }

            [YamlMember(Alias = "mainchar", ApplyNamingConventions = false)]
            public MainCharAction MainCharAction { get; set; }
        }
    }
}