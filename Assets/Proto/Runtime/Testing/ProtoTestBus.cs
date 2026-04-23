using System;
using System.Collections.Generic;
using UnityEngine;

namespace Proto.Testing
{
    public enum ProtoEventKind
    {
        Info,
        Move,
        Collision,
        ModeChange,
        TransitionCompleted,
        Teleport,
        GroundState,
        Action,
        Warning,
        Error
    }

    [Serializable]
    public struct ProtoEvent
    {
        public double timeAsDouble;
        public ProtoEventKind kind;
        public string detail;
    }

    /// <summary>
    /// 런타임/에디터가 공유하는 정적 이벤트 버스.
    /// VKL 관측값 기록·에디터 윈도우 로그 표시에 사용.
    /// 링 버퍼 (최대 200개).
    /// </summary>
    public static class ProtoTestBus
    {
        public const int MaxEntries = 200;
        private static readonly List<ProtoEvent> s_events = new List<ProtoEvent>(MaxEntries);

        public static event Action<ProtoEvent> OnEvent;
        public static event Action OnCleared;

        public static IReadOnlyList<ProtoEvent> Events => s_events;
        public static int Count => s_events.Count;

        public static void Emit(ProtoEventKind kind, string detail)
        {
            var evt = new ProtoEvent
            {
                timeAsDouble = Application.isPlaying ? Time.timeAsDouble : 0.0,
                kind = kind,
                detail = detail ?? string.Empty
            };
            s_events.Add(evt);
            if (s_events.Count > MaxEntries) s_events.RemoveAt(0);
            OnEvent?.Invoke(evt);
        }

        public static void Clear()
        {
            s_events.Clear();
            OnCleared?.Invoke();
        }
    }
}
