using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ViveSR.anipal.Lip;

namespace Retinize
{
    public class IT_ViveLipSyncHandler : IIT_FacialAnimationHandler
    {
        public event Action<byte[]> OnParametersSetByLivePerformanceOfOwnPlayer;
        private IT_FacialAnimationController _facialAnimationController;
        private Dictionary<LipShape_v2, float> _lipWeightings = new Dictionary<LipShape_v2, float>();
        
        private readonly List<KeyValuePair<IT_FacialAnimationShapes, LipShape_v2>> _lipShapesAndFacialAnimationBlendShapeKeyValuePairByLipShapeIndex = 
            new List<KeyValuePair<IT_FacialAnimationShapes, LipShape_v2>>()
        {
            new KeyValuePair<IT_FacialAnimationShapes, LipShape_v2>(IT_FacialAnimationShapes.jawRight, LipShape_v2.Jaw_Right),
            new KeyValuePair<IT_FacialAnimationShapes, LipShape_v2>(IT_FacialAnimationShapes.jawLeft, LipShape_v2.Jaw_Left),
            new KeyValuePair<IT_FacialAnimationShapes, LipShape_v2>(IT_FacialAnimationShapes.jawForward, LipShape_v2.Jaw_Forward),
            new KeyValuePair<IT_FacialAnimationShapes, LipShape_v2>(IT_FacialAnimationShapes.jawOpen, LipShape_v2.Jaw_Open),
            new KeyValuePair<IT_FacialAnimationShapes, LipShape_v2>(IT_FacialAnimationShapes.mouthApeShape, LipShape_v2.Mouth_Ape_Shape),
            new KeyValuePair<IT_FacialAnimationShapes, LipShape_v2>(IT_FacialAnimationShapes.mouthUpper_R, LipShape_v2.Mouth_Upper_Right),
            new KeyValuePair<IT_FacialAnimationShapes, LipShape_v2>(IT_FacialAnimationShapes.mouthUpper_L, LipShape_v2.Mouth_Upper_Left), 
            new KeyValuePair<IT_FacialAnimationShapes, LipShape_v2>(IT_FacialAnimationShapes.mouthLower_R, LipShape_v2.Mouth_Lower_Right),
            new KeyValuePair<IT_FacialAnimationShapes, LipShape_v2>(IT_FacialAnimationShapes.mouthLower_L, LipShape_v2.Mouth_Lower_Left),
            new KeyValuePair<IT_FacialAnimationShapes, LipShape_v2>(IT_FacialAnimationShapes.mouthUpperOverturn, LipShape_v2.Mouth_Upper_Overturn),
            new KeyValuePair<IT_FacialAnimationShapes, LipShape_v2>(IT_FacialAnimationShapes.mouthLowerOverturn, LipShape_v2.Mouth_Lower_Overturn),
            new KeyValuePair<IT_FacialAnimationShapes, LipShape_v2>(IT_FacialAnimationShapes.mouthPucker, LipShape_v2.Mouth_Pout),
            new KeyValuePair<IT_FacialAnimationShapes, LipShape_v2>(IT_FacialAnimationShapes.mouthSmile_R, LipShape_v2.Mouth_Smile_Right),
            new KeyValuePair<IT_FacialAnimationShapes, LipShape_v2>(IT_FacialAnimationShapes.mouthSmile_L, LipShape_v2.Mouth_Smile_Left),
            new KeyValuePair<IT_FacialAnimationShapes, LipShape_v2>(IT_FacialAnimationShapes.mouthFrown_R, LipShape_v2.Mouth_Sad_Right),
            new KeyValuePair<IT_FacialAnimationShapes, LipShape_v2>(IT_FacialAnimationShapes.mouthFrown_L, LipShape_v2.Mouth_Sad_Left),
            new KeyValuePair<IT_FacialAnimationShapes, LipShape_v2>(IT_FacialAnimationShapes.cheekPuff_R, LipShape_v2.Cheek_Puff_Right),
            new KeyValuePair<IT_FacialAnimationShapes, LipShape_v2>(IT_FacialAnimationShapes.cheekPuff_L, LipShape_v2.Cheek_Puff_Left),
            new KeyValuePair<IT_FacialAnimationShapes, LipShape_v2>(IT_FacialAnimationShapes.cheekSuck, LipShape_v2.Cheek_Suck),
            new KeyValuePair<IT_FacialAnimationShapes, LipShape_v2>(IT_FacialAnimationShapes.mouthUpperUp_R, LipShape_v2.Mouth_Upper_UpRight),
            new KeyValuePair<IT_FacialAnimationShapes, LipShape_v2>(IT_FacialAnimationShapes.mouthUpperUp_L, LipShape_v2.Mouth_Upper_UpLeft),
            new KeyValuePair<IT_FacialAnimationShapes, LipShape_v2>(IT_FacialAnimationShapes.mouthLowerDown_R, LipShape_v2.Mouth_Lower_DownRight),
            new KeyValuePair<IT_FacialAnimationShapes, LipShape_v2>(IT_FacialAnimationShapes.mouthLowerDown_L, LipShape_v2.Mouth_Lower_DownLeft),
            new KeyValuePair<IT_FacialAnimationShapes, LipShape_v2>(IT_FacialAnimationShapes.mouthRollUpper, LipShape_v2.Mouth_Upper_Inside),
            new KeyValuePair<IT_FacialAnimationShapes, LipShape_v2>(IT_FacialAnimationShapes.mouthRollLower, LipShape_v2.Mouth_Lower_Inside),
            new KeyValuePair<IT_FacialAnimationShapes, LipShape_v2>(IT_FacialAnimationShapes.mouthLowerOverlay, LipShape_v2.Mouth_Lower_Overlay),
            new KeyValuePair<IT_FacialAnimationShapes, LipShape_v2>(IT_FacialAnimationShapes.tongue_longStep1, LipShape_v2.Tongue_LongStep1),
            new KeyValuePair<IT_FacialAnimationShapes, LipShape_v2>(IT_FacialAnimationShapes.tongue_longStep2, LipShape_v2.Tongue_LongStep2),
            new KeyValuePair<IT_FacialAnimationShapes, LipShape_v2>(IT_FacialAnimationShapes.tongueOut, LipShape_v2.Tongue_Down),
            new KeyValuePair<IT_FacialAnimationShapes, LipShape_v2>(IT_FacialAnimationShapes.tongueUp, LipShape_v2.Tongue_Up),
            new KeyValuePair<IT_FacialAnimationShapes, LipShape_v2>(IT_FacialAnimationShapes.tongue_R, LipShape_v2.Tongue_Right),
            new KeyValuePair<IT_FacialAnimationShapes, LipShape_v2>(IT_FacialAnimationShapes.tongue_L, LipShape_v2.Tongue_Left),
            new KeyValuePair<IT_FacialAnimationShapes, LipShape_v2>(IT_FacialAnimationShapes.tongueRoll, LipShape_v2.Tongue_Roll),
            new KeyValuePair<IT_FacialAnimationShapes, LipShape_v2>(IT_FacialAnimationShapes.tongueUp_L_Morph, LipShape_v2.Tongue_UpLeft_Morph),
            new KeyValuePair<IT_FacialAnimationShapes, LipShape_v2>(IT_FacialAnimationShapes.tongueUp_R_Morph, LipShape_v2.Tongue_UpRight_Morph),
            new KeyValuePair<IT_FacialAnimationShapes, LipShape_v2>(IT_FacialAnimationShapes.tongueDown_L_Morph, LipShape_v2.Tongue_DownLeft_Morph),
            new KeyValuePair<IT_FacialAnimationShapes, LipShape_v2>(IT_FacialAnimationShapes.tongueDown_R_Morph, LipShape_v2.Tongue_DownRight_Morph),
        };

        public void Initialize(IT_FacialAnimationController facialAnimationController)
        {
            _facialAnimationController = facialAnimationController;
        }
        private void MoveBlendShapesAccordingToViveLipSyncTick()
        {
            if (SRanipal_Lip_Framework.Status != SRanipal_Lip_Framework.FrameworkStatus.WORKING || !SRanipal_Lip_v2.GetLipWeightings(out _lipWeightings)) return; //return if values aren't news
            
            var parametersData = new byte[numberOfShapes];
            for (var lipShapeIndex = 0; lipShapeIndex < _lipShapesAndFacialAnimationBlendShapeKeyValuePairByLipShapeIndex.Count; lipShapeIndex++)
            {
                var lipShapeByFacialAnimationBlendShape = _lipShapesAndFacialAnimationBlendShapeKeyValuePairByLipShapeIndex[lipShapeIndex];
                var facialAnimationBlendShape = lipShapeByFacialAnimationBlendShape.Key;
                var blendShapeIndexesBySkinnedMeshRendererIndex = _facialAnimationController.blendShapeIndexBySkinnedMeshRendererIndexByFacialAnimationBlendShape[facialAnimationBlendShape];

                parametersData[lipShapeIndex] = (byte) Mathf.RoundToInt(_lipWeightings[lipShapeByFacialAnimationBlendShape.Value] * 100);
                foreach (var blendShapeIndexBySkinnedMeshRendererIndex in blendShapeIndexesBySkinnedMeshRendererIndex)
                {
                    _facialAnimationController.characterOriginalSkinnedMeshRenderers[blendShapeIndexBySkinnedMeshRendererIndex.Key].SetBlendShapeWeight(blendShapeIndexBySkinnedMeshRendererIndex.Value, 
                        _facialAnimationController.facialAnimationShapesCurvesModifier.animationCurvesByFacialAnimationShapeIndex[(int) facialAnimationBlendShape].animationCurve.Evaluate(parametersData[lipShapeIndex]));
                }
            }
            
            OnParametersSetByLivePerformanceOfOwnPlayer?.Invoke(parametersData);
        }   
        public int numberOfShapes => _lipShapesAndFacialAnimationBlendShapeKeyValuePairByLipShapeIndex.Count;
        
        public void SetShapeValuesToDefault()
        {
            for (int i = 0; i < numberOfShapes; i++)
            {
                SetShapeValue(i, 0);
            }
        }
        public float GetShapeValue(int indexEnum)
        {
            var blendShapeIndexesBySkinnedMeshRendererIndex = _facialAnimationController.blendShapeIndexBySkinnedMeshRendererIndexByFacialAnimationBlendShape[_lipShapesAndFacialAnimationBlendShapeKeyValuePairByLipShapeIndex[indexEnum].Key];
            var blendShapeIndexBySkinnedMeshRendererIndex = blendShapeIndexesBySkinnedMeshRendererIndex.FirstOrDefault();
            return _facialAnimationController.characterOriginalSkinnedMeshRenderers[blendShapeIndexBySkinnedMeshRendererIndex.Key].GetBlendShapeWeight(blendShapeIndexBySkinnedMeshRendererIndex.Value);
        }

        public void SetShapeValue(int indexEnum, float value)
        {
            var blendShapeIndexesBySkinnedMeshRendererIndex = _facialAnimationController.blendShapeIndexBySkinnedMeshRendererIndexByFacialAnimationBlendShape[_lipShapesAndFacialAnimationBlendShapeKeyValuePairByLipShapeIndex[indexEnum].Key];
            foreach (var blendShapeIndexBySkinnedMeshRendererIndex in blendShapeIndexesBySkinnedMeshRendererIndex)
            {
                _facialAnimationController.characterOriginalSkinnedMeshRenderers[blendShapeIndexBySkinnedMeshRendererIndex.Key].SetBlendShapeWeight(blendShapeIndexBySkinnedMeshRendererIndex.Value, 
                    _facialAnimationController.facialAnimationShapesCurvesModifier.animationCurvesByFacialAnimationShapeIndex[(int) _lipShapesAndFacialAnimationBlendShapeKeyValuePairByLipShapeIndex[indexEnum].Key].animationCurve.Evaluate(value));
            }
        }

        public void Enable()
        {
            if (_facialAnimationController.characterToEmbody.playerIndexControllingEntity == IT_PlayerIdentification.OwnPlayerId)
            {
                _facialAnimationController.OnFacialAnimationUpdateTick += MoveBlendShapesAccordingToViveLipSyncTick;
            }
        }

        public void Disable()
        {
            _facialAnimationController.OnFacialAnimationUpdateTick -= MoveBlendShapesAccordingToViveLipSyncTick;
        }

    }
}
