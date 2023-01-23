﻿using UnityEngine;
using System.Collections;

namespace M8 {
    [AddComponentMenu("M8/Screen Transition/Type - Scale Fade")]
    public class STScaleFade : ScreenTrans {
        public SourceType source = SourceType.CameraSnapShot;
        public Texture sourceTexture; //if source = SourceType.Texture

        public Texture alphaMask;

        public AnimationCurve scaleCurveX;
        public AnimationCurve scaleCurveY;
        public bool scaleCurveNormalized;

        public Anchor anchor = Anchor.Center;

        private Vector4 mParam;

        protected override void OnPrepare() {
            SetSourceTexture(source, sourceTexture);

            material.SetTexture("_AlphaMaskTex", alphaMask);

            Vector2 anchorPt = GetAnchorPoint(anchor);
            mParam.x = anchorPt.x;
            mParam.y = anchorPt.y;
        }

        protected override void OnUpdate() {
            material.SetFloat("_t", curCurveValue);

            float t = scaleCurveNormalized ? curTimeNormalized : curTime;
            mParam.z = scaleCurveX.Evaluate(t);
            mParam.w = scaleCurveY.Evaluate(t);

            material.SetVector("_Params", mParam);
        }
    }
}