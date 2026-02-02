namespace UnityEngine.UI.Windows.Utilities {
    
    using System.Collections.Generic;

    public interface ITasksUpdate {

        void Update();

    }

    public struct WaitTaskCancellationToken<T> : System.IEquatable<WaitTaskCancellationToken<T>> {

        public uint id;
        public int index;
        public bool isCreated;

        public bool Cancel() {
            if (this.isCreated == false) return false;
            return WaitTasks.Cancel(this);
        }

        public bool Equals(WaitTaskCancellationToken<T> other) {
            return this.id == other.id && this.index == other.index;
        }

        public override bool Equals(object obj) {
            return obj is WaitTaskCancellationToken<T> other && this.Equals(other);
        }

        public override int GetHashCode() {
            return System.HashCode.Combine(this.id, this.index);
        }

    }

    public struct WaitTaskCancellationToken : System.IEquatable<WaitTaskCancellationToken> {

        public uint id;
        public int index;
        public bool isCreated;

        public bool Cancel() {
            if (this.isCreated == false) return false;
            return WaitTasks.Cancel(this);
        }

        public bool Equals(WaitTaskCancellationToken other) {
            return this.id == other.id && this.index == other.index;
        }

        public override bool Equals(object obj) {
            return obj is WaitTaskCancellationToken other && this.Equals(other);
        }

        public override int GetHashCode() {
            return System.HashCode.Combine(this.id, this.index);
        }

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
        
        public static bool Cancel<T>(WaitTaskCancellationToken<T> token) {
            return CacheType<T>.instance.Cancel(token);
        }

        public static bool Cancel(WaitTaskCancellationToken token) {
            return CacheType.instance.Cancel(token);
        }

        public static WaitTaskCancellationToken<TState> Add<TState>(TState state, System.Func<TState, bool> waitFor, System.Action<TState> callback) {
            if (waitFor.Invoke(state) == true) {
                callback.Invoke(state);
                return default;
            }
            if (CacheType<TState>.instance == null) {
                updates.Add(CacheType<TState>.instance = new WaitTasks<TState>());
            }
            return CacheType<TState>.instance.Add(state, waitFor, callback);
        }

        public static WaitTaskCancellationToken Add(System.Func<bool> waitFor, System.Action callback) {
            if (waitFor.Invoke() == true) {
                callback.Invoke();
                return default;
            }
            if (CacheType.instance == null) {
                updates.Add(CacheType.instance = new WaitTasksEmpty());
            }
            return CacheType.instance.Add(waitFor, callback);
        }

        public static WaitTaskCancellationToken<TState> AddEndOfFrame<TState>(TState state, System.Func<TState, bool> waitFor, System.Action<TState> callback) {
            if (waitFor.Invoke(state) == true) {
                callback.Invoke(state);
                return default;
            }
            if (CacheType<TState>.instance == null) {
                endOfFrameUpdates.Add(CacheType<TState>.instance = new WaitTasks<TState>());
            }
            return CacheType<TState>.instance.Add(state, waitFor, callback);
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

            public uint id;
            public TState state;
            public System.Func<TState, bool> waitFor;
            public System.Action<TState> callback;

        }

        private static readonly List<Item> items = new List<Item>();
        private static uint id;
        
        public bool Cancel(WaitTaskCancellationToken<TState> token) {
            
            for (int i = token.index; i < items.Count; ++i) {
                var item = items[i];
                if (item.id == token.id) {
                    items.RemoveAt(i);
                    return true;
                }
            }

            for (int i = 0; i < items.Count; ++i) {
                var item = items[i];
                if (item.id == token.id) {
                    items.RemoveAt(i);
                    return true;
                }
            }

            return false;
            
        }

        public WaitTaskCancellationToken<TState> Add(TState state, System.Func<TState, bool> waitFor, System.Action<TState> callback) {
            items.Add(new Item { id = ++id, state = state, waitFor = waitFor, callback = callback });
            return new WaitTaskCancellationToken<TState>() { id = id, index = items.Count - 1, isCreated = true, };
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

            public uint id;
            public System.Func<bool> waitFor;
            public System.Action callback;
            
        }

        private static readonly List<Item> items = new List<Item>();
        private uint id;
        
        public bool Cancel(WaitTaskCancellationToken token) {
            
            for (int i = token.index; i < items.Count; ++i) {
                var item = items[i];
                if (item.id == token.id) {
                    items.RemoveAt(i);
                    return true;
                }
            }

            for (int i = 0; i < items.Count; ++i) {
                var item = items[i];
                if (item.id == token.id) {
                    items.RemoveAt(i);
                    return true;
                }
            }

            return false;
            
        }

        public WaitTaskCancellationToken Add(System.Func<bool> waitFor, System.Action callback) {
            items.Add(new Item { id = ++id, waitFor = waitFor, callback = callback });
            return new WaitTaskCancellationToken() { id = id, index = items.Count - 1, isCreated = true, };
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