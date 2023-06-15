using System.Collections.Generic;

namespace UnityEngine.UI.Windows.Utilities {

    public class Tweener : MonoBehaviour {

        public enum EaseFunction : int {

            Linear,
            InQuad,
            OutQuad,
            InOutQuad,
            InCubic,
            OutCubic,
            InOutCubic,
            InQuart,
            OutQuart,
            InOutQuart,
            InQuint,
            OutQuint,
            InOutQuint,
            InSine,
            OutSine, 
            InOutSine, 
            InExpo,
            OutExpo, 
            InOutExpo, 
            InCirc,
            OutCirc, 
            InOutCirc,
            InElastic,
            OutElastic,
            InOutElastic,
            InBack,
            OutBack,
            InOutBack,
            InBounce,
            OutBounce,
            InOutBounce,
            OutInQuad,
            OutInCubic,
            OutInQuart,
            OutInQuint,
            OutInSine, 
            OutInExpo, 
            OutInCirc,
            OutInElastic,
            OutInBack,
            OutInBounce,

        }

        public static class EaseFunctions {

	        public static System.Func<float, float, float, float, float> GetEase(EaseFunction func) {

		        return EaseFunctions.easings[(int)func];

	        }
	        
            private static System.Func<float, float, float, float, float>[] easings = new System.Func<float, float, float, float, float>[] {
	            Linear,
	            InQuad,
	            OutQuad,
	            InOutQuad,
	            InCubic,
	            OutCubic,
	            InOutCubic,
	            InQuart,
	            OutQuart,
	            InOutQuart,
	            InQuint,
	            OutQuint,
	            InOutQuint,
	            InSine,
	            OutSine, 
	            InOutSine,
	            InExpo,
	            OutExpo, 
	            InOutExpo,
	            InCirc,
	            OutCirc, 
	            InOutCirc,
	            InElastic,
	            OutElastic,
	            InOutElastic,
	            InBack,
	            OutBack,
	            InOutBack,
	            InBounce,
	            OutBounce,
	            InOutBounce,
	            OutInQuad,
	            OutInCubic,
	            OutInQuart,
	            OutInQuint,
	            OutInSine, 
	            OutInExpo, 
	            OutInCirc,
	            OutInElastic,
	            OutInBack,
	            OutInBounce,
            };
            
            #region Linear
            /// <summary>
            /// Easing equation function for a simple linear tweening, with no easing.
            /// </summary>
            /// <param name="t">Current time in seconds.</param>
            /// <param name="b">Starting value.</param>
            /// <param name="c">Final value.</param>
            /// <param name="d">Duration of animation.</param>
            /// <returns>The correct value.</returns>
            public static float Linear(float t, float b, float c, float d) {
	            return c * t / d + b;
            }
            #endregion

            #region Expo
            /// <summary>
            /// Easing equation function for an exponential (2^t) easing out: 
            /// decelerating from zero velocity.
            /// </summary>
            /// <param name="t">Current time in seconds.</param>
            /// <param name="b">Starting value.</param>
            /// <param name="c">Final value.</param>
            /// <param name="d">Duration of animation.</param>
            /// <returns>The correct value.</returns>
            public static float OutExpo(float t, float b, float c, float d) {
	            return (t == d) ? b + c : c * (-Mathf.Pow(2, -10 * t / d) + 1) + b;
            }

            /// <summary>
            /// Easing equation function for an exponential (2^t) easing in: 
            /// accelerating from zero velocity.
            /// </summary>
            /// <param name="t">Current time in seconds.</param>
            /// <param name="b">Starting value.</param>
            /// <param name="c">Final value.</param>
            /// <param name="d">Duration of animation.</param>
            /// <returns>The correct value.</returns>
            public static float InExpo(float t, float b, float c, float d) {
	            return (t == 0) ? b : c * Mathf.Pow(2, 10 * (t / d - 1)) + b;
            }

            /// <summary>
            /// Easing equation function for an exponential (2^t) easing in/out: 
            /// acceleration until halfway, then deceleration.
            /// </summary>
            /// <param name="t">Current time in seconds.</param>
            /// <param name="b">Starting value.</param>
            /// <param name="c">Final value.</param>
            /// <param name="d">Duration of animation.</param>
            /// <returns>The correct value.</returns>
            public static float InOutExpo(float t, float b, float c, float d) {
	            if (t == 0) return b;

	            if (t == d) return b + c;

	            if ((t /= d / 2) < 1) return c / 2 * Mathf.Pow(2, 10 * (t - 1)) + b;

	            return c / 2 * (-Mathf.Pow(2, -10 * --t) + 2) + b;
            }

            /// <summary>
            /// Easing equation function for an exponential (2^t) easing out/in: 
            /// deceleration until halfway, then acceleration.
            /// </summary>
            /// <param name="t">Current time in seconds.</param>
            /// <param name="b">Starting value.</param>
            /// <param name="c">Final value.</param>
            /// <param name="d">Duration of animation.</param>
            /// <returns>The correct value.</returns>
            public static float OutInExpo(float t, float b, float c, float d) {
	            if (t < d / 2) return OutExpo(t * 2, b, c / 2, d);

	            return InExpo((t * 2) - d, b + c / 2, c / 2, d);
            }
            #endregion

            #region Circular
            /// <summary>
            /// Easing equation function for a circular (sqrt(1-t^2)) easing out: 
            /// decelerating from zero velocity.
            /// </summary>
            /// <param name="t">Current time in seconds.</param>
            /// <param name="b">Starting value.</param>
            /// <param name="c">Final value.</param>
            /// <param name="d">Duration of animation.</param>
            /// <returns>The correct value.</returns>
            public static float OutCirc(float t, float b, float c, float d) {
	            return c * Mathf.Sqrt(1 - (t = t / d - 1) * t) + b;
            }

            /// <summary>
            /// Easing equation function for a circular (sqrt(1-t^2)) easing in: 
            /// accelerating from zero velocity.
            /// </summary>
            /// <param name="t">Current time in seconds.</param>
            /// <param name="b">Starting value.</param>
            /// <param name="c">Final value.</param>
            /// <param name="d">Duration of animation.</param>
            /// <returns>The correct value.</returns>
            public static float InCirc(float t, float b, float c, float d) {
	            return -c * (Mathf.Sqrt(1 - (t /= d) * t) - 1) + b;
            }

            /// <summary>
            /// Easing equation function for a circular (sqrt(1-t^2)) easing in/out: 
            /// acceleration until halfway, then deceleration.
            /// </summary>
            /// <param name="t">Current time in seconds.</param>
            /// <param name="b">Starting value.</param>
            /// <param name="c">Final value.</param>
            /// <param name="d">Duration of animation.</param>
            /// <returns>The correct value.</returns>
            public static float InOutCirc(float t, float b, float c, float d) {
	            if ((t /= d / 2) < 1) return -c / 2 * (Mathf.Sqrt(1 - t * t) - 1) + b;

	            return c / 2 * (Mathf.Sqrt(1 - (t -= 2) * t) + 1) + b;
            }

            /// <summary>
            /// Easing equation function for a circular (sqrt(1-t^2)) easing in/out: 
            /// acceleration until halfway, then deceleration.
            /// </summary>
            /// <param name="t">Current time in seconds.</param>
            /// <param name="b">Starting value.</param>
            /// <param name="c">Final value.</param>
            /// <param name="d">Duration of animation.</param>
            /// <returns>The correct value.</returns>
            public static float OutInCirc(float t, float b, float c, float d) {
	            if (t < d / 2) return OutCirc(t * 2, b, c / 2, d);

	            return InCirc((t * 2) - d, b + c / 2, c / 2, d);
            }
            #endregion

            #region Quad
            /// <summary>
            /// Easing equation function for a quadratic (t^2) easing out: 
            /// decelerating from zero velocity.
            /// </summary>
            /// <param name="t">Current time in seconds.</param>
            /// <param name="b">Starting value.</param>
            /// <param name="c">Final value.</param>
            /// <param name="d">Duration of animation.</param>
            /// <returns>The correct value.</returns>
            public static float OutQuad(float t, float b, float c, float d) {
	            return -c * (t /= d) * (t - 2) + b;
            }

            /// <summary>
            /// Easing equation function for a quadratic (t^2) easing in: 
            /// accelerating from zero velocity.
            /// </summary>
            /// <param name="t">Current time in seconds.</param>
            /// <param name="b">Starting value.</param>
            /// <param name="c">Final value.</param>
            /// <param name="d">Duration of animation.</param>
            /// <returns>The correct value.</returns>
            public static float InQuad(float t, float b, float c, float d) {
	            return c * (t /= d) * t + b;
            }

            /// <summary>
            /// Easing equation function for a quadratic (t^2) easing in/out: 
            /// acceleration until halfway, then deceleration.
            /// </summary>
            /// <param name="t">Current time in seconds.</param>
            /// <param name="b">Starting value.</param>
            /// <param name="c">Final value.</param>
            /// <param name="d">Duration of animation.</param>
            /// <returns>The correct value.</returns>
            public static float InOutQuad(float t, float b, float c, float d) {
	            if ((t /= d / 2) < 1) return c / 2 * t * t + b;

	            return -c / 2 * ((--t) * (t - 2) - 1) + b;
            }

            /// <summary>
            /// Easing equation function for a quadratic (t^2) easing out/in: 
            /// deceleration until halfway, then acceleration.
            /// </summary>
            /// <param name="t">Current time in seconds.</param>
            /// <param name="b">Starting value.</param>
            /// <param name="c">Final value.</param>
            /// <param name="d">Duration of animation.</param>
            /// <returns>The correct value.</returns>
            public static float OutInQuad(float t, float b, float c, float d) {
	            if (t < d / 2) return OutQuad(t * 2, b, c / 2, d);

	            return InQuad((t * 2) - d, b + c / 2, c / 2, d);
            }
            #endregion

            #region Sine
            /// <summary>
            /// Easing equation function for a sinusoidal (sin(t)) easing out: 
            /// decelerating from zero velocity.
            /// </summary>
            /// <param name="t">Current time in seconds.</param>
            /// <param name="b">Starting value.</param>
            /// <param name="c">Final value.</param>
            /// <param name="d">Duration of animation.</param>
            /// <returns>The correct value.</returns>
            public static float OutSine(float t, float b, float c, float d) {
	            return c * Mathf.Sin(t / d * (Mathf.PI / 2)) + b;
            }

            /// <summary>
            /// Easing equation function for a sinusoidal (sin(t)) easing in: 
            /// accelerating from zero velocity.
            /// </summary>
            /// <param name="t">Current time in seconds.</param>
            /// <param name="b">Starting value.</param>
            /// <param name="c">Final value.</param>
            /// <param name="d">Duration of animation.</param>
            /// <returns>The correct value.</returns>
            public static float InSine(float t, float b, float c, float d) {
	            return -c * Mathf.Cos(t / d * (Mathf.PI / 2)) + c + b;
            }

            /// <summary>
            /// Easing equation function for a sinusoidal (sin(t)) easing in/out: 
            /// acceleration until halfway, then deceleration.
            /// </summary>
            /// <param name="t">Current time in seconds.</param>
            /// <param name="b">Starting value.</param>
            /// <param name="c">Final value.</param>
            /// <param name="d">Duration of animation.</param>
            /// <returns>The correct value.</returns>
            public static float InOutSine(float t, float b, float c, float d) {
	            if ((t /= d / 2) < 1) return c / 2 * (Mathf.Sin(Mathf.PI * t / 2)) + b;

	            return -c / 2 * (Mathf.Cos(Mathf.PI * --t / 2) - 2) + b;
            }

            /// <summary>
            /// Easing equation function for a sinusoidal (sin(t)) easing in/out: 
            /// deceleration until halfway, then acceleration.
            /// </summary>
            /// <param name="t">Current time in seconds.</param>
            /// <param name="b">Starting value.</param>
            /// <param name="c">Final value.</param>
            /// <param name="d">Duration of animation.</param>
            /// <returns>The correct value.</returns>
            public static float OutInSine(float t, float b, float c, float d) {
	            if (t < d / 2) return OutSine(t * 2, b, c / 2, d);

	            return InSine((t * 2) - d, b + c / 2, c / 2, d);
            }
            #endregion

            #region Cubic
            /// <summary>
            /// Easing equation function for a cubic (t^3) easing out: 
            /// decelerating from zero velocity.
            /// </summary>
            /// <param name="t">Current time in seconds.</param>
            /// <param name="b">Starting value.</param>
            /// <param name="c">Final value.</param>
            /// <param name="d">Duration of animation.</param>
            /// <returns>The correct value.</returns>
            public static float OutCubic(float t, float b, float c, float d) {
	            return c * ((t = t / d - 1) * t * t + 1) + b;
            }

            /// <summary>
            /// Easing equation function for a cubic (t^3) easing in: 
            /// accelerating from zero velocity.
            /// </summary>
            /// <param name="t">Current time in seconds.</param>
            /// <param name="b">Starting value.</param>
            /// <param name="c">Final value.</param>
            /// <param name="d">Duration of animation.</param>
            /// <returns>The correct value.</returns>
            public static float InCubic(float t, float b, float c, float d) {
	            return c * (t /= d) * t * t + b;
            }

            /// <summary>
            /// Easing equation function for a cubic (t^3) easing in/out: 
            /// acceleration until halfway, then deceleration.
            /// </summary>
            /// <param name="t">Current time in seconds.</param>
            /// <param name="b">Starting value.</param>
            /// <param name="c">Final value.</param>
            /// <param name="d">Duration of animation.</param>
            /// <returns>The correct value.</returns>
            public static float InOutCubic(float t, float b, float c, float d) {
	            if ((t /= d / 2) < 1) return c / 2 * t * t * t + b;

	            return c / 2 * ((t -= 2) * t * t + 2) + b;
            }

            /// <summary>
            /// Easing equation function for a cubic (t^3) easing out/in: 
            /// deceleration until halfway, then acceleration.
            /// </summary>
            /// <param name="t">Current time in seconds.</param>
            /// <param name="b">Starting value.</param>
            /// <param name="c">Final value.</param>
            /// <param name="d">Duration of animation.</param>
            /// <returns>The correct value.</returns>
            public static float OutInCubic(float t, float b, float c, float d) {
	            if (t < d / 2) return OutCubic(t * 2, b, c / 2, d);

	            return InCubic((t * 2) - d, b + c / 2, c / 2, d);
            }
            #endregion

            #region Quartic
            /// <summary>
            /// Easing equation function for a quartic (t^4) easing out: 
            /// decelerating from zero velocity.
            /// </summary>
            /// <param name="t">Current time in seconds.</param>
            /// <param name="b">Starting value.</param>
            /// <param name="c">Final value.</param>
            /// <param name="d">Duration of animation.</param>
            /// <returns>The correct value.</returns>
            public static float OutQuart(float t, float b, float c, float d) {
	            return -c * ((t = t / d - 1) * t * t * t - 1) + b;
            }

            /// <summary>
            /// Easing equation function for a quartic (t^4) easing in: 
            /// accelerating from zero velocity.
            /// </summary>
            /// <param name="t">Current time in seconds.</param>
            /// <param name="b">Starting value.</param>
            /// <param name="c">Final value.</param>
            /// <param name="d">Duration of animation.</param>
            /// <returns>The correct value.</returns>
            public static float InQuart(float t, float b, float c, float d) {
	            return c * (t /= d) * t * t * t + b;
            }

            /// <summary>
            /// Easing equation function for a quartic (t^4) easing in/out: 
            /// acceleration until halfway, then deceleration.
            /// </summary>
            /// <param name="t">Current time in seconds.</param>
            /// <param name="b">Starting value.</param>
            /// <param name="c">Final value.</param>
            /// <param name="d">Duration of animation.</param>
            /// <returns>The correct value.</returns>
            public static float InOutQuart(float t, float b, float c, float d) {
	            if ((t /= d / 2) < 1) return c / 2 * t * t * t * t + b;

	            return -c / 2 * ((t -= 2) * t * t * t - 2) + b;
            }

            /// <summary>
            /// Easing equation function for a quartic (t^4) easing out/in: 
            /// deceleration until halfway, then acceleration.
            /// </summary>
            /// <param name="t">Current time in seconds.</param>
            /// <param name="b">Starting value.</param>
            /// <param name="c">Final value.</param>
            /// <param name="d">Duration of animation.</param>
            /// <returns>The correct value.</returns>
            public static float OutInQuart(float t, float b, float c, float d) {
	            if (t < d / 2) return OutQuart(t * 2, b, c / 2, d);

	            return InQuart((t * 2) - d, b + c / 2, c / 2, d);
            }
            #endregion

            #region Quintic
            /// <summary>
            /// Easing equation function for a quintic (t^5) easing out: 
            /// decelerating from zero velocity.
            /// </summary>
            /// <param name="t">Current time in seconds.</param>
            /// <param name="b">Starting value.</param>
            /// <param name="c">Final value.</param>
            /// <param name="d">Duration of animation.</param>
            /// <returns>The correct value.</returns>
            public static float OutQuint(float t, float b, float c, float d) {
	            return c * ((t = t / d - 1) * t * t * t * t + 1) + b;
            }

            /// <summary>
            /// Easing equation function for a quintic (t^5) easing in: 
            /// accelerating from zero velocity.
            /// </summary>
            /// <param name="t">Current time in seconds.</param>
            /// <param name="b">Starting value.</param>
            /// <param name="c">Final value.</param>
            /// <param name="d">Duration of animation.</param>
            /// <returns>The correct value.</returns>
            public static float InQuint(float t, float b, float c, float d) {
	            return c * (t /= d) * t * t * t * t + b;
            }

            /// <summary>
            /// Easing equation function for a quintic (t^5) easing in/out: 
            /// acceleration until halfway, then deceleration.
            /// </summary>
            /// <param name="t">Current time in seconds.</param>
            /// <param name="b">Starting value.</param>
            /// <param name="c">Final value.</param>
            /// <param name="d">Duration of animation.</param>
            /// <returns>The correct value.</returns>
            public static float InOutQuint(float t, float b, float c, float d) {
	            if ((t /= d / 2) < 1) return c / 2 * t * t * t * t * t + b;
	            return c / 2 * ((t -= 2) * t * t * t * t + 2) + b;
            }

            /// <summary>
            /// Easing equation function for a quintic (t^5) easing in/out: 
            /// acceleration until halfway, then deceleration.
            /// </summary>
            /// <param name="t">Current time in seconds.</param>
            /// <param name="b">Starting value.</param>
            /// <param name="c">Final value.</param>
            /// <param name="d">Duration of animation.</param>
            /// <returns>The correct value.</returns>
            public static float OutInQuint(float t, float b, float c, float d) {
	            if (t < d / 2) return OutQuint(t * 2, b, c / 2, d);
	            return InQuint((t * 2) - d, b + c / 2, c / 2, d);
            }
            #endregion

            #region Elastic
            /// <summary>
            /// Easing equation function for an elastic (exponentially decaying sine wave) easing out: 
            /// decelerating from zero velocity.
            /// </summary>
            /// <param name="t">Current time in seconds.</param>
            /// <param name="b">Starting value.</param>
            /// <param name="c">Final value.</param>
            /// <param name="d">Duration of animation.</param>
            /// <returns>The correct value.</returns>
            public static float OutElastic(float t, float b, float c, float d) {
	            if ((t /= d) == 1) return b + c;

	            float p = d * .3f;
	            float s = p / 4.0f;

	            return (c * Mathf.Pow(2, -10 * t) * Mathf.Sin((t * d - s) * (2 * Mathf.PI) / p) + c + b);
            }

            /// <summary>
            /// Easing equation function for an elastic (exponentially decaying sine wave) easing in: 
            /// accelerating from zero velocity.
            /// </summary>
            /// <param name="t">Current time in seconds.</param>
            /// <param name="b">Starting value.</param>
            /// <param name="c">Final value.</param>
            /// <param name="d">Duration of animation.</param>
            /// <returns>The correct value.</returns>
            public static float InElastic(float t, float b, float c, float d) {
	            if ((t /= d) == 1) return b + c;

	            float p = d * .3f;
	            float s = p / 4;

	            return -(c * Mathf.Pow(2, 10 * (t -= 1)) * Mathf.Sin((t * d - s) * (2 * Mathf.PI) / p)) + b;
            }

            /// <summary>
            /// Easing equation function for an elastic (exponentially decaying sine wave) easing in/out: 
            /// acceleration until halfway, then deceleration.
            /// </summary>
            /// <param name="t">Current time in seconds.</param>
            /// <param name="b">Starting value.</param>
            /// <param name="c">Final value.</param>
            /// <param name="d">Duration of animation.</param>
            /// <returns>The correct value.</returns>
            public static float InOutElastic(float t, float b, float c, float d) {
	            if ((t /= d / 2) == 2) return b + c;

	            float p = d * (.3f * 1.5f);
	            float s = p / 4;

	            if (t < 1) return -.5f * (c * Mathf.Pow(2, 10 * (t -= 1)) * Mathf.Sin((t * d - s) * (2 * Mathf.PI) / p)) + b;
	            return c * Mathf.Pow(2, -10 * (t -= 1)) * Mathf.Sin((t * d - s) * (2 * Mathf.PI) / p) * .5f + c + b;
            }

            /// <summary>
            /// Easing equation function for an elastic (exponentially decaying sine wave) easing out/in: 
            /// deceleration until halfway, then acceleration.
            /// </summary>
            /// <param name="t">Current time in seconds.</param>
            /// <param name="b">Starting value.</param>
            /// <param name="c">Final value.</param>
            /// <param name="d">Duration of animation.</param>
            /// <returns>The correct value.</returns>
            public static float OutInElastic(float t, float b, float c, float d) {
	            if (t < d / 2) return OutElastic(t * 2, b, c / 2, d);
	            return InElastic((t * 2) - d, b + c / 2, c / 2, d);
            }
            #endregion

            #region Bounce
            /// <summary>
            /// Easing equation function for a bounce (exponentially decaying parabolic bounce) easing out: 
            /// decelerating from zero velocity.
            /// </summary>
            /// <param name="t">Current time in seconds.</param>
            /// <param name="b">Starting value.</param>
            /// <param name="c">Final value.</param>
            /// <param name="d">Duration of animation.</param>
            /// <returns>The correct value.</returns>
            public static float OutBounce(float t, float b, float c, float d) {
	            if ((t /= d) < (1 / 2.75))
		            return c * (7.5625f * t * t) + b;
	            else if (t < (2 / 2.75f))
		            return c * (7.5625f * (t -= (1.5f / 2.75f)) * t + .75f) + b;
	            else if (t < (2.5f / 2.75f))
		            return c * (7.5625f * (t -= (2.25f / 2.75f)) * t + .9375f) + b;
	            else
		            return c * (7.5625f * (t -= (2.625f / 2.75f)) * t + .984375f) + b;
            }

            /// <summary>
            /// Easing equation function for a bounce (exponentially decaying parabolic bounce) easing in: 
            /// accelerating from zero velocity.
            /// </summary>
            /// <param name="t">Current time in seconds.</param>
            /// <param name="b">Starting value.</param>
            /// <param name="c">Final value.</param>
            /// <param name="d">Duration of animation.</param>
            /// <returns>The correct value.</returns>
            public static float InBounce(float t, float b, float c, float d) {
	            return c - OutBounce(d - t, 0, c, d) + b;
            }

            /// <summary>
            /// Easing equation function for a bounce (exponentially decaying parabolic bounce) easing in/out: 
            /// acceleration until halfway, then deceleration.
            /// </summary>
            /// <param name="t">Current time in seconds.</param>
            /// <param name="b">Starting value.</param>
            /// <param name="c">Final value.</param>
            /// <param name="d">Duration of animation.</param>
            /// <returns>The correct value.</returns>
            public static float InOutBounce(float t, float b, float c, float d) {
	            if (t < d / 2)
		            return InBounce(t * 2, 0, c, d) * .5f + b;
	            else
		            return OutBounce(t * 2 - d, 0, c, d) * .5f + c * .5f + b;
            }

            /// <summary>
            /// Easing equation function for a bounce (exponentially decaying parabolic bounce) easing out/in: 
            /// deceleration until halfway, then acceleration.
            /// </summary>
            /// <param name="t">Current time in seconds.</param>
            /// <param name="b">Starting value.</param>
            /// <param name="c">Final value.</param>
            /// <param name="d">Duration of animation.</param>
            /// <returns>The correct value.</returns>
            public static float OutInBounce(float t, float b, float c, float d) {
	            if (t < d / 2) return OutBounce(t * 2, b, c / 2, d);
	            return InBounce((t * 2) - d, b + c / 2, c / 2, d);
            }
            #endregion

            #region Back
            /// <summary>
            /// Easing equation function for a back (overshooting cubic easing: (s+1)*t^3 - s*t^2) easing out: 
            /// decelerating from zero velocity.
            /// </summary>
            /// <param name="t">Current time in seconds.</param>
            /// <param name="b">Starting value.</param>
            /// <param name="c">Final value.</param>
            /// <param name="d">Duration of animation.</param>
            /// <returns>The correct value.</returns>
            public static float OutBack(float t, float b, float c, float d) {
	            return c * ((t = t / d - 1) * t * ((1.70158f + 1) * t + 1.70158f) + 1) + b;
            }

            /// <summary>
            /// Easing equation function for a back (overshooting cubic easing: (s+1)*t^3 - s*t^2) easing in: 
            /// accelerating from zero velocity.
            /// </summary>
            /// <param name="t">Current time in seconds.</param>
            /// <param name="b">Starting value.</param>
            /// <param name="c">Final value.</param>
            /// <param name="d">Duration of animation.</param>
            /// <returns>The correct value.</returns>
            public static float InBack(float t, float b, float c, float d) {
	            return c * (t /= d) * t * ((1.70158f + 1) * t - 1.70158f) + b;
            }

            /// <summary>
            /// Easing equation function for a back (overshooting cubic easing: (s+1)*t^3 - s*t^2) easing in/out: 
            /// acceleration until halfway, then deceleration.
            /// </summary>
            /// <param name="t">Current time in seconds.</param>
            /// <param name="b">Starting value.</param>
            /// <param name="c">Final value.</param>
            /// <param name="d">Duration of animation.</param>
            /// <returns>The correct value.</returns>
            public static float InOutBack(float t, float b, float c, float d) {
	            float s = 1.70158f;
	            if ((t /= d / 2) < 1) return c / 2 * (t * t * (((s *= (1.525f)) + 1) * t - s)) + b;
	            return c / 2 * ((t -= 2) * t * (((s *= (1.525f)) + 1) * t + s) + 2) + b;
            }

            /// <summary>
            /// Easing equation function for a back (overshooting cubic easing: (s+1)*t^3 - s*t^2) easing out/in: 
            /// deceleration until halfway, then acceleration.
            /// </summary>
            /// <param name="t">Current time in seconds.</param>
            /// <param name="b">Starting value.</param>
            /// <param name="c">Final value.</param>
            /// <param name="d">Duration of animation.</param>
            /// <returns>The correct value.</returns>
            public static float OutInBack(float t, float b, float c, float d) {
	            if (t < d / 2) return OutBack(t * 2, b, c / 2, d);
	            return InBack((t * 2) - d, b + c / 2, c / 2, d);
            }
            #endregion
		
        }

        public interface ITween {

            bool Update(float dt);
            bool HasTag(object tag);
            void Stop(bool ignoreEvents = false);
            void Complete();

        }

        internal interface ITweenInternal {

            object GetTag();
            float GetTimer();
            float GetDelay();
            float GetDuration();
            float GetFrom();
            float GetTo();

        }

        public class Tween<T> : ITween, ITweenInternal {

	        internal Tweener tweener;
            internal T obj;
            internal float duration;
            internal float from;
            internal float to;
            internal object tag;
            internal float delay;

            internal int loops = 1;
            internal bool reflect;

            internal System.Action<T> onComplete;
            internal System.Action onCompleteParameterless;
            internal System.Action<T, float> onUpdate;
            internal System.Action<T> onCancel;
            internal System.Action onCancelParameterless;

            internal float timer;
            internal float direction;

            private EaseFunction easeFunction;

            float ITweenInternal.GetTimer() { return this.timer; }
            float ITweenInternal.GetDelay() { return this.delay; }
            float ITweenInternal.GetDuration() { return this.duration; }
            float ITweenInternal.GetFrom() { return this.from; }
            float ITweenInternal.GetTo() { return this.to; }
            object ITweenInternal.GetTag() { return this.tag; }
            
            bool ITween.Update(float dt) {

                this.delay -= dt;
                if (this.delay <= 0f) {

                    this.timer += (dt / this.duration) * this.direction;

                    try {

	                    if (this.onUpdate != null) {
		                    
		                    var val = EaseFunctions.GetEase(this.easeFunction).Invoke(this.timer, this.@from, this.to - this.from, 1f);
		                    this.onUpdate.Invoke(this.obj, Mathf.Clamp(val, Mathf.Min(this.from, this.to), Mathf.Max(this.from, this.to)));
		                    
	                    }

                        if (this.timer >= 1f) {

                            if (this.reflect == true) {

                                this.direction = -1f;
                                return false;

                            } else {

                                this.timer = 0f;

                            }
	                        
	                        --this.loops;
	                        if (this.loops == 0) {

		                        return true;

	                        }
	                        
	                        return false;

                        } else if (this.timer <= 0f) {

	                        --this.loops;
	                        if (this.loops == 0) {
		                        
		                        return true;

	                        }

	                        if (this.reflect == true) {

		                        this.direction = 1f;
		                        return false;
	                        
	                        }

	                        return false;

                        }

                    } catch (System.Exception ex) {

                        Debug.LogException(ex);

                    }
                    
                    if (this.timer >= 1f) {

                        return true;

                    }

                } else {

                    try {
                        
                        if (this.onUpdate != null) {

                            this.onUpdate.Invoke(this.obj, this.from);

                        }
                        
                    } catch (System.Exception ex) {

                        Debug.LogException(ex);

                    }

                }

                return false;

            }

            void ITween.Complete() {

	            if (this.onComplete != null) this.onComplete.Invoke(this.obj);
	            if (this.onCompleteParameterless != null) this.onCompleteParameterless.Invoke();

	            this.delay = 0f;
	            this.timer = this.direction;
	            this.reflect = false;
	            this.loops = 1;

            }

            void ITween.Stop(bool ignoreEvents) {

                if (ignoreEvents == false && this.timer < 1f) {

	                if (this.onCancel != null) this.onCancel.Invoke(this.obj);
	                if (this.onCancelParameterless != null) this.onCancelParameterless.Invoke();
	                
                }

                this.delay = 0f;
                this.timer = this.direction;
                this.reflect = false;
                this.loops = 1;

            }

            bool ITween.HasTag(object tag) {

                return this.tag == tag;

            }

            public float GetDirection() => this.direction;

            public Tween<T> Loop(int loops = 1) {
				
	            this.loops = loops;
	            return this;

            }

            public Tween<T> SetValue(float timer, float direction) {
				
	            this.timer = timer;
	            this.direction = direction;
	            return this;

            }

            public Tween<T> Reflect() {
				
	            this.reflect = true;
	            return this;

            }

            public Tween<T> Tag(object tag) {

                this.tag = tag;
                return this;

            }

            public Tween<T> Delay(float delay) {

                this.delay = delay;
                return this;

            }

            public Tween<T> Ease(EaseFunction easeFunction) {

                this.easeFunction = easeFunction;
                return this;

            }

            public Tween<T> OnComplete(System.Action<T> onResult) {

                this.onComplete = onResult;
                return this;

            }

            public Tween<T> OnComplete(System.Action onResult) {

                this.onCompleteParameterless = onResult;
                return this;

            }

            public Tween<T> OnCancel(System.Action<T> onResult) {

	            this.onCancel = onResult;
	            return this;

            }

            public Tween<T> OnCancel(System.Action onResult) {

                this.onCancelParameterless = onResult;
                return this;

            }

            public Tween<T> OnUpdate(System.Action<T, float> onResult) {

                this.onUpdate = onResult;
                //this.tweener.Step(this, Time.deltaTime);
                return this;

            }

        }

        public List<ITween> tweens = new List<ITween>();
        public List<ITween> frameTweens = new List<ITween>();

        public Tween<T> Add<T>(T obj, float duration, float from, float to) {

            var tween = new Tweener.Tween<T>();
            tween.tweener = this;
            tween.obj = obj;
            tween.duration = duration;
            tween.from = from;
            tween.to = to;
            tween.direction = 1f;

            this.tweens.Add(tween);

            return tween;

        }

        public void Stop(object tag, bool ignoreEvents = false) {

	        var list = PoolList<ITween>.Spawn();
            for (int i = this.tweens.Count - 1; i >= 0; --i) {

	            var tw = this.tweens[i];
                if (tw.HasTag(tag) == true) {

	                this.tweens.RemoveAt(i);
	                list.Add(tw);
                    
                }

            }

            for (int i = 0; i < list.Count; ++i) {
	            
	            list[i].Stop(ignoreEvents);

            }
            PoolList<ITween>.Recycle(ref list);

        }

        private bool Step(ITween tween, float dt) {
	        
	        if (tween.Update(dt) == true) {

		        this.completeList.Add(tween);
		        this.tweens.Remove(tween);
		        return true;
                    
	        }

	        return false;

        }

        private List<ITween> completeList = new List<ITween>();
        public void Update() {
            
            var dt = Time.deltaTime;
            this.completeList.Clear();
            this.frameTweens.Clear();
            this.frameTweens.AddRange(this.tweens);
            for (int cnt = this.frameTweens.Count, i = cnt - 1; i >= 0; --i) {

                if (this.frameTweens[i].Update(dt) == true) {

	                var tween = this.frameTweens[i];
	                this.completeList.Add(tween);
                    
                }

            }
            
            for (int i = 0, cnt = this.completeList.Count; i < cnt; ++i) {

                this.tweens.Remove(this.completeList[i]);
	            this.completeList[i].Complete();
	            
            }
            this.completeList.Clear();

        }

    }

}