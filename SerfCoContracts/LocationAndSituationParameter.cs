using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Contracts;

namespace SerfCoContracts
{
    //
    public class LocationAndSituationParameter : ContractParameter
    {
        // Unfortunately the default Squad parameters for this are insufficient for my needs, I need to rewrite them here.
        public CelestialBody targetBody = null;
        public Vessel.Situations targetSituation = Vessel.Situations.ORBITING;
        public string noun = "satellite";

        public LocationAndSituationParameter()
        {
            targetSituation = Vessel.Situations.ORBITING;
            targetBody = Planetarium.fetch.Home;
        }

        public LocationAndSituationParameter(CelestialBody targetBody, Vessel.Situations targetSituation, string noun)
        {
            this.targetBody = targetBody;
            this.targetSituation = targetSituation;
            this.noun = noun;
        }

        protected override string GetHashString()
        {
            return "SerfCoLocationAndSituationParameter";
        }

        protected override string GetTitle()
        {
            switch (targetSituation)
            {
                case Vessel.Situations.DOCKED:
                    return "Dock your " + noun + " near " + targetBody.theName;
                case Vessel.Situations.ESCAPING:
                    return "Get your " + noun + " into an escape trajectory from " + targetBody.theName;
                case Vessel.Situations.FLYING:
                    return "Fly your " + noun + " on " + targetBody.theName;
                case Vessel.Situations.LANDED:
                    return "Land your " + noun + " on " + targetBody.theName;
                case Vessel.Situations.ORBITING:
                    return "Orbit your " + noun + " around " + targetBody.theName;
                case Vessel.Situations.PRELAUNCH:
                    return "Be ready to launch your " + noun + " from " + targetBody.theName;
                case Vessel.Situations.SPLASHED:
                    return "Splash your " + noun + " down on " + targetBody.theName;
                case Vessel.Situations.SUB_ORBITAL:
                    return "Set your " + noun + " on a crash course for " + targetBody.theName;
                default:
                    return "Have your " + noun + " near " + targetBody.theName;
            }
        }

        protected override void OnRegister()
        {

        }

        protected override void OnSave(ConfigNode node)
        {
            int bodyID = targetBody.flightGlobalsIndex;
            node.AddValue("targetBody", bodyID);
            node.AddValue("targetSituation", (int)targetSituation);
            node.AddValue("noun", noun);
        }
        protected override void OnLoad(ConfigNode node)
        {
            int bodyID = int.Parse(node.GetValue("targetBody"));
            foreach (var body in FlightGlobals.Bodies)
            {
                if (body.flightGlobalsIndex == bodyID)
                    targetBody = body;
            }
            targetSituation = (Vessel.Situations)int.Parse(node.GetValue("targetSituation"));
            noun = node.GetValue("noun");
        }

        protected override void OnUpdate()
        {
            Vessel v = FlightGlobals.ActiveVessel;

            if (v.situation != targetSituation || v.mainBody != targetBody)
                base.SetIncomplete();
            else
                base.SetComplete();
        }
    }
}
