using System.Linq;
using UnityEngine.UI.Windows.Components;
using UnityEngine.UI.Windows.WindowTypes;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows.Runtime.Windows {

    public interface IConsoleModule {}

    public abstract class ConsoleModule : IConsoleModule {

        public ConsoleScreen screen;
        
        public void Help() {

            this.screen.PrintHelp(this);

        }

    }

    public class HelpAttribute : System.Attribute {

        public string text;

        public HelpAttribute(string text) {
            
            this.text = text;
            
        }

    }

    public class AliasAttribute : System.Attribute {

        public string text;

        public AliasAttribute(string text) {
            
            this.text = text;
            
        }

    }

    public enum FastLinkType {

        Default = 0,
        Notice,
        Warning,
        Alarm,

    }
    
    public class FastLinkAttribute : System.Attribute {

        public string text;
        public FastLinkType style;

        public FastLinkAttribute(string richText, FastLinkType style = FastLinkType.Default) {
            
            this.text = richText;
            this.style = style;

        }

    }

    /*[Help("Test module")][Alias("T")]
    public class Test : ConsoleModule {

        [Help("Prints help bool")]
        public void Z(bool a) {

            Debug.Log("Z: " + a);

        }

        [Help("Prints help string")][Alias("A")][FastLink("T HELP1")]
        public void Help1() {

            Debug.Log("Help");

        }

        [Help("Prints help string with a")][FastLink("T HELP2")]
        public void Help2(int a) {

            Debug.Log("Help2: " + a);

        }

        [Help("Prints help string with a and b")]
        public void Help3(int a, float b) {

            Debug.Log("Help3: " + a + " :: " + b);

        }

    }*/

    public class ConsoleScreen : LayoutWindowType, IDataSource {

        public struct FastLink {

            public string cmd;
            public bool run;
            public string caption;
            public FastLinkType style;

        }

        [System.Serializable]
        public struct CommandItem {

            public string str;
            public string moduleName;
            public string methodName;
            public int argsCount;
            public string[] args;

        }

        [System.Serializable]
        public struct DrawItem {

            public string line;
            public LogType logType;
            public bool isCommand;

        }
        
        private ListComponent list;
        private ListComponent fastLinks;
        private InputFieldComponent inputField;
        private LogsCounterComponent logsCounterComponent;
        
        private readonly List<FastLink> fastLinkItems = new List<FastLink>();
        private readonly List<CommandItem> commands = new List<CommandItem>();
        private readonly List<IConsoleModule> moduleItems = new List<IConsoleModule>();
        private System.Collections.Concurrent.ConcurrentQueue<DrawItem> drawItems = new System.Collections.Concurrent.ConcurrentQueue<DrawItem>();
        private System.Collections.Concurrent.ConcurrentDictionary<LogType, int> logsCounter = new System.Collections.Concurrent.ConcurrentDictionary<LogType, int>();
        private int logsFilter;
        private char openCloseChar;
        private int currentIndex;

        public bool autoConnectLogs = true;
        private string helpInitPrint;
        private System.Threading.Thread unityThread;

        private int _isDirty;
        public bool isDirty {
            get => System.Threading.Interlocked.Exchange(ref this._isDirty, 0) == 1;
            set => System.Threading.Interlocked.Exchange(ref this._isDirty, value == true ? 1 : 0);
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

            this.CollectModules();

            this.helpInitPrint = this.GetInitHelp();
            this.SetLogFilterType(LogType.Log, true);
            this.SetLogFilterType(LogType.Warning, true);
            this.SetLogFilterType(LogType.Error, true);

            this.unityThread = System.Threading.Thread.CurrentThread;

            if (this.autoConnectLogs == true) {

                Application.logMessageReceivedThreaded += this.OnAddLogThreaded;

                if (string.IsNullOrEmpty(this.helpInitPrint) == false) this.AddLine(this.helpInitPrint);
                this.helpInitPrint = string.Empty;

            }

        }

        public override void OnShowBegin() {
            
            base.OnShowBegin();

            if (string.IsNullOrEmpty(this.helpInitPrint) == false) this.AddLine(this.helpInitPrint);
            this.helpInitPrint = string.Empty;
            
            this.logsCounterComponent.SetInfo();

        }

        public override void OnDeInit() {
            
            base.OnDeInit();
            
            Application.logMessageReceivedThreaded -= this.OnAddLogThreaded;
            
        }

        private string GetInitHelp() {

            var welcomeMessage = "<color=#0f0>Welcome to UI.Windows Console</color>\n" +
                                 "------------------------------------\n" +
                                 "\t<color=#3af>help</color>\t\t\t\t\t\t\t\t\tshow all modules.\n" +
                                 "\t<color=#3af>[module] help</color>\t\t\t\t\t\tshow module's help.\n" +
                                 "\t<color=#3af>[module] [method] arg1 arg2 ...</color>\t\tcall method of the module.\n" +
                                 "\t<color=#3af>modulesample</color>\t\t\t\t\t\t\tshow module sample text.\n" +
                                 "------------------------------------\n";
            
            return welcomeMessage;

        }

        public void PrintModuleSample() {
            
            this.AddLine("Check console log for more details.");
            var file = Resources.Load<TextAsset>("uiws-resource-console-modulesample");
            if (file != null) {

                var text = file.text;
                Debug.Log(text);

            }

        }

        public bool HasLogFilterType(LogType logType) {

            var mask = 1 << (int)(logType + 1);
            if ((this.logsFilter & mask) != 0) return true;
            
            return false;
            
        }

        public void SetLogFilterType(LogType logType, bool state) {
            
            if (logType == LogType.Error) {

                this.SetLogFilterType(LogType.Assert, state);
                this.SetLogFilterType(LogType.Exception, state);

            }

            var mask = 1 << (int)(logType + 1);
            if (state == true) {
                this.logsFilter |= mask;
            } else {
                this.logsFilter &= ~mask;
            }
            
            this.logsCounterComponent.SetInfo();
            
        }

        public int GetCounter(LogType logType) {

            if (this.logsCounter.TryGetValue(logType, out var count) == true) {

                return count;

            }
            
            return 0;
            
        }

        private void AddLog(string text, LogType type = LogType.Log, string trace = null) {

            if (this.logsCounter == null) return;
            
            if (this.logsCounter.TryGetValue(type, out var count) == true) {

                this.logsCounter[type] = count + 1;

            } else {

                this.logsCounter.TryAdd(type, 1);

            }

            this.AddLine(text, type);
            if (type == LogType.Exception && string.IsNullOrEmpty(trace) == false) this.AddLine(trace, type);

            this.isDirty = true;

        }

        private void OnAddLogThreaded(string text, string trace, LogType type) {

            if (System.Threading.Thread.CurrentThread != this.unityThread) {

                this.AddLog($"<color=#bb0>[Thread {System.Threading.Thread.CurrentThread.Name}]</color> {text}", type, trace);

            } else {
                
                this.AddLog(text, type, trace);

            }

        }

        public void CollectModules() {
            
            var types = System.AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes().Where(y => y.IsAbstract == false && y.GetInterfaces().Contains(typeof(IConsoleModule)) == true)).ToArray();
            foreach (var type in types) {

                var module = (IConsoleModule)System.Activator.CreateInstance(type);
                this.moduleItems.Add(module);
                
                var methods = module.GetType().GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                foreach (var method in methods) {

                    if (this.IsValidMethod(method) == false) continue;

                    if (this.GetFastLink(method, out var caption, out var style) == true) {

                        this.fastLinkItems.Add(new FastLink() {
                            run = method.GetParameters().Length == 0 && style < FastLinkType.Alarm,
                            cmd = $"{module.GetType().Name.ToLower()} {method.Name.ToLower()}",
                            caption = caption,
                            style = style,
                        });

                    }

                }
                
            }
            
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
            if (Input.GetKeyDown(KeyCode.KeypadEnter) == true || Input.GetKeyDown(KeyCode.Return)) {

                this.ApplyCommand(text);

            }
            #endif

        }

        private void MoveUp() {

            if (this.commands.Count == 0) return;
            --this.currentIndex;
            if (this.currentIndex < 0) this.currentIndex = 0;
            this.inputField.SetText(this.commands[this.currentIndex].str);

        }

        private void MoveDown() {

            if (this.commands.Count == 0) return;
            ++this.currentIndex;
            if (this.currentIndex >= this.commands.Count) this.currentIndex = this.commands.Count - 1;
            this.inputField.SetText(this.commands[this.currentIndex].str);

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

        public void AddModule(IConsoleModule module) {
            
            this.moduleItems.Add(module);
            
        }

        public void AddHR() {

            this.AddLine("<color=#777>--------------------------------------</color>");

        }
        
        public void AddLine(string text, LogType logType = LogType.Log, bool isCommand = false) {

            this.drawItems.Enqueue(new DrawItem() {
                line = text,
                logType = logType,
                isCommand = isCommand,
            });
            
        }

        private bool GetFastLink(System.Reflection.ICustomAttributeProvider methodInfo, out string text, out FastLinkType style) {
            
            text = string.Empty;
            style = FastLinkType.Default;
            
            var aliasAttrs = methodInfo.GetCustomAttributes(typeof(FastLinkAttribute), false);
            if (aliasAttrs.Length == 0) return false;
            var attr = (aliasAttrs[0] as FastLinkAttribute);
            text = attr.text.ToLower();
            style = attr.style;
            return true;
            
        }

        private string GetAlias(System.Reflection.ICustomAttributeProvider methodInfo) {

            var aliasAttrs = methodInfo.GetCustomAttributes(typeof(AliasAttribute), false);
            if (aliasAttrs.Length == 0) return string.Empty;
            return (aliasAttrs[0] as AliasAttribute)?.text.ToLower();

        }

        public void RunCommand(CommandItem command) {

            this.AddLine(command.str, isCommand: true);

            var run = false;
            var module = this.GetModule(command);
            if (module != null) {

                if (module is ConsoleModule consoleModule) {

                    consoleModule.screen = this;

                }

                var methodName = command.methodName.ToLower();
                var methods = module.GetType().GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                System.Reflection.MethodInfo methodFound = null;
                foreach (var method in methods) {

                    if (this.IsValidMethod(method) == false) continue;

                    var alias = this.GetAlias(method);
                    
                    if (method.Name.ToLower() == methodName || alias == methodName) {

                        methodFound = method;

                        var pars = method.GetParameters();
                        if (pars.Length == command.argsCount) {

                            try {

                                var objs = new object[command.argsCount];
                                for (int i = 0; i < command.argsCount; ++i) {

                                    objs[i] = this.ConvertArg(command.args[i], pars[i].ParameterType);

                                }

                                method.Invoke(module, objs);
                                run = true;

                            } catch (System.Exception ex) {
                                
                                this.AddLog(ex.Message, LogType.Error);
                                
                            }

                            break;

                        }

                    }
                    
                }

                if (run == false) {

                    if (methodFound == null) {
                        
                        this.AddLog($"Module `{command.moduleName}` has no method `{methodName}`.", LogType.Warning);

                    } else {

                        this.AddLog($"Module `{command.moduleName}` has no method `{methodName}` with parameters length = {command.argsCount} (Required length {methodFound.GetParameters().Length})", LogType.Warning);
                        this.AddLog(this.GetMethodCallString(methodFound), LogType.Log);

                    }

                }

            } else {
                
                this.AddLog($"Module `{command.moduleName}` not found", LogType.Warning);

            }
            
            this.currentIndex = this.commands.Count;
            
        }

        private bool IsValidMethod(System.Reflection.MethodInfo methodInfo) {

            var name = methodInfo.Name;
            if (name == "GetHashCode" || name == "ToString" || name == "Equals" || name == "GetType") return false;

            return true;

        }

        private string TypeToString(System.Type type) {

            if (type == typeof(int)) return "int";
            if (type == typeof(uint)) return "uint";
            if (type == typeof(short)) return "short";
            if (type == typeof(ushort)) return "ushort";
            if (type == typeof(long)) return "long";
            if (type == typeof(ulong)) return "ulong";
            if (type == typeof(float)) return "float";
            if (type == typeof(double)) return "double";
            if (type == typeof(bool)) return "bool";
            
            return type.ToString();
            
        }
        
        private object ConvertArg(string arg, System.Type targetType) {

            if (targetType == typeof(bool)) {

                if (arg.Trim().ToLower() == "true" || arg.Trim().ToLower() == "1") {

                    return true;

                }
                
                return false;

            }
            
            return System.Convert.ChangeType(arg, targetType);
            
        }

        public void PrintHelp(IConsoleModule module) {
            
            this.AddLog(this.GetHelpString(string.Empty, module.GetType()).Trim(), LogType.Log);
            this.AddLog("Module methods:", LogType.Log);
            this.AddHR();
            var count = 0;
            var methods = module.GetType().GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            foreach (var method in methods) {
                
                if (this.IsValidMethod(method) == false) continue;

                var str = this.GetMethodCallString(method);
                this.AddLog(this.GetSpace(4) + str, LogType.Log);
                ++count;

            }

            if (count == 0) {
                
                this.AddLog($"No suitable methods found for module `{module.GetType().Name}`", LogType.Log);
                
            }
            this.AddHR();

        }

        private string GetMethodCallString(System.Reflection.MethodInfo methodInfo) {

            var pars = methodInfo.GetParameters();
            var parameters = pars.Select(x => $"{this.TypeToString(x.ParameterType)} {x.Name}").ToArray();
            var parsStr = string.Join(", ", parameters);
            return $"<color=#3af>{methodInfo.Name.ToLower()}</color>({parsStr}){this.GetHelpString($"{this.GetSpace(4)}{methodInfo.Name.ToLower()}({parsStr})", methodInfo)}";
            
        }

        public void PrintHelp(List<IConsoleModule> modules) {
            
            this.AddLog("Modules:");
            this.AddHR();
            foreach (var module in modules) {
                
                this.AddLog($"{this.GetSpace(4)}<color=#3af>{module.GetType().Name.ToLower()}</color>{this.GetHelpString(this.GetSpace(4) + module.GetType().Name.ToLower(), module.GetType())}");
                
            }
            this.AddHR();
            
        }

        public void PrintHelp(string moduleName, List<System.Reflection.MethodInfo> methods) {
            
            this.AddLog($"Module {moduleName}");
            this.AddLog("Methods:");
            this.AddHR();
            foreach (var methodInfo in methods) {
                
                this.AddLog($"{this.GetSpace(4)}<color=#3af>{methodInfo.Name.ToLower()}</color>{this.GetHelpString(this.GetSpace(4) + methodInfo.Name.ToLower(), methodInfo)}");
                
            }
            this.AddHR();
            
        }

        private string GetHelpString(string callStr, System.Reflection.ICustomAttributeProvider type) {

            var length = callStr.Length;
            var str = string.Empty;
            
            var attrs = type.GetCustomAttributes(typeof(HelpAttribute), false);
            if (attrs.Length > 0) {

                str += $"{this.GetSpace(4)}<color=#999>{((HelpAttribute)attrs[0]).text}</color>";

            }

            var attrsAlias = type.GetCustomAttributes(typeof(AliasAttribute), false);
            if (attrsAlias.Length > 0) {

                str += $"\n{this.GetSpace(length + 4)}Alias: <color=#3af>{((AliasAttribute)attrsAlias[0]).text}</color>";

            }

            return str;

        }

        private string GetSpace(int length) {

            var str = string.Empty;
            for (int i = 0; i < length; ++i) str += " ";
            return str;

        }

        private IConsoleModule GetModule(CommandItem command) {

            return this.GetModule(command.moduleName);

        }

        private IConsoleModule GetModule(string moduleName) {

            moduleName = moduleName.ToLower();
            foreach (var module in this.moduleItems) {

                var alias = this.GetAlias(module.GetType());
                if (module.GetType().Name.ToLower() == moduleName || alias == moduleName) {

                    return module;

                }
                
            }
            
            return null;

        }

        private List<System.Reflection.MethodInfo> GetMethodsByPart(string moduleName, string methodNamePart, out string first) {

            first = methodNamePart;

            var list = new List<System.Reflection.MethodInfo>();
            var module = this.GetModule(moduleName);
            if (module != null) {

                var methods = module.GetType().GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                foreach (var method in methods) {
                    
                    if (this.IsValidMethod(method) == false) continue;

                    var alias = this.GetAlias(method);
                    if (method.Name.ToLower().StartsWith(methodNamePart) == true) {

                        if (list.Contains(method) == false) list.Add(method);
                        first = method.Name.ToLower();

                    }
                    
                    if (alias.StartsWith(methodNamePart) == true) {

                        if (list.Contains(method) == false) list.Add(method);
                        first = alias;

                    }

                }
                
            }
            
            return list;

        }
        
        private List<IConsoleModule> GetModulesByPart(string moduleNamePart, out string first) {

            moduleNamePart = moduleNamePart.ToLower();
            first = moduleNamePart;
            
            var list = new List<IConsoleModule>();
            foreach (var module in this.moduleItems) {

                var alias = this.GetAlias(module.GetType());
                var name = module.GetType().Name.ToLower();
                if (name.StartsWith(moduleNamePart) == true) {
                    
                    if (list.Contains(module) == false) list.Add(module);
                    first = name;

                }
                
                if (alias.StartsWith(moduleNamePart) == true) {

                    if (list.Contains(module) == false) list.Add(module);
                    first = alias;

                }
                
            }
            
            return list;

        }
        
        public void ApplyCommand(string command, bool autoComplete = false) {

            var cmd = command.Trim();

            if (cmd == "modulesample") {
                
                this.AddLine(cmd, isCommand: true);
                var itemHelp = new CommandItem() {
                    str = cmd,
                    moduleName = null,
                    methodName = null,
                    argsCount = 0,
                    args = null,
                };
                this.commands.Add(itemHelp);
                this.PrintModuleSample();
                this.ClearInput();
                return;

            }
            
            if (cmd == "help" || (string.IsNullOrEmpty(cmd) == true && autoComplete == true)) {

                this.AddLine(cmd, isCommand: true);
                var itemHelp = new CommandItem() {
                    str = cmd,
                    moduleName = null,
                    methodName = null,
                    argsCount = 0,
                    args = null,
                };
                this.commands.Add(itemHelp);
                this.PrintHelp(this.moduleItems);
                this.ClearInput();
                return;
                
            }
            
            if (string.IsNullOrEmpty(cmd) == true) return;
            
            var splitted = cmd.Split(new [] { " " }, System.StringSplitOptions.RemoveEmptyEntries);
            if (splitted.Length < 2) {

                if (autoComplete == true) {
                    
                    var moduleNamePart = splitted[0];
                    var modules = this.GetModulesByPart(moduleNamePart, out var first);
                    if (modules.Count == 1) {
                        
                        // Auto complete
                        this.ReplaceInput($"{first} ");
                        
                    } else {
                        
                        // Print all variants
                        this.PrintHelp(modules);
                        
                    }
                    
                }
                return;
                
            }
            
            var moduleName = splitted[0];
            var methodName = splitted[1];
            if (autoComplete == true) {

                var methodNamePart = methodName;
                var methods = this.GetMethodsByPart(moduleName, methodNamePart, out var first);
                if (methods.Count == 1) {
                        
                    // Auto complete
                    this.ReplaceInput($"{moduleName} {first} ");
                        
                } else {
                        
                    // Print all variants
                    this.PrintHelp(moduleName, methods);
                        
                }
                return;

            }
            
            var argsCount = splitted.Length - 2;
            var args = new string[argsCount];
            System.Array.Copy(splitted, 2, args, 0, argsCount);

            var item = new CommandItem() {
                str = cmd,
                moduleName = moduleName,
                methodName = methodName,
                argsCount = argsCount,
                args = args,
            };
            this.commands.Add(item);
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

        float IDataSource.GetSize(int index) {

            var button = (this.list.source.directRef as ButtonComponent);
            var layoutGroupPadding = button.GetComponent<LayoutGroup>().padding;
            var size = this.list.rectTransform.rect.size;
            size.x -= layoutGroupPadding.left + layoutGroupPadding.right;
            var text = (button.Get<TextComponent>().graphics as Text);
            var gen = text.GetGenerationSettings(size);
            var textGen = text.cachedTextGenerator;
            var canvas = this.GetWindow().GetCanvas();
            var scaleFactor = canvas.scaleFactor;
            return (textGen.GetPreferredHeight(this.GetDrawItem(index).line, gen) + layoutGroupPadding.top + layoutGroupPadding.bottom) / scaleFactor;
            
        }
        
        private struct ClosureParameters : IListClosureParameters {

            public int index { get; set; }
            public ConsoleScreen data;

        }

        private struct ClosureFastLinksParameters : IListClosureParameters {

            public int index { get; set; }
            public List<FastLink> data;

        }

        public static void AOT() {
            
            new ListEndlessComponentModule().SetItems<ButtonComponent, ClosureParameters>(0, default, null, default, null);

        }

        private int GetDrawItemsCount() {

            return this.drawItems.Count(x => x.isCommand == true || this.HasLogFilterType(x.logType) == true);

        }

        private DrawItem GetDrawItem(int index) {

            var k = 0;
            foreach (var item in this.drawItems) {

                if (item.isCommand == true || this.HasLogFilterType(item.logType) == true) {

                    if (k == index) return item;
                    ++k;

                }

            }
            
            return default;

        }

        public void LateUpdate() {

            if (this.GetState() != ObjectState.Shown) return;
            
            if (Input.GetKeyDown(KeyCode.UpArrow) == true) {

                this.MoveUp();

            }

            if (Input.GetKeyDown(KeyCode.DownArrow) == true) {

                this.MoveDown();

            }

            if (Input.GetKeyDown(KeyCode.Tab) == true) {

                this.AutoComplete();

            }

            if (Application.isMobilePlatform == false) {

                if (this.inputField.IsFocused() == false) {

                    this.inputField.SetFocus();

                }

            }

            this.inputField.Get<ButtonComponent>().SetInteractable(this.inputField.GetText().Length > 0);

            this.fastLinks.SetItems<ButtonComponent, ClosureFastLinksParameters>(this.fastLinkItems.Count, (button, parameters) => {

                var item = parameters.data[parameters.index];
                button.Get<TextComponent>().SetText(item.caption);
                button.Get<ImageComponent>().SetColor(ConsoleScreen.GetColorByFastLinkStyle(item.style));
                button.SetCallback(() => {

                    if (item.run == true) {

                        this.ApplyCommand(item.cmd);

                    } else {
                        
                        this.ReplaceInput($"{item.cmd} ");
                        
                    }
                    
                });
                button.Show();

            }, new ClosureFastLinksParameters() {
                data = this.fastLinkItems,
            });
            
            this.list.SetDataSource(this);
            this.list.SetItems<ButtonComponent, ClosureParameters>(this.GetDrawItemsCount(), (component, parameters) => {

                var item = parameters.data.GetDrawItem(parameters.index);
                var text = component.Get<TextComponent>();
                text.SetText(item.isCommand == true ? "<color=#777><b>></b></color> " : string.Empty, item.line);
                text.SetColor(item.isCommand == true ? new Color(0.15f, 0.6f, 1f) : ConsoleScreen.GetColorByLogType(item.logType));
                component.SetCallback(() => {
                    this.ReplaceInput(item.line);
                });
                component.SetInteractable(item.isCommand);
                component.Show();
                
            }, new ClosureParameters() {
                data = this,
            });

            if (this.isDirty == true) {

                this.logsCounterComponent.SetInfo();
                this.isDirty = false;

            }

        }

    }
    
}