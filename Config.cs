using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nex_BringMyVehicle
{
    public class Config
    {
        // Distance Vehicle Will Stop To Player
        public static float StoppingDistance { get; set; } = 10f; // This seems to be a safe value. Configure To Your Liking.

        // Is The Mechanic Killable?
        public static bool IsMechanicInvincible { get; set; } = false; // Set to false if players can kill the mechanic.

        // Should the mechanic follow traffic laws?
        public static bool ShouldMechanicFollowTrafficLaws { get; set; } = true; // Set to false to ignore traffic lights & other vehicles.

        // Should we add a blip to the vehicle when the mechanic enters the vehicle?
        public static bool ShowVehicleBlip { get; set; } = true; // Set to false to show no vehicle blip.

        // Max Vehicle Speed
        public static float MaxVehicleSpeed { get; set; } = 60f; // Seems to be relatively safe. Increase if needed.
    }
}
