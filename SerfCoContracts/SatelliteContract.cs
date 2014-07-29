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

        public static string GenerateRandomSatName(SatelliteType satType)
        {
            string word = "SAT";

            if(satType == SatelliteType.SatTypeCommunications)
            {
                word = GenerateRandomCommName();
            }
            else if(satType == SatelliteType.SatTypeWeather)
            {
                word = GenerateRandomWeatherName();
            }
            else if(satType == SatelliteType.SatTypeEspionage)
            {
                word = GenerateRandomEspionageName();
            }

            int num = (int)(UnityEngine.Random.Range(0, 50));

            return word + "-" + num.ToString();
        }

        private static string GenerateRandomWeatherName()
        {
            string[] names = { "NIMBUS", "CIRRUS", "STRATUS", "METEOR", "KOAA", "BLIZZARD", "TYPHOON", "TORNADO" };

            return names[(int)UnityEngine.Random.Range(0, names.Length)];
        }

        private static string GenerateRandomCommName()
        {
            string[] pre = { "TEL", "COM", "SYN", "RELAY", "ECHO", "KERB", "K-" };
            string[] post = { "STAR", "SAT", "COM", "" };

            string first = pre[(int)UnityEngine.Random.Range(0, pre.Length)];
            string second = pre[(int)UnityEngine.Random.Range(0, post.Length)];

            return first + second;
        }

        private static string GenerateRandomEspionageName()
        {
            string[] names = 
            {
                "ARCTIC",
                "BLUE",
                "CONCORD",
                "DEFECT",
                "ENSURE",
                "FORESIGHT",
                "GREEN",
                "HEAT",
                "ICEBERG",
                "JUSTIFY",
                "KEYHOLE",
                "LIME",
                "MELTDOWN",
                "NERVE",
                "ORGAN",
                "PROPER",
                "QUIET",
                "REFLECT",
                "SANITY",
                "TRUST",
                "UNITE",
                "VENOM",
                "WORTHY",
                "XRAY",
                "YELLOW",
                "ZINC"
            };

            return names[(int)UnityEngine.Random.Range(0, names.Length)];
        }
    }

    public class SatelliteContract : Contract
    {

        CelestialBody mTargetBody;
        SatelliteType mSatType;
        String mSatName;

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

            mSatName = SatHelper.GenerateRandomSatName(mSatType);

            base.SetExpiry();
            base.SetDeadlineYears(2, mTargetBody);
            base.SetFunds(1000, 1000, mTargetBody);
            base.SetReputation(10, mTargetBody);
            base.SetScience(100, mTargetBody);

            AddParameter(new SatelliteParameter(mSatType));
            AddParameter(new LocationAndSituationParameter(mTargetBody, Vessel.Situations.ORBITING, SatHelper.GetSatTypeString(mSatType)+" satellite"));

            // Randomly add an altitude window
            if(UnityEngine.Random.Range(0,100) > 50)
            {
                var targetAlt = UnityEngine.Mathf.Floor(UnityEngine.Random.Range(1200, 5000)) * 100;
                var altWindow = UnityEngine.Mathf.Floor(UnityEngine.Random.Range(100, 500)) * 100;
                AddParameter(new OrbitalParameter(targetAlt, altWindow));
            }

            return true;
        }

        protected override void OnAccepted()
        {
            var ap = PartLoader.getPartInfoByName("dockingPort2");
            ResearchAndDevelopment.AddExperimentalPart(ap);
        }

        protected override void OnCompleted()
        {
            var v = FlightGlobals.ActiveVessel;

            v.vesselName = mSatName;
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
            return "Satellite" + mSatName;
        }

        
        protected override string GetTitle()
        {
            return "Orbit "+SatHelper.GetSatTypeString(mSatType)+" satellite "+mSatName+" around " + mTargetBody.theName;
        }

        protected override string GetSynopsys()
        {
            return "Orbit " + SatHelper.GetSatTypeString(mSatType) + " satellite "+mSatName+" containing "+SatHelper.GetRequiredParts(mSatType)+" around " + mTargetBody.theName;
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
            var desc = "Uh, yes, we're here from "+this.Agent.Name+". Pay no attention to our dark suits and sunglasses.\n\n"
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
            mSatName = node.GetValue("satName");
        }

        protected override void OnSave(ConfigNode node)
        {
            int bodyID = mTargetBody.flightGlobalsIndex;
            node.AddValue("targetBody", bodyID);
            node.AddValue("satType", (int)mSatType);
            node.AddValue("satName", mSatName);
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
