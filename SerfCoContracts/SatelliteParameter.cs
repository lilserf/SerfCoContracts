using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Contracts;
using UnityEngine;

namespace SerfCoContracts
{
    class SatelliteParameter : ContractParameter
    {
        private SatelliteType mSatType;

        public SatelliteParameter()
        {
            mSatType = SatelliteType.SatTypeCommunications;
        }

        public SatelliteParameter(SatelliteType type)
        {
            mSatType = type;
        }

        protected override void OnRegister()
        {
            if(mSatType == SatelliteType.SatTypeWeather)
            {
                var barometer = PartLoader.getPartInfoByName("sensorBarometer");
                ResearchAndDevelopment.AddExperimentalPart(barometer);
                var thermometer = PartLoader.getPartInfoByName("sensorThermometer");
                ResearchAndDevelopment.AddExperimentalPart(thermometer);
            }
            else if(mSatType == SatelliteType.SatTypeEspionage)
            {
                var smallGearBay = PartLoader.getPartInfoByName("SmallGearBay");
                ResearchAndDevelopment.AddExperimentalPart(smallGearBay);
            }
            else if(mSatType == SatelliteType.SatTypeCommunications)
            {
                var commDish = PartLoader.getPartInfoByName("commDish");
                ResearchAndDevelopment.AddExperimentalPart(commDish);
            }
        }

        protected override void OnUnregister()
        {
            if (mSatType == SatelliteType.SatTypeWeather)
            {
                var barometer = PartLoader.getPartInfoByName("sensorBarometer");
                ResearchAndDevelopment.RemoveExperimentalPart(barometer);
                var thermometer = PartLoader.getPartInfoByName("sensorThermometer");
                ResearchAndDevelopment.RemoveExperimentalPart(thermometer);
            }
            else if (mSatType == SatelliteType.SatTypeEspionage)
            {
                var smallGearBay = PartLoader.getPartInfoByName("SmallGearBay");
                ResearchAndDevelopment.RemoveExperimentalPart(smallGearBay);
            }
            else if (mSatType == SatelliteType.SatTypeCommunications)
            {
                var commDish = PartLoader.getPartInfoByName("commDish");
                ResearchAndDevelopment.RemoveExperimentalPart(commDish);
            }

        }

        protected override void OnLoad(ConfigNode node)
        {
            mSatType = (SatelliteType)int.Parse(node.GetValue("satType"));
        }

        protected override void OnSave(ConfigNode node)
        {
            node.AddValue("satType", (int)mSatType);
        }

        protected override string GetTitle()
        {
            return "Build an unmanned " + SatHelper.GetSatTypeString(mSatType) + " satellite that includes "+SatHelper.GetRequiredParts(mSatType)+".";
        }

        protected override string GetHashString()
        {
            return "SatelliteParameter";
        }

        protected override void OnUpdate()
        {
            Vessel v = FlightGlobals.ActiveVessel;

            //Debug.Log("[SatelliteParameter] OnUpdate");

            if (v != null)
            {
                if(v.GetCrewCapacity() > 0 || v.FindPartModulesImplementing<ModuleDockingNode>().Count() == 0)
                {
                    // Manned craft don't count, and satellites must have a docking port
                    base.SetIncomplete();
                    return;
                }

                if (mSatType == SatelliteType.SatTypeCommunications && v.FindPartModulesImplementing<ModuleDataTransmitter>().Count() > 0)
                {
                    base.SetComplete();
                }
                else if (mSatType == SatelliteType.SatTypeWeather)
                {
                    if (v.FindPartModulesImplementing<ModuleEnviroSensor>().Count() > 0)
                    {
                        base.SetComplete();
                    }
                    else
                    {
                        base.SetIncomplete();
                    }
                }
                else if (mSatType == SatelliteType.SatTypeEspionage)
                {
                    if(v.Parts.Where(p => p.name == "SmallGearBay").Count() > 0)
                    {
                        base.SetComplete();
                    }
                    else
                    {
                        base.SetIncomplete();
                    }
                }
                else
                {
                    base.SetIncomplete();
                }
            }
        }
    }
}
