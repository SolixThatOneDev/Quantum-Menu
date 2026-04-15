using GorillaLocomotion;
using System;

namespace Quantum.Mods
{
    public static class Diagnostic
    {
        public static void Test()
        {
            // This should compile if the publicized assembly is working correctly
            typeof(GorillaLocomotion.GTPlayer).ToString();
        }
    }
}

