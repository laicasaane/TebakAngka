using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using TebakAngka.Gameplay;
using TebakAngka.View;
using UnityEngine;
using VContainer.Unity;
using Random = UnityEngine.Random;

namespace TebakAngka.Presenter
{
    public class LevelAudioPresenter : IInitializable, IDisposable
    {
        private readonly IAsyncSubscriber<GameStateEnum, int> _correctAnswerSubscriber;
        private readonly AudioClipCollection _introClipsIbun;
        private readonly AudioClipCollection _introClipsRanca;
        private readonly AudioClipCollection _numberClipsIbun;
        private readonly AudioClipCollection _numberClipsRanca;
        private readonly AudioSource _audioSource;

        private IDisposable _subscription;

        public LevelAudioPresenter(
            IAsyncSubscriber<GameStateEnum, int> correctAnswerSubscriber,
            AudioClipCollection[] audioClipCollections,
            AudioSource audioSource)
        {
            _correctAnswerSubscriber = correctAnswerSubscriber;
            _introClipsIbun = audioClipCollections[0];
            _introClipsRanca = audioClipCollections[1];
            _numberClipsIbun = audioClipCollections[2];
            _numberClipsRanca = audioClipCollections[3];
             _audioSource = audioSource;
        }

        public void Initialize()
        {
            var bag = DisposableBag.CreateBuilder();

            _correctAnswerSubscriber.Subscribe(GameStateEnum.GenerateLevel, PlayLevelIntroAudio).AddTo(bag);

            _subscription = bag.Build();
        }

        private async UniTask PlayLevelIntroAudio(int answer, CancellationToken token)
        {
            var baseIdx = Random.Range(0, 2);

            _audioSource.clip = baseIdx == 0
                ? _introClipsIbun[Random.Range(0, _introClipsIbun.Count)]
                : _introClipsRanca[Random.Range(0, _introClipsRanca.Count)];
            _audioSource.pitch = Random.Range(0.95f, 1.05f);
            _audioSource.Play();

            await UniTask.Delay(TimeSpan.FromSeconds(_audioSource.clip.length), cancellationToken: token);
            await UniTask.NextFrame(token);

            _audioSource.clip = baseIdx == 0 ? _numberClipsIbun[answer] : _numberClipsRanca[answer];
            _audioSource.pitch = Random.Range(0.95f, 1.05f);
            _audioSource.Play();

            await UniTask.Delay(TimeSpan.FromSeconds(_audioSource.clip.length), cancellationToken: token);
        }

        public void Dispose()
        {
            _subscription?.Dispose();
        }
    }
}