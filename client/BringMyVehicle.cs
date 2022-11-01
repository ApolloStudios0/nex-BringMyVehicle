using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.UI.Screen;
using static CitizenFX.Core.Native.API;

namespace nex_BringMyVehicle.client
{
    public class BringMyVehicle : BaseScript
    {
        public static Vehicle CurrentSavedVehicle { get; set; } = null;

        public BringMyVehicle()
        {
            // [!] Command Register
            RegisterCommand("SaveVehicle", new Action<dynamic, List<dynamic>, string>((dynamic source, List<dynamic> args, string rawCommand) =>
            {
                SaveCurrentVehicle();
            }), false);

            // [!] Command Register
            RegisterCommand("BringVehicle", new Action<dynamic, List<dynamic>, string>((dynamic source, List<dynamic> args, string rawCommand) =>
            {
                BringMyVehicleToMe();
            }), false);
        }

        public static async void BringMyVehicleToMe()
        {
            if (CurrentSavedVehicle != null && CurrentSavedVehicle.Exists())
            {
                // Get Player
                var CurrentPlayer = Game.PlayerPed;

                // Get Coords of Saved Vehicle
                var VehicleCoords = GetEntityCoords(CurrentSavedVehicle.Handle, false);

                // Create Vehicle Blip
                if (Config.ShowVehicleBlip)
                {
                    if (CurrentSavedVehicle.AttachedBlip == null || !CurrentSavedVehicle.AttachedBlip.Exists())
                    {
                        CurrentSavedVehicle.AttachBlip();
                    }
                    CurrentSavedVehicle.AttachedBlip.Sprite = BlipSprite.PersonalVehicleCar;
                    CurrentSavedVehicle.AttachedBlip.Name = "Personal Vehicle";
                }

                // [!] Mechanic Ped
                RequestModel((uint)PedHash.Xmech01SMY);

                while (!HasModelLoaded((uint)PedHash.Xmech01SMY))
                {
                    await Delay(0);
                }

                // Spawn Invinsible Mechanic Ped On Personal Vehicle
                var MechanicPed = await World.CreatePed(PedHash.Xmech01SMY, VehicleCoords);

                // Mechanic Invinicble?
                if (Config.IsMechanicInvincible) { SetEntityInvincible(MechanicPed.Handle, true); }
                else { SetEntityInvincible(MechanicPed.Handle, false); }
            
                // Set Mechanic Ped As Driver
                SetPedIntoVehicle(MechanicPed.Handle, CurrentSavedVehicle.Handle, -1);
                Notify.Success("Your Vehicle Is On Its Way. You can now see it on the map.");

                // Drive To Player
                if (Config.ShouldMechanicFollowTrafficLaws)
                {
                    TaskVehicleDriveToCoordLongrange(MechanicPed.Handle, CurrentSavedVehicle.Handle, CurrentPlayer.Position.X, CurrentPlayer.Position.Y, CurrentPlayer.Position.Z, Config.MaxVehicleSpeed, 191, Config.StoppingDistance);
                }
                else
                {
                    TaskVehicleDriveToCoordLongrange(MechanicPed.Handle, CurrentSavedVehicle.Handle, CurrentPlayer.Position.X, CurrentPlayer.Position.Y, CurrentPlayer.Position.Z, Config.MaxVehicleSpeed, 786944, Config.StoppingDistance);
                }

                // While Mechanic Is Far Away
                while (GetDistanceBetweenCoords(CurrentPlayer.Position.X, CurrentPlayer.Position.Y, CurrentPlayer.Position.Z, MechanicPed.Position.X, MechanicPed.Position.Y, MechanicPed.Position.Z, false) > Config.StoppingDistance)
                {
                    await Delay(0);
                }

                // Slown down when getting close to prevent slamming into player
                //while (GetDistanceBetweenCoords(CurrentPlayer.Position.X, CurrentPlayer.Position.Y, CurrentPlayer.Position.Z, MechanicPed.Position.X, MechanicPed.Position.Y, MechanicPed.Position.Z, false) > 10f)
                //{
                //    await Delay(0);
                //}

                // Vehicle Arrived - Notify
                Notify.Alert("Your Vehicle Has Arrived!");

                // Get Mechanic Out
                TaskLeaveVehicle(MechanicPed.Handle, CurrentSavedVehicle.Handle, 0);

                // Mechanic Flee
                MechanicPed.Task.ReactAndFlee(MechanicPed);

                // Wait & Delete Ped
                await Delay(60000);
                MechanicPed.Delete();
            }
            else
            {
                Notify.Error("You don't have a saved vehicle! You can set a vehicle to save by using /SaveVehicle command.");
            }
        }

        /// <summary>
        /// Save Current Vehicle Handle
        /// </summary>
        public static void SaveCurrentVehicle()
        {
            // Complete Some Basic Checks
            if (Game.PlayerPed.IsInVehicle())
            {
                var veh = GetVehicle();
                if (veh != null && veh.Exists())
                {
                    if (Game.PlayerPed == veh.Driver)
                    {
                        SetVehicleNeedsToBeHotwired(veh.Handle, false);
                        SetVehicleOnGroundProperly(veh.Handle);
                        SetEntityAsMissionEntity(veh.Handle, true, true);
                        SetVehicleHasBeenOwnedByPlayer(veh.Handle, true);
                        CurrentSavedVehicle = veh;
                        veh.PreviouslyOwnedByPlayer = true;
                        veh.IsPersistent = true;
                        Notify.Alert("Saved Current Vehicle. You can bring this vehicle to your location by using the /BringVehicle command.");
                    }
                }
            }

            Vehicle GetVehicle(bool lastVehicle = false)
            {
                if (lastVehicle)
                {
                    return Game.PlayerPed.LastVehicle;
                }
                else
                {
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        return Game.PlayerPed.CurrentVehicle;
                    }
                }
                return null;
            }
        }
    }
}
