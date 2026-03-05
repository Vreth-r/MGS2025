using UnityEngine;

// alright this is jank but theres issues that i have no time to solve normally
public sealed class CoroutineRunner : MonoBehaviour
{
    private static CoroutineRunner _i;

    public static CoroutineRunner I
    {
        get
        {
            if (_i != null) return _i;
            var go = new GameObject("~CoroutineRunner");
            DontDestroyOnLoad(go);
            _i = go.AddComponent<CoroutineRunner>();
            return _i;
        }
    }
}