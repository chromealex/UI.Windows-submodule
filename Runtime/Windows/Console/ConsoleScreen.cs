using System.Linq;
using UnityEngine.UI.Windows;
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
    
    /*[Help("Test module")]
    public class Test : ConsoleModule {

        [Help("Prints help bool")]
        public void Z(bool a) {

            Debug.Log("Z: " + a);

        }

        [Help("Prints help string")]
        public void Help1() {

            Debug.Log("Help");

        }

        [Help("Prints help string with a")]
        public void Help2(int a) {

            Debug.Log("Help2: " + a);

        }

        [Help("Prints help string with a and b")]
        public void Help3(int a, float b) {

            Debug.Log("Help3: " + a + " :: " + b);

        }

    }*/

    public class ConsoleScreen : LayoutWindowType {

        public struct CommandItem {

            public string str;
            public string moduleName;
            public string methodName;
            public int argsCount;
            public string[] args;

        }

        public struct DrawItem {

            public string line;
            public LogType logType;
            public bool isCommand;

        }
        
        private ListComponent list;
        private InputFieldComponent inputField;
        
        private readonly List<CommandItem> commands = new List<CommandItem>();
        private readonly List<IConsoleModule> moduleItems = new List<IConsoleModule>();
        private readonly List<DrawItem> drawItems = new List<DrawItem>();
        private char openCloseChar;
        private int currentIndex;

        public bool autoConnectLogs = true;
        private string helpInitPrint;
        
        public void OnParametersPass(char openCloseChar) {

            this.openCloseChar = openCloseChar;

        }

        public override void OnEmptyPass() {

            this.openCloseChar = '`';

        }
        
        public override void OnInit() {
            
            base.OnInit();

            this.GetLayoutComponent(out this.list);
            this.GetLayoutComponent(out this.inputField);
            
            this.inputField.SetCallbackValidateChar(this.OnChar);
            this.inputField.SetCallbackEditEnd(this.OnEditEnd);

            this.CollectModules();

            if (this.autoConnectLogs == true) {

                Application.logMessageReceived += this.OnAddLog;

            }

            this.helpInitPrint = this.GetInitHelp();

        }

        public override void OnShowBegin() {
            
            base.OnShowBegin();

            if (string.IsNullOrEmpty(this.helpInitPrint) == false) this.AddLine(this.helpInitPrint);
            this.helpInitPrint = string.Empty;

        }

        public override void OnDeInit() {
            
            base.OnDeInit();
            
            Application.logMessageReceived -= this.OnAddLog;
            
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

        private void OnAddLog(string condition, string trace, LogType type) {
            
            this.AddLine(condition, type);
            if (type == LogType.Exception) this.AddLine(trace);

        }

        public void CollectModules() {
            
            var types = System.AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes().Where(y => y.IsAbstract == false && y.GetInterfaces().Contains(typeof(IConsoleModule)) == true)).ToArray();
            foreach (var type in types) {

                var module = (IConsoleModule)System.Activator.CreateInstance(type);
                this.moduleItems.Add(module);
                
            }
            
        }

        public void ClearInput() {

            this.inputField.Clear();

        }

        public void ReplaceInput(string str) {
            
            this.inputField.SetText(str);
            
        }

        private void OnEditEnd(string text) {

            if (Input.GetKeyDown(KeyCode.KeypadEnter) == true || Input.GetKeyDown(KeyCode.Return)) {

                this.ApplyCommand(text);

            }

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

        private char OnChar(string arg1, int arg2, char arg3) {

            if (arg3 == this.openCloseChar) {
                
                return '\0';
                
            }
            
            return arg3;

        }

        private IConsoleModule GetModule(CommandItem command) {

            var moduleName = command.moduleName.ToLower();
            foreach (var module in this.moduleItems) {

                if (module.GetType().Name.ToLower() == moduleName) {

                    return module;

                }
                
            }
            
            return null;

        }

        public void AddModule(IConsoleModule module) {
            
            this.moduleItems.Add(module);
            
        }

        public void AddHR() {

            this.AddLine("<color=#777>--------------------------------------</color>");

        }
        
        public void AddLine(string text, LogType logType = LogType.Log, bool isCommand = false) {
            
            this.drawItems.Add(new DrawItem() {
                line = text,
                logType = logType,
                isCommand = isCommand,
            });
            
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
                foreach (var method in methods) {

                    if (this.IsValidMethod(method) == false) continue;

                    if (method.Name.ToLower() == methodName) {

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
                                
                                this.AddLine(ex.Message, LogType.Error);
                                
                            }

                            break;

                        }

                    }
                    
                }

                if (run == false) {

                    this.AddLine($"Module `{command.moduleName}` has no method with parameters length = {command.argsCount}", LogType.Warning);

                }

            } else {
                
                this.AddLine($"Module `{command.moduleName}` not found", LogType.Warning);

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
            
            this.AddLine(this.GetHelpString(module.GetType()).Trim());
            this.AddLine("Module methods:");
            this.AddHR();
            var methods = module.GetType().GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            foreach (var method in methods) {
                
                if (this.IsValidMethod(method) == false) continue;
                
                var pars = method.GetParameters();
                var parameters = pars.Select(x => this.TypeToString(x.ParameterType) + " " + x.Name).ToArray();
                this.AddLine("\t<color=#3af>" + method.Name.ToLower() + "</color>(" + string.Join(", ", parameters) + ")" + this.GetHelpString(method.GetType()));

            }
            this.AddHR();

        }

        public void PrintHelp() {
            
            this.AddLine("Modules:");
            this.AddHR();
            foreach (var module in this.moduleItems) {
                
                this.AddLine("\t<color=#3af>" + module.GetType().Name.ToLower() + "</color>" + this.GetHelpString(module.GetType()));
                
            }
            this.AddHR();
            
        }

        private string GetHelpString(System.Type type) {
            
            var attrs = type.GetCustomAttributes(typeof(HelpAttribute), false);
            if (attrs.Length > 0) {

                return "\t<color=#999>" + ((HelpAttribute)attrs[0]).text + "</color>";

            }

            return string.Empty;

        }
        
        public void ApplyCommand(string command) {

            var cmd = command.Trim();
            if (string.IsNullOrEmpty(cmd) == true) return;

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
            
            if (cmd == "help") {

                this.AddLine(cmd, isCommand: true);
                var itemHelp = new CommandItem() {
                    str = cmd,
                    moduleName = null,
                    methodName = null,
                    argsCount = 0,
                    args = null,
                };
                this.commands.Add(itemHelp);
                this.PrintHelp();
                this.ClearInput();
                return;
                
            }
            
            var splitted = cmd.Split(new [] { " " }, System.StringSplitOptions.RemoveEmptyEntries);
            if (splitted.Length < 2) return;
            
            var moduleName = splitted[0];
            var methodName = splitted[1];
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

        }

        private struct ClosureParameters : IListClosureParameters {

            public int index { get; set; }
            public List<DrawItem> data;

        }

        public static Color GetColorByLogType(LogType logType) {

            var color = Color.white;
            switch (logType) {
                
                case LogType.Warning:
                    color = Color.yellow;
                    break;

                case LogType.Error:
                    color = Color.red;
                    break;

            }
            
            return color;

        }
        
        public void LateUpdate() {

            if (this.GetState() != ObjectState.Shown) return;
            
            if (Input.GetKeyDown(KeyCode.UpArrow) == true) {

                this.MoveUp();

            }

            if (Input.GetKeyDown(KeyCode.DownArrow) == true) {

                this.MoveDown();

            }

            if (this.inputField.IsFocused() == false) {
                
                this.inputField.SetFocus();
                
            }
            
            this.list.SetItems<ButtonComponent, ClosureParameters>(this.drawItems.Count, (component, parameters) => {

                var item = parameters.data[parameters.index];
                var text = component.Get<TextComponent>();
                text.SetText(item.isCommand == true ? "<color=#777><b>></b></color> " : string.Empty, item.line);
                text.SetColor(item.isCommand == true ? new Color(0.15f, 0.6f, 1f) : ConsoleScreen.GetColorByLogType(item.logType));
                component.SetCallback(() => {
                    this.ReplaceInput(item.line);
                });
                component.SetInteractable(item.isCommand);
                component.Show();
                
            }, new ClosureParameters() {
                data = this.drawItems
            });

        }

    }
    
}