using System;
using System.Collections.Generic;
using System.Linq;
using Dissonance;
using Dissonance.Audio.Capture;
using Dissonance.Audio.Playback;
using NAudio.Wave;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Retinize
{
    public class IT_PhonemeOculusLipSyncHandler : IIT_FacialAnimationHandler, IMicrophoneSubscriber
    {
        public event Action<byte[]> OnParametersSetByLivePerformanceOfOwnPlayer; //Won't be called, since we don't need to synchronize this in the networking, because the audio data is already being sent
        private IT_FacialAnimationController _facialAnimationController;
        private VoicePlayerState _voicePlayerState;
        private IT_PlaybackAudioSubscription _playBackAudioSubscription;
        private OVRLipSyncContextDissonance _lipSyncContext;
        private OVRLipSyncContextMorphTarget _ovrLipSyncContextMorphTarget;
        private Queue<float> _amplitudeCache = new Queue<float>();
        private int _playerIndexEmbodying;
        private const int _AmplitudeFrameDelta = 20;
        private const float _LipSyncAmplitudeThreshold = 0.005f;

        private IT_FacialAnimationShapes[] _facialAnimationBlendShapeByPhonemeIndex =
        {
            IT_FacialAnimationShapes.neutral,
            IT_FacialAnimationShapes.pp,
            IT_FacialAnimationShapes.ff,
            IT_FacialAnimationShapes.th,
            IT_FacialAnimationShapes.dd,
            IT_FacialAnimationShapes.kk,
            IT_FacialAnimationShapes.ch,
            IT_FacialAnimationShapes.ss,
            IT_FacialAnimationShapes.nn,
            IT_FacialAnimationShapes.rr,
            IT_FacialAnimationShapes.aa,
            IT_FacialAnimationShapes.E, //For now, until we change it in the blendShapes as "e" again
            IT_FacialAnimationShapes.ih,
            IT_FacialAnimationShapes.oh,
            IT_FacialAnimationShapes.uh
        };
        
        public int numberOfShapes => _facialAnimationBlendShapeByPhonemeIndex.Length;
        public float GetShapeValue(int indexEnum)
        {
            var facialAnimationBlendShape = _facialAnimationBlendShapeByPhonemeIndex[indexEnum];
            var skinnedMeshRendererIndexWithThisBlendShape = (new List<int>(_facialAnimationController.blendShapeIndexBySkinnedMeshRendererIndexByFacialAnimationBlendShape[facialAnimationBlendShape].Keys))[0];
            return _facialAnimationController.characterOriginalSkinnedMeshRenderers[skinnedMeshRendererIndexWithThisBlendShape].GetBlendShapeWeight((int) facialAnimationBlendShape);
        }

        public void SetShapeValue(int indexEnum, float value)
        {
            var facialAnimationBlendShape = _facialAnimationBlendShapeByPhonemeIndex[indexEnum];
            foreach (var blendShapeIndexBySkinnedMeshRendererIndex in _facialAnimationController.blendShapeIndexBySkinnedMeshRendererIndexByFacialAnimationBlendShape[facialAnimationBlendShape])
            {
                _facialAnimationController.characterOriginalSkinnedMeshRenderers[blendShapeIndexBySkinnedMeshRendererIndex.Key].SetBlendShapeWeight(blendShapeIndexBySkinnedMeshRendererIndex.Value, value);
            }
        }

        public void Initialize(IT_FacialAnimationController facialAnimationController)
        {
            _facialAnimationController = facialAnimationController;
            _lipSyncContext = facialAnimationController.gameObject.AddComponent<OVRLipSyncContextDissonance>();
            _ovrLipSyncContextMorphTarget = facialAnimationController.gameObject.AddComponent<OVRLipSyncContextMorphTarget>();
            
            _ovrLipSyncContextMorphTarget.skinnedMeshRenderers = _facialAnimationController.characterOriginalSkinnedMeshRenderers;
            _ovrLipSyncContextMorphTarget.blendShapeIndexBySkinnedMeshRendererIndexByVisemeIndex = new List<Dictionary<int, int>>();
            foreach (var phonemeBlendShape in _facialAnimationBlendShapeByPhonemeIndex)
            {
                var blendShapesIndexBySkinnedMeshRendererIndex = new Dictionary<int, int>();
                foreach (var blendShapeIndexBySkinnedMeshRendererIndex in _facialAnimationController.blendShapeIndexBySkinnedMeshRendererIndexByFacialAnimationBlendShape[phonemeBlendShape])
                {
                    blendShapesIndexBySkinnedMeshRendererIndex.Add(blendShapeIndexBySkinnedMeshRendererIndex.Key, blendShapeIndexBySkinnedMeshRendererIndex.Value);
                }
                _ovrLipSyncContextMorphTarget.blendShapeIndexBySkinnedMeshRendererIndexByVisemeIndex.Add(blendShapesIndexBySkinnedMeshRendererIndex);
            }

            _lipSyncContext.provider = OVRLipSync.ContextProviders.Original;
            SetEnabledOfOvrScripts(false);
        }

        private void SetEnabledOfOvrScripts(bool enabledValue)
        {
            _lipSyncContext.enabled = enabledValue;
            _ovrLipSyncContextMorphTarget.enabled = enabledValue;
        }

        public void Enable()
        {
            SetEnabledOfOvrScripts(true);
            _playerIndexEmbodying = _facialAnimationController.characterToEmbody.playerIndexControllingEntity;
            if (_playerIndexEmbodying == IT_PlayerIdentification.OwnPlayerId)
            {
                IT_DissonanceManager.DissonanceComms.GetComponent<IMicrophoneCapture>().Subscribe(this);
                _voicePlayerState = IT_DissonanceManager.GetVoicePlayerStateByIndex(IT_PlayerIdentification.OwnPlayerId);
            }
            else
            {
                _playBackAudioSubscription = ((VoicePlayback)IT_DissonanceManager.GetVoicePlayerStateByIndex(_playerIndexEmbodying).Playback).gameObject.GetComponent<IT_PlaybackAudioSubscription>();
                _playBackAudioSubscription.Subscribe(this);
                _voicePlayerState = IT_DissonanceManager.GetVoicePlayerStateByIndex(_playerIndexEmbodying);
            }
        }
        public void Disable()
        {
            SetEnabledOfOvrScripts(false);
            if (_playerIndexEmbodying == IT_PlayerIdentification.OwnPlayerId)
            {
                IT_DissonanceManager.DissonanceComms.GetComponent<IMicrophoneCapture>().Unsubscribe(this);
            }
            else
            {
                _playBackAudioSubscription.UnSubscribe(this);
            }
        }
        
        public void SetShapeValuesToDefault()
        {
            for (int i = 0; i < numberOfShapes; i++)
            {
                SetShapeValue(i, 0);
            }
        }

        public void ReceiveData(float[] data, int channels)
        {
            _lipSyncContext.ProcessAudioSamples(data, channels);
        }

        public void ReceiveMicrophoneData(ArraySegment<float> buffer, WaveFormat format)
        {
            _amplitudeCache.Enqueue(_voicePlayerState.Amplitude);
            while (_amplitudeCache.Count > _AmplitudeFrameDelta)
            {
                _amplitudeCache.Dequeue();
            }
            //Saves 1000s of unnecessary lip sync processes during a session by checking the amplitude and not running if it is too low
            if (_amplitudeCache.Average() < _LipSyncAmplitudeThreshold)
            {
                ReceiveData(new float[1], 1);
            }
            else
            {
                ReceiveData(buffer.Array, format.Channels);
            }
        }

        public void Reset() {}
        
        /* //Previous implementation to move the jaw, please delete all these lines commented once we decide if we'll use them or not
        public void ProcessAudio(float[] data, int channels)
        {
            float peakSoundValue = 0f;
            //for (int i = 0; i < data.Length; i++)
            //{
            //    if (Mathf.Abs(data[i]) > peakSoundValue)
            //    {
            //        peakSoundValue = Mathf.Abs(data[i]);
            //    }
            //}
            float max = data.Max();
            float min = Mathf.Abs(data.Min());
            peakSoundValue = max > min ? max : min;
            peakSoundValue = Mathf.Clamp(peakSoundValue * _sensitivity,0,1) * 0.7f;
            peakSoundValue = Mathf.Clamp((peakSoundValue * peakSoundValue - peakSoundValue * peakSoundValue * peakSoundValue)/0.15f + 0.01f, 0f,1f); //(x^2 - x^3)/0.15f 
            _currentPeak = peakSoundValue;
        }
        private void Update()
        {
            _animator.SetFloat(_jawStringHash, _currentPeak);
        }*/
    }
}
