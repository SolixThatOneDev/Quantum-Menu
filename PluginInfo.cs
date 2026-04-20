/*
 * Quantum Menu  PluginInfo.cs
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

namespace Quantum
{
    public class PluginInfo
    {
        public const string GUID = "org.quantum.gorillatag.quantummenu";
        public const string Name = "Quantum Menu";
        public const string Description = "Community powered mod menu for Gorilla Tag.";
        public const string BuildTimestamp = "2026-04-20T21:36:28Z";
        public const string Version = "1.2.2";

        public const string BaseDirectory = "Quantum";
        public const string ClientResourcePath = "QuantumMenu.Resources.Client";

        // RENDER SETUP: Replace 'YOUR-RENDER-NAME' with your actual Render service name
        public const string ServerResourcePath = "https://quantum-menu-api.onrender.com/Resources/Server";
        
        // BACKEND SETUP: Replace 'YOUR-RENDER-NAME' with your actual Render service name
        public const string ServerAPI = "https://quantum-menu-api.onrender.com";
        public const string Logo = @"
                                      ################                                      
                                  ########################                                  
                                #######              #######                                
                              #######                  #######                              
                             #######                    #######                             
                            #######                      #######                            
                            #######                      #######                            
                            #######                      #######                            
                            #######                      #######                            
                             #######                    #######                             
                              #######                  #######                              
                                #######              #######                                
                                  ########################                                  
                                      ################  #######                             
                                                           #######                          
                                                             #######                        
                                                               #######                      ";

#if DEBUG
        public static bool BetaBuild = true;
#else
        public static bool BetaBuild = false;
#endif

        // Configuration Flags
        public static bool UseRemoteResources = true; // Enabled for your new Backend
        public static bool UseServerAPI = true;      // Enabled for your new Backend
    }
}
