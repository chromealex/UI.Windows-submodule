using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEngine.UI.Windows.Runtime.Windows.Components {

    using Button = UnityEngine.UIElements.Button;

    internal enum EventSource {

        UIButton,
        Keyboard,
        Input,

    }
    
    public class UIConsoleComponent : WindowComponent {

        public LinePopupComponent linePopupComponent;
        public CustomPopupComponent customPopupComponent;
        
        private UnityEngine.UIElements.UIDocument document;
        public VisualTreeAsset lineElement;
        public VisualTreeAsset fastLinkButtonElement;
        public int itemHeight = 25;

        internal UnityEngine.UI.Windows.WindowSystemConsole console;

        private TextField commandField;
        private ListView content;
        private ScrollView scrollView;
        private Button sendButton;
        private VisualElement fastLinksContainer;

        private Button buttonInfo;
        private Button buttonWarning;
        private Button buttonError;
        private Label buttonInfoLabel;
        private Label buttonWarningLabel;
        private Label buttonErrorLabel;

        private bool autoScroll;

        private class Line {

            private UIConsoleComponent uiConsole;
            private Button copyButton;
            private Button button;
            private Label collapseCounter;

            public Line(UIConsoleComponent uiConsole) {

                this.uiConsole = uiConsole;

            }

            public void SetVisualElement(VisualElement line) {

                this.copyButton = line.Q<Button>(className: "copy-button");
                this.button = line.Q<Button>(className: "line-button");
                this.collapseCounter = line.Q<Label>(className: "collapse-counter");

            }

            public void SetData(UnityEngine.UI.Windows.WindowSystemConsole.DrawItem data, int index, int localIndex, List<int> items) {

                this.button.style.color = new StyleColor(data.isCommand == true ? new Color(0.15f, 0.6f, 1f) : UIConsoleComponent.GetColorByLogType(data.logType));
                this.button.text = data.line;
                this.button.UnregisterCallback<ClickEvent, string>(this.uiConsole.ReApplyCommand);
                this.button.UnregisterCallback<ClickEvent, LineInfo>(this.uiConsole.ShowLine);
                if (data.isCommand == true) {
                    this.button.RegisterCallback<ClickEvent, string>(this.uiConsole.ReApplyCommand, data.line);
                } else {
                    this.button.RegisterCallback<ClickEvent, LineInfo>(this.uiConsole.ShowLine, new LineInfo() {
                        localIndex = localIndex,
                        filteredItems = items,
                    });
                }

                this.copyButton.style.display = new StyleEnum<DisplayStyle>(data.canCopy == true ? DisplayStyle.Flex : DisplayStyle.None);
                this.copyButton.UnregisterCallback<ClickEvent, string>(this.uiConsole.CopyLine);
                this.copyButton.RegisterCallback<ClickEvent, string>(this.uiConsole.CopyLine, data.line);

                if (data.collapseCount > 0) {

                    this.collapseCounter.text = (data.collapseCount + 1).ToString();
                    this.collapseCounter.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);

                } else {
                    
                    this.collapseCounter.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                    
                }

            }

        }

        private void ShowLine(ClickEvent evt, LineInfo lineInfo) {
            
            this.linePopupComponent.Show();
            this.linePopupComponent.SetInfo(this, lineInfo);
            
        }

        private void ReApplyCommand(ClickEvent evt, string cmd) {
            this.ApplyCommand(cmd, false);
        }

        internal void CopyLine(ClickEvent evt, string data) {

            GUIUtility.systemCopyBuffer = System.Text.RegularExpressions.Regex.Replace(data, "<.*?>", System.String.Empty);

        }
        
        private void OnDirty() {

            lock (this.content.itemsSource) {
                #if UNITY_2021_1_OR_NEWER
                this.content.Rebuild();
                #else
                this.content.Refresh();
                #endif
            }

            if (this.autoScroll == true) {
                this.ScrollDown();
            }

            this.UpdateCounters();

        }

        private void UpdateCounters() {

            this.buttonInfoLabel.text = this.console.GetCounter(LogType.Log).ToString();
            this.buttonWarningLabel.text = this.console.GetCounter(LogType.Warning).ToString();
            this.buttonErrorLabel.text = (this.console.GetCounter(LogType.Error)
				+ this.console.GetCounter(LogType.Exception)
				+ this.console.GetCounter(LogType.Assert)).ToString();

        }

        private void ClearInput() {

            this.commandField.SetValueWithoutNotify(" ");
            this.commandField.Focus();

        }

        private void ScrollDown() {

            this.content.ScrollToItem(-1);

        }

        private void TryToFocus() {

            if (Application.isMobilePlatform == false) {

                if (this.customPopupComponent.IsVisible() == true ||
                    this.linePopupComponent.IsVisible() == true) {
                    return;
                }
                
                if (this.commandField.focusController == null ||
                    this.commandField.focusController.focusedElement != this.commandField.ElementAt(0)) {

                    this.commandField.ElementAt(0).Focus();

                }

            }

        }

        private void ReplaceInput(string newText) {

            this.commandField.value = newText;

        }

        private int prevScreenSize;
        public void Update() {

            if (this.console == null) return;
            
            if (this.prevScreenSize != Screen.width) {
                
                this.OnDirty();
                
            }
            this.prevScreenSize = Screen.width;

            if (this.console.isDirty == true) {
                
                this.OnDirty();
                this.console.isDirty = false;

            }
            
            if (this.autoScroll == true) this.ScrollDown();

            this.UpdateList();

        }

        public override void OnHideEnd() {
            
            base.OnHideEnd();
            
            this.document.rootVisualElement.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            
        }

        private readonly List<WindowSystemConsole.FastLink> fastLinkCache = new List<WindowSystemConsole.FastLink>();
        private int currentDirectoryId = -1;

        private void SetInfoToFastLinkButton(Button button, WindowSystemConsole.FastLink data, int index) {
            
            if (data.style == FastLinkType.Directory) {
                
                if (this.currentDirectoryId != -1 && index == 0) {
                 
                    button.AddToClassList("fastlink-dir-up");
   
                } else {

                    button.AddToClassList("fastlink-dir");
   
                }
                
            }

            button.Q<Label>(className: "label").text = data.caption;
            button.name = data.style.ToString();
            
        }
        
        private void GenerateFastLinks() {
            
            this.fastLinksContainer.Clear();
            
            this.console.GetFastLinks(this.currentDirectoryId, this.fastLinkCache);
            for (int i = 0; i < this.fastLinkCache.Count; ++i) {

                var item = this.fastLinkCache[i];
                
                var button = this.fastLinkButtonElement.Instantiate();
                this.fastLinksContainer.Add(button);
                this.SetInfoToFastLinkButton(button.Q<Button>("Button"), item, i);
                
                if (item.style == FastLinkType.Directory) {
                    
                    button.SetEnabled(true);

                    // Draw directory
                    button.RegisterCallback<ClickEvent>(evt => {

                        this.currentDirectoryId = item.id;
                        this.GenerateFastLinks();
                        
                    });
                    button.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);

                } else {
                    
                    button.SetEnabled(this.console.ValidateFastLink(item));

                    button.RegisterCallback<ClickEvent>(evt => {

                        if (item.run == true) {

                            this.ApplyCommand(item.cmd);

                        } else {
                        
                            this.ReplaceInput($"{item.cmd} ");
                        
                        }
                        
                        this.GenerateFastLinks();
                    
                    });
                    button.style.display = new StyleEnum<DisplayStyle>(this.console.ValidateFastLinkVisibility(item) == true ? DisplayStyle.Flex : DisplayStyle.None);

                }

            }
            
        }

        private void OnSelectLogType(ClickEvent evt, LogType type) {
            
            this.console.SetLogFilterType(type, !this.console.HasLogFilterType(type));
            this.UpdateButtons();
            this.UpdateFilters();

        }

        private void UpdateButtons() {

            {
                var type = LogType.Log;
                var button = this.buttonInfo;
                button.UnregisterCallback<ClickEvent, LogType>(this.OnSelectLogType);
                button.RegisterCallback<ClickEvent, LogType>(this.OnSelectLogType, type);
                if (this.console.HasLogFilterType(type) == true) {
                    button.AddToClassList("selected");
                } else {
                    button.RemoveFromClassList("selected");
                }
            }
            {
                var type = LogType.Warning;
                var button = this.buttonWarning;
                button.UnregisterCallback<ClickEvent, LogType>(this.OnSelectLogType);
                button.RegisterCallback<ClickEvent, LogType>(this.OnSelectLogType, type);
                if (this.console.HasLogFilterType(type) == true) {
                    button.AddToClassList("selected");
                } else {
                    button.RemoveFromClassList("selected");
                }
            }
            {
                var type = LogType.Error;
                var button = this.buttonError;
                button.UnregisterCallback<ClickEvent, LogType>(this.OnSelectLogType);
                button.RegisterCallback<ClickEvent, LogType>(this.OnSelectLogType, type);
                if (this.console.HasLogFilterType(type) == true) {
                    button.AddToClassList("selected");
                } else {
                    button.RemoveFromClassList("selected");
                }
            }

        }

        public void SetInfo(UIDocument document) {

            this.autoScroll = true;
            this.document = document;
            {

                this.console = WindowSystem.GetWindowSystemModule<WindowSystemConsoleModule>().console;

                this.commandField = this.document.rootVisualElement.Q<TextField>(className: "command-input");
                this.sendButton = this.document.rootVisualElement.Q<Button>(className: "button-send");
                this.sendButton.RegisterCallback<ClickEvent>(evt => {
                    if (this.commandField == null) return;
                    this.OnEditEnd(this.commandField.text, EventSource.UIButton);
                });

                this.fastLinksContainer = this.document.rootVisualElement.Q<VisualElement>(className: "top-panel");

                this.buttonInfo = this.document.rootVisualElement.Q<Button>(className: "button-info");
                this.buttonWarning = this.document.rootVisualElement.Q<Button>(className: "button-warning");
                this.buttonError = this.document.rootVisualElement.Q<Button>(className: "button-error");
                this.buttonInfoLabel = this.buttonInfo.Q<Label>();
                this.buttonWarningLabel = this.buttonWarning.Q<Label>();
                this.buttonErrorLabel = this.buttonError.Q<Label>();
                
                var content = this.document.rootVisualElement.Q<ListView>("Content");
                this.content = content;
                this.scrollView = content.Q<ScrollView>(className: "unity-scroll-view");
                content.makeItem = () => {

                    var line = this.lineElement.Instantiate();
                    var lineController = new Line(this);
                    line.userData = lineController;
                    lineController.SetVisualElement(line);
                    return line;

                };

                #if UNITY_2021_1_OR_NEWER
                content.fixedItemHeight = this.itemHeight;
                content.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
                #else
                content.itemHeight = this.itemHeight;
                #endif
                content.bindItem = (element, i) => {
                    var items = this.console.GetItemsFiltered(this.console.GetLogFilterType()).items;
                    ((Line)element.userData).SetData(this.console.GetItems()[items[i]], items[i], i, items);
                };
                content.selectionType = SelectionType.None;

                content.Q<ScrollView>().Query<Scroller>().ForEach(scroller => {
                    scroller.RegisterCallback<PointerDownEvent>((e) => {
                        this.autoScroll = false;
                    }, TrickleDown.TrickleDown);
                    scroller.RegisterCallback<ChangeEvent<float>>(evt => {
                        if (evt.newValue >= scroller.highValue) {
                            this.autoScroll = true;
                        } else {
                            this.autoScroll = false;
                        }
                    });
                });
                
                this.commandField.RegisterCallback<InputEvent>(evt => { this.OnEditEnd(evt.newData, EventSource.Input); });
                this.commandField.RegisterCallback<KeyDownEvent>(evt => {
                    if (this.commandField == null) return;
                    this.OnEditEnd(this.commandField.text, EventSource.Keyboard);
                });

                this.GenerateFastLinks();
                this.UpdateCounters();
                this.UpdateButtons();
                this.UpdateFilters();

            }

            this.document.rootVisualElement.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            
            base.OnShowBegin();
            
        }

        private void UpdateFilters() {
            
            this.content.itemsSource = this.console.GetItemsFiltered(this.console.GetLogFilterType()).items;
            this.OnDirty();

        }

        private int currentIndex = 0;
        private void MoveUp() {

            var console = this.console;
            var commands = console.GetCommands();
            
            if (commands.Count == 0) return;
            --this.currentIndex;
            if (this.currentIndex < 0) this.currentIndex = 0;
            this.commandField.value = commands[this.currentIndex].str;

        }

        private void MoveDown() {

            var console = this.console;
            var commands = console.GetCommands();

            if (commands.Count == 0) return;
            ++this.currentIndex;
            if (this.currentIndex >= commands.Count) this.currentIndex = commands.Count - 1;
            this.commandField.value = commands[this.currentIndex].str;

        }

        private void UpdateList() {

            #if ENABLE_INPUT_SYSTEM

            if (UnityEngine.InputSystem.Keyboard.current?.upArrowKey.wasPressedThisFrame == true) {

                this.MoveUp();

            }

            if (UnityEngine.InputSystem.Keyboard.current?.downArrowKey.wasPressedThisFrame == true) {

                this.MoveDown();

            }

            if (UnityEngine.InputSystem.Keyboard.current?.tabKey.wasPressedThisFrame == true) {

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

            this.TryToFocus();

            this.sendButton.SetEnabled(this.commandField.text.Length > 0);
            
        }

        private void OnEditEnd(string text, EventSource eventSource) {

            if (eventSource == EventSource.UIButton) {
                this.ApplyCommand(text);
                return;
            }

            #if ENABLE_INPUT_SYSTEM
            var shouldApply = UnityEngine.InputSystem.Keyboard.current?.enterKey.wasPressedThisFrame == true;
            #else
            var shouldApply = Input.GetKeyDown(KeyCode.KeypadEnter) == true || Input.GetKeyDown(KeyCode.Return);
            #endif

            if (shouldApply == true) {

                this.ApplyCommand(text);

            }

        }
        
        public object RunCommand(WindowSystemConsole.CommandItem command) {

            var console = this.console;
            var result = console.RunCommand(command, this.GetWindow<UIConsoleScreen>());
            this.currentIndex = console.GetCommands().Count;
            return result;
            
        }
        
        private void AutoComplete() {
            
            this.ApplyCommand(this.commandField.text, autoComplete: true);
            
        }
        
        public void ApplyCommand(string command, bool autoComplete = false) {

            var cmd = command.Trim();
            var console = this.console;

            if (cmd.ToLower() == "help" || (string.IsNullOrEmpty(cmd) == true && autoComplete == true)) {

                this.console.AddLine(cmd, isCommand: true);
                var itemHelp = new UnityEngine.UI.Windows.WindowSystemConsole.CommandItem() {
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

            if (cmd.ToLower() == "clear" || (string.IsNullOrEmpty(cmd) == true && autoComplete == true)) {

                this.console.AddLine(cmd, isCommand: true);
                console.Clear();
                this.ClearInput();
                return;

            }

            if (string.IsNullOrEmpty(cmd) == true) return;

            var splitted = cmd.Split(new[] { " " }, System.StringSplitOptions.RemoveEmptyEntries);
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

            var item = new UnityEngine.UI.Windows.WindowSystemConsole.CommandItem() {
                str = cmd,
                moduleName = moduleName,
                methodName = methodName,
                argsCount = argsCount,
                args = args,
            };
            console.AddCommand(item);
            var result = this.RunCommand(item);

            this.ClearInput();
            this.ScrollDown();

            if (result is ConsolePopup popup) {

                var root = popup.root;
                this.customPopupComponent.Show();
                this.customPopupComponent.SetInfo(this, root);

            }

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

    }

}