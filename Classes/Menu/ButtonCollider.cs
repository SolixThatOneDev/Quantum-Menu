/*
 * Quantum Menu  Classes/Menu/ButtonCollider.cs
 * A community driven mod menu for Gorilla Tag with over 1000+ mods
 *
 * Copyright (C) 2026  Quantum Software
 * https://github.com/Quantum/Quantum-Menu
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Quantum.Managers;
using UnityEngine;
using static Quantum.Menu.Main;

namespace Quantum.Classes.Menu
{
    public class ButtonCollider : MonoBehaviour
    {
        public string relatedText;

        public bool incremental;
        public bool positive;

        public void OnTriggerEnter(Collider collider)
        {
            // Cooldown failsafe
            if (buttonCooldown > Time.time + 1f) buttonCooldown = 0f;
            if (Time.time < buttonCooldown || joystickMenu || menu == null) return;

            // Relaxed hand check
            bool isHand = (collider == buttonCollider || collider == lKeyCollider || collider == rKeyCollider) || 
                          collider.name.Contains("Sphere") || 
                          collider.name.Contains("Pointer");

            if (!isHand) return;

            PerformToggle();
        }

        public void PerformToggle()
        {
            buttonCooldown = Time.time + 0.35f;

            if (incremental)
                ToggleIncremental(relatedText, positive);
            else
                Toggle(relatedText, true);

            // Restore sounds and notifications
            try
            {
                /* Sound removed by user request
                if (relatedText != "Global Return") 
                    SoundManager.Play(SoundManager.DefaultSounds["Button"], buttonText: relatedText);
                */
            }
            catch { }
        }
    }
}

