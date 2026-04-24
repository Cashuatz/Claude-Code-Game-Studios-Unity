namespace Proto.UI.Speech
{
    /// <summary>
    /// 말풍선 자동 소멸 정책.
    /// Show() 호출 시 개별 지정 가능, 지정 안 하면 SpeechBubble 인스턴스 기본값 사용.
    /// </summary>
    public enum BubbleDismissMode
    {
        /// <summary>타이핑 완료 후 holdSeconds 만큼 기다린 뒤 자동 소멸.</summary>
        AutoHide,
        /// <summary>Hide() 를 명시적으로 호출할 때까지 유지.</summary>
        Manual,
        /// <summary>타이핑 완료 후 유지되다가 다음 Show 호출 시점에서 교체.</summary>
        HideOnNext,
    }

    /// <summary>
    /// 한 말풍선이 이미 말하는 중에 Show 가 다시 호출됐을 때의 정책.
    /// 말풍선 인스턴스 당 하나의 정책을 가진다 (런타임 변경 가능).
    /// </summary>
    public enum BubbleConcurrencyMode
    {
        /// <summary>즉시 새 텍스트로 교체 (타이핑 재시작). 기본값.</summary>
        Overwrite,
        /// <summary>현재 것이 끝나면 다음 것을 이어서 재생 (FIFO).</summary>
        Queue,
        /// <summary>말하는 중이면 새 요청 무시.</summary>
        Ignore,
        /// <summary>현재 텍스트 뒤에 이어쓰기 (타이핑 계속 진행).</summary>
        Append,
    }
}
