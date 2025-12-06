using UnityEngine;

namespace ME.Taptic {

    public class TapticAnimationCurveImpl : ITapticModuleInternal {

        [System.Serializable]
        public struct TapticChain {

            public float duration;
            public float strength;
            public TapticType tapticType;

        }

        private TapticChain[] tapticChains;
        private int maxChannels = 5;

        private float[] channels;
        private readonly System.Collections.Generic.List<TapticChain> searchCache = new System.Collections.Generic.List<TapticChain>();
        private readonly System.Collections.Generic.List<TapticChain> searchCacheRnd = new System.Collections.Generic.List<TapticChain>();

        private ITapticEngineInternal engine;

        void ITapticModuleInternal.Initialize(ITapticEngineInternal engine) {

            // Initialize chains
            this.tapticChains = new[] {
                new TapticChain() { duration = 0.19f, strength = 0.5f, tapticType = TapticType.Success },
                new TapticChain() { duration = 0.23f, strength = 0.4f, tapticType = TapticType.Warning },
                new TapticChain() { duration = 0.42f, strength = 0.6f, tapticType = TapticType.Failure },
                new TapticChain() { duration = 0.064f, strength = 0.1f, tapticType = TapticType.Light },
                new TapticChain() { duration = 0.064f, strength = 0.3f, tapticType = TapticType.Medium },
                new TapticChain() { duration = 0.064f, strength = 0.5f, tapticType = TapticType.Heavy },
                new TapticChain() { duration = 0.5f, strength = 0.9f, tapticType = TapticType.Vibrate },
                new TapticChain() { duration = 0.048f, strength = 0.05f, tapticType = TapticType.Selection },
            };

            this.engine = engine;

        }

        bool ITapticModule.IsPlaying() {

            return this.isPlaying;

        }

        void ITapticModuleInternal.Update() {

            if (this.isPlaying == false) {
                return;
            }

            var curve = this.playingCurve;
            var lastKey = curve.keys[curve.length - 1];
            this.playingTime += Time.deltaTime;
            if (this.playingTime >= lastKey.time) {

                this.isPlaying = false;
                this.playingCurve = null;
                return;

            }

            var channelIdx = this.GetFreeChannelIndex(this.playingTime);
            if (channelIdx >= 0) {

                var strength = curve.Evaluate(this.playingTime);
                strength = Mathf.Clamp01(strength);
                int keyIdx;
                var nextKey = this.GetNextKey(curve, this.playingTime, out keyIdx);

                var item = this.FindItemNearestToDurationAndStrength(keyIdx == curve.length - 1 ? -1f : nextKey.time - this.playingTime, strength, this.playingRandomize);
                if (item.tapticType != TapticType.None) {

                    //Debug.LogWarning("Play " + item.tapticType + " for length: " + (nextKey.time - this.playingTime) + ", strength: " + strength + ", time: " + this.playingTime);

                    this.PlaySingle_INTERNAL(item.tapticType, item.duration, item.strength);
                    this.channels[channelIdx] += item.duration;

                } else {

                    //Debug.LogWarning("Skip for length: " + (nextKey.time - this.playingTime) + ", strength: " + strength + ", time: " + this.playingTime);

                }

            }

        }

        void ITapticModule.SetMaxChannels(int value) {

            this.maxChannels = value;
            this.ValidateChannels();

        }

        void ITapticModule.PlayCurve(AnimationCurve curve, bool randomize) {

            if (curve.length == 0) {
                return;
            }

            if (this.isPlaying == true) {
                return;
            }

            this.isPlaying = true;
            this.playingCurve = curve;
            this.playingTime = 0f;
            this.playingRandomize = randomize;
            this.ValidateChannels();
            for (var i = 0; i < this.channels.Length; ++i) {
                this.channels[i] = 0f;
            }

        }

        void ITapticModule.PlaySingle(TapticType type) {

            this.PlaySingle_INTERNAL(type, -1f, -1f);

        }

        private void PlaySingle_INTERNAL(TapticType type, float duration, float strength) {

            this.engine.Play(type, duration, strength);

        }

        private TapticChain FindItemNearestToDurationAndStrength(float maxDuration, float maxStrength, bool randomize) {

            var found = default(TapticChain);

            // Looking for nearest amplitude
            var max = float.MaxValue;
            for (var i = 0; i < this.tapticChains.Length; ++i) {

                var item = this.tapticChains[i];
                var s = Mathf.Abs(item.strength - maxStrength);
                if (s <= max && item.strength <= maxStrength) {

                    if (randomize == false) {

                        this.searchCache.Clear();

                    }

                    this.searchCache.Add(item);
                    max = s;

                }

            }

            // Looking for nearest duration in searchCache
            max = float.MaxValue;
            for (var i = 0; i < this.searchCache.Count; ++i) {

                var item = this.searchCache[i];
                var d = maxDuration <= 0f ? 0f : Mathf.Abs(item.duration - maxDuration);
                if (d <= max && (maxDuration <= 0f || item.duration <= maxDuration)) {

                    found = item;
                    max = d;

                    if (randomize == true) {

                        this.searchCacheRnd.Add(item);

                    } else {

                        break;

                    }

                }

            }

            if (randomize == true) {

                found = this.searchCacheRnd.Count > 0 ? this.searchCacheRnd[Random.Range(0, this.searchCacheRnd.Count)] : found;
                this.searchCacheRnd.Clear();

            }

            this.searchCache.Clear();

            return found;

        }

        private bool isPlaying;
        private AnimationCurve playingCurve;
        private float playingTime;
        private bool playingRandomize;

        private void ValidateChannels() {

            if (this.channels == null || this.channels.Length != this.maxChannels) {
                this.channels = new float[this.maxChannels];
            }

        }

        private Keyframe GetNextKey(AnimationCurve curve, float time, out int keyIdx) {

            keyIdx = -1;
            var result = new Keyframe(time, 0f);
            var min = float.MaxValue;
            for (var i = 0; i < curve.keys.Length; ++i) {

                var key = curve.keys[i];
                if (key.time <= time) {
                    continue;
                }

                var delta = curve.keys[i].time - time;
                if (delta <= min) {

                    keyIdx = i;
                    min = delta;
                    result = key;

                }

            }

            return result;

        }

        private int GetFreeChannelIndex(float time) {

            for (var i = 0; i < this.channels.Length; ++i) {

                if (time > this.channels[i]) {

                    return i;

                }

            }

            return -1;

        }

    }

}