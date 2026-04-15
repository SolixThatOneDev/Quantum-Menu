using GorillaLocomotion;
using System;
using System.Reflection;
using UnityEngine;

namespace Quantum.Mods
{
    public static class MethodDiagnostics
    {
        public static void Run()
        {
            Debug.Log("Quantum Menu: Starting Method Diagnostics for GTPlayer...");
            try
            {
                Type type = typeof(GTPlayer);
                Debug.Log($"Quantum Menu: Found type {type.FullName}");

                Debug.Log("Quantum Menu: All Methods:");
                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                {
                    Debug.Log($"Quantum Menu: Method - {method.Name}");
                }

                Debug.Log("Quantum Menu: All Fields:");
                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                {
                    Debug.Log($"Quantum Menu: Field - {field.Name}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Quantum Menu: Diagnostics failed: {ex}");
            }
        }
    }
}
