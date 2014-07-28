using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Contracts;
using Contracts.Parameters;

namespace SerfCoContracts
{
    public enum SatelliteType
    {
        SatTypeCommunications,
        SatTypeEspionage,
        SatTypeWeather,
        SatTypeCount,
    };

    public class SatHelper
    {
        public static string GetSatTypeString(SatelliteType satType)
        {
            switch (satType)
            {
                case SatelliteType.SatTypeEspionage: return "clandestine";
                case SatelliteType.SatTypeCommunications: return "communications";
                case SatelliteType.SatTypeWeather: return "weather";
            }

            return "unknown";
        }

        public static string GetRequiredParts(SatelliteType satType)
        {
            switch(satType)
            {
                case SatelliteType.SatTypeWeather: return "a docking port and a sensor";
                case SatelliteType.SatTypeCommunications: return "a docking port and an antenna";
                case SatelliteType.SatTypeEspionage: return "a docking port and a Small Gear Bay";
            }

            return "unknown";
        }
    }

    public class SatelliteContract : Contract
    {

        CelestialBody mTargetBody;
        SatelliteType mSatType;

        private CelestialBody ChooseTargetBody()
        {
            if(Prestige == ContractPrestige.Trivial)
            {
                return Planetarium.fetch.Home;
            }
            else
            {
                return GetBodies_NextUnreached(1).FirstOrDefault();
            }
        }

        protected override bool Generate()
        {
            mSatType = (SatelliteType)(UnityEngine.Random.Range(0, (int)SatelliteType.SatTypeCount));

            if(mSatType == SatelliteType.SatTypeWeather || mSatType == SatelliteType.SatTypeEspionage)
            {
                // Weather sats are always around Kerbin
                mTargetBody = Planetarium.fetch.Home;
            }
            else
            {
                mTargetBody = ChooseTargetBody();
            }

            if (mTargetBody == null)
            {
                mTargetBody = Planetarium.fetch.Home;
            }

            base.SetExpiry();
            base.SetDeadlineYears(2, mTargetBody);
            base.SetFunds(1000, 1000, mTargetBody);
            base.SetReputation(10, mTargetBody);
            base.SetScience(100, mTargetBody);

            AddParameter(new SatelliteParameter(mSatType));
            AddParameter(new LocationAndSituationParameter(mTargetBody, Vessel.Situations.ORBITING, SatHelper.GetSatTypeString(mSatType)+" satellite"));

            return true;
        }

        protected override void OnAccepted()
        {
            var ap = PartLoader.getPartInfoByName("dockingPort2");
            ResearchAndDevelopment.AddExperimentalPart(ap);
        }

        public override bool CanBeCancelled()
        {
            return true;
        }

        public override bool CanBeDeclined()
        {
            return true;
        }

        protected override string GetHashString()
        {
            return "Satellite" + mTargetBody.bodyName;
        }

        
        protected override string GetTitle()
        {
            return "Orbit a "+SatHelper.GetSatTypeString(mSatType)+" satellite around " + mTargetBody.theName;
        }

        protected override string GetSynopsys()
        {
            return "Orbit a " + SatHelper.GetSatTypeString(mSatType) + " satellite containing "+SatHelper.GetRequiredParts(mSatType)+" around " + mTargetBody.theName;
        }

        protected override string GetDescription()
        {
            switch(mSatType)
            {
                case SatelliteType.SatTypeCommunications: return GetDefaultDescription();
                case SatelliteType.SatTypeWeather: return GetWeatherDescription();
                case SatelliteType.SatTypeEspionage: return GetEspionageDescription();
            }

            return GetDefaultDescription();
        }

        private string GetWeatherDescription()
        {
            var desc = "In order to better understand the weather of our beloved home planet, we need to observe and measure it from space!\n\n"
                + "We need you to launch a satellite with a thermometer or barometer on it that we can use to continuously monitor the temperature "
                + "and barometric pressure on Kerbin. Those sensors will work correctly from space, right?";

            return desc;
        }

        private string GetEspionageDescription()
        {
            var desc = "Uh, yes, we're here from the agency stated in the contract. Pay no attention to our dark suits and sunglasses.\n\n"
               + "We need you to put a satellite in orbit that has a very... specific piece of gear on board. It may look like a simple "
               + "landing gear pod, but we've modified it to suit our needs. Please place it into the specified orbit and DON'T TELL ANYONE.";

            return desc;
        }

        private string GetDefaultDescription()
        {
            return TextGen.GenerateBackStories(Agent.Name, Agent.GetMindsetString(), "satellites", "satellites", "science", new System.Random().Next());
        }

        protected override string MessageCompleted()
        {
            return "You put a " + SatHelper.GetSatTypeString(mSatType) + " satellite into orbit around " + mTargetBody.theName;
        }

        protected override void OnLoad(ConfigNode node)
        {
            int bodyID = int.Parse(node.GetValue("targetBody"));
            foreach (var body in FlightGlobals.Bodies)
            {
                if (body.flightGlobalsIndex == bodyID)
                    mTargetBody = body;
            }
            mSatType = (SatelliteType)(int.Parse(node.GetValue("satType")));
        }

        protected override void OnSave(ConfigNode node)
        {
            int bodyID = mTargetBody.flightGlobalsIndex;
            node.AddValue("targetBody", bodyID);
            node.AddValue("satType", (int)mSatType);
        }

        public override bool MeetRequirements()
        {
            // Temp: always available
            return true;


            string[] parts =
            {
                "probeCoreCube",
                "probeCoreHex",
                "probeCoreOcto",
                "probeCoreOcto2",
                "probeCoreSphere",
                "probeStackLarge",
                "probeStackSmall"
            };
            
            foreach(var partName in parts)
            {
                var ap = PartLoader.getPartInfoByName(partName);
                if(ResearchAndDevelopment.PartTechAvailable(ap))
                {
                    // Early return
                    return true;
                }
            }

            return false;
        }
    }
}
