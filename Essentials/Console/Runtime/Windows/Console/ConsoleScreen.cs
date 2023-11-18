using System.Linq;
using UnityEngine.UI.Windows.Components;
using UnityEngine.UI.Windows.WindowTypes;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows.Runtime.Windows {

    public class ConsoleScreen : LayoutWindowType, IDataSource, IConsoleScreen {

        public Font consoleFont;
        private int fixedCharWidth;
        private int fixedCharHeight;
        
        private ListComponent list;
        private ListComponent fastLinks;
        private InputFieldComponent inputField;
        private LogsCounterComponent logsCounterComponent;
        
        private char openCloseChar;
        private int currentIndex;

        private WindowSystemConsoleModule consoleModule;
        private WindowSystemConsole GetConsole() {

            if (this.consoleModule is null == false) return this.consoleModule.console;
            
            var module = WindowSystem.GetWindowSystemModule<WindowSystemConsoleModule>();
            if (module != null) {

                this.consoleModule = module;

            }
            return module.console;

        }
        
        public void OnParametersPass(char openCloseChar) {

            this.openCloseChar = openCloseChar;

        }

        public override void OnEmptyPass() {

            this.openCloseChar = '`';

        }
        
        public override void OnInit() {
            
            base.OnInit();

            this.GetLayoutComponent(out this.list);
            this.GetLayoutComponent(out this.fastLinks);
            this.GetLayoutComponent(out this.inputField);
            this.GetLayoutComponent(out this.logsCounterComponent);
            
            this.inputField.SetCallbackValidateChar(this.OnChar);
            this.inputField.SetCallbackEditEnd(this.OnEditEnd);
            this.inputField.Get<ButtonComponent>().SetCallback(this.EndEdit);

            this.consoleFont.RequestCharactersInTexture("A", 16, FontStyle.Normal);
            this.consoleFont.GetCharacterInfo('A', out var charInfo, 16, FontStyle.Normal);
            this.fixedCharWidth = charInfo.glyphWidth;
            this.fixedCharHeight = charInfo.glyphHeight;

        }

        public override void OnShowBegin() {
            
            base.OnShowBegin();
            
            WindowSystem.AddInteractablesIgnoreContainer(this);

            this.logsCounterComponent.SetInfo();

        }

        public override void OnHideBegin() {
            
            WindowSystem.RemoveInteractablesIgnoreContainer(this);
            
            base.OnHideBegin();
            
        }

        public void PrintModuleSample() {
            
            var console = this.GetConsole();
            console.PrintModuleSample();

        }

        public bool HasLogFilterType(LogType logType) {

            var console = this.GetConsole();
            return console.HasLogFilterType(logType);

        }

        public WindowSystemConsole.LogsFilter GetLogFilterType() {
            
            var console = this.GetConsole();
            return console.GetLogFilterType();
            
        }

        public void SetLogFilterType(LogType logType, bool state) {
            
            var console = this.GetConsole();
            console.SetLogFilterType(logType, state);
            
        }

        public int GetCounter(LogType logType) {

            var console = this.GetConsole();
            return console.GetCounter(logType);
            
        }

        private void AddLog(string text, LogType type = LogType.Log, string trace = null) {

            var console = this.GetConsole();
            console.AddLog(text, type, trace);

        }

        public void ScrollDown() {

            this.list.GetComponent<ScrollRect>().verticalNormalizedPosition = 0f;

        }

        public void ClearInput() {

            this.inputField.Clear();

        }

        public void ReplaceInput(string str) {
            
            this.inputField.SetText(str);
            
        }

        private void EndEdit() {
            
            this.ApplyCommand(this.inputField.GetText());
            this.inputField.Get<ButtonComponent>().SetInteractable(false);
            
        }

        private void OnEditEnd(string text) {

            #if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
            this.ApplyCommand(text);
            #else

            #if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame == true) {

                this.ApplyCommand(text);

            }
            #else
            if (Input.GetKeyDown(KeyCode.KeypadEnter) == true || Input.GetKeyDown(KeyCode.Return)) {

                this.ApplyCommand(text);

            }
            #endif

            #endif

        }

        private void MoveUp() {

            var console = this.GetConsole();
            var commands = console.GetCommands();
            
            if (commands.Count == 0) return;
            --this.currentIndex;
            if (this.currentIndex < 0) this.currentIndex = 0;
            this.inputField.SetText(commands[this.currentIndex].str);

        }

        private void MoveDown() {

            var console = this.GetConsole();
            var commands = console.GetCommands();

            if (commands.Count == 0) return;
            ++this.currentIndex;
            if (this.currentIndex >= commands.Count) this.currentIndex = commands.Count - 1;
            this.inputField.SetText(commands[this.currentIndex].str);

        }

        private void AutoComplete() {
            
            this.ApplyCommand(this.inputField.GetText(), autoComplete: true);
            
        }

        private char OnChar(string arg1, int arg2, char arg3) {

            if (arg3 == this.openCloseChar) {
                
                return '\0';
                
            }
            
            return arg3;

        }

        public void AddLine(string text, LogType logType = LogType.Log, bool isCommand = false, bool canCopy = false) {

            var console = this.GetConsole();
            console.AddLine(text, logType, isCommand, canCopy);
            
        }

        public void RunCommand(WindowSystemConsole.CommandItem command) {

            var console = this.GetConsole();
            console.RunCommand(command, this);
            this.currentIndex = console.GetCommands().Count;
            
        }

        public void ApplyCommand(string command, bool autoComplete = false) {

            var cmd = command.Trim();
            var console = this.GetConsole();

            if (cmd.ToLower() == "modulesample") {
                
                this.AddLine(cmd, isCommand: true);
                var itemHelp = new WindowSystemConsole.CommandItem() {
                    str = cmd,
                    moduleName = null,
                    methodName = null,
                    argsCount = 0,
                    args = null,
                };
                console.AddCommand(itemHelp);
                
                this.PrintModuleSample();
                this.ClearInput();
                return;

            }
            
            if (cmd.ToLower() == "help" || (string.IsNullOrEmpty(cmd) == true && autoComplete == true)) {

                this.AddLine(cmd, isCommand: true);
                var itemHelp = new WindowSystemConsole.CommandItem() {
                    str = cmd,
                    moduleName = null,
                    methodName = null,
                    argsCount = 0,
                    args = null,
                };
                console.AddCommand(itemHelp);
                console.PrintHelpAllModules();
                this.ClearInput();
                return;
                
            }
            
            if (string.IsNullOrEmpty(cmd) == true) return;
            
            var splitted = cmd.Split(new [] { " " }, System.StringSplitOptions.RemoveEmptyEntries);
            if (splitted.Length < 2) {

                if (autoComplete == true) {
                    
                    var moduleNamePart = splitted[0];
                    var modules = console.GetModulesByPart(moduleNamePart, out var first);
                    if (modules.Count == 1) {
                        
                        // Auto complete
                        this.ReplaceInput($"{first} ");
                        
                    } else {
                        
                        // Print all variants
                        console.PrintHelp(modules);
                        
                    }
                    
                }
                return;
                
            }
            
            var moduleName = splitted[0];
            var methodName = splitted[1];
            if (autoComplete == true) {

                var methodNamePart = methodName;
                var methods = console.GetMethodsByPart(moduleName, methodNamePart, out var first);
                if (methods.Count == 1) {
                        
                    // Auto complete
                    this.ReplaceInput($"{moduleName} {first} ");
                        
                } else {
                        
                    // Print all variants
                    console.PrintHelp(moduleName, methods);
                        
                }
                return;

            }
            
            var argsCount = splitted.Length - 2;
            var args = new string[argsCount];
            System.Array.Copy(splitted, 2, args, 0, argsCount);

            var item = new WindowSystemConsole.CommandItem() {
                str = cmd,
                moduleName = moduleName,
                methodName = methodName,
                argsCount = argsCount,
                args = args,
            };
            console.AddCommand(item);
            this.RunCommand(item);

            this.ClearInput();
            this.ScrollDown();

        }

        public static Color GetColorByFastLinkStyle(FastLinkType style) {

            var color = Color.white;
            switch (style) {
                
                case FastLinkType.Notice:
                    color = new Color(0.15f, 0.6f, 1f);
                    break;

                case FastLinkType.Warning:
                    color = new Color(1f, 0.8f, 0.4f);
                    break;

                case FastLinkType.Alarm:
                    color = new Color(1f, 0.15f, 0.4f);
                    break;

                case FastLinkType.Directory:
                    color = new Color(0f, 0f, 0f, 0f);
                    break;

            }

            return color;

        }

        public static Color GetColorByLogType(LogType logType) {

            var color = Color.white;
            switch (logType) {
                
                case LogType.Warning:
                    color = Color.yellow;
                    break;

                case LogType.Assert:
                case LogType.Exception:
                case LogType.Error:
                    color = Color.red;
                    break;

            }
            
            return color;

        }

        public static Color GetScrollbarColorByLogType(LogType logType) {

            var color = Color.white;
            switch (logType) {
                
                case LogType.Warning:
                    color = Color.yellow;
                    break;

                case LogType.Assert:
                case LogType.Exception:
                case LogType.Error:
                    color = Color.red;
                    break;

            }
            color.a = 0.3f;
            
            return color;

        }

        float IDataSource.GetSize(int index) {

            //var canvas = this.GetWindow().GetCanvas();
            var button = (this.list.source.directRef as ButtonComponent);
            var layoutGroupPadding = button.GetComponent<LayoutGroup>().padding;
            var size = this.list.rectTransform.rect.size;
            size.x -= layoutGroupPadding.left + layoutGroupPadding.right;
            //var text = (button.Get<TextComponent>().graphics as Text);
            //var gen = text.GetGenerationSettings(size);
            //var textGen = text.cachedTextGenerator;
            //var scaleFactor = canvas.scaleFactor;
            int linesCount = 1;
            if (size.x > 0f) {
                var w = this.fixedCharWidth; //textGen.characters[0].charWidth;
                var len = this.GetDrawItem(index, ignoreFilters: true).line.Length;
                linesCount = Mathf.CeilToInt(len * w / size.x);
            }
            var charHeight = this.fixedCharHeight;
            var h = linesCount * charHeight;//textGen.GetPreferredHeight(this.GetDrawItem(index, ignoreFilters: true).line, gen);
            return (h + layoutGroupPadding.top + layoutGroupPadding.bottom);// / scaleFactor;
            
        }
        
        private struct ClosureParameters : IListClosureParameters {

            public int index { get; set; }
            public ConsoleScreen data;
            public List<WindowSystemConsole.DrawItem> allItems;
            public List<int> items;

        }

        private struct ClosureParametersItem : System.IEquatable<ClosureParametersItem> {

            public ClosureParameters parameters;
            public WindowSystemConsole.DrawItem data;

            public bool Equals(ClosureParametersItem other) {

                return this.data.line == other.data.line;

            }

        }

        /*private struct ClosureParametersScrollbar : IListClosureParameters {

            public int index { get; set; }
            public ListEndlessComponentModule.Item lastItem;
            public ConsoleScreen data;

        }*/

        private struct ClosureFastLinksParameters : IListClosureParameters {

            public int index { get; set; }
            public List<WindowSystemConsole.FastLink> data;
            public ConsoleScreen screen;

        }

        private struct ClosureFastLinksParametersButtonCallback : System.IEquatable<ClosureFastLinksParametersButtonCallback> {

            public ClosureFastLinksParameters parameters;
            public WindowSystemConsole.FastLink data;

            public bool Equals(ClosureFastLinksParametersButtonCallback other) {

                return this.data.cmd == other.data.cmd;

            }

        }

        public static void AOT() {
            
            new ListEndlessComponentModule().SetItems<ButtonComponent, ClosureParameters>(0, default, null, default, null);

        }

        private int GetDrawItemsCount() {

            var console = this.GetConsole();
            var items = console.GetItems();
            var cnt = 0;
            var logsFilterType = console.GetLogFilterType();
            lock (items) {

                foreach (var item in items) {

                    var mask = (WindowSystemConsole.LogsFilter)(1 << (int)(item.logType + 1));
                    if (item.isCommand == true || ((logsFilterType & mask) != 0) == true) {

                        ++cnt;

                    }

                }

            }

            return cnt;

        }

        private WindowSystemConsole.DrawItem GetDrawItem(int index, bool ignoreFilters) {

            var console = this.GetConsole();
            var logsFilterType = console.GetLogFilterType();
            var items = console.GetItems();
            if (ignoreFilters == true || logsFilterType == WindowSystemConsole.LogsFilter.All) {

                return items[index];

            }
            
            var k = 0;
            lock (items) {
                
                foreach (var item in items) {
    
                    var mask = (WindowSystemConsole.LogsFilter)(1 << (int)(item.logType + 1));
                    if (item.isCommand == true || ((logsFilterType & mask) != 0) == true) {
    
                        if (k == index) return item;
                        ++k;
    
                    }
    
                }
                
            }
            
            return default;

        }

        public struct ScrollbarItem {

            public LogType logType;
            public ListEndlessComponentModule.Item item;

        }

        private readonly List<WindowSystemConsole.FastLink> fastLinkCache = new List<WindowSystemConsole.FastLink>();
        private int currentDirectoryId = -1;
        private readonly List<ScrollbarItem> scrollbarItems = new List<ScrollbarItem>();
        public void LateUpdate() {

            if (this.GetState() != ObjectState.Shown) return;
            
            #if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current.upArrowKey.wasPressedThisFrame == true) {

                this.MoveUp();

            }

            if (UnityEngine.InputSystem.Keyboard.current.downArrowKey.wasPressedThisFrame == true) {

                this.MoveDown();

            }

            if (UnityEngine.InputSystem.Keyboard.current.tabKey.wasPressedThisFrame == true) {

                this.AutoComplete();

            }
            #else
            if (Input.GetKeyDown(KeyCode.UpArrow) == true) {

                this.MoveUp();

            }

            if (Input.GetKeyDown(KeyCode.DownArrow) == true) {

                this.MoveDown();

            }

            if (Input.GetKeyDown(KeyCode.Tab) == true) {

                this.AutoComplete();

            }
            #endif

            if (Application.isMobilePlatform == false) {

                if (this.inputField.IsFocused() == false) {

                    this.inputField.SetFocus();

                }

            }

            this.inputField.Get<ButtonComponent>().SetInteractable(this.inputField.GetText().Length > 0);

            var console = this.GetConsole();
            console.GetFastLinks(this.currentDirectoryId, this.fastLinkCache);
            this.fastLinks.Get<TextComponent>().SetText(console.GetDirectoryPath(this.currentDirectoryId));
            this.fastLinks.SetItems<FastLinkButtonComponent, ClosureFastLinksParameters>(this.fastLinkCache.Count, (button, parameters) => {

                var item = parameters.data[parameters.index];
                button.SetInfo(item);
                
                if (item.style == FastLinkType.Directory) {
                    
                    button.SetInteractable(true);

                    // Draw directory
                    button.SetCallback(new ClosureFastLinksParametersButtonCallback() { parameters = parameters, data = item, }, (innerData) => {

                        innerData.parameters.screen.currentDirectoryId = innerData.data.id;

                    });
                    button.Show();
                    
                } else {
                    
                    button.SetInteractable(this.GetConsole().ValidateFastLink(item));

                    button.SetCallback(new ClosureFastLinksParametersButtonCallback() { parameters = parameters, data = item, }, (innerData) => {

                        if (innerData.data.run == true) {

                            innerData.parameters.screen.ApplyCommand(innerData.data.cmd);

                        } else {
                        
                            innerData.parameters.screen.ReplaceInput($"{innerData.data.cmd} ");
                        
                        }
                    
                    });
                    button.ShowHide(this.GetConsole().ValidateFastLinkVisibility(item));

                }
                
            }, new ClosureFastLinksParameters() {
                data = this.fastLinkCache,
                screen = this,
            });

            this.list.SetDataSource(this);
            var allItems = console.GetItems();
            var items = console.GetItemsFiltered(this.GetLogFilterType()).items;
            this.list.SetItems<ButtonComponent, ClosureParameters>(items.Count, (component, parameters) => {

                var item = parameters.allItems[parameters.items[parameters.index]];//parameters.data.GetDrawItem(parameters.index, false);
                var text = component.Get<TextComponent>();
                ConsoleScreen.SetText(text, item.isCommand == true ? "<color=#777><b>></b></color> " : string.Empty, item.line);
                text.SetColor(item.isCommand == true ? new Color(0.15f, 0.6f, 1f) : ConsoleScreen.GetColorByLogType(item.logType));
                component.SetCallback(new ClosureParametersItem() { parameters = parameters, data = item, },(innerData) => {
                    innerData.parameters.data.ReplaceInput(innerData.data.line);
                });
                {
                    var copyButton = component.Get<ButtonComponent>();
                    copyButton.SetCallback(new ClosureParametersItem() { parameters = parameters, data = item, }, (innerData) => {
                        GUIUtility.systemCopyBuffer = System.Text.RegularExpressions.Regex.Replace(innerData.data.line, "<.*?>", System.String.Empty);
                    });
                    copyButton.ShowHide(item.canCopy);
                }
                component.SetInteractable(item.isCommand);
                component.Show();
                
            }, new ClosureParameters() {
                data = this,
                allItems = allItems,
                items = items,
            });

            if (console.isDirty == true) {

                this.logsCounterComponent.SetInfo();
                console.isDirty = false;

                /*{
                    var module = this.list.GetModule<ListEndlessComponentModule>();
                    this.scrollbarItems.Clear();
                    var i = 0;
                    var prevLog = LogType.Log;
                    var prevRectItem = new ListEndlessComponentModule.Item();
                    foreach (var item in this.drawItems) {

                        if (this.HasLogFilterType(item.logType) == true) {

                            if (i < this.drawItems.Count - 1 && item.logType != LogType.Log && item.logType != prevLog) {

                                prevLog = item.logType;
                                prevRectItem = module.GetItemByIndex(i);

                            } else if ((i == this.drawItems.Count - 1 && item.logType != LogType.Log) || (item.logType == LogType.Log && item.logType != prevLog)) {

                                if (i == this.drawItems.Count - 1) {

                                    if (prevLog == LogType.Log) {

                                        prevRectItem = module.GetItemByIndex(i);
                                        prevRectItem.accumulatedSize -= prevRectItem.size;
                                        prevLog = item.logType;

                                    }

                                }
                                
                                var rectItem = module.GetItemByIndex(i);
                                prevRectItem.size = rectItem.accumulatedSize - prevRectItem.accumulatedSize;
                                this.scrollbarItems.Add(new ScrollbarItem() {
                                    logType = prevLog,
                                    item = prevRectItem,
                                });
                                
                                prevRectItem = rectItem;
                                prevLog = item.logType;
                                
                            }
                            
                            ++i;

                        }
                        
                    }

                    var lastItem = new ListEndlessComponentModule.Item() {
                        accumulatedSize = 10f,
                    };
                    if (this.scrollbarItems.Count > 0 && i > 1 && i < module.GetCount()) {

                        lastItem = module.GetItemByIndex(i - 1);

                    }

                    var scrollbarList = this.list.Get<ListComponent>();
                    scrollbarList.SetItems<LogScrollbarRectComponent, ClosureParametersScrollbar>(this.scrollbarItems.Count, (item, data) => {
                        
                        item.SetInfo(data.data.scrollbarItems[data.index], data.lastItem.accumulatedSize);
                        
                    }, new ClosureParametersScrollbar() {
                        data = this,
                        lastItem = lastItem,
                    });
                }*/

            }

        }

        private static void SetText(TextComponent component, string text1, string text2) {

            component.SetText(text1, text2);

        }

        public void CloseCustomPopup() {
            
        }

    }

}