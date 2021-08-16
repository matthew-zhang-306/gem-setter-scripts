using UnityEngine;


namespace DG.Tweening {

    [System.Serializable]
    public struct TweenTiming {
        public float easeTime;
        public Ease easingType;
        public float delay;
    }


    public static class TweenTimingExt {
        public static Tween ApplyTiming(this Tween tween, TweenTiming timing, float delayOffset = 0) {
            return tween.SetEase(timing.easingType).SetDelay(timing.delay + delayOffset);
        }
    }

}