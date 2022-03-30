using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LunarBind;
using LunarBind.Yielding;
using UnityEngine;

namespace LunarBind
{
    [LunarBindIgnoreAssemblyAdd]
    public static class UnityHelper
    {
        static GameObject coroutineRunnerGob = null;

        static LBCoroutineRunner coroutineRunner = null;
        public static LBCoroutineRunner CoroutineRunner
        {
            get
            {
                EnsureRunnerExists();
                return coroutineRunner;
            }
        }

        private static void EnsureRunnerExists()
        {
            if (coroutineRunnerGob == null)
            {
                coroutineRunnerGob = new GameObject("LB_Unity_Coroutine_Helper");
                coroutineRunnerGob.transform.position = Vector3.zero;
                coroutineRunnerGob.isStatic = true;
                coroutineRunner = coroutineRunnerGob.AddComponent<LBCoroutineRunner>();
            }
        }
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void OnAfterAssembliesLoadRuntimeMethod()
        {
            GlobalScriptBindings.RegisterUserDataType(typeof(IEnumerator));
            GlobalScriptBindings.BindTypeFuncs(typeof(UnityHelper));
        }

        //[LunarBindHide]
        [LunarBindDocumentation("Starts an IEnumerator coroutine and continues without waiting for completion")]
        [LunarBindFunction("Unity.StartCoroutine")]
        static void StartUnityCoroutine(IEnumerator coroutine)
        {
            CoroutineRunner.RunCoroutine(coroutine);
        }

        //[LunarBindHide]
        [LunarBindDocumentation("Starts an IEnumerator coroutine and waits for it to complete. Identical to calling coroutine.yield(YourFunc)")]
        [LunarBindFunction("Unity.AwaitCoroutine")]
        static WaitForDone WaitForUnityCoroutine(IEnumerator coroutine)
        {
            return CoroutineRunner.WaitForCoroutine(coroutine);
        }

        public class LBCoroutineRunner : UnityEngine.MonoBehaviour
        {
            public WaitForDone WaitForCoroutine(IEnumerator coroutine)
            {
                var yielder = new WaitForDone();
                IEnumerator Routine(WaitForDone waitForDone)
                {
                    yield return coroutine;
                    waitForDone.Done = true;
                }
                StartCoroutine(Routine(yielder));
                return yielder;
            }

            public void RunCoroutine(IEnumerator coroutine)
            {
                StartCoroutine(coroutine);
            }
        }

    }
}