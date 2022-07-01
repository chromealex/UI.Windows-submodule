using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEngine.UI.Windows {

    [CreateAssetMenu(menuName = "UI.Windows/Console/System Module")]
    public class WindowSystemConsoleModule : WindowSystemModule {

        internal WindowSystemConsole console;
        
        public override void OnStart() {
            
            this.console = new WindowSystemConsole();
            
            /*for (int i = 0; i < 100_000; ++i) {

                var randomString = RandomString(Random.Range(10, 200));
                Debug.Log("Test update: " + Time.frameCount + " :: " + i + " :: " + randomString);

            }*/

        }
        
        /*
        private static string RandomString(int length) {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(System.Linq.Enumerable.Repeat(chars, length).Select(s => s[Random.Range(0, s.Length)]).ToArray());
        }*/

        public override void OnUpdate() {
            
            this.console.Update();
            
        }

        public override void OnDestroy() {
            
            if (this.console != null) this.console.Dispose();
            this.console = null;

        }

    }

}