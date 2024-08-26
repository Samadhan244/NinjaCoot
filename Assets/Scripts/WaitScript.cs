using UnityEngine;
using UnityEngine.Events;
using System.Threading.Tasks;

public static class WaitScript
{
    public static async void Wait(this MonoBehaviour mono, float delay, UnityAction action)
    {
        await Task.Delay((int)(delay * 1000));
        if (mono != null && mono.gameObject != null) action?.Invoke();
    }
}