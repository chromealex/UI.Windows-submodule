namespace UnityEngine.UI.Windows.Utilities {
    
    using System.Collections.Generic;

    public interface ITasksUpdate {

        void Update();

    }
        
    public static class WaitTasks {

        private static class CacheType<TState> {

            public static WaitTasks<TState> instance;

        }

        private static class CacheType {

            public static WaitTasksEmpty instance;

        }

        private static readonly List<ITasksUpdate> updates = new List<ITasksUpdate>();
        private static readonly List<ITasksUpdate> endOfFrameUpdates = new List<ITasksUpdate>();
        
        public static void Add<TState>(TState state, System.Func<TState, bool> waitFor, System.Action<TState> callback) {
            if (waitFor.Invoke(state) == true) {
                callback.Invoke(state);
                return;
            }
            if (CacheType<TState>.instance == null) {
                updates.Add(CacheType<TState>.instance = new WaitTasks<TState>());
            }
            CacheType<TState>.instance.Add(state, waitFor, callback);
        }

        public static void Add(System.Func<bool> waitFor, System.Action callback) {
            if (waitFor.Invoke() == true) {
                callback.Invoke();
                return;
            }
            if (CacheType.instance == null) {
                updates.Add(CacheType.instance = new WaitTasksEmpty());
            }
            CacheType.instance.Add(waitFor, callback);
        }

        public static void AddEndOfFrame<TState>(TState state, System.Func<TState, bool> waitFor, System.Action<TState> callback) {
            if (waitFor.Invoke(state) == true) {
                callback.Invoke(state);
                return;
            }
            if (CacheType<TState>.instance == null) {
                endOfFrameUpdates.Add(CacheType<TState>.instance = new WaitTasks<TState>());
            }
            CacheType<TState>.instance.Add(state, waitFor, callback);
        }

        public static void AddEndOfFrame(System.Func<bool> waitFor, System.Action callback) {
            if (waitFor.Invoke() == true) {
                callback.Invoke();
                return;
            }
            if (CacheType.instance == null) {
                endOfFrameUpdates.Add(CacheType.instance = new WaitTasksEmpty());
            }
            CacheType.instance.Add(waitFor, callback);
        }

        public static void Update() {
            for (int i = updates.Count - 1; i >= 0; --i) {
                updates[i].Update();
            }
        }

        public static void EndOfFrameUpdate() {
            for (int i = endOfFrameUpdates.Count - 1; i >= 0; --i) {
                endOfFrameUpdates[i].Update();
            }
        }

    }

    internal class WaitTasks<TState> : ITasksUpdate {

        private struct Item {

            public TState state;
            public System.Func<TState, bool> waitFor;
            public System.Action<TState> callback;

        }

        private static readonly List<Item> items = new List<Item>();
	        
        public void Add(TState state, System.Func<TState, bool> waitFor, System.Action<TState> callback) {
            items.Add(new Item { state = state, waitFor = waitFor, callback = callback });
        }

        public void Update() {

            for (int i = items.Count - 1; i >= 0; --i) {
                var item = items[i];
                if (item.waitFor.Invoke(item.state) == true) {
                    item.callback.Invoke(item.state);
                    items.RemoveAt(i);
                }
            }

        }

    }

    internal class WaitTasksEmpty : ITasksUpdate {

        private struct Item {

            public System.Func<bool> waitFor;
            public System.Action callback;

        }

        private static readonly List<Item> items = new List<Item>();
	        
        public void Add(System.Func<bool> waitFor, System.Action callback) {
            items.Add(new Item { waitFor = waitFor, callback = callback });
        }

        public void Update() {

            for (int i = items.Count - 1; i >= 0; --i) {
                var item = items[i];
                if (item.waitFor.Invoke() == true) {
                    item.callback.Invoke();
                    items.RemoveAt(i);
                }
            }

        }

    }

}