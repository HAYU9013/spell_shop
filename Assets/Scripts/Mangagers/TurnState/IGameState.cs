using System.Collections;
using UnityEngine;

public interface IGameState
{
    IEnumerator Enter(); // 進入狀態時執行
    IEnumerator Execute(); // 狀態進行中
    IEnumerator Exit(); // 離開狀態前執行
}
