using System.Collections;
using UnityEngine;

namespace Proto.Testing
{
    /// <summary>
    /// Play 모드 테스트 시퀀스용 코루틴 러너.
    /// 싱글톤 + 숨김 GameObject. 씬 재로드·Play 중단 시 자연 소멸.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ProtoTestRunner : MonoBehaviour
    {
        private static ProtoTestRunner s_instance;

        public static ProtoTestRunner Instance
        {
            get
            {
                if (s_instance != null) return s_instance;
                var go = new GameObject("Proto.TestRunner");
                go.hideFlags = HideFlags.DontSaveInEditor;
                s_instance = go.AddComponent<ProtoTestRunner>();
                return s_instance;
            }
        }

        public Coroutine Run(IEnumerator seq)
        {
            if (seq == null) return null;
            return StartCoroutine(seq);
        }

        private void OnDestroy()
        {
            if (s_instance == this) s_instance = null;
        }
    }
}
