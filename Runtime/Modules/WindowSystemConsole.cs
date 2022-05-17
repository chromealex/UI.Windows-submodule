namespace UnityEngine.UI.Windows {
    
    using Runtime.Windows;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    public class CaptionComparer : IComparer<WindowSystemConsole.FastLink> {

        public int Compare(WindowSystemConsole.FastLink x, WindowSystemConsole.FastLink y) {
            
            if (Object.ReferenceEquals(x, y)) {
                return 0;
            }

            if (Object.ReferenceEquals(null, y)) {
                return 1;
            }

            if (Object.ReferenceEquals(null, x)) {
                return -1;
            }

            if (x.style < y.style) return 1;
            if (x.style > y.style) return -1;
            
            return string.Compare(x.caption, y.caption, System.StringComparison.InvariantCultureIgnoreCase);
            
        }

    }

    public interface IConsoleModule {

        void OnStart();

    }

    [UnityEngine.Scripting.PreserveAttribute]
    public abstract class ConsoleModule : IConsoleModule {

        public ConsoleScreen screen;

        [Ignore]
        [UnityEngine.Scripting.PreserveAttribute]
        public virtual void OnStart() { }

        [UnityEngine.Scripting.PreserveAttribute]
        [Help("Prints available methods for this module")]
        public void Help() {

            var console = WindowSystem.GetConsole();
            console.PrintHelp(this);

        }

    }

    [System.AttributeUsageAttribute(System.AttributeTargets.Method)]
    public class IgnoreAttribute : System.Attribute {

    }

    [System.AttributeUsageAttribute(System.AttributeTargets.Class | System.AttributeTargets.Method)]
    public class HelpAttribute : System.Attribute {

        public string text;

        public HelpAttribute(string text) {
            
            this.text = text;
            
        }

    }

    [System.AttributeUsageAttribute(System.AttributeTargets.Class | System.AttributeTargets.Method)]
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
        Directory,

    }
    
    [System.AttributeUsageAttribute(System.AttributeTargets.Method)]
    public class FastLinkAttribute : System.Attribute {

        public string text;
        public FastLinkType style;
        public string validationMethod;
        public string visibilityMethod;

        public FastLinkAttribute(string richText, FastLinkType style = FastLinkType.Default, string validationMethod = null, string visibilityMethod = null) {
            
            this.text = richText;
            this.style = style;
            this.validationMethod = validationMethod;
            this.visibilityMethod = visibilityMethod;

        }

    }

    public class WindowSystemConsole {

        private const int MAX_TEXT_LENGTH = 2000;

        public enum LogsFilter {

            Any = -1,
            All = LogsFilter.Assert | LogsFilter.Error | LogsFilter.Warning | LogsFilter.Log | LogsFilter.Exception,
            Error = 1 << 1,
            Assert = 1 << 2,
            Warning = 1 << 3,
            Log = 1 << 4,
            Exception = 1 << 5,

        }
        
        [System.Serializable]
        public struct DrawItem {

            public string line;
            public LogType logType;
            public bool isCommand;
            public bool canCopy;

        }
        
        public class FastLinkDirectory {

            public int id;
            public int parentId;

        }

        [System.Serializable]
        public class FastLink : FastLinkDirectory {

            public string cmd;
            public bool run;
            public string caption;
            public IConsoleModule module;
            public System.Reflection.MethodInfo validationMethodInfo;
            public System.Reflection.MethodInfo visibilityMethodInfo;
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

        private List<DrawItem> drawItems = new List<DrawItem>();
        private readonly List<FastLink> fastLinkItems = new List<FastLink>();
        private readonly List<CommandItem> commands = new List<CommandItem>();
        private readonly List<IConsoleModule> moduleItems = new List<IConsoleModule>();
        private Dictionary<LogType, int> logsCounter = new Dictionary<LogType, int>();
        private LogsFilter logsFilter;
        
        private string helpInitPrint;
        private System.Threading.Thread unityThread;

        private int _isDirty;
        public bool isDirty {
            get => System.Threading.Interlocked.Exchange(ref this._isDirty, 0) == 1;
            set => System.Threading.Interlocked.Exchange(ref this._isDirty, value == true ? 1 : 0);
        }

        public WindowSystemConsole() {
            
            this.helpInitPrint = this.GetInitHelp();
            this.SetLogFilterType(LogType.Log, true);
            this.SetLogFilterType(LogType.Warning, true);
            this.SetLogFilterType(LogType.Error, true);

            this.unityThread = System.Threading.Thread.CurrentThread;

            this.CollectModules();

            Application.logMessageReceivedThreaded += this.OnAddLogThreaded;

            if (string.IsNullOrEmpty(this.helpInitPrint) == false) this.AddLine(this.helpInitPrint);
            this.helpInitPrint = string.Empty;
            
        }

        private WindowBase consoleWindowInstance;
        public void Update() {
            
            if (Input.GetKeyDown(KeyCode.BackQuote) == true ||
                Input.touchCount >= 3) {

                var isActive = true;
                var checkTouch = Input.touchCount >= 3;
                if (checkTouch == true) {

                    isActive = false;
                    if (this.consoleWindowInstance == null) {
                        
                        isActive = (Input.touchCount == 5 && Input.GetTouch(Input.touchCount - 1).phase == TouchPhase.Began);
                        
                    } else {
                        
                        isActive = (Input.touchCount == 3 && Input.GetTouch(Input.touchCount - 1).phase == TouchPhase.Began);
                        
                    }

                }

                if (isActive == true) {

                    if (this.consoleWindowInstance == null) {

                        this.consoleWindowInstance = WindowSystem.ShowSync<UnityEngine.UI.Windows.Runtime.Windows.ConsoleScreen>(x => {

                            WindowSystem.GetEvents().RegisterOnce(x, WindowEvent.OnHideBegin, () => { this.consoleWindowInstance = null; });
                            x.OnEmptyPass();

                        });

                    } else {

                        this.consoleWindowInstance.Hide();
                        this.consoleWindowInstance = null;

                    }

                }

            }

        }

        public void Dispose() {
            
            Application.logMessageReceivedThreaded -= this.OnAddLogThreaded;
            if (this.consoleWindowInstance != null) {
                
                this.consoleWindowInstance.Hide(TransitionParameters.Default.ReplaceImmediately(true));
                this.consoleWindowInstance = null;

            }

        }

        public List<DrawItem> GetItems() {

            return this.drawItems;

        }

        public void GetFastLinks(int dirId, List<WindowSystemConsole.FastLink> results) {

            results.Clear();
            if (dirId >= 0) {

                int parentDirectoryId = -1;
                for (int i = 0; i < this.fastLinkItems.Count; ++i) {
                    
                    var item = this.fastLinkItems[i];
                    if (item.id == dirId) {

                        parentDirectoryId = item.parentId;
                        break;

                    }
                    
                }

                // Add [DIR UP] link
                results.Add(new WindowSystemConsole.FastLink() {
                    id = parentDirectoryId,
                    style = FastLinkType.Directory,
                    caption = "[DIR UP]",
                });
                
            }
            
            for (int i = 0; i < this.fastLinkItems.Count; ++i) {
                
                var item = this.fastLinkItems[i];
                if (item.parentId == dirId) {
                    
                    results.Add(item);
                    
                }
                
            }
            
        }

        public bool ValidateFastLink(WindowSystemConsole.FastLink link) {

            if (link.validationMethodInfo != null) {

                var result = (bool)link.validationMethodInfo.Invoke(link.module, null);
                return result;

            }
            
            return true;

        }

        public bool ValidateFastLinkVisibility(WindowSystemConsole.FastLink link) {

            if (link.visibilityMethodInfo != null) {

                var result = (bool)link.visibilityMethodInfo.Invoke(link.module, null);
                return result;

            }
            
            return true;

        }

        private readonly System.Text.StringBuilder builderCache = new System.Text.StringBuilder();
        private int prevDirId = -1;
        private string prevDirPath;
        public string GetDirectoryPath(int dirId) {

            if (this.prevDirId == dirId) return this.prevDirPath;

            var str = this.builderCache;
            str.Clear();
            while (dirId >= 0) {

                var found = false;
                for (int i = 0; i < this.fastLinkItems.Count; ++i) {

                    var cache = this.fastLinkItems[i];
                    if (cache.id == dirId) {

                        found = true;
                        dirId = cache.parentId;
                        str.Insert(0, '/');
                        str.Insert(0, cache.caption);
                        break;
                        
                    }

                }

                if (found == false) {
                    
                    break;
                    
                }
                
            }
            str.Insert(0, '/');

            this.prevDirPath = str.ToString();
            this.prevDirId = dirId;
            return this.prevDirPath;

        }

        public void CollectModules() {

            var globalId = 0;
            var types = System.AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes().Where(y => y.IsAbstract == false && y.GetInterfaces().Contains(typeof(IConsoleModule)) == true)).ToArray();
            foreach (var type in types) {

                var module = (IConsoleModule)System.Activator.CreateInstance(type);
                module.OnStart();
                this.moduleItems.Add(module);
                
                var methods = module.GetType().GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                foreach (var method in methods) {

                    if (this.IsValidMethod(method) == false) continue;

                    if (this.GetFastLink(module, method, out var caption, out var style, out var validationMethod, out var visibilityMethod) == true) {

                        var path = caption.Split('/');
                        var parentId = -1;
                        for (var i = 0; i < path.Length; ++i) {

                            caption = path[i];
                            var item = this.fastLinkItems.FirstOrDefault(x => x.style == FastLinkType.Directory && x.caption == caption);
                            if (item != null) {
                                
                                // The same directory found - skip current
                                parentId = item.id;
                                continue;
                                
                            }
                            
                            var id = ++globalId;
                            if (i == path.Length - 1) {
                                
                                this.fastLinkItems.Add(new FastLink() {
                                    id = id,
                                    parentId = parentId,
                                    run = method.GetParameters().Length == 0 && style < FastLinkType.Alarm,
                                    cmd = $"{module.GetType().Name.ToLower()} {method.Name.ToLower()}",
                                    caption = caption,
                                    module = module,
                                    validationMethodInfo = validationMethod,
                                    visibilityMethodInfo = visibilityMethod,
                                    style = style,
                                });
                                
                            } else {

                                this.fastLinkItems.Add(new FastLink() {
                                    id = id,
                                    parentId = parentId,
                                    caption = caption,
                                    style = FastLinkType.Directory,
                                });
                                parentId = id;

                            }

                        }
                        
                    }

                }
                
            }
            
            this.fastLinkItems.Sort(new CaptionComparer());
            
        }

        private bool GetFastLink(IConsoleModule module, System.Reflection.ICustomAttributeProvider methodInfo, out string text, out FastLinkType style, out System.Reflection.MethodInfo validationMethodInfo, out System.Reflection.MethodInfo visibilityMethodInfo) {
            
            text = string.Empty;
            style = FastLinkType.Default;
            validationMethodInfo = null;
            visibilityMethodInfo = null;
            
            var aliasAttrs = methodInfo.GetCustomAttributes(typeof(FastLinkAttribute), false);
            if (aliasAttrs.Length == 0) return false;
            var attr = (aliasAttrs[0] as FastLinkAttribute);
            if (string.IsNullOrEmpty(attr.validationMethod) == false) validationMethodInfo = module.GetType().GetMethod(attr.validationMethod, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (string.IsNullOrEmpty(attr.visibilityMethod) == false) visibilityMethodInfo = module.GetType().GetMethod(attr.visibilityMethod, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            text = attr.text.ToLower();
            style = attr.style;
            return true;
            
        }

        private string GetAlias(System.Reflection.ICustomAttributeProvider methodInfo) {

            var aliasAttrs = methodInfo.GetCustomAttributes(typeof(AliasAttribute), false);
            if (aliasAttrs.Length == 0) return string.Empty;
            return (aliasAttrs[0] as AliasAttribute)?.text.ToLower();

        }

        private bool IsValidMethod(System.Reflection.MethodInfo methodInfo) {

            var name = methodInfo.Name;
            if (name == "GetHashCode" || name == "ToString" || name == "Equals" || name == "GetType") return false;
            
            var ignoreAttrs = methodInfo.GetCustomAttributes(typeof(IgnoreAttribute), false);
            if (ignoreAttrs.Length > 0) return false;

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

        private string GetMethodCallString(System.Reflection.MethodInfo methodInfo) {

            var pars = methodInfo.GetParameters();
            var parameters = pars.Select(x => $"{this.TypeToString(x.ParameterType)} {x.Name}").ToArray();
            var parsStr = string.Join(", ", parameters);
            return $"<color=#3af>{methodInfo.Name.ToLower()}</color>({parsStr}){this.GetHelpString($"{this.GetSpace(4)}{methodInfo.Name.ToLower()}({parsStr})", methodInfo)}";
            
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

        public void PrintHelpAllModules() {
            
            this.PrintHelp(this.moduleItems);
            
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

        public void AddHR() {

            this.AddLine("<color=#777>--------------------------------------</color>");

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

        private IConsoleModule GetModule(WindowSystemConsole.CommandItem command) {

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

        public List<System.Reflection.MethodInfo> GetMethodsByPart(string moduleName, string methodNamePart, out string first) {

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
        
        public List<IConsoleModule> GetModulesByPart(string moduleNamePart, out string first) {

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

        public void AddCommand(CommandItem command) {
            
            this.commands.Add(command);
            
        }
        
        public void RunCommand(CommandItem command, ConsoleScreen consoleScreen) {

            this.AddLine(command.str, isCommand: true);

            var run = false;
            var module = this.GetModule(command);
            if (module != null) {

                if (module is ConsoleModule consoleModule) {

                    consoleModule.screen = consoleScreen;

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

                                if (command.argsCount == 0) {

                                    method.Invoke(module, null);

                                } else {

                                    var objs = new object[command.argsCount];
                                    for (int i = 0; i < command.argsCount; ++i) {

                                        objs[i] = this.ConvertArg(command.args[i], pars[i].ParameterType);

                                    }

                                    method.Invoke(module, objs);

                                }

                                run = true;

                            } catch (System.Exception ex) {

                                if (ex.InnerException != null) {
                                    
                                    this.AddLog(ex.InnerException.Message, LogType.Exception, ex.InnerException.StackTrace);
                                    
                                } else {

                                    this.AddLog(ex.Message, LogType.Error);

                                }

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
            
        }

        public List<CommandItem> GetCommands() {

            return this.commands;

        }

        public void AddModule(IConsoleModule module) {
            
            this.moduleItems.Add(module);
            
        }

        public void AddLine(string text, LogType logType = LogType.Log, bool isCommand = false, bool canCopy = false) {

            lock (this.drawItems) {

                if (text.Length > WindowSystemConsole.MAX_TEXT_LENGTH) {

                    text = text.Substring(0, WindowSystemConsole.MAX_TEXT_LENGTH) + "<truncate message>";

                }

                this.drawItems.Add(new DrawItem() {
                    line = text,
                    logType = logType,
                    isCommand = isCommand,
                    canCopy = canCopy,
                });

            }

        }

        private void OnAddLogThreaded(string text, string trace, LogType type) {

            if (System.Threading.Thread.CurrentThread != this.unityThread) {

                this.AddLog($"<color=#bb0>[Thread {System.Threading.Thread.CurrentThread.Name}]</color> {text}", type, trace);

            } else {
                
                this.AddLog(text, type, trace);

            }

        }

        public void AddLog(string text, LogType type = LogType.Log, string trace = null) {

            if (this.logsCounter == null) return;

            lock (this.logsCounter) {

                if (this.logsCounter.TryGetValue(type, out var count) == true) {

                    this.logsCounter[type] = count + 1;

                } else {

                    this.logsCounter.Add(type, 1);

                }

            }

            if (type == LogType.Exception && string.IsNullOrEmpty(trace) == false) {
                
                this.AddLine(text + "\n" + trace, type, canCopy: true);
                
            } else {
                
                this.AddLine(text, type, canCopy: true);
                
            }

            this.isDirty = true;

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

            var mask = (LogsFilter)(1 << (int)(logType + 1));
            if ((this.logsFilter & mask) != 0) return true;
            
            return false;
            
        }

        public LogsFilter GetLogFilterType() => this.logsFilter;

        public void SetLogFilterType(LogType logType, bool state) {
            
            if (logType == LogType.Error) {

                this.SetLogFilterType(LogType.Assert, state);
                this.SetLogFilterType(LogType.Exception, state);

            }

            var mask = 1 << (int)(logType + 1);
            if (state == true) {
                this.logsFilter |= (LogsFilter)mask;
            } else {
                this.logsFilter &= ~(LogsFilter)mask;
            }
            
            this.isDirty = true;
            
        }

        public int GetCounter(LogType logType) {

            if (this.logsCounter.TryGetValue(logType, out var count) == true) {

                return count;

            }
            
            return 0;
            
        }

    }

}